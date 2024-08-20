using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BlockManipulator : MonoBehaviour
{
    [SerializeField] private List<BlockInfo> _blocks = new List<BlockInfo>();
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private MovingBlock _blockToSpawn;
    private MovingBlock _spawnedBlock;

    [SerializeField] private float _spedUpSpeed = 0.1f;
    [SerializeField] private float _minSpeed = 0.5f;
    [SerializeField] private float _stepSpeed = 0.05f;
    [SerializeField] private float _maxSpeed = 0.05f;
    private float _currentSpeed;
    private int _droppingFaster;
    private int _nameIndex = 0;
    [SerializeField] private float _rotateAmount = 90.0f;

    [SerializeField] private float _maxSpacing = 2f;
    List<BlockInfo> blocks = new List<BlockInfo>();
    Quaternion _defaultRot;

    private void Start()
    {
        _currentSpeed = _minSpeed;

        foreach (BlockInfo block in _blocks)
        {
            for (int i = 0; i < block.BlockChance; i++)
            {
                blocks.Add(block);
            }
        }

        _defaultRot = Quaternion.identity;

        SpawnBlock();
    }

    // -1 left, +1 right
    public void MoveBlock(int direction)
    {
        if (_spawnedBlock != null)
        {
            _spawnedBlock.MoveBlock(direction);
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

        _droppingFaster = enable;
    }

    public void SpawnBlock()
    {
        BlockInfo block = GetBlockToSpawn();
        string path = Application.dataPath + "/Resources/" + block.BlockType.ToString() + ".txt";
        StreamReader reader = new StreamReader(path);
        string shapeText = reader.ReadToEnd();

        int blockInterval = 0;
        Vector2[] blockLocations = new Vector2[4];
        Vector2[] updatedBlockLocations = new Vector2[4];
        Vector2 originLocation = new Vector2();
        string[] shapeRowStrings = shapeText.Split("\n");
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

                    if (shapeCharacters[j] == 'O')
                    {
                        originLocation.x = blockLocations[blockInterval].x;
                        originLocation.y = blockLocations[blockInterval].y;
                    }

                    blockInterval++;
                }
            }
        }

        for (int z = 0; z < blockLocations.Length; z++)
        {
            blockLocations[z].x -= originLocation.x;
            blockLocations[z].y -= originLocation.y;
            updatedBlockLocations[z] = blockLocations[z];
        }

        _spawnedBlock = Instantiate(_blockToSpawn, _spawnPoint.position, _defaultRot);
        _spawnedBlock.name = _nameIndex.ToString();
        _nameIndex++;
        _spawnedBlock.UpdateShape(_currentSpeed, updatedBlockLocations, this, block);
        if (_droppingFaster == 1)
        {
            _spawnedBlock.SpeedUp(_spedUpSpeed);
        }

        SetCurrentSpeed();

    }

    private void SetCurrentSpeed()
    {
        _currentSpeed -= _stepSpeed;
        _currentSpeed = Mathf.Clamp(_currentSpeed, _maxSpeed, _minSpeed);
    }

    private BlockInfo GetBlockToSpawn()
    {
        int randInt = Random.Range(0, blocks.Count);

        return blocks[randInt];
    }
}