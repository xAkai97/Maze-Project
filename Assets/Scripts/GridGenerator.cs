using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private int gridWidth = 70;
    [SerializeField] private int gridHeight = 40;
    [SerializeField] private Vector2 cellSize = new Vector2(1f, 1f); // Adjusted cell size for better precision
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject wallTilePrefab;
    [SerializeField] private GameObject obstaclePrefab;

    private Dictionary<Vector2, Cell> cells;

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public Vector2 CellSize => cellSize;

    private void Start()
    {
        if (!CheckForPremadeGrid())
        {
            GenerateGrid();
        }
    }

    private bool CheckForPremadeGrid()
    {
        // Check if there are any existing ground tiles in the scene
        GameObject[] existingTiles = GameObject.FindGameObjectsWithTag("GroundTile");
        if (existingTiles.Length > 0)
        {
            // If there are existing tiles, calculate the grid dimensions and cell size
            CalculateGridDimensions(existingTiles);
            return true;
        }
        return false;
    }

    private void CalculateGridDimensions(GameObject[] existingTiles)
    {
        // Assume all tiles are uniformly spaced and aligned to the grid
        Vector2 minPosition = existingTiles[0].transform.position;
        Vector2 maxPosition = existingTiles[0].transform.position;

        foreach (GameObject tile in existingTiles)
        {
            Vector2 position = tile.transform.position;
            if (position.x < minPosition.x) minPosition.x = position.x;
            if (position.y < minPosition.y) minPosition.y = position.y;
            if (position.x > maxPosition.x) maxPosition.x = position.x;
            if (position.y > maxPosition.y) maxPosition.y = position.y;
        }

        // Calculate grid dimensions based on the min and max positions
        gridWidth = Mathf.RoundToInt((maxPosition.x - minPosition.x) / cellSize.x) + 1;
        gridHeight = Mathf.RoundToInt((maxPosition.y - minPosition.y) / cellSize.y) + 1;

        // Log the calculated grid dimensions
        Debug.Log($"Premade grid detected. Grid dimensions: {gridWidth}x{gridHeight}, Cell size: {cellSize}");
    }

    public void GenerateGrid()
    {
        cells = new Dictionary<Vector2, Cell>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 position = new Vector2(x * cellSize.x, y * cellSize.y);
                if (!cells.ContainsKey(position))
                {
                    cells[position] = new Cell(position);
                    Debug.Log($"Generated cell at position: {position}");

                    if (groundTilePrefab != null)
                    {
                        Instantiate(groundTilePrefab, position, Quaternion.identity);
                    }

                    // Randomly place wall tiles
                    if (wallTilePrefab != null && Random.value < 0.1f) // 10% chance to place a wall tile
                    {
                        Instantiate(wallTilePrefab, position, Quaternion.identity);
                        cells[position].isWall = true; // Mark the cell as a wall
                    }

                    // Randomly place obstacle tiles
                    if (obstaclePrefab != null && Random.value < 0.05f) // 5% chance to place an obstacle
                    {
                        Instantiate(obstaclePrefab, position, Quaternion.identity);
                    }
                }
            }
        }
    }

    public Dictionary<Vector2, Cell> GetCells()
    {
        return cells;
    }
}

public class Cell
{
    public Vector2 position;
    public int gCost;
    public int hCost;
    public int fCost;
    public Vector2 connection;
    public bool isWall; // Add a flag to indicate if the cell is a wall

    public Cell(Vector2 position)
    {
        this.position = position;
        this.gCost = int.MaxValue;
        this.hCost = 0;
        this.fCost = int.MaxValue;
        this.connection = Vector2.zero;
        this.isWall = false; // Initialize as not a wall
    }
}
