using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/LevelInfo")]
public class LevelInfoScriptableObject : ScriptableObject
{
    [SerializeField] public List<BlockInfo> Blocks = new List<BlockInfo>();
    [SerializeField] public float StartSpeed = 0.5f;
    [SerializeField] public float StepSpeed = 0.01f;
    [SerializeField] public float MaxSpeed = 0.1f;
    [SerializeField] public int ScoreRequired = 5000;
}
