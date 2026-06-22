using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 6;
    public int columns = 6;
    public float cellSize = 1.2f;

    [Header("Block Prefabs")]
    public GameObject plasticPrefab;
    public GameObject metalPrefab;
    public GameObject organicPrefab;
    public GameObject smogPrefab;

    private GameObject[,] grid;
    private List<GameObject> blockTypes = new List<GameObject>();

    void Start()
    {
        // Populate the list with standard recyclable blocks
        blockTypes.Add(plasticPrefab);
        blockTypes.Add(metalPrefab);
        blockTypes.Add(organicPrefab);

        GenerateGrid();
    }

    // Generates the initial 6x6 board layout
    void GenerateGrid()
    {
        grid = new GameObject[columns, rows];

        // Calculate the starting position to keep the grid perfectly centered on screen
        Vector2 startPosition = new Vector2(-(columns - 1) * cellSize / 2f, -(rows - 1) * cellSize / 2f);

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector2 spawnPos = startPosition + new Vector2(x * cellSize, y * cellSize);
                GameObject randomBlockPrefab = blockTypes[Random.Range(0, blockTypes.Count)];

                GameObject newBlock = Instantiate(randomBlockPrefab, spawnPos, Quaternion.identity);
                newBlock.transform.SetParent(this.transform);
                grid[x, y] = newBlock;
            }
        }
        Debug.Log("6x6 Puzzle Grid successfully generated!");
    }

    // Removes destroyed blocks from the data tracking array
    public void RemoveBlockFromGrid(GameObject block)
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (grid[x, y] == block)
                {
                    grid[x, y] = null;
                    return;
                }
            }
        }
    }

    // External entry point to trigger board updates after a match
    public void ClearAndRefill()
    {
        StartCoroutine(RefillRoutine());
    }

    private IEnumerator RefillRoutine()
    {
        // 1. Shift remaining blocks downward into empty slots
        yield return StartCoroutine(ApplyGravity());
        // 2. Spawn new blocks from above to complete the board
        yield return StartCoroutine(RefillTopRow());
    }

    // Shifts floating blocks down into empty array spaces
    private IEnumerator ApplyGravity()
    {
        Vector2 startPosition = new Vector2(-(columns - 1) * cellSize / 2f, -(rows - 1) * cellSize / 2f);

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (grid[x, y] == null)
                {
                    // Scan upwards looking for the next available block to drop down
                    for (int posUp = y + 1; posUp < rows; posUp++)
                    {
                        if (grid[x, posUp] != null)
                        {
                            grid[x, y] = grid[x, posUp];
                            grid[x, posUp] = null;

                            Vector2 newPos = startPosition + new Vector2(x * cellSize, y * cellSize);
                            StartCoroutine(SmoothMove(grid[x, y], newPos));
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.2f); // Wait briefly for gravity animation to finish
    }

    // Spawns brand new blocks right above the board screen
    private IEnumerator RefillTopRow()
    {
        Vector2 startPosition = new Vector2(-(columns - 1) * cellSize / 2f, -(rows - 1) * cellSize / 2f);

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (grid[x, y] == null)
                {
                    GameObject randomBlockPrefab = blockTypes[Random.Range(0, blockTypes.Count)];
                    // Spawn slightly off-screen above the grid boundaries
                    Vector2 spawnPos = startPosition + new Vector2(x * cellSize, (rows + 1) * cellSize);

                    GameObject newBlock = Instantiate(randomBlockPrefab, spawnPos, Quaternion.identity);
                    newBlock.transform.SetParent(this.transform);
                    grid[x, y] = newBlock;

                    Vector2 targetPos = startPosition + new Vector2(x * cellSize, y * cellSize);
                    StartCoroutine(SmoothMove(newBlock, targetPos));
                }
            }
        }
        yield return null;
    }

    // Interpolates position over time for visual polish
    private IEnumerator SmoothMove(GameObject block, Vector2 targetPos)
    {
        if (block == null) yield break;

        float time = 0;
        Vector2 startPos = block.transform.position;
        float duration = 0.2f;

        while (time < duration)
        {
            if (block == null) yield break;
            block.transform.position = Vector2.Lerp(startPos, targetPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        if (block != null) block.transform.position = targetPos;
    }
}
