using UnityEngine;

public class Cell
{
    public Vector2 position;
    public int gCost;
    public int hCost;
    public int fCost;
    public Vector2 connection;
    public bool isWall; // Add this property

    public Cell(Vector2 position)
    {
        this.position = position;
        this.gCost = int.MaxValue;
        this.hCost = 0;
        this.fCost = int.MaxValue;
        this.connection = Vector2.zero;
        this.isWall = false; // Initialize as false
    }
}
