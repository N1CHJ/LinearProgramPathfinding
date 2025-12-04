using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingAgent : MonoBehaviour
{
    public enum PlayMode
    {
        Autoplay,
        Step
    }
    
    [Header("References")]
    public GridManager gridManager;
    public UIController uiController;
    
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float solveCooldown = 2f;
    public float pauseAtStart = 0.5f;
    public float pauseAtGoal = 1f;
    
    private GridCell currentStart;
    private GridCell currentGoal;
    private bool isMoving = false;
    private PlayMode currentMode = PlayMode.Autoplay;
    private List<GridCell> currentPath;
    private int currentStepIndex = 0;
    private bool pathComplete = false;
    private int currentCost = 0;
    private int totalPathCost = 0;
    
    void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }
        
        if (uiController == null)
        {
            uiController = FindAnyObjectByType<UIController>();
        }
        
        StartCoroutine(PathfindingLoop());
    }
    
    public void SetPlayMode(PlayMode mode)
    {
        currentMode = mode;
        
        if (mode == PlayMode.Step)
        {
            StopAllCoroutines();
            isMoving = false;
            GenerateNewPath();
        }
        else
        {
            StartCoroutine(PathfindingLoop());
        }
    }
    
    IEnumerator PathfindingLoop()
    {
        yield return new WaitForSeconds(1f);
        
        while (currentMode == PlayMode.Autoplay)
        {
            if (!isMoving && !gridManager.IsVisualizingAlgorithm())
            {
                SolveAndMoveAlongPath();
            }
            
            yield return new WaitForSeconds(solveCooldown);
        }
    }
    
    void GenerateNewPath()
    {
        currentStart = gridManager.GetRandomNonObstacleCell();
        currentGoal = gridManager.GetRandomNonObstacleCell();
        
        if (currentStart != null && currentGoal != null && currentStart != currentGoal)
        {
            Debug.Log($"Solving path from ({currentStart.X}, {currentStart.Y}) to ({currentGoal.X}, {currentGoal.Y})");
            
            Vector3 startPosition = currentStart.GetWorldPosition(gridManager.cellSize);
            startPosition.y = 0.5f;
            transform.position = startPosition;
            
            currentPath = gridManager.SolveAndVisualizePath(currentStart, currentGoal);
            
            if (currentPath == null && gridManager.visualizeAlgorithm)
            {
                StartCoroutine(WaitForVisualizationThenGetPath());
            }
            else
            {
                InitializePathData();
            }
        }
    }
    
    IEnumerator WaitForVisualizationThenGetPath()
    {
        while (gridManager.IsVisualizingAlgorithm())
        {
            yield return null;
        }
        
        currentPath = gridManager.GetLastSolvedPath(currentStart, currentGoal);
        InitializePathData();
    }
    
    void InitializePathData()
    {
        currentStepIndex = 0;
        currentCost = 0;
        pathComplete = false;
        
        if (currentPath != null && currentPath.Count > 0)
        {
            totalPathCost = currentPath.Count - 1;
        }
        else
        {
            totalPathCost = 0;
        }
        
        if (uiController != null)
        {
            uiController.UpdateNextButtonText("Next");
            uiController.UpdateTotalCost(currentCost, totalPathCost);
        }
    }
    
    public void StepNext()
    {
        if (pathComplete)
        {
            GenerateNewPath();
            return;
        }
        
        if (currentPath == null || currentPath.Count == 0)
        {
            GenerateNewPath();
            return;
        }
        
        if (currentStepIndex < currentPath.Count)
        {
            GridCell targetCell = currentPath[currentStepIndex];
            Vector3 targetPosition = targetCell.GetWorldPosition(gridManager.cellSize);
            targetPosition.y = 0.5f;
            transform.position = targetPosition;
            
            currentStepIndex++;
            
            if (currentStepIndex > 1)
            {
                currentCost++;
                if (uiController != null)
                {
                    uiController.UpdateTotalCost(currentCost, totalPathCost);
                }
            }
            
            if (currentStepIndex >= currentPath.Count)
            {
                pathComplete = true;
                if (uiController != null)
                {
                    uiController.UpdateNextButtonText("New Route");
                }
            }
        }
    }
    
    void SolveAndMoveAlongPath()
    {
        currentStart = gridManager.GetRandomNonObstacleCell();
        currentGoal = gridManager.GetRandomNonObstacleCell();
        
        if (currentStart != null && currentGoal != null && currentStart != currentGoal)
        {
            Debug.Log($"Solving path from ({currentStart.X}, {currentStart.Y}) to ({currentGoal.X}, {currentGoal.Y})");
            
            Vector3 startPosition = currentStart.GetWorldPosition(gridManager.cellSize);
            startPosition.y = 0.5f;
            transform.position = startPosition;
            
            List<GridCell> path = gridManager.SolveAndVisualizePath(currentStart, currentGoal);
            
            if (path == null && gridManager.visualizeAlgorithm)
            {
                StartCoroutine(WaitForVisualizationThenMove());
            }
            else if (path != null && path.Count > 0)
            {
                currentCost = 0;
                totalPathCost = path.Count - 1;
                
                if (uiController != null)
                {
                    uiController.UpdateTotalCost(currentCost, totalPathCost);
                }
                
                StartCoroutine(MoveCoroutine(path));
            }
            else
            {
                Debug.LogWarning("No valid path found, trying again...");
            }
        }
    }
    
    IEnumerator WaitForVisualizationThenMove()
    {
        while (gridManager.IsVisualizingAlgorithm())
        {
            yield return null;
        }
        
        List<GridCell> path = gridManager.GetLastSolvedPath(currentStart, currentGoal);
        
        if (path != null && path.Count > 0)
        {
            currentCost = 0;
            totalPathCost = path.Count - 1;
            
            if (uiController != null)
            {
                uiController.UpdateTotalCost(currentCost, totalPathCost);
            }
            
            StartCoroutine(MoveCoroutine(path));
        }
        else
        {
            Debug.LogWarning("No valid path found after visualization, trying again...");
        }
    }
    
    IEnumerator MoveCoroutine(List<GridCell> path)
    {
        isMoving = true;
        
        yield return new WaitForSeconds(pauseAtStart);
        
        int stepIndex = 0;
        
        foreach (GridCell cell in path)
        {
            Vector3 targetPosition = cell.GetWorldPosition(gridManager.cellSize);
            targetPosition.y = 0.5f;
            
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            
            stepIndex++;
            
            if (stepIndex > 1)
            {
                currentCost++;
                if (uiController != null)
                {
                    uiController.UpdateTotalCost(currentCost, totalPathCost);
                }
            }
        }
        
        yield return new WaitForSeconds(pauseAtGoal);
        
        isMoving = false;
    }
}
