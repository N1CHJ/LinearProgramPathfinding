using System.Collections.Generic;
using UnityEngine;

public class QPSafetyFilter : MonoBehaviour
{
    [Header("Safety Parameters")]
    public float safeDistance = 2.0f;
    public float gamma0 = 1.0f;
    public float gamma1 = 2.0f;
    public float agentMass = 1.0f;
    
    [Header("QP Settings")]
    public int maxIterations = 100;
    public float convergenceThreshold = 0.001f;
    
    [Header("Debug")]
    public bool enableDebugLogs = false;
    
    public Vector3 ComputeSafeForce(
        Vector3 forceRL,
        Vector3 agentPosition,
        Vector3 agentVelocity,
        List<AsteroidData> asteroids)
    {
        if (asteroids == null || asteroids.Count == 0)
            return forceRL;
        
        List<Vector3> constraintNormals = new List<Vector3>();
        List<float> constraintBounds = new List<float>();
        
        foreach (var asteroid in asteroids)
        {
            if (BuildHOCBFConstraint(
                agentPosition, agentVelocity,
                asteroid.position, asteroid.velocity, asteroid.radius,
                out Vector3 A, out float b))
            {
                constraintNormals.Add(A);
                constraintBounds.Add(b);
            }
        }
        
        if (constraintNormals.Count == 0)
            return forceRL;
        
        Vector3 safeForceCurrent = forceRL;
        
        for (int iter = 0; iter < maxIterations; iter++)
        {
            int violatedIndex = -1;
            float maxViolation = 0f;
            
            for (int i = 0; i < constraintNormals.Count; i++)
            {
                float violation = Vector3.Dot(constraintNormals[i], safeForceCurrent) - constraintBounds[i];
                if (violation > maxViolation)
                {
                    maxViolation = violation;
                    violatedIndex = i;
                }
            }
            
            if (maxViolation < convergenceThreshold)
                break;
            
            if (violatedIndex >= 0)
            {
                Vector3 A = constraintNormals[violatedIndex];
                float b = constraintBounds[violatedIndex];
                
                float numerator = Vector3.Dot(A, safeForceCurrent) - b;
                float denominator = Vector3.Dot(A, A);
                
                if (denominator > 0.0001f)
                {
                    float lambda = numerator / denominator;
                    safeForceCurrent -= lambda * A;
                }
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"QP: RL Force: {forceRL}, Safe Force: {safeForceCurrent}, Constraints: {constraintNormals.Count}");
        }
        
        return safeForceCurrent;
    }
    
    private bool BuildHOCBFConstraint(
        Vector3 p, Vector3 v,
        Vector3 p_obs, Vector3 v_obs, float obstacleRadius,
        out Vector3 A, out float b)
    {
        Vector3 relativePos = p - p_obs;
        Vector3 relativeVel = v - v_obs;
        
        float adjustedSafeDistance = safeDistance + obstacleRadius;
        
        float h = relativePos.sqrMagnitude - adjustedSafeDistance * adjustedSafeDistance;
        
        float h_dot = 2f * Vector3.Dot(relativePos, relativeVel);
        
        float h_ddot_const = 2f * relativeVel.sqrMagnitude;
        
        A = (2f * relativePos) / agentMass;
        
        b = -(h_ddot_const + gamma1 * h_dot + gamma0 * h);
        
        if (h < 0f)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"Already inside safety margin! h = {h}");
        }
        
        return true;
    }
}
