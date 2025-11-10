using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaypointConnection
{
    public GameObject target;
    public float cost = 1f;
}

public class Waypoint : MonoBehaviour
{
    [Header("Waypoint Manager Reference")]
    public WaypointManager waypointManager;

    [Header("Connections to Other Waypoints")]
    public List<WaypointConnection> connections = new List<WaypointConnection>();

    [Header("Gizmo Settings")]
    public Color connectionColor = Color.cyan;
    public bool drawConnections = true;

    private Room parentRoom;

    void Awake()
    {
        parentRoom = GetComponentInParent<Room>();

        // Register waypoint with manager
        if (waypointManager == null)
            waypointManager = FindObjectOfType<WaypointManager>();

        if (waypointManager != null)
            waypointManager.AddWaypoint(gameObject);
    }

    void Start()
    {
        StartCoroutine(GenerateConnectionsNextFrame());
    }

    private IEnumerator GenerateConnectionsNextFrame()
    {
        yield return null; // wait a frame for all rooms/waypoints to initialize
        AddConnectionsInCardinalDirections();
    }

    public void AddConnection(GameObject target, float cost = 1f)
    {
        if (target == null || target == gameObject) return;
        if (!connections.Exists(c => c.target == target))
            connections.Add(new WaypointConnection { target = target, cost = cost });
    }

    void AddConnectionsInCardinalDirections()
    {
        float maxDistance = 20f;
        if (waypointManager == null) return;

        Vector3 myPos = transform.position;
        int wallMask = LayerMask.GetMask("Wall"); // assumes your wall layer is named "Wall"

        foreach (var wp in waypointManager.allWaypoints)
        {
            if (wp == null || wp == gameObject || connections.Exists(c => c.target == wp))
                continue;

            Vector3 targetPos = wp.transform.position;
            Vector3 delta = targetPos - myPos;

            bool isCardinal =
                (Mathf.Abs(delta.x) <= 0.1f && Mathf.Abs(delta.y) <= maxDistance) ||
                (Mathf.Abs(delta.y) <= 0.1f && Mathf.Abs(delta.x) <= maxDistance);

            if (!isCardinal)
                continue;

            //  Linecast check for wall obstruction
            RaycastHit2D hit = Physics2D.Linecast(myPos, targetPos, wallMask);
            if (hit.collider != null)
            {
                // There’s a wall between the two waypoints → skip
                continue;
            }

            // No wall detected → add connection
            AddConnection(wp);
        }
    }


    void OnDrawGizmos()
    {
        if (!drawConnections) return;

        Gizmos.color = connectionColor;
        foreach (var conn in connections)
        {
            if (conn.target != null)
                Gizmos.DrawLine(transform.position, conn.target.transform.position);
        }
    }
}
