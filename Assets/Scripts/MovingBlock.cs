using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class MovingBlock : MonoBehaviour
{
    /// Member Variables
    // How much the block should move with any input to stay in the grid
    [SerializeField] private int _descentIncrement = 2;
    
    // Grid Variables that a shared between each spawned shape
    static private int _gridSizeX = 15;
    static private int _gridSizeY = 26;
    // 2D Transform array that use the total grid size as the inputs
    static private Transform[,] _shapeGrid = new Transform[_gridSizeX, _gridSizeY];

    private float _currentFallSpeed;
    BlockManipulator _manipulator;
    Vector2 _currentPos;
    private IEnumerator _currentState;
    ///

    // Initializes some variables from the spawner and starts the blocks descent
    public void UpdateShape(float fallSpeed, Vector2[] positions, BlockManipulator manipulator, BlockInfo blockInfo)
    {
        // Set member variables
        _currentFallSpeed = fallSpeed;
        _manipulator = manipulator;

        // Sets the shapes color
        GetShapeColor(blockInfo);

        // Set the position of all blocks to the input positions
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = positions[i];
        }

        // If the block already cant move, this means its Game-Over, if it can move, start descending
        if (!CheckCanMoveY()) { ChangeState(ResetState()); }
        else { ChangeState(FallingState()); }
    }

    // Speeds up the falling process, and updates it by reseting the FallingState coroutine
    public void SpeedUp(float newFallSpeed)
    {
        _currentFallSpeed = newFallSpeed;
        ChangeState(FallingState());
    }

    // Changes the active coroutine to the input coroutine, and stops the existing coroutine
    private void ChangeState(IEnumerator newState)
    {
        if (_currentState != null) StopCoroutine(_currentState);

        _currentState = newState;
        StartCoroutine(_currentState);
    }

    // While this Coroutine is active, check if the block can descend, and if it can, 
    // lower is position by the descent increment
    private IEnumerator FallingState()
    {
        while (true)
        {
            if (CheckCanMoveY())
            {
                _currentPos = transform.position;
                _currentPos.y -= _descentIncrement;
                transform.position = _currentPos;
            }

            // Wait for the fall speed before looping again
            yield return new WaitForSeconds(_currentFallSpeed);
        }
    }

    // Empty Coroutine that is entered when the block should stop falling
    private IEnumerator ResetState()
    {
        yield return null;
    }

    // Function for the player to manually move the block on the X axis
    public void MoveBlockX(int direction)
    {
        // If the block cant move in the given direction, stop
        if (!CheckCanMoveX(direction)) return;

        // If it can move, set the new position based off the direction
        _currentPos = transform.position;
        _currentPos.x += _descentIncrement * direction;
        transform.position = _currentPos;
    }

    // Rotate the block, then check if the rotation was valid, if it wasnt, rotate it back to the original position
    public void RotateBlock(float rotateAmount)
    {
        transform.Rotate(Vector3.forward, -rotateAmount);
        if (!CheckCanRotate()) { transform.Rotate(Vector3.forward, rotateAmount); }
    }

    private bool CheckCanMoveY()
    {
        foreach (Transform child in transform)
        {
            // Round the Vector3 positions to ints
            Vector3 childPos = child.position;
            int X = Mathf.RoundToInt(childPos.x);
            int Y = Mathf.RoundToInt(childPos.y);

            // Check if Block is overlapping another before decrement is done
            if (_shapeGrid[X, Y] != null)
            {
                /// Fail State here
                ChangeState(ResetState());
                return false;
            }

            // Do decrement and if its at the bottom, or will be overlapping another shape, stop moving and spawn another block
            Y -= _descentIncrement;
            if (Y < 1 || _shapeGrid[X, Y] != null)
            {
                ChangeState(ResetState());
                AddShapeToGrid();
                _manipulator.SpawnBlock();

                return false;
            }
        }

        return true;
    }

    // Check if the block can move along the X Axis
    private bool CheckCanMoveX(float movementIncrement)
    {
        foreach (Transform child in transform)
        {
            Vector3 childPos = child.position;
            int X = Mathf.RoundToInt(childPos.x + (_descentIncrement * movementIncrement));
            int Y = Mathf.RoundToInt(childPos.y);

            // Check if the block is outside of the grid on the left or right
            if (X < 1 || X >= _gridSizeX) return false;

            // Check if the space its going to be in, already has a shape in it
            if (_shapeGrid[X, Y] != null) return false;
        }

        return true;
    }

    // Check both the X and Y if their new positions are valid, and check if the new position is overlapping with another shape
    private bool CheckCanRotate()
    {
        foreach (Transform child in transform)
        {
            Vector3 childPos = child.position;
            int X = Mathf.RoundToInt(childPos.x);
            int Y = Mathf.RoundToInt(childPos.y);

            if (X < 1 || X >= _gridSizeX || Y < 1) return false;

            if (_shapeGrid[X, Y] != null) return false;
        }

        return true;
    }

    // Adds the shape to the 2D Grid array and checks if the row(s) is now full
    private void AddShapeToGrid()
    {
        foreach (Transform child in transform)
        {
            Vector3 childPos = child.position;
            int X = Mathf.RoundToInt(childPos.x);
            int Y = Mathf.RoundToInt(childPos.y);
            
            _shapeGrid[X, Y] = child;

            CheckForRowMatch();
        }
    }

    // Checks if theirs any full rows in the grid, if their is, clear the row, and drop all rows above the cleared row
    private void CheckForRowMatch()
    {
        // Loop through each row
        for (int i = _gridSizeY - 1; i >= 1; i--)
        {
            if (CheckRow(i)) 
            {
                ScoreManager.Instance.AddScore();
                ClearRow(i);
                DropRows(i);
            }
        }
    }

    // Check if a row is full by sorting through each column in the _shapeGrid array
    private bool CheckRow(int i)
    {
        for (int j = _gridSizeX - 1; j >= 1; j--)
        {
            // If at any point theirs an empty space in the row, return false
            if (_shapeGrid[j, i] == null) { return false; }
        }

        // If there was no empty spaces, this row is full
        return true;
    }

    // Deletes the row that is full of shapes
    private void ClearRow(int i)
    {
        for (int j = _gridSizeX - 1; j >= 1; j--)
        {
            if (_shapeGrid[j, i] != null) 
            {
                // Remove the shape visually and clear its position in the array
                Destroy(_shapeGrid[j, i].gameObject);
                _shapeGrid[j, i] = null; 
            }
        }
    }

    // Drops all rows above the cleared row
    private void DropRows(int i)
    {
        // Check each row above the given I value for blocks that need to come down.
        for (int y = i; y < _gridSizeY - 1; y++)
        {
            // Check through each column
            for (int j = _gridSizeX - 1; j >= 1; j--)
            {
                // If the block above is not null, that means it needs to come down
                if (_shapeGrid[j, y + 1] != null)
                {
                    // Set the new position in the array, and lower its position by the Descent Increment
                    _shapeGrid[j, y] = _shapeGrid[j, y + 1];
                    _shapeGrid[j, y + 1] = null;
                    Vector3 newPos = _shapeGrid[j, y].transform.position;
                    newPos.y -= _descentIncrement;
                    _shapeGrid[j, y].transform.position = newPos;
                }
            }
        }
    }

    // Gets the shape colors depending on the block info, then sets it
    private void GetShapeColor(BlockInfo info)
    {
        switch (info.BlockType)
        {
            case BlockTypes.l_shape:
                SetShapeColor(new Color(1, 0.7f, 0, 1));
                break;
            case BlockTypes.j_shape:
                SetShapeColor(Color.blue);
                break;
            case BlockTypes.z_shape:
                SetShapeColor(Color.red);
                break;
            case BlockTypes.s_shape:
                SetShapeColor(Color.green);
                break;
            case BlockTypes.i_shape:
                SetShapeColor(Color.cyan);
                break;
            case BlockTypes.t_shape:
                SetShapeColor(Color.magenta);
                break;
            case BlockTypes.o_shape:
                SetShapeColor(Color.yellow);
                break;
            default:
                break;

        }
    }

    private void SetShapeColor(Color color)
    {
        foreach (Transform t in transform)
        {
            SpriteRenderer renderer = t.GetComponent<SpriteRenderer>();
            renderer.color = color;
        }
    }
}
