using UnityEngine;
using UnityEngine.InputSystem;

public class DemoController : MonoBehaviour
{
    public GridManager gridManager;
    public PathfindingAgent agent;
    
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RegenerateGrid();
        }
    }
    
    void RegenerateGrid()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
