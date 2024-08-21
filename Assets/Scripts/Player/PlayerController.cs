using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]

/// All functions in this class are called using the new input system. Controls can be found in Assets/Input/
public class PlayerController : MonoBehaviour
{
    BlockManipulator _blockManipulator;
    private bool _isDropping = false;

    private void Awake()
    {
        _blockManipulator = GetComponent<BlockManipulator>();
    }

    // Tells the block manipulator to rotate block
    private void OnRotate()
    {
        _blockManipulator.RotateBlock();
    }

    // Tells the block manipulator to move the block to the left (-1)
    private void OnMoveLeft()
    {
        _blockManipulator.MoveBlock(-1);
    }

    // Tells the block manipulator to move the block to the right (1)
    private void OnMoveRight()
    {
        _blockManipulator.MoveBlock(1);
    }

    // When button is pressed, tell block manipulator to start dropping the block faster,
    // when released, stop dropping the block faster
    private void OnDropFaster()
    {
        _isDropping = !_isDropping;

        if (_isDropping) { _blockManipulator.DropFaster(1); }
        else { _blockManipulator.DropFaster(0); }
    }
}
