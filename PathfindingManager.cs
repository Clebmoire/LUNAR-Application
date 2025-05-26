using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public GameObject pathMarkerPrefab;
    public GameObject movingObjectPrefab;

    private Dictionary<string, Waypoint> waypoints = new Dictionary<string, Waypoint>();
    private LineRenderer lineRenderer;
    private PinSpawner pinSpawner;

    public bool testInEditor = true;
    public string testStartName = "LUNAR_START_MAIN_GATE";
    public string testDestinationName = "LIB2";

    void Start()
    {
        InitializeWaypoints();
        SetupLineRenderer();
        pinSpawner = FindObjectOfType<PinSpawner>();
        Debug.Log("🟢 Unity PathfindingManager started.");

    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var intent = activity.Call<AndroidJavaObject>("getIntent");
                string destination = intent.Call<string>("getStringExtra", "destination");

                Debug.Log($"📍 DESTINATION from Android intent: {destination}");
                if (!string.IsNullOrEmpty(destination))
                {
                    ReceiveDestinationFromAndroid(destination);
                }
                else
                {
                    Debug.LogWarning("⚠️ Destination not found in intent extras.");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Exception reading intent: " + ex.Message);
        }
    #endif

        if (testInEditor)
        {
            ComputeAndRenderPath(testStartName, testDestinationName);
        }
    }

    void InitializeWaypoints()
    {
        waypoints.Clear();
        foreach (Waypoint wp in FindObjectsOfType<Waypoint>())
        {
            if (!string.IsNullOrEmpty(wp.waypointName))
            {
                string key = NormalizeKey(wp.waypointName);
                if (!waypoints.ContainsKey(key))
                    waypoints[key] = wp;

                foreach (Renderer r in wp.GetComponentsInChildren<Renderer>())
                {
                    r.enabled = false;
                }
            }
        }

        Debug.Log($"✅ Initialized {waypoints.Count} waypoints and hid their visuals.");
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.useWorldSpace = true;
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
        }
    }

    string GetDestinationFromAndroid()
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var intent = activity.Call<AndroidJavaObject>("getIntent");
            return intent.Call<string>("getStringExtra", "destination");
        }
    }

    public void ComputeAndRenderPath(string startName, string destinationName)
    {
        string startKey = NormalizeKey(startName);
        string endKey = NormalizeKey(destinationName);

        if (!waypoints.ContainsKey(startKey) || !waypoints.ContainsKey(endKey))
        {
            Debug.LogError($"❌ Start or end waypoint not found: {startKey} or {endKey}");
            return;
        }

        Debug.Log($"🔍 Computing path from '{startKey}' to '{endKey}'");

        List<Waypoint> path = Dijkstra(startKey, endKey);
        if (path.Count == 0)
        {
            Debug.LogError("❌ Pathfinding failed: Path not found.");
            return;
        }

        RenderPath(path);
        DrawPathLine(path);

        pinSpawner?.SpawnDestinationPin(path);

        if (testInEditor)
        {
            var xrOrigin = GameObject.Find("XR Origin (XR Rig)")?.transform;
            if (xrOrigin != null)
            {
                xrOrigin.position = waypoints[startKey].transform.position;
                Debug.Log($"✅ Moved XR Origin to {startKey}");
            }
        }

        if (movingObjectPrefab != null)
        {
            GameObject traveler = Instantiate(movingObjectPrefab, waypoints[startKey].transform.position, Quaternion.identity);
            StartCoroutine(MoveAlongPath(traveler, path));
        }
    }

    List<Waypoint> Dijkstra(string start, string end)
    {
        start = NormalizeKey(start);
        end = NormalizeKey(end);

        var distances = new Dictionary<string, float>();
        var previous = new Dictionary<string, Waypoint>();
        var unvisited = new List<Waypoint>(waypoints.Values);

        foreach (var wp in waypoints.Values)
            distances[NormalizeKey(wp.waypointName)] = float.MaxValue;

        distances[start] = 0;

        while (unvisited.Count > 0)
        {
            unvisited.Sort((a, b) =>
                distances[NormalizeKey(a.waypointName)].CompareTo(distances[NormalizeKey(b.waypointName)]));

            var current = unvisited[0];
            unvisited.RemoveAt(0);

            string currentKey = NormalizeKey(current.waypointName);
            if (currentKey == end)
                break;

            foreach (var neighbor in current.neighbors)
            {
                if (neighbor == null) continue;

                string neighborKey = NormalizeKey(neighbor.waypointName);
                float alt = distances[currentKey] +
                            Vector3.Distance(current.transform.position, neighbor.transform.position);

                if (alt < distances[neighborKey])
                {
                    distances[neighborKey] = alt;
                    previous[neighborKey] = current;
                }
            }
        }

        var path = new List<Waypoint>();
        string stepKey = end;

        if (!previous.ContainsKey(stepKey))
        {
            Debug.LogWarning($"⚠️ No path found from '{start}' to '{end}'");
            return path;
        }

        Waypoint step = waypoints[stepKey];
        while (previous.ContainsKey(stepKey))
        {
            path.Insert(0, step);
            step = previous[stepKey];
            stepKey = NormalizeKey(step.waypointName);
        }

        path.Insert(0, waypoints[start]);
        Debug.Log($"✅ Path length: {path.Count}");
        return path;
    }

    void RenderPath(List<Waypoint> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("⚠️ No path provided to render.");
            return;
        }

        foreach (var existingLine in GameObject.FindGameObjectsWithTag("PathLine"))
            Destroy(existingLine);

        GameObject lineObj = new GameObject("PathLine") { tag = "PathLine" };
        LineRenderer renderer = lineObj.AddComponent<LineRenderer>();

        renderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            renderer.SetPosition(i, path[i].transform.position + Vector3.up * 0.1f);

        Color navy = new Color(0f, 0f, 0.5f);
        var mat = new Material(Shader.Find("Sprites/Default")) { color = navy };
        renderer.material = mat;
        renderer.startColor = navy;
        renderer.endColor = navy;
        renderer.widthMultiplier = 0.5f;
        renderer.numCapVertices = 2;
        renderer.numCornerVertices = 2;

        Debug.Log("✅ Navy blue path rendered.");
    }

    void DrawPathLine(List<Waypoint> path)
    {
        if (path == null || path.Count == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            lineRenderer.SetPosition(i, path[i].transform.position);

        Debug.Log("✅ Pathline drawn using LineRenderer.");
    }

    IEnumerator MoveAlongPath(GameObject obj, List<Waypoint> path)
    {
        float speed = 1.5f;
        foreach (Waypoint wp in path)
        {
            Vector3 target = wp.transform.position;
            while (Vector3.Distance(obj.transform.position, target) > 0.1f)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, speed * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("✅ Traveler reached the destination.");
    }

    public void ReceiveDestinationFromAndroid(string destinationName)
    {
        Debug.Log($"📍 Received destination from Android: {destinationName}");
        string startName = "LUNAR_START_MAIN_GATE";
        ComputeAndRenderPath(startName, destinationName);
    }

    string NormalizeKey(string raw)
    {
        return raw?.Trim().ToLower().Replace(" ", "_");
    }
}
