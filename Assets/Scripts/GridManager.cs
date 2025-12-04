using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 1f;
    
    [Header("Obstacle Settings")]
    [Range(0f, 0.4f)]
    public float obstaclePercentage = 0.2f;
    
    [Header("Algorithm Visualization")]
    public bool visualizeAlgorithm = true;
    public float distanceLevelDelay = 1f;
    public Color exploringColorLight = new Color(1f, 1f, 0.5f);
    public Color exploringColorDark = new Color(0.8f, 0.6f, 0f);
    
    [Header("References")]
    public UIController uiController;
    
    [Header("Materials")]
    public Material groundMaterial;
    public Material obstacleMaterial;
    public Material pathMaterial;
    public Material startMaterial;
    public Material goalMaterial;
    
    private GridCell[,] grid;
    private List<SimplexLPSolver.Edge> edges;
    private List<GameObject> pathVisuals = new List<GameObject>();
    private Dictionary<float, List<int>> nodesAtDistance = new Dictionary<float, List<int>>();
    private bool isVisualizingAlgorithm = false;
    private float maxDistance = 0f;
    
    void Start()
    {
        if (uiController == null)
        {
            uiController = FindAnyObjectByType<UIController>();
        }
        
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
        
        if (pathMaterial == null)
        {
            pathMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pathMaterial.color = Color.cyan;
        }
        
        if (startMaterial == null)
        {
            startMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            startMaterial.color = Color.green;
        }
        
        if (goalMaterial == null)
        {
            goalMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            goalMaterial.color = Color.red;
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
    
    private IEnumerator SolveAndVisualizePathCoroutine(GridCell start, GridCell goal)
    {
        isVisualizingAlgorithm = true;
        nodesAtDistance.Clear();
        maxDistance = 0f;
        ClearPathVisualization();
        
        int startNode = start.GetNodeIndex(gridWidth);
        int goalNode = goal.GetNodeIndex(gridWidth);
        
        Renderer startRenderer = start.VisualObject.GetComponent<Renderer>();
        Renderer goalRenderer = goal.VisualObject.GetComponent<Renderer>();
        startRenderer.material = startMaterial;
        goalRenderer.material = goalMaterial;
        
        if (uiController != null)
        {
            uiController.UpdateTotalCost(0, 0);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        List<int> pathIndices = SimplexLPSolver.SolveShortestPath(
            gridWidth * gridHeight, 
            edges, 
            startNode, 
            goalNode,
            OnNodeVisitedDuringSearch
        );
        
        List<float> distances = new List<float>(nodesAtDistance.Keys);
        distances.Sort();
        
        int totalNodesExplored = 0;
        
        foreach (float distance in distances)
        {
            List<int> nodesAtThisDistance = nodesAtDistance[distance];
            
            foreach (int nodeIndex in nodesAtThisDistance)
            {
                if (nodeIndex != startNode && nodeIndex != goalNode)
                {
                    int x = nodeIndex % gridWidth;
                    int y = nodeIndex / gridWidth;
                    
                    GridCell cell = grid[x, y];
                    Renderer renderer = cell.VisualObject.GetComponent<Renderer>();
                    
                    float normalizedDistance = maxDistance > 0 ? distance / maxDistance : 0f;
                    Color gradientColor = Color.Lerp(exploringColorLight, exploringColorDark, normalizedDistance);
                    
                    Material exploringMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    exploringMaterial.color = gradientColor;
                    renderer.material = exploringMaterial;
                }
                
                totalNodesExplored++;
            }
            
            if (uiController != null)
            {
                uiController.UpdateTotalCost(0, totalNodesExplored);
            }
            
            yield return new WaitForSeconds(distanceLevelDelay);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (pathIndices.Count > 0)
        {
            Debug.Log($"Path found with {pathIndices.Count} nodes. Total cost: {pathIndices.Count - 1}");
            VisualizePath(pathIndices);
        }
        else
        {
            Debug.LogWarning("No path found!");
        }
        
        isVisualizingAlgorithm = false;
    }
    
    private void OnNodeVisitedDuringSearch(int nodeIndex, float distance)
    {
        if (!nodesAtDistance.ContainsKey(distance))
        {
            nodesAtDistance[distance] = new List<int>();
        }
        nodesAtDistance[distance].Add(nodeIndex);
        
        if (distance > maxDistance)
        {
            maxDistance = distance;
        }
    }
    
    public bool IsVisualizingAlgorithm()
    {
        return isVisualizingAlgorithm;
    }
    
    public List<GridCell> GetLastSolvedPath(GridCell start, GridCell goal)
    {
        int startNode = start.GetNodeIndex(gridWidth);
        int goalNode = goal.GetNodeIndex(gridWidth);
        
        List<int> pathIndices = SimplexLPSolver.SolveShortestPath(gridWidth * gridHeight, edges, startNode, goalNode);
        
        List<GridCell> pathCells = new List<GridCell>();
        
        if (pathIndices.Count > 0)
        {
            foreach (int nodeIndex in pathIndices)
            {
                int x = nodeIndex % gridWidth;
                int y = nodeIndex / gridWidth;
                pathCells.Add(grid[x, y]);
            }
        }
        
        return pathCells;
    }
    
    public List<GridCell> SolveAndVisualizePath(GridCell start, GridCell goal)
    {
        if (visualizeAlgorithm)
        {
            StartCoroutine(SolveAndVisualizePathCoroutine(start, goal));
            return null;
        }
        else
        {
            return SolvePathImmediate(start, goal);
        }
    }
    
    private List<GridCell> SolvePathImmediate(GridCell start, GridCell goal)
    {
        ClearPathVisualization();
        
        int startNode = start.GetNodeIndex(gridWidth);
        int goalNode = goal.GetNodeIndex(gridWidth);
        
        List<int> pathIndices = SimplexLPSolver.SolveShortestPath(gridWidth * gridHeight, edges, startNode, goalNode);
        
        List<GridCell> pathCells = new List<GridCell>();
        
        if (pathIndices.Count > 0)
        {
            Debug.Log($"Path found with {pathIndices.Count} nodes. Total cost: {pathIndices.Count - 1}");
            
            foreach (int nodeIndex in pathIndices)
            {
                int x = nodeIndex % gridWidth;
                int y = nodeIndex / gridWidth;
                pathCells.Add(grid[x, y]);
            }
            
            VisualizePath(pathIndices);
        }
        else
        {
            Debug.LogWarning("No path found!");
        }
        
        return pathCells;
    }
    
    void VisualizePath(List<int> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            int nodeIndex = path[i];
            int x = nodeIndex % gridWidth;
            int y = nodeIndex / gridWidth;
            
            GridCell cell = grid[x, y];
            Renderer renderer = cell.VisualObject.GetComponent<Renderer>();
            
            if (i == 0)
                renderer.material = startMaterial;
            else if (i == path.Count - 1)
                renderer.material = goalMaterial;
            else
                renderer.material = pathMaterial;
        }
    }
    
    void ClearPathVisualization()
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
