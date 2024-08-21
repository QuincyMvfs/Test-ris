using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BlockManipulator : MonoBehaviour
{
    // Data Assets
    [Header("Data")]
    [SerializeField] private LevelInfoScriptableObject _levelData;
    [SerializeField] private AllShapesScritpableObject _allShapesData;

    // Spawn point and block to spawn
    [Header("Spawning")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private MovingBlock _blockToSpawn;

    // How much the block should speed up when the player is dropping it faster
    [Header("Speeds")]
    [Tooltip("Block Drop speed when player is dropping it manually")][SerializeField] private float _spedUpSpeed = 0.1f;

    // Grid Variables that a shared between each spawned shape
    static public int GridSizeX = 15;
    static public int GridSizeY = 26;
    // 2D Transform array that use the total grid size as the inputs
    static public Transform[,] ShapeGrid = new Transform[GridSizeX, GridSizeY];

    // Scriptable Object Fields
    private List<BlockInfo> _blocks = new List<BlockInfo>();
    private float _minSpeed = 0.5f;
    private float _stepSpeed = 0.05f;
    private float _maxSpeed = 0.05f;

    // Variables changed in code
    private float _currentSpeed;
    private List<BlockInfo> blockChances = new List<BlockInfo>();
    private MovingBlock _spawnedBlock;
    private BlockInfo _nextBlock;

    private void Start()
    {
        // Setting member variables from scritpable object
        _blocks = _levelData.Blocks;
        _minSpeed = _levelData.StartSpeed;
        _stepSpeed = _levelData.StepSpeed;
        _maxSpeed = _levelData.MaxSpeed;

        _currentSpeed = _minSpeed;

        // Create the block chance list, which will be used to pick a random block later
        foreach (BlockInfo block in _blocks)
        {
            for (int i = 0; i < block.BlockChance; i++)
            {
                blockChances.Add(block);
            }
        }

        ScoreManager.Instance.SetTargetScore(_levelData.ScoreRequired);

        _nextBlock = GetBlockToSpawn();
        SpawnBlock();
    }

    // Gets a random block from the blockInfo array created on start
    private BlockInfo GetBlockToSpawn()
    {
        int randInt = Random.Range(0, blockChances.Count);

        return blockChances[randInt];
    }

    /// Inputs from Player Controller
    // -1 left, +1 right
    // Tries to move the spawned block in the given direction
    public void MoveBlock(int direction)
    {
        if (_spawnedBlock != null)
        {
            _spawnedBlock.MoveBlockX(direction);
        }
    }

    // Tries to rotate the block by the rotate amount
    public void RotateBlock()
    {
        if (_spawnedBlock != null)
        {
            _spawnedBlock.RotateBlock(90.0f);
        }
    }

    // Tries to drop the block faster, if the button is held down,
    // speed it up, if its released, return its speed to normal
    public void DropFaster(int enable)
    {
        if (_spawnedBlock != null)
        {
            if (enable == 1) { _spawnedBlock.SpeedUp(_spedUpSpeed); }
            else if (enable == 0) { _spawnedBlock.SpeedUp(_currentSpeed); }
        }
    }
    ///

    // Sets the block to spawn as the previously generated _nextBlock
    // Then creates a new _nextBlock to be generated next time.
    // Based off the block.BlockType, it gets the raw block info that contains the positions, and spawns the block
    // at the given positions with the given block type
    public void SpawnBlock()
    {
        BlockInfo block = _nextBlock;
        _nextBlock = GetBlockToSpawn();

        Vector2[] updatedBlockLocations = new Vector2[4];

        // Checks through the raw data created in editor for a matching block type
        foreach (RawBlockInfo rawInfo in _allShapesData.RawBlockInfo)
        {
            if (rawInfo.BlockType == block.BlockType) 
            { 
                updatedBlockLocations = rawInfo.BlockPositions;
            }

            // Updates the hud with the next block visual
            if (rawInfo.BlockType == _nextBlock.BlockType)
            {
                ScoreManager.Instance.SetNextShapeText(rawInfo);
            }
        }

        // Spawns a block and updates it with the current speed and block positions
        _spawnedBlock = Instantiate(_blockToSpawn, _spawnPoint.position, Quaternion.identity);
        _spawnedBlock.UpdateShape(_currentSpeed, updatedBlockLocations, this, block);

        if (_currentSpeed > _maxSpeed) SetCurrentSpeed();
    }

    private void SetCurrentSpeed()
    {
        _currentSpeed -= _stepSpeed;
        _currentSpeed = Mathf.Clamp(_currentSpeed, _maxSpeed, _minSpeed);
    }
}