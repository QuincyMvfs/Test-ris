using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class MovingBlock : MonoBehaviour
{
    [HideInInspector] public float DefaultFallSpeed = 2;
    [HideInInspector] public UnityEvent OnBlockDropped;
    private float _currentFallSpeed;

    [SerializeField] private int _descentIncrement = 2;
    
    // Grid
    static private int _gridSizeX = 15;
    static private int _gridSizeY = 26;
    static private Transform[,] _shapeGrid = new Transform[_gridSizeX, _gridSizeY];
    BlockManipulator _manipulator;

    Transform[] _childTransforms = new Transform[4];
    Vector2 _currentPos;
    Vector2 _spawnPos;
    private IEnumerator _currentState;

    public void MoveBlock(int direction)
    {
        if (!CheckCanMoveX(direction)) return;

        _currentPos = transform.position;
        _currentPos.x += _descentIncrement * direction;
        transform.position = _currentPos;
    }

    public void RotateBlock(float rotateAmount)
    {
        transform.Rotate(Vector3.forward, -rotateAmount);
        if (!CheckCanRotate(rotateAmount)) { transform.Rotate(Vector3.forward, rotateAmount); }
    }

    public void UpdateShape(float fallSpeed, Vector2[] positions, BlockManipulator manipulator, BlockInfo blockInfo)
    {
        _currentFallSpeed = fallSpeed;
        DefaultFallSpeed = fallSpeed;
        _manipulator = manipulator;
        if (_childTransforms[0] == null)
        {
            _spawnPos = transform.position;
            for (int i = 0; i < transform.childCount; i++)
            {
                _childTransforms[i] = transform.GetChild(i);
            }

            transform.position = _spawnPos;
        }
        else { transform.position = _spawnPos; }

        if (positions.Length != _childTransforms.Length) return;

        DefaultFallSpeed = fallSpeed;
        for (int i = 0; i < positions.Length; i++)
        {
            _childTransforms[i].localPosition = positions[i];
        }

        GetShapeColor(blockInfo);
        if (!CheckCanMoveY())
        {
            ChangeState(ResetState());
        }
        else
        {
            ChangeState(FallingState());
        }
    }

    public void SpeedUp(float newFallSpeed)
    {
        _currentFallSpeed = newFallSpeed;
        ChangeState(FallingState());
    }

    private void ChangeState(IEnumerator newState)
    {
        if (_currentState != null) StopCoroutine(_currentState);

        _currentState = newState;
        StartCoroutine(_currentState);
    }

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

            yield return new WaitForSeconds(_currentFallSpeed);
        }
    }

    private IEnumerator ResetState()
    {
        OnBlockDropped.Invoke();
        yield return null;
    }

    private void AddShapeToGrid()
    {
        foreach (Transform child in _childTransforms)
        {
            Vector3 childPos = child.position;
            int X = Mathf.RoundToInt(childPos.x);
            int Y = Mathf.RoundToInt(childPos.y);
            
            _shapeGrid[X, Y] = child;
        }
    }

    private bool CheckCanMoveY()
    {
        foreach (Transform child in _childTransforms)
        {
            Vector3 childPos = child.position;
            int X = Mathf.RoundToInt(childPos.x);
            int Y = Mathf.RoundToInt(childPos.y);

            // Block is overlapping another before decrement is done
            if (_shapeGrid[X, Y] != null) 
            {
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

    private bool CheckCanMoveX(float movementIncrement)
    {
        foreach (Transform child in _childTransforms)
        {
            Vector3 childPos = child.position;
            int X = Mathf.RoundToInt(childPos.x + (_descentIncrement * movementIncrement));
            int Y = Mathf.RoundToInt(childPos.y);

            if (movementIncrement < 0) 
            { 
                if (X < 1) return false; 
            }
            else
            {
                if (X >= _gridSizeX) return false;
            }

            if (_shapeGrid[X, Y] != null) return false;
        }

        return true;
    }

    private bool CheckCanRotate(float rotateAmount)
    {
        foreach (Transform child in _childTransforms)
        {
            Vector3 childPos = child.position;
            int X = Mathf.RoundToInt(childPos.x);
            int Y = Mathf.RoundToInt(childPos.y);

            if (X < 1) return false;

            if (X >= _gridSizeX) return false;

            if (Y < 1) return false;

            if (_shapeGrid[X, Y] != null) return false;
        }

        return true;
    }

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
        foreach (Transform t in _childTransforms)
        {
            SpriteRenderer renderer = t.GetComponent<SpriteRenderer>();
            renderer.color = color;
        }
    }
}
