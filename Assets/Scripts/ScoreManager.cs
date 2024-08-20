using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _currentScoreText;
    [SerializeField] private TextMeshProUGUI _targetScoreText;
    [SerializeField] private TextMeshProUGUI _nextShapeText;

    [SerializeField] private int _scoreAddedPerRow = 100;

    public UnityEvent TargetScoreReached;

    private int _currentScore;
    private int _targetScore;

    private void Awake()
    {
        Instance = this;
    }

    public void SetTargetScore(int newTargetScore)
    {
        _targetScore = newTargetScore;
        _targetScoreText.text = _targetScore.ToString();
        _currentScoreText.text = "0";
    }

    public void SetNextShapeText(BlockInfo block)
    {
        _nextShapeText.text = block.BlockType.ToString();
    }

    public void AddScore()
    {
        _currentScore += _scoreAddedPerRow;
        if (_currentScore >= _targetScore)
        {
            TargetScoreReached?.Invoke();
        }

        _currentScoreText.text = _currentScore.ToString();
    }
}
