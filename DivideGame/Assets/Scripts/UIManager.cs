using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private UICellAnimation keepCellUI;
    [SerializeField] private UICellAnimation[] gridCellsUI;
    [SerializeField] private UICellAnimation[] queueCellsUI;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private GameObject gameOverPopUp;

    private UICellAnimation[][] gridCellsUI2D;  //Two-dimensional version of gridCellsUI array


    private void OnEnable()
    {
        Game.OnGridCellUpdated += UpdateGridCellText;
        Game.OnKeepUpdated += UpdateKeepCellText;
        Game.OnScoreUpdated += UpdateScoreText;
        Game.OnQueueUpdated += UpdateQueueText;
        Game.OnGameOver += EndGame;
    }

    private void Start()
    {
        //Converts one-dimensional array of cell UI to two-dimensional array
        int gridSize = Game.Instance.GridSize;
        if (gridCellsUI != null || gridCellsUI.Length == gridSize * gridSize)
        {
            gridCellsUI2D = new UICellAnimation[gridSize][];
            for (int i = 0; i < Game.Instance.GridSize; i++)
            {
                gridCellsUI2D[i] = new UICellAnimation[gridSize];
                for (int j = 0; j < Game.Instance.GridSize; j++)
                {
                    gridCellsUI2D[i][j] = gridCellsUI[i * Game.Instance.GridSize + j];
                }
            }
        }
    }

    private void UpdateGridCellText(int row, int col, int value)
    {
        if (value == 0)
        {
            StartCoroutine(gridCellsUI2D[row][col].ShrinkAnimation());
        }
        else if (gridCellsUI2D[row][col].IsVisible())
        {
            
            StartCoroutine(gridCellsUI2D[row][col].AnimateCellChange(value.ToString()));
        }
        else
        {
            gridCellsUI2D[row][col].PlaceCellWithoutAnimation(value.ToString());
        }
    }

    private void UpdateKeepCellText(int value)
    {
        if (value == 0)
        {
            keepCellUI.RemoveCellWithoutAnimation();
        }
        else
        {
            keepCellUI.PlaceCellWithoutAnimation(value.ToString());
        }
    }

    private void UpdateScoreText(int value)
    {
        scoreText.text = $"Score: {value.ToString()}";
    }

    private void UpdateQueueText(List<int> values)
    {
       StartCoroutine(QueueAnimation(values));
    }

    private IEnumerator QueueAnimation(List<int> values)
    {
        if(values.Count != queueCellsUI.Length) yield break;
        canvasGroup.blocksRaycasts = false;       //Disables UI interaction while animation is being played
        queueCellsUI[queueCellsUI.Length - 1].RemoveCellWithoutAnimation();
        for (int i = queueCellsUI.Length - 1; i >= 0; i--)
        {
            if (i > 0)
            {
                yield return StartCoroutine(queueCellsUI[i-1].ShrinkAnimation());
            }
            yield return StartCoroutine(queueCellsUI[i].AnimateCellChange(values[i].ToString()));
        }
        canvasGroup.blocksRaycasts = true;      //Enables UI interaction again

    }

    private void EndGame(int score)
    {
        gameOverPopUp.SetActive(true);
        gameOverScoreText.text = $"Score: {score.ToString()}";
    }

    private void OnDisable()
    {
        Game.OnGridCellUpdated -= UpdateGridCellText;
        Game.OnKeepUpdated -= UpdateKeepCellText;
        Game.OnScoreUpdated -= UpdateScoreText;
        Game.OnQueueUpdated -= UpdateQueueText;
        Game.OnGameOver -= EndGame;
    }
}
