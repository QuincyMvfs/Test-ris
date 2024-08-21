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
    [SerializeField] private RectTransform _nextShapeContainer;
    private RectTransform[] _shapes;

    [SerializeField] private int _scoreAddedPerRow = 100;

    public UnityEvent OnTargetScoreReached;
    public UnityEvent OnGameOver;

    private int _currentScore;
    private int _targetScore;
    private ColorManager _colorManager;
    
    private void Awake()
    {
        Instance = this;

        _colorManager = GetComponent<ColorManager>();

        _shapes = new RectTransform[_nextShapeContainer.childCount];
        for (int i = 0; i < _nextShapeContainer.childCount; i++)
        {
            RectTransform child = _nextShapeContainer.GetChild(i).GetComponent<RectTransform>();
            _shapes[i] = child;
        }
    }

    // Sets the target score to the current levels target score in the BlockManipulator
    public void SetTargetScore(int newTargetScore)
    {
        _targetScore = newTargetScore;
        _targetScoreText.text = _targetScore.ToString();
        _currentScoreText.text = "0";
    }

    public void SetNextShapeText(RawBlockInfo block)
    {
        _nextShapeText.text = block.BlockType.ToString();
        for (int i = 0; i < block.BlockPositions.Length; i++)
        {
            _shapes[i].localPosition = block.BlockPositions[i] * 75;
        }

        _colorManager.GetShapeColor(block.BlockType);
    }

    public void TriggerGameOver()
    {
        // Prevent game over after win
        if (_currentScore < _targetScore)
        {
            OnGameOver?.Invoke();
        }
    }

    public void AddScore()
    {
        // Adds the score to the current score and checks if its reached the target score
        _currentScore += _scoreAddedPerRow;
        if (_currentScore >= _targetScore)
        {
            OnTargetScoreReached?.Invoke();
        }
        else
        {
            _currentScoreText.text = _currentScore.ToString();
        }
    }
}
