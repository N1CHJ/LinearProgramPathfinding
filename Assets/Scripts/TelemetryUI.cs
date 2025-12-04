using UnityEngine;
using TMPro;

public class TelemetryUI : MonoBehaviour
{
    [Header("References")]
    public Rigidbody cubeSatRigidbody;
    public Transform goalTransform;
    
    [Header("UI Text Elements")]
    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI angularVelocityText;
    public TextMeshProUGUI cubeSatPositionText;
    public TextMeshProUGUI goalPositionText;
    
    [Header("Display Settings")]
    public bool showDebugInfo = true;
    public int decimalPlaces = 2;
    
    private Vector3 currentVelocity;
    private Vector3 currentAngularVelocity;
    private Vector3 currentPosition;
    private Vector3 goalPosition;
    private float currentSpeed;
    
    void Update()
    {
        if (cubeSatRigidbody == null)
            return;
        
        currentVelocity = cubeSatRigidbody.linearVelocity;
        currentAngularVelocity = cubeSatRigidbody.angularVelocity;
        currentSpeed = currentVelocity.magnitude;
        currentPosition = cubeSatRigidbody.position;
        
        if (goalTransform != null)
        {
            goalPosition = goalTransform.position;
        }
        
        UpdateUI();
    }
    
    void UpdateUI()
    {
        string format = $"F{decimalPlaces}";
        
        if (velocityText != null)
        {
            velocityText.text = $"Velocity: ({currentVelocity.x.ToString(format)}, " +
                               $"{currentVelocity.y.ToString(format)}, " +
                               $"{currentVelocity.z.ToString(format)}) m/s";
        }
        
        if (angularVelocityText != null)
        {
            angularVelocityText.text = $"Angular Velocity: ({currentAngularVelocity.x.ToString(format)}, " +
                                       $"{currentAngularVelocity.y.ToString(format)}, " +
                                       $"{currentAngularVelocity.z.ToString(format)}) rad/s";
        }
        
        if (cubeSatPositionText != null)
        {
            cubeSatPositionText.text = $"CubeSat Pos: ({currentPosition.x.ToString(format)}, " +
                                       $"{currentPosition.y.ToString(format)}, " +
                                       $"{currentPosition.z.ToString(format)})";
        }
        
        if (goalPositionText != null)
        {
            goalPositionText.text = $"Goal Pos: ({goalPosition.x.ToString(format)}, " +
                                    $"{goalPosition.y.ToString(format)}, " +
                                    $"{goalPosition.z.ToString(format)})";
        }
    }
    
    public void UpdateStatus(string status)
    {
        if (cubeSatPositionText != null)
        {
            cubeSatPositionText.text = status;
        }
    }
    
    public Vector3 GetVelocity() => currentVelocity;
    public Vector3 GetAngularVelocity() => currentAngularVelocity;
    public float GetSpeed() => currentSpeed;
    public Vector3 GetPosition() => currentPosition;
    public Vector3 GetGoalPosition() => goalPosition;
}
