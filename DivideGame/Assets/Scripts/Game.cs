using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private struct Cell
    {
        public int row;
        public int col;
        public int value;
        public Cell(int row, int col, int value)
        {
            this.row = row;
            this.col = col;
            this.value = value;
        }
    }

    public static Game Instance { get; private set; } //Singleton

    private const int gridSize = 3;
    public int GridSize => gridSize;
    private int[,] grid = new int[gridSize, gridSize];

    const int queueSize = 3;

    private List<int> queue = new List<int>(queueSize);

    int score = 0;
    int keepValue = 0; //value of keep cell


    const int minNumber = 2;
    const int maxNumber = 24;

    private List<Cell> cellsToRemove = new List<Cell>();               //List of divisor cells to be removed
    private List<Cell> cellsToUpdate = new List<Cell>();               //List of dividend cells to be updated
    private List<Cell> potentialDivisorsToRemove = new List<Cell>();   //List of potential divisors
    private Queue<Cell> divisionQueue = new Queue<Cell>();             //Queue of divisons to be checked and processed

    private int[] neighbourRowDif = { -1, 1, 0, 0 };
    private int[] neighbourColDif = { 0, 0, -1, 1 };


    public static event Action<int> OnScoreUpdated;             //Takes score as parameter
    public static event Action<int> OnKeepUpdated;              //Takes keep value as parameter
    public static event Action<int, int, int> OnGridCellUpdated;//Takes row, collumn and cell value as parameter
    public static event Action<List<int>> OnQueueUpdated;       //Takes queue list as parameter
    public static event Action<int> OnGameOver;                 //Takes final score as parameter

    private bool isInProcess = false;
    private float waitBetweenDivisions = 1.0f;

    private void Awake()
    {
        //Creating singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetGame();
    }

    public void ResetGame()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                grid[i, j] = 0;
                OnGridCellUpdated?.Invoke(i, j, grid[i, j]);
            }
        }
        score = 0;
        OnScoreUpdated?.Invoke(score);
        keepValue = 0;
        OnKeepUpdated?.Invoke(keepValue);
        cellsToRemove.Clear();
        cellsToUpdate.Clear();
        queue.Clear();
        for (int i = 0; i < queueSize; i++)
        {
            queue.Add(UnityEngine.Random.Range(minNumber, maxNumber + 1));
        }
        OnQueueUpdated?.Invoke(queue);
    }


    public bool TryPlaceInKeep()
    {
        if (keepValue == 0)
        {
            keepValue = queue[2];
            OnKeepUpdated?.Invoke(keepValue);
            ShiftQueue();
            return true;
        }
        return false;
    }

    public bool TryPlaceInGrid(bool isFromKeep, int row, int col)
    {
        if (isInProcess) return false;  //If previous move is still in the process of calculation, prevent player from making next move
        if (grid[row, col] == 0)
        {
            if (isFromKeep)
            {
                grid[row, col] = keepValue;
                keepValue = 0;
                OnKeepUpdated?.Invoke(keepValue);
            }
            else
            {
                grid[row, col] = queue[2];
                ShiftQueue();
            }
            OnGridCellUpdated?.Invoke(row, col, grid[row, col]);
            StartCoroutine(ProcessDivisions(row, col));
            return true;
        }
        return false;
    }

    private void ShiftQueue()
    {
        queue[2] = queue[1];
        queue[1] = queue[0];
        queue[0] = UnityEngine.Random.Range(minNumber, maxNumber + 1);
        OnQueueUpdated?.Invoke(queue);
    }

    private IEnumerator ProcessDivisions(int startRow, int startCol)
    {
        isInProcess = true;

        divisionQueue.Clear();
        divisionQueue.Enqueue(new Cell(startRow, startCol, grid[startRow, startCol])); //
        while (divisionQueue.Count > 0)
        {
            Cell currentCell = divisionQueue.Dequeue();
            if(DivisionCheck(currentCell.row, currentCell.col))
            {
                CalculateScore();
                ApplyCellChanges();

                yield return new WaitForSeconds(waitBetweenDivisions);
            }
        }
        if(GameOverCheck())
        {
            OnGameOver?.Invoke(score);
        }
        isInProcess = false;
    }


    private void ApplyCellChanges()
    {
        for (int i = 0; i < cellsToRemove.Count; i++)
        {
            Cell cell = cellsToRemove[i];
            grid[cell.row, cell.col] = 0;
            OnGridCellUpdated?.Invoke(cell.row, cell.col, 0);
        }

        for (int i = 0; i < cellsToUpdate.Count; i++)
        {
            Cell cell = cellsToUpdate[i];
            if(cell.value > 1)
            {
                grid[cell.row, cell.col] = cell.value;
                OnGridCellUpdated?.Invoke(cell.row, cell.col, cell.value);
                divisionQueue.Enqueue(new Cell(cell.row, cell.col, cell.value)); //Adding every updated cell to queue in order to check their neighbours for new division opportunity (chain reaction)
            }
            else
            {
                grid[cell.row, cell.col] = 0;
                OnGridCellUpdated?.Invoke(cell.row, cell.col, 0);
            }
        }
        cellsToRemove.Clear();
        cellsToUpdate.Clear();
    }

    private bool DivisionCheck(int row, int col)
    {
        Cell currentCell = new Cell(row, col, grid[row, col]);
        if (currentCell.value == 0) return false;

        cellsToRemove.Clear();
        cellsToUpdate.Clear();
        potentialDivisorsToRemove.Clear();

        bool isDivisor = false;
        int maxDivisorValue = 0;

        for(int i = 0; i < 4; i++)
        {
            int neighbourRow = currentCell.row + neighbourRowDif[i];
            int neighbourCol = currentCell.col + neighbourColDif[i];

            if (neighbourRow < 0 || neighbourRow >= gridSize || neighbourCol < 0 || neighbourCol >= gridSize) continue;
            Cell neighbour = new Cell(neighbourRow, neighbourCol, grid[neighbourRow, neighbourCol]);
            if (neighbour.value == 0) continue;

            //If current cell is a divisor, it cannot be a dividend.
            if (neighbour.value % currentCell.value == 0)
            {
                isDivisor = true;
                int result = neighbour.value / currentCell.value;

                cellsToUpdate.Add(new Cell(neighbour.row, neighbour.col, result));
            }
            else if (!isDivisor && currentCell.value % neighbour.value == 0)
            {
                if(neighbour.value > maxDivisorValue) //If there are multiple divisors around our cell, it gets divided only by the bigger value
                {
                    maxDivisorValue = neighbour.value;

                    potentialDivisorsToRemove.Clear();
                    potentialDivisorsToRemove.Add(neighbour);
                }
                else if (neighbour.value == maxDivisorValue)
                {
                    potentialDivisorsToRemove.Add(neighbour); //If our cell is surrounded by multiple divisors of the same value, all divisors count
                }
            }
        }
        if (isDivisor)
        {
            cellsToRemove.Add(currentCell); 
        }
        else if (maxDivisorValue > 0)
        {
            cellsToUpdate.Add(new Cell(currentCell.row, currentCell.col, currentCell.value / maxDivisorValue));
            cellsToRemove.AddRange(potentialDivisorsToRemove);
        }
        return cellsToRemove.Count > 0 || cellsToUpdate.Count > 0; ;

    }

    private int CalculateScore()
    {
        int divisor = 0;
        if (cellsToRemove.Count > 0)
        {
            divisor = cellsToRemove[0].value;
        }
        int processedNumbersCount = cellsToRemove.Count + cellsToUpdate.Count;
        score += divisor * processedNumbersCount;
        OnScoreUpdated?.Invoke(score);
        return score;
    }

    private bool GameOverCheck()
    {
        for(int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (grid[i, j] == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
                

