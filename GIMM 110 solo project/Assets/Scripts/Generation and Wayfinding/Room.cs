using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Room : MonoBehaviour
{
    [Header("Waypoints in this room")]
    public List<GameObject> roomWaypoints = new List<GameObject>();

    [Header("Room Tracking")]
    [HideInInspector]
    public bool playerIsInRoom = false;

    private Collider2D roomCollider;

    private void Awake()
    {
        // Find the child GameObject named "roomBounds" and get its Collider2D
        Transform boundsTransform = transform.Find("roomBounds");
        if (boundsTransform != null)
        {
            roomCollider = boundsTransform.GetComponent<Collider2D>();
            if (roomCollider != null)
            {
                roomCollider.isTrigger = true;
            }
            else
            {
                Debug.LogWarning($"Room '{name}' child 'roomBounds' has no Collider2D!");
            }
        }
        else
        {
            Debug.LogWarning($"Room '{name}' does not have a child named 'roomBounds'!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"TriggerEnter fired on {name} with object: {other.name}");

        if (other.CompareTag("Player"))
        {
            playerIsInRoom = true;
            Debug.Log($" Player ENTERED room: {name}");
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRoom = false;
            Debug.Log($"Player EXITED room: {name}");
        }
    }
}

