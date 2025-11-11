using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerRoomTracker : MonoBehaviour
{
    [HideInInspector] public GameObject currentRoom;
    private Collider2D playerCollider;

    void Awake()
    {
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Ensure the current room updates even if triggers don't fire
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.1f);
        bool foundRoom = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("RoomBounds"))
            {
                Room room = hit.GetComponentInParent<Room>();
                if (room != null)
                {
                    if (currentRoom != room.gameObject)
                    {
                        EnterRoom(room);
                    }
                    foundRoom = true;
                }
            }
        }

        // Handle case where player leaves all rooms
        if (!foundRoom && currentRoom != null)
        {
            ExitRoom();
        }
    }

    private void EnterRoom(Room room)
    {
        if (currentRoom != null)
        {
            Room prevRoom = currentRoom.GetComponent<Room>();
            if (prevRoom != null) prevRoom.playerIsInRoom = false;
        }

        currentRoom = room.gameObject;
        room.playerIsInRoom = true;
        Debug.Log($"Player entered room: {room.name}");
    }

    private void ExitRoom()
    {
        Room prevRoom = currentRoom.GetComponent<Room>();
        if (prevRoom != null) prevRoom.playerIsInRoom = false;

        Debug.Log($"Player exited room: {currentRoom.name}");
        currentRoom = null;
    }
}
