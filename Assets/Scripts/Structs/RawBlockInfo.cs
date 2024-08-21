using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RawBlockInfo
{
    public BlockTypes BlockType = BlockTypes.l_shape;
    public Vector2[] BlockPositions = new Vector2[4];
}
