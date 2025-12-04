using UnityEngine;
using Unity.MLAgents.Policies;

public class SceneSetupValidator : MonoBehaviour
{
    [Header("References to Validate")]
    public GameObject cubeSat;
    public GameObject goal;
    public GameObject asteroidPrefab;
    public AsteroidSpawner asteroidSpawner;
    public TelemetryUI telemetryUI;
    
    [ContextMenu("Validate Scene Setup")]
    public void ValidateSetup()
    {
        Debug.Log("=== SCENE SETUP VALIDATION ===\n");
        
        bool allValid = true;
        
        allValid &= ValidateCubeSat();
        allValid &= ValidateGoal();
        allValid &= ValidateAsteroidPrefab();
        allValid &= ValidateAsteroidSpawner();
        allValid &= ValidateTelemetryUI();
        
        Debug.Log("\n=== VALIDATION COMPLETE ===");
        if (allValid)
        {
            Debug.Log("<color=green>✓ ALL CHECKS PASSED! Scene is ready for training.</color>");
        }
        else
        {
            Debug.LogWarning("<color=yellow>⚠ Some issues found. Please fix the items marked above.</color>");
        }
    }
    
    bool ValidateCubeSat()
    {
        Debug.Log("\n--- CubeSat Validation ---");
        bool valid = true;
        
        if (cubeSat == null)
        {
            Debug.LogError("✗ CubeSat reference is missing!");
            return false;
        }
        
        if (cubeSat.tag != "Player")
        {
            Debug.LogWarning($"✗ CubeSat tag is '{cubeSat.tag}', should be 'Player'");
            valid = false;
        }
        else
        {
            Debug.Log("✓ CubeSat tag is correct");
        }
        
        Rigidbody rb = cubeSat.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("✗ CubeSat missing Rigidbody!");
            valid = false;
        }
        else
        {
            if (rb.useGravity)
            {
                Debug.LogWarning("✗ CubeSat Rigidbody has 'Use Gravity' enabled. Should be disabled.");
                valid = false;
            }
            else
            {
                Debug.Log("✓ CubeSat Rigidbody configured correctly");
            }
        }
        
        CubeSatAgent agent = cubeSat.GetComponent<CubeSatAgent>();
        if (agent == null)
        {
            Debug.LogError("✗ CubeSat missing CubeSatAgent component!");
            valid = false;
        }
        else
        {
            Debug.Log("✓ CubeSatAgent component present");
        }
        
        BehaviorParameters bp = cubeSat.GetComponent<BehaviorParameters>();
        if (bp == null)
        {
            Debug.LogError("✗ CubeSat missing BehaviorParameters!");
            valid = false;
        }
        else
        {
            Debug.Log($"✓ BehaviorParameters present (Behavior: {bp.BehaviorName})");
        }
        
        return valid;
    }
    
    bool ValidateGoal()
    {
        Debug.Log("\n--- Goal Validation ---");
        bool valid = true;
        
        if (goal == null)
        {
            Debug.LogError("✗ Goal reference is missing!");
            return false;
        }
        
        if (goal.tag != "Goal")
        {
            Debug.LogWarning($"✗ Goal tag is '{goal.tag}', should be 'Goal'");
            valid = false;
        }
        else
        {
            Debug.Log("✓ Goal tag is correct");
        }
        
        Collider col = goal.GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("✗ Goal missing Collider!");
            valid = false;
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning("✗ Goal Collider is not a trigger!");
            valid = false;
        }
        else
        {
            Debug.Log("✓ Goal Collider is trigger");
        }
        
        return valid;
    }
    
    bool ValidateAsteroidPrefab()
    {
        Debug.Log("\n--- Asteroid Prefab Validation ---");
        bool valid = true;
        
        if (asteroidPrefab == null)
        {
            Debug.LogError("✗ Asteroid Prefab reference is missing!");
            return false;
        }
        
        if (asteroidPrefab.tag != "Asteroid")
        {
            Debug.LogWarning($"✗ Asteroid Prefab tag is '{asteroidPrefab.tag}', should be 'Asteroid'");
            valid = false;
        }
        else
        {
            Debug.Log("✓ Asteroid Prefab tag is correct");
        }
        
        Rigidbody rb = asteroidPrefab.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("✗ Asteroid Prefab missing Rigidbody! Spawner needs this to apply velocities.");
            valid = false;
        }
        else
        {
            if (!rb.isKinematic)
            {
                Debug.LogWarning("⚠ Asteroid Rigidbody should be kinematic");
            }
            if (rb.useGravity)
            {
                Debug.LogWarning("⚠ Asteroid Rigidbody should not use gravity");
            }
            Debug.Log("✓ Asteroid Prefab has Rigidbody");
        }
        
        return valid;
    }
    
    bool ValidateAsteroidSpawner()
    {
        Debug.Log("\n--- Asteroid Spawner Validation ---");
        bool valid = true;
        
        if (asteroidSpawner == null)
        {
            Debug.LogError("✗ AsteroidSpawner reference is missing!");
            return false;
        }
        
        if (asteroidSpawner.asteroidPrefab == null)
        {
            Debug.LogError("✗ AsteroidSpawner has no prefab assigned!");
            valid = false;
        }
        else
        {
            Debug.Log($"✓ AsteroidSpawner has prefab: {asteroidSpawner.asteroidPrefab.name}");
        }
        
        if (asteroidSpawner.agentTransform == null)
        {
            Debug.LogWarning("✗ AsteroidSpawner missing Agent Transform reference!");
            valid = false;
        }
        else
        {
            Debug.Log("✓ Agent Transform assigned");
        }
        
        if (asteroidSpawner.goalTransform == null)
        {
            Debug.LogWarning("✗ AsteroidSpawner missing Goal Transform reference!");
            valid = false;
        }
        else
        {
            Debug.Log("✓ Goal Transform assigned");
        }
        
        return valid;
    }
    
    bool ValidateTelemetryUI()
    {
        Debug.Log("\n--- Telemetry UI Validation ---");
        bool valid = true;
        
        if (telemetryUI == null)
        {
            Debug.LogError("✗ TelemetryUI reference is missing!");
            return false;
        }
        
        if (telemetryUI.cubeSatRigidbody == null)
        {
            Debug.LogError("✗ TelemetryUI missing CubeSat Rigidbody reference!");
            valid = false;
        }
        else
        {
            Debug.Log("✓ CubeSat Rigidbody assigned");
        }
        
        if (telemetryUI.velocityText == null)
        {
            Debug.LogWarning("✗ Velocity Text not assigned!");
            valid = false;
        }
        else
        {
            Debug.Log("✓ All UI text elements assigned");
        }
        
        return valid;
    }
}
