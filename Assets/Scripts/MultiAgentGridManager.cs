using System.Collections.Generic;
using UnityEngine;

public class MultiAgentGridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 1f;
    
    [Header("Obstacle Settings")]
    [Range(0f, 0.4f)]
    public float obstaclePercentage = 0.2f;
    
    [Header("Materials")]
    public Material groundMaterial;
    public Material obstacleMaterial;
    public Material agent1PathMaterial;
    public Material agent1StartMaterial;
    public Material agent1GoalMaterial;
    public Material agent2PathMaterial;
    public Material agent2StartMaterial;
    public Material agent2GoalMaterial;
    
    private GridCell[,] grid;
    private List<SimplexLPSolver.Edge> edges;
    private Dictionary<int, Material> cellOriginalMaterials = new Dictionary<int, Material>();
    
    void Start()
    {
        CreateDefaultMaterials();
        GenerateGrid();
        GenerateEdges();
    }
    
    void CreateDefaultMaterials()
    {
        if (groundMaterial == null)
        {
            groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMaterial.color = Color.white;
        }
        
        if (obstacleMaterial == null)
        {
            obstacleMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            obstacleMaterial.color = Color.black;
        }
        
        if (agent1PathMaterial == null)
        {
            agent1PathMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            agent1PathMaterial.color = Color.cyan;
        }
        
        if (agent1StartMaterial == null)
        {
            agent1StartMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            agent1StartMaterial.color = Color.green;
        }
        
        if (agent1GoalMaterial == null)
        {
            agent1GoalMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            agent1GoalMaterial.color = Color.red;
        }
        
        if (agent2PathMaterial == null)
        {
            agent2PathMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            agent2PathMaterial.color = Color.yellow;
        }
        
        if (agent2StartMaterial == null)
        {
            agent2StartMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            agent2StartMaterial.color = new Color(0f, 0.5f, 1f);
        }
        
        if (agent2GoalMaterial == null)
        {
            agent2GoalMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            agent2GoalMaterial.color = new Color(1f, 0.5f, 0f);
        }
    }
    
    void GenerateGrid()
    {
        grid = new GridCell[gridWidth, gridHeight];
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                bool isObstacle = Random.value < obstaclePercentage;
                grid[x, y] = new GridCell(x, y, isObstacle);
                
                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cell.transform.parent = transform;
                cell.transform.position = grid[x, y].GetWorldPosition(cellSize);
                cell.transform.localScale = new Vector3(cellSize * 0.95f, isObstacle ? 0.5f : 0.05f, cellSize * 0.95f);
                
                Renderer renderer = cell.GetComponent<Renderer>();
                renderer.material = isObstacle ? obstacleMaterial : groundMaterial;
                
                grid[x, y].VisualObject = cell;
            }
        }
    }
    
    void GenerateEdges()
    {
        edges = new List<SimplexLPSolver.Edge>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y].IsObstacle) continue;
                
                int fromNode = grid[x, y].GetNodeIndex(gridWidth);
                
                int[] dx = { 0, 1, 0, -1 };
                int[] dy = { 1, 0, -1, 0 };
                
                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];
                    
                    if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight && !grid[nx, ny].IsObstacle)
                    {
                        int toNode = grid[nx, ny].GetNodeIndex(gridWidth);
                        edges.Add(new SimplexLPSolver.Edge(fromNode, toNode, 1f));
                    }
                }
            }
        }
    }
    
    public List<GridCell> SolveAndVisualizePath(GridCell start, GridCell goal, int agentId)
    {
        int startNode = start.GetNodeIndex(gridWidth);
        int goalNode = goal.GetNodeIndex(gridWidth);
        
        List<int> pathIndices = SimplexLPSolver.SolveShortestPath(gridWidth * gridHeight, edges, startNode, goalNode);
        
        List<GridCell> pathCells = new List<GridCell>();
        
        if (pathIndices.Count > 0)
        {
            Debug.Log($"Agent {agentId}: Path found with {pathIndices.Count} nodes. Total cost: {pathIndices.Count - 1}");
            
            foreach (int nodeIndex in pathIndices)
            {
                int x = nodeIndex % gridWidth;
                int y = nodeIndex / gridWidth;
                pathCells.Add(grid[x, y]);
            }
            
            VisualizePath(pathIndices, agentId);
        }
        else
        {
            Debug.LogWarning($"Agent {agentId}: No path found!");
        }
        
        return pathCells;
    }
    
    void VisualizePath(List<int> path, int agentId)
    {
        Material pathMat = agentId == 1 ? agent1PathMaterial : agent2PathMaterial;
        Material startMat = agentId == 1 ? agent1StartMaterial : agent2StartMaterial;
        Material goalMat = agentId == 1 ? agent1GoalMaterial : agent2GoalMaterial;
        
        for (int i = 0; i < path.Count; i++)
        {
            int nodeIndex = path[i];
            int x = nodeIndex % gridWidth;
            int y = nodeIndex / gridWidth;
            
            GridCell cell = grid[x, y];
            Renderer renderer = cell.VisualObject.GetComponent<Renderer>();
            
            if (i == 0)
                renderer.material = startMat;
            else if (i == path.Count - 1)
                renderer.material = goalMat;
            else
                renderer.material = pathMat;
        }
    }
    
    public void ClearPathVisualization()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!grid[x, y].IsObstacle)
                {
                    Renderer renderer = grid[x, y].VisualObject.GetComponent<Renderer>();
                    renderer.material = groundMaterial;
                }
            }
        }
    }
    
    public GridCell GetRandomNonObstacleCell()
    {
        List<GridCell> validCells = new List<GridCell>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!grid[x, y].IsObstacle)
                {
                    validCells.Add(grid[x, y]);
                }
            }
        }
        
        return validCells.Count > 0 ? validCells[Random.Range(0, validCells.Count)] : null;
    }
    
    public GridCell GetCell(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return grid[x, y];
        return null;
    }
}
