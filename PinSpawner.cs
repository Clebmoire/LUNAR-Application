using System.Collections.Generic;
using UnityEngine;

public class PinSpawner : MonoBehaviour
{
    public GameObject pinPrefab;       // Assign this in the inspector
    public float pinHeightOffset = 0.2f;  // Height above the destination where the pin appears

    private GameObject currentPin;

    public void SpawnDestinationPin(List<Waypoint> path)
    {
        // Remove old pin if it exists
        if (currentPin != null)
        {
            Destroy(currentPin);
        }

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("⚠️ No path provided for pin placement.");
            return;
        }

        // Use the last waypoint in the path (destination)
        Waypoint destination = path[path.Count - 1];

        // Spawn pin at destination + height offset
        Vector3 pinPosition = destination.transform.position;
        pinPosition.y += pinHeightOffset;

        currentPin = Instantiate(pinPrefab, pinPosition, Quaternion.identity);
        Debug.Log($"📍 Destination pin spawned at: {pinPosition}");
    }
}
