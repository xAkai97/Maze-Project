using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private GridGenerator gridGenerator;
    private Dictionary<Vector2, Cell> cells;
    private SimplePriorityQueue<Vector2> openList; // Use a simple priority queue
    private List<Vector2> searchedCells;
    private List<Vector2> finalPath;
    private Vector2 startPos;
    private Vector2 endPos;

    private static readonly Vector2[] neighborOffsets = new Vector2[]
    {
        new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1),
        new Vector2(-1, -1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(1, 1) // Added diagonal directions
    };

    public int GridWidth => gridGenerator.GridWidth;
    public int GridHeight => gridGenerator.GridHeight;
    public Vector2 CellSize => gridGenerator.CellSize;

    private void Start()
    {
        gridGenerator = FindObjectOfType<GridGenerator>();
        if (gridGenerator != null)
        {
            cells = gridGenerator.GetCells();
        }
    }

    public void SetPathPositions(Vector2 leaderPosition, Vector2 mouseClickPosition)
    {
        // Round positions to align with grid cells
        startPos = new Vector2(Mathf.Round(leaderPosition.x), Mathf.Round(leaderPosition.y));
        endPos = new Vector2(Mathf.Round(mouseClickPosition.x), Mathf.Round(mouseClickPosition.y));

        // Reset the finalPath and searchedCells lists
        finalPath = new List<Vector2>();
        searchedCells = new List<Vector2>();

        if (!IsValidPosition(startPos) || !IsValidPosition(endPos))
        {
            Debug.Log($"Invalid start or end position: startPos={startPos}, endPos={endPos}");
            return;
        }

        FindPath(startPos, endPos);
    }

    public bool IsWithinGridBounds(Vector2 position)
    {
        return position.x >= 0 && position.x < GridWidth * CellSize.x &&
               position.y >= 0 && position.y < GridHeight * CellSize.y;
    }

    private void FindPath(Vector2 startPos, Vector2 endPos)
    {
        InitializePathfinding(startPos, endPos);
        Debug.Log($"Finding path from {startPos} to {endPos}");

        while (openList.Count > 0)
        {
            Vector2 currentCell = openList.Dequeue();
            Debug.Log($"Current cell: {currentCell}");

            if (currentCell == endPos)
            {
                RetracePath(startPos, endPos);
                return; // Break out of the loop once the end position is reached
            }

            searchedCells.Add(currentCell);

            SearchCellNeighbors(currentCell, endPos);
        }

        finalPath = null; // Ensure finalPath is null if no path is found
        Debug.Log("Path not found");
    }

    private void InitializePathfinding(Vector2 startPos, Vector2 endPos)
    {
        openList = new SimplePriorityQueue<Vector2>();
        openList.Enqueue(startPos, 0);
        searchedCells = new List<Vector2>();
        finalPath = new List<Vector2>();

        Cell startCell = cells[startPos];
        startCell.gCost = 0;
        startCell.hCost = GetDistance(startPos, endPos);
        startCell.fCost = startCell.hCost;
    }

    private void RetracePath(Vector2 startPos, Vector2 endPos)
    {
        Cell pathCell = cells[endPos];
        Debug.Log("Retracing path");

        while (pathCell.position != startPos)
        {
            finalPath.Add(pathCell.position);
            Debug.Log($"Path cell: {pathCell.position}");
            pathCell = cells[pathCell.connection];
        }

        finalPath.Add(startPos);
        finalPath.Reverse();

        Debug.Log("Path found!");
    }

    private void SearchCellNeighbors(Vector2 cellPos, Vector2 endPos)
    {
        foreach (Vector2 offset in neighborOffsets)
        {
            Vector2 neighborPos = cellPos + offset;

            if (IsValidNeighbor(neighborPos))
            {
                EvaluateNeighbor(cellPos, neighborPos, endPos);
            }
            else
            {
                Debug.Log($"Invalid neighbor: {neighborPos}");
            }
        }
    }

    private bool IsValidNeighbor(Vector2 neighborPos)
    {
        bool isValid = IsWithinGridBounds(neighborPos) &&
                       cells.TryGetValue(neighborPos, out Cell c) && !searchedCells.Contains(neighborPos) &&
                       !c.isWall; // Check if the cell is not a wall

        if (!isValid)
        {
            Debug.Log($"Invalid neighbor position: {neighborPos}");
        }

        return isValid;
    }

    private void EvaluateNeighbor(Vector2 cellPos, Vector2 neighborPos, Vector2 endPos)
    {
        Cell neighborCell = cells[neighborPos];
        int moveCost = (neighborPos.x != cellPos.x && neighborPos.y != cellPos.y) ? 14 : 10; // Diagonal movement cost
        int newGCost = cells[cellPos].gCost + moveCost;

        if (newGCost < neighborCell.gCost)
        {
            neighborCell.connection = cellPos;
            neighborCell.gCost = newGCost;
            neighborCell.hCost = GetDistance(neighborPos, endPos); // Heuristic function
            neighborCell.fCost = neighborCell.gCost + neighborCell.hCost;

            if (!openList.Contains(neighborPos))
            {
                openList.Enqueue(neighborPos, neighborCell.fCost);
            }
            else
            {
                openList.UpdatePriority(neighborPos, neighborCell.fCost);
            }
        }
    }

    private int GetDistance(Vector2 a, Vector2 b)
    {
        int dx = Mathf.Abs((int)a.x - (int)b.x);
        int dy = Mathf.Abs((int)a.y - (int)b.y);
        if (dx > dy)
            return 14 * dy + 10 * (dx - dy); // Diagonal distance
        return 14 * dx + 10 * (dy - dx);
    }

    public List<Vector2> GetFinalPath()
    {
        return finalPath;
    }

    private bool IsValidPosition(Vector2 position)
    {
        Vector2 roundedPosition = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
        bool isValid = roundedPosition.x >= 0 && roundedPosition.x < GridWidth &&
                       roundedPosition.y >= 0 && roundedPosition.y < GridHeight &&
                       cells.ContainsKey(roundedPosition) && !cells[roundedPosition].isWall; // Check if the cell is not a wall

        if (!isValid)
        {
            Debug.Log($"Invalid position: {roundedPosition}. Grid bounds: (0, 0) to ({GridWidth - 1}, {GridHeight - 1})");
        }

        return isValid;
    }
}

// SimplePriorityQueue implementation
public class SimplePriorityQueue<T>
{
    private List<KeyValuePair<T, float>> elements = new List<KeyValuePair<T, float>>();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add(new KeyValuePair<T, float>(item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Value < elements[bestIndex].Value)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Key;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public bool Contains(T item)
    {
        foreach (var element in elements)
        {
            if (EqualityComparer<T>.Default.Equals(element.Key, item))
            {
                return true;
            }
        }
        return false;
    }

    public void UpdatePriority(T item, float priority)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(elements[i].Key, item))
            {
                elements[i] = new KeyValuePair<T, float>(item, priority);
                return;
            }
        }
    }
}
