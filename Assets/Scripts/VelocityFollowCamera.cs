using UnityEngine;

public class VelocityFollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    
    [Header("Camera Rotation")]
    public bool lookAtTarget = true;
    public Vector3 lookAtOffset = Vector3.zero;
    
    void LateUpdate()
    {
        if (target == null)
            return;
        
        transform.position = target.position + offset;
        
        if (lookAtTarget)
        {
            Vector3 lookPosition = target.position + lookAtOffset;
            transform.LookAt(lookPosition);
        }
    }
}
