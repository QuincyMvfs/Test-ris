using UnityEngine;
using System;

[Serializable]
public class BlockInfo
{
    public BlockTypes BlockType = BlockTypes.l_shape;
    public int BlockChance = 5;
    public Vector2[] BlockPositions = new Vector2[4];

}
