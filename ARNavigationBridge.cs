using UnityEngine;
using ZXing.PDF417.Internal;

public class ARNavigationBridge : MonoBehaviour
{
    private PathfindingManager pathfindingManager;

    private void Awake()
    {
        pathfindingManager = FindObjectOfType<PathfindingManager>();
    }

    public void StartNavigation(string startName, string destinationName)
    {
        if (pathfindingManager != null)
        {
            pathfindingManager.ComputeAndRenderPath("lunar_start_main_gate", "openstage");

        }
        else
        {
            Debug.LogError("PathfindingManager not found in scene.");
        }
    }
}