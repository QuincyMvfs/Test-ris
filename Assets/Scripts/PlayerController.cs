using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]

public class PlayerController : MonoBehaviour
{
    BlockManipulator _blockManipulator;
    private bool _movingLeftRight = false;
    private bool _isDropping = false;
    private int _movementDir = 0;

    private void Awake()
    {
        _blockManipulator = GetComponent<BlockManipulator>();
    }

    private void OnRotate()
    {
        _blockManipulator.RotateBlock();
    }

    private void OnMoveLeft(InputValue value)
    {
        _movingLeftRight = !_movingLeftRight;

        if (_movingLeftRight && _movementDir != -1)
        {
            _blockManipulator.MoveBlock(-1);
            _movementDir = -1;
        }
        else if (!_movingLeftRight && _movementDir == -1)
        {
            ResetMovement();
        }

    }

    private void OnMoveRight()
    {
        _movingLeftRight = !_movingLeftRight;

        if (_movingLeftRight && _movementDir != 1)
        {
            _blockManipulator.MoveBlock(1);
            _movementDir = 1;
        }
        else if (!_movingLeftRight && _movementDir == 1)
        {
            ResetMovement();
        }

    }

    private void ResetMovement()
    {
        _blockManipulator.MoveBlock(0);
        _movementDir = 0;
    }

    private void OnDropFaster()
    {
        _isDropping = !_isDropping;

        if (_isDropping) { _blockManipulator.DropFaster(1); }
        else { _blockManipulator.DropFaster(0); }
    }
}
