using UnityEngine;

public class GridCell
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public bool IsObstacle { get; set; }
    public GameObject VisualObject { get; set; }
    
    public GridCell(int x, int y, bool isObstacle = false)
    {
        X = x;
        Y = y;
        IsObstacle = isObstacle;
    }
    
    public Vector3 GetWorldPosition(float cellSize)
    {
        return new Vector3(X * cellSize, 0, Y * cellSize);
    }
    
    public int GetNodeIndex(int gridWidth)
    {
        return Y * gridWidth + X;
    }
}
