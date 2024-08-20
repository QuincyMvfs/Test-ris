using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BlockManipulator : MonoBehaviour
{
    [SerializeField] private LevelInfoScriptableObject _levelData;

    // Scriptable Object Fields
    private List<BlockInfo> _blocks = new List<BlockInfo>();
    private float _minSpeed = 0.5f;
    private float _stepSpeed = 0.05f;
    private float _maxSpeed = 0.05f;

    // Spawn point and block to spawn
    [Header("Spawning")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private MovingBlock _blockToSpawn;

    // How much the block should speed up when the player is dropping it faster
    [Tooltip("Block Drop speed when player is dropping it manually")][SerializeField] private float _spedUpSpeed = 0.1f;

    private float _maxSpacing = 1f;
    private float _rotateAmount = 90.0f;
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

    // -1 left, +1 right
    public void MoveBlock(int direction)
    {
        if (_spawnedBlock != null)
        {
            _spawnedBlock.MoveBlockX(direction);
        }
    }

    public void RotateBlock()
    {
        if (_spawnedBlock != null)
        {
            _spawnedBlock.RotateBlock(_rotateAmount);
        }
    }

    public void DropFaster(int enable)
    {
        if (_spawnedBlock != null)
        {
            if (enable == 1) { _spawnedBlock.SpeedUp(_spedUpSpeed); }
            else if (enable == 0) { _spawnedBlock.SpeedUp(_currentSpeed); }
        }
    }

    public void SpawnBlock()
    {
        BlockInfo block = _nextBlock;
        string[] shapeRowStrings = GetShapeTextData(block).Split("\n");
        _nextBlock = GetBlockToSpawn();
        ScoreManager.Instance.SetNextShapeText(_nextBlock);

        int blockInterval = 0;
        Vector2[] blockLocations = new Vector2[4];
        Vector2[] updatedBlockLocations = new Vector2[4];
        Vector2 originLocation = new();

        // Row
        for (int i = 1; i < shapeRowStrings.Length; i++)
        {
            // Column
            char[] shapeCharacters = shapeRowStrings[i].ToCharArray();
            for (int j = 0; j < shapeCharacters.Length; j++)
            {
                int yIndex;
                if (shapeCharacters[j] == 'O' || shapeCharacters[j] == 'X')
                {
                    yIndex = shapeCharacters.Length - i;
                    blockLocations[blockInterval].x = j * _maxSpacing;
                    blockLocations[blockInterval].y = yIndex * _maxSpacing;

                    // Sets the rotational pivot (origin)
                    if (shapeCharacters[j] == 'O')
                    {
                        originLocation.x = blockLocations[blockInterval].x;
                        originLocation.y = blockLocations[blockInterval].y;
                    }

                    blockInterval++;
                }
            }
        }

        // Confines the other block locations to the origins location
        for (int z = 0; z < blockLocations.Length; z++)
        {
            blockLocations[z].x -= originLocation.x;
            blockLocations[z].y -= originLocation.y;
            updatedBlockLocations[z] = blockLocations[z];
        }

        _spawnedBlock = Instantiate(_blockToSpawn, _spawnPoint.position, Quaternion.identity);
        _spawnedBlock.UpdateShape(_currentSpeed, updatedBlockLocations, this, block);

        SetCurrentSpeed();
    }

    private string GetShapeTextData(BlockInfo block)
    {
        string path = Application.dataPath + "/Resources/" + block.BlockType.ToString() + ".txt";
        StreamReader reader = new StreamReader(path);
        string shapeText = reader.ReadToEnd();
        return shapeText;
    }

    private void SetCurrentSpeed()
    {
        _currentSpeed -= _stepSpeed;
        _currentSpeed = Mathf.Clamp(_currentSpeed, _maxSpeed, _minSpeed);
    }

    // Gets a random block from the blockInfo array created on start
    private BlockInfo GetBlockToSpawn()
    {
        int randInt = Random.Range(0, blockChances.Count);

        return blockChances[randInt];
    }
}