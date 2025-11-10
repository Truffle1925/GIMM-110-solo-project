using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    [Header("Global Waypoints")]
    public List<GameObject> allWaypoints = new List<GameObject>();

    void Awake()
    {
        // Collect all room waypoints
        Room[] rooms = FindObjectsOfType<Room>();
        allWaypoints.Clear();

        foreach (var room in rooms)
        {
            if (room.roomWaypoints != null)
            {
                foreach (var wp in room.roomWaypoints)
                {
                    if (wp != null && !allWaypoints.Contains(wp))
                        allWaypoints.Add(wp);
                }
            }
        }

        Debug.Log($"WaypointManager: Collected {allWaypoints.Count} total waypoints in the scene.");
    }

    public void AddWaypoint(GameObject wp)
    {
        if (wp != null && !allWaypoints.Contains(wp))
            allWaypoints.Add(wp);
    }

    public GameObject GetClosestWaypoint(Vector3 position)
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var wp in allWaypoints)
        {
            if (wp == null) continue;
            float dist = Vector2.Distance(position, wp.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = wp;
            }
        }

        return closest;
    }

    public List<GameObject> GetAllWaypoints() => allWaypoints;
}
