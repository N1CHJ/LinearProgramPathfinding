using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiAgentPathfinder : MonoBehaviour
{
    [Header("References")]
    public MultiAgentGridManager gridManager;
    public int agentId = 1;
    
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float pauseAtStart = 0.5f;
    public float pauseAtGoal = 1f;
    
    private GridCell currentStart;
    private GridCell currentGoal;
    private List<GridCell> currentPath;
    private int currentStepIndex = 0;
    private bool pathComplete = false;
    private int currentCost = 0;
    private int totalPathCost = 0;
    private bool isMoving = false;
    
    public int CurrentCost => currentCost;
    public int TotalPathCost => totalPathCost;
    public bool IsMoving => isMoving;
    public bool PathComplete => pathComplete;
    
    void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<MultiAgentGridManager>();
        }
    }
    
    public void GenerateNewPath()
    {
        currentStart = gridManager.GetRandomNonObstacleCell();
        currentGoal = gridManager.GetRandomNonObstacleCell();
        
        if (currentStart != null && currentGoal != null && currentStart != currentGoal)
        {
            Debug.Log($"Agent {agentId}: Solving path from ({currentStart.X}, {currentStart.Y}) to ({currentGoal.X}, {currentGoal.Y})");
            
            Vector3 startPosition = currentStart.GetWorldPosition(gridManager.cellSize);
            startPosition.y = 0.5f;
            transform.position = startPosition;
            
            currentPath = gridManager.SolveAndVisualizePath(currentStart, currentGoal, agentId);
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
        }
    }
    
    public void StepNext()
    {
        if (pathComplete)
        {
            return;
        }
        
        if (currentPath == null || currentPath.Count == 0)
        {
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
            }
            
            if (currentStepIndex >= currentPath.Count)
            {
                pathComplete = true;
            }
        }
    }
    
    public void StartAutoplayMovement()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            currentCost = 0;
            totalPathCost = currentPath.Count - 1;
            StartCoroutine(MoveCoroutine(currentPath));
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
            }
        }
        
        yield return new WaitForSeconds(pauseAtGoal);
        
        isMoving = false;
        pathComplete = true;
    }
    
    public void ResetPath()
    {
        currentPath = null;
        currentStepIndex = 0;
        currentCost = 0;
        totalPathCost = 0;
        pathComplete = false;
        isMoving = false;
    }
}
