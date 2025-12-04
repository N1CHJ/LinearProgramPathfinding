using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CubeSatAgent : Agent
{
    [Header("References")]
    public Transform goalTransform;
    public AsteroidSpawner asteroidSpawner;
    public TelemetryUI telemetryUI;
    
    [Header("Physics Settings")]
    public float maxThrust = 10f;
    public float maxTorque = 5f;
    public float agentMass = 1f;
    
    [Header("Training Settings")]
    public float maxEpisodeTime = 60f;
    public float goalReachDistance = 2f;
    
    [Header("Boundary Settings")]
    public bool enforceBoundary = true;
    public Vector3 boundarySize = new Vector3(80f, 80f, 80f);
    public float boundaryPenalty = -1.0f;
    
    [Header("Reward Settings")]
    public float goalReward = 3.0f;
    public float collisionPenalty = -1.0f;
    public float timeStepPenalty = -0.0005f;
    public float velocityTowardGoalReward = 0.0002f;
    public float maxSpeedAtGoal = 3.0f;
    public float speedPenaltyMultiplier = 0.1f;
    public float linearVelocityPenalty = 0.0001f;
    public float angularVelocityPenalty = 0.0005f;
    public float maxSafeLinearVelocity = 10f;
    public float maxSafeAngularVelocity = 2f;
    
    [Header("Spawn Settings")]
    public bool randomizeStartPosition = true;
    public Vector3 startSpawnAreaSize = new Vector3(20f, 20f, 20f);
    public bool randomizeGoalPosition = true;
    public Vector3 goalSpawnAreaSize = new Vector3(40f, 40f, 40f);
    public float minGoalDistance = 15f;
    public bool addInitialTumble = false;
    public float maxInitialAngularVelocity = 0.5f;
    
    [Header("Safety Filter")]
    public bool useSafetyFilter = false;
    public QPSafetyFilter safetyFilter;
    
    private Rigidbody agentRigidbody;
    private float thrustInput;
    private Vector3 torqueInput;
    private float episodeTimer;
    private bool hasReachedGoal;
    private bool hasCrashed;
    private bool hasLeftBoundary;
    
    void Awake()
    {
        agentRigidbody = GetComponent<Rigidbody>();
        agentRigidbody.mass = agentMass;
        agentRigidbody.useGravity = false;
    }
    
    public override void OnEpisodeBegin()
    {
        episodeTimer = 0f;
        hasReachedGoal = false;
        hasCrashed = false;
        hasLeftBoundary = false;
        
        agentRigidbody.linearVelocity = Vector3.zero;
        agentRigidbody.angularVelocity = Vector3.zero;
        
        if (randomizeStartPosition)
        {
            transform.localPosition = GetRandomStartPosition();
        }
        else
        {
            transform.localPosition = Vector3.zero;
        }
        
        transform.rotation = Quaternion.identity;
        
        if (addInitialTumble)
        {
            agentRigidbody.angularVelocity = Random.insideUnitSphere * maxInitialAngularVelocity;
        }
        
        if (randomizeGoalPosition && goalTransform != null)
        {
            goalTransform.position = GetRandomGoalPosition();
        }
        
        if (asteroidSpawner != null)
        {
            asteroidSpawner.RandomizeAsteroids();
        }
        
        if (telemetryUI != null)
        {
            telemetryUI.UpdateStatus("Active");
        }
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(agentRigidbody.linearVelocity);
        sensor.AddObservation(agentRigidbody.angularVelocity);
        
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(transform.up);
        
        if (goalTransform != null)
        {
            Vector3 relativeGoalPosition = goalTransform.position - transform.position;
            
            sensor.AddObservation(relativeGoalPosition.x);
            sensor.AddObservation(relativeGoalPosition.y);
            sensor.AddObservation(relativeGoalPosition.z);
            
            sensor.AddObservation(relativeGoalPosition.magnitude);
            
            Vector3 localGoalDirection = transform.InverseTransformDirection(relativeGoalPosition.normalized);
            sensor.AddObservation(localGoalDirection);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector3.zero);
        }
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        thrustInput = Mathf.Clamp01(actions.ContinuousActions[0]);
        torqueInput = new Vector3(
            actions.ContinuousActions[1],
            actions.ContinuousActions[2],
            actions.ContinuousActions[3]
        );
        
        Vector3 thrustForce = transform.forward * thrustInput * maxThrust;
        agentRigidbody.AddForce(thrustForce);
        
        Vector3 torque = torqueInput * maxTorque;
        agentRigidbody.AddTorque(torque);
        
        AddReward(timeStepPenalty);
        
        if (goalTransform != null)
        {
            Vector3 directionToGoal = (goalTransform.position - transform.position).normalized;
            float velocityTowardGoal = Vector3.Dot(agentRigidbody.linearVelocity, directionToGoal);
            
            if (velocityTowardGoal > 0)
            {
                AddReward(velocityTowardGoalReward * velocityTowardGoal);
            }
        }
        
        float currentLinearSpeed = agentRigidbody.linearVelocity.magnitude;
        if (currentLinearSpeed > maxSafeLinearVelocity)
        {
            float excessLinearVelocity = currentLinearSpeed - maxSafeLinearVelocity;
            AddReward(-linearVelocityPenalty * excessLinearVelocity);
        }
        
        float currentAngularSpeed = agentRigidbody.angularVelocity.magnitude;
        if (currentAngularSpeed > maxSafeAngularVelocity)
        {
            float excessAngularVelocity = currentAngularSpeed - maxSafeAngularVelocity;
            AddReward(-angularVelocityPenalty * excessAngularVelocity);
        }
        
        episodeTimer += Time.fixedDeltaTime;
        if (episodeTimer >= maxEpisodeTime)
        {
            EndEpisode();
        }
        
        CheckGoalReached();
        CheckBoundary();
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            continuousActions[0] = 0f;
            continuousActions[1] = 0f;
            continuousActions[2] = 0f;
            continuousActions[3] = 0f;
            return;
        }
        
        float thrust = 0f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            thrust = 1f;
        
        float pitch = 0f;
        if (keyboard.qKey.isPressed)
            pitch = -1f;
        if (keyboard.eKey.isPressed)
            pitch = 1f;
        
        float yaw = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            yaw = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            yaw = 1f;
        
        float roll = 0f;
        if (keyboard.zKey.isPressed)
            roll = -1f;
        if (keyboard.xKey.isPressed)
            roll = 1f;
        
        continuousActions[0] = thrust;
        continuousActions[1] = pitch;
        continuousActions[2] = yaw;
        continuousActions[3] = roll;
    }
    
    private void CheckGoalReached()
    {
        if (goalTransform != null && !hasReachedGoal)
        {
            float distance = Vector3.Distance(transform.position, goalTransform.position);
            if (distance < goalReachDistance)
            {
                hasReachedGoal = true;
                
                float currentSpeed = agentRigidbody.linearVelocity.magnitude;
                float baseReward = goalReward;
                
                if (currentSpeed > maxSpeedAtGoal)
                {
                    float excessSpeed = currentSpeed - maxSpeedAtGoal;
                    float speedPenalty = excessSpeed * speedPenaltyMultiplier;
                    baseReward -= speedPenalty;
                }
                
                AddReward(baseReward);
                
                if (telemetryUI != null)
                {
                    telemetryUI.UpdateStatus($"Goal Reached! Time: {episodeTimer:F2}s, Speed: {currentSpeed:F2}m/s");
                }
                
                EndEpisode();
            }
        }
    }
    
    private void CheckBoundary()
    {
        if (!enforceBoundary || hasLeftBoundary)
            return;
        
        Vector3 pos = transform.position;
        float halfX = boundarySize.x / 2f;
        float halfY = boundarySize.y / 2f;
        float halfZ = boundarySize.z / 2f;
        
        bool isOutOfBounds = pos.x < -halfX || pos.x > halfX ||
                             pos.y < -halfY || pos.y > halfY ||
                             pos.z < -halfZ || pos.z > halfZ;
        
        if (isOutOfBounds)
        {
            hasLeftBoundary = true;
            AddReward(boundaryPenalty);
            
            if (telemetryUI != null)
            {
                telemetryUI.UpdateStatus($"OUT OF BOUNDS at {episodeTimer:F2}s");
            }
            
            EndEpisode();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid") && !hasCrashed)
        {
            hasCrashed = true;
            AddReward(collisionPenalty);
            
            if (telemetryUI != null)
            {
                telemetryUI.UpdateStatus($"CRASHED at {episodeTimer:F2}s");
            }
            
            EndEpisode();
        }
    }
    
    private Vector3 GetRandomStartPosition()
    {
        return new Vector3(
            Random.Range(-startSpawnAreaSize.x / 2, startSpawnAreaSize.x / 2),
            Random.Range(-startSpawnAreaSize.y / 2, startSpawnAreaSize.y / 2),
            Random.Range(-startSpawnAreaSize.z / 2, startSpawnAreaSize.z / 2)
        );
    }
    
    private Vector3 GetRandomGoalPosition()
    {
        const int maxAttempts = 50;
        Vector3 startPos = transform.localPosition;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-goalSpawnAreaSize.x / 2, goalSpawnAreaSize.x / 2),
                Random.Range(-goalSpawnAreaSize.y / 2, goalSpawnAreaSize.y / 2),
                Random.Range(-goalSpawnAreaSize.z / 2, goalSpawnAreaSize.z / 2)
            );
            
            float distanceFromStart = Vector3.Distance(randomPos, startPos);
            if (distanceFromStart >= minGoalDistance)
            {
                return randomPos;
            }
        }
        
        Vector3 fallbackDirection = Random.onUnitSphere;
        return startPos + fallbackDirection * minGoalDistance;
    }
    
    public float GetThrustInput() => thrustInput;
    public Vector3 GetTorqueInput() => torqueInput;
    public float GetEpisodeTime() => episodeTimer;
    
    void OnDrawGizmosSelected()
    {
        if (!enforceBoundary)
            return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, boundarySize);
        
        if (randomizeStartPosition)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, startSpawnAreaSize);
        }
        
        if (randomizeGoalPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, goalSpawnAreaSize);
        }
    }
}
