using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class HouseGenerator : MonoBehaviour
{
    [Header("Scene References")]
    public Transform mainRoom;
    public GameObject playerPrefab;

    [Header("Prefabs")]
    public List<GameObject> insideRoomPrefabs = new List<GameObject>();
    public List<GameObject> outsideRoomPrefabs = new List<GameObject>();
    public GameObject spawnerPrefab;

    [Header("Generation Settings")]
    [Range(1, 20)] public int minInsideRooms = 4;
    [Range(1, 20)] public int maxInsideRooms = 8;
    [Range(1, 100)] public int maxPlacementTries = 100;
    public int maxSpawners = 3;

    [Header("Collision Safeguards")]
    [Tooltip("Select which layers count as walls for doorway overlap checking.")]
    public LayerMask wallLayerMask;
    public float doorwayClearRadius = 0.3f;

    [Header("Validation Settings")]
    [Tooltip("How many times to retry generation until all rooms are accessible.")]
    public int maxGenerationRetries = 5;
    public float doorwayConnectDistance = 2.0f;


    private class Doorway
    {
        public Transform transform;
        public GameObject room;
        public bool used;
    }

    private List<GameObject> spawnedRooms = new List<GameObject>();
    private List<Bounds> roomBounds = new List<Bounds>();
    private List<Doorway> openDoorways = new List<Doorway>();

    void Start()
    {
        int attempts = 0;
        bool valid = false;

        while (!valid && attempts < maxGenerationRetries)
        {
            attempts++;
            Debug.Log($"[Generation Attempt {attempts}] Starting...");

            ClearPreviousGeneration();
            GenerateHouse();

            if (IsHouseFullyConnected())
            {
                valid = true;
                Debug.Log($"[Generation Attempt {attempts}] ✅ House fully connected.");
            }
            else
            {
                Debug.LogWarning($"[Generation Attempt {attempts}] ❌ Disconnected sections detected. Retrying...");
            }
        }

        if (!valid)
            Debug.LogError($"[Generation] Failed to create a connected layout after {maxGenerationRetries} attempts.");
    }



    void GenerateHouse()
    {
        if (!mainRoom)
        {
            Debug.LogError("Main room not assigned!");
            return;
        }

        Transform parent = transform;

        spawnedRooms.Add(mainRoom.gameObject);
        roomBounds.Add(GetRoomBounds(mainRoom.gameObject));
        foreach (Transform d in GetDoorways(mainRoom.gameObject))
            openDoorways.Add(new Doorway { transform = d, room = mainRoom.gameObject });

        // ---- PHASE 1: Interior Rooms ----
        int targetInsideCount = Random.Range(minInsideRooms, maxInsideRooms + 1);
        Debug.Log($"[Phase 1] Targeting {targetInsideCount} inside rooms...");

        for (int i = 0; i < targetInsideCount; i++)
        {
            if (!TryPlaceRoom(insideRoomPrefabs, parent))
            {
                Debug.LogWarning($"[Phase 1] Failed to place inside room {i + 1}");
                break;
            }
        }

        // ---- PHASE 2: End Rooms ----
        Debug.Log($"[Phase 2] Placing end rooms on {openDoorways.Count} open doorways...");

        List<Doorway> remainingDoorways = new List<Doorway>(openDoorways);
        openDoorways.Clear();

        foreach (var attachTo in remainingDoorways)
        {
            if (attachTo.used) continue;

            bool success = false;
            int tries = 0;

            while (!success && tries < maxPlacementTries)
            {
                tries++;
                if (outsideRoomPrefabs.Count == 0) break;

                var prefab = outsideRoomPrefabs[Random.Range(0, outsideRoomPrefabs.Count)];
                success = TryAttachSpecificRoom(attachTo, prefab, parent);
            }

            if (!success)
            {
                Debug.LogWarning($"[Phase 2] Failed to attach end room to doorway {attachTo.transform.name} after {tries} tries.");
            }
        }

        Debug.Log($"Generation complete. Total rooms: {spawnedRooms.Count}");
    }

    bool TryPlaceRoom(List<GameObject> prefabList, Transform parent)
    {
        if (prefabList == null || prefabList.Count == 0)
        {
            Debug.LogWarning("No prefabs available for this phase!");
            return false;
        }

        for (int t = 0; t < maxPlacementTries; t++)
        {
            if (openDoorways.Count == 0)
            {
                Debug.LogWarning("No more open doorways available!");
                return false;
            }

            Doorway attachTo = openDoorways[Random.Range(0, openDoorways.Count)];
            if (attachTo.used) continue;

            GameObject prefab = prefabList[Random.Range(0, prefabList.Count)];
            Transform[] prefabDoors = GetDoorways(prefab);
            if (prefabDoors.Length == 0) continue;

            Transform prefabDoor = prefabDoors[Random.Range(0, prefabDoors.Length)];
            GameObject newRoom = Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);

            Vector3 prefabDoorForward = prefabDoor.forward;
            Vector3 targetDir = -attachTo.transform.forward;
            Quaternion rotation = Quaternion.FromToRotation(prefabDoorForward, targetDir);
            newRoom.transform.rotation = rotation;

            Vector3 doorOffset = prefabDoor.position - prefab.transform.position;
            Vector3 rotatedOffset = rotation * doorOffset;
            Vector3 finalPos = attachTo.transform.position - rotatedOffset;
            newRoom.transform.position = finalPos + (attachTo.transform.forward * 0.001f);

            float facingDot = Vector3.Dot(attachTo.transform.forward, -prefabDoor.forward);
            if (facingDot < 0.98f)
            {
                DestroyImmediate(newRoom);
                continue;
            }

            Bounds newBounds = GetRoomBounds(newRoom);
            bool overlaps = false;
            foreach (Bounds rb in roomBounds)
            {
                if (newBounds.Intersects(rb))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                // SAFEGUARD: Prevent doorway-over-wall placement
                if (DoorwayOverlapsWall(newRoom))
                {
                    Debug.Log($"[Safety] {prefab.name} doorway would overlap a wall, skipping.");
                    DestroyImmediate(newRoom);
                    continue;
                }

                spawnedRooms.Add(newRoom);
                roomBounds.Add(newBounds);

                attachTo.used = true;
                openDoorways.Remove(attachTo);

                foreach (Transform d in GetDoorways(newRoom))
                {
                    if (Vector3.Distance(d.position, attachTo.transform.position) < 0.2f)
                        continue;

                    openDoorways.Add(new Doorway { transform = d, room = newRoom });
                }

                return true;
            }
            else
            {
                DestroyImmediate(newRoom);
            }
        }

        return false;
    }

    bool TryAttachSpecificRoom(Doorway attachTo, GameObject prefab, Transform parent)
    {
        Transform[] prefabDoors = GetDoorways(prefab);
        if (prefabDoors.Length == 0) return false;

        Transform prefabDoor = prefabDoors[Random.Range(0, prefabDoors.Length)];
        GameObject newRoom = Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);

        Vector3 prefabDoorForward = prefabDoor.forward;
        Vector3 targetDir = -attachTo.transform.forward;
        Quaternion rotation = Quaternion.FromToRotation(prefabDoorForward, targetDir);
        newRoom.transform.rotation = rotation;

        Vector3 doorOffset = prefabDoor.position - prefab.transform.position;
        Vector3 rotatedOffset = rotation * doorOffset;
        Vector3 finalPos = attachTo.transform.position - rotatedOffset;
        newRoom.transform.position = finalPos + (attachTo.transform.forward * 0.001f);

        float facingDot = Vector3.Dot(attachTo.transform.forward, -prefabDoor.forward);
        if (facingDot < 0.98f)
        {
            DestroyImmediate(newRoom);
            return false;
        }

        Bounds newBounds = GetRoomBounds(newRoom);
        foreach (Bounds rb in roomBounds)
        {
            if (newBounds.Intersects(rb))
            {
                DestroyImmediate(newRoom);
                return false;
            }
        }

        // SAFEGUARD: Prevent doorway-over-wall placement
        if (DoorwayOverlapsWall(newRoom))
        {
            Debug.Log($"[Safety] {prefab.name} doorway would overlap a wall, skipping.");
            DestroyImmediate(newRoom);
            return false;
        }

        spawnedRooms.Add(newRoom);
        roomBounds.Add(newBounds);
        attachTo.used = true;

        return true;
    }

    bool DoorwayOverlapsWall(GameObject room)
    {
        foreach (Transform d in GetDoorways(room))
        {
            Collider[] hits = Physics.OverlapSphere(d.position, doorwayClearRadius, wallLayerMask);
            if (hits.Length > 0)
            {
                return true;
            }
        }
        return false;
    }

    Bounds GetRoomBounds(GameObject obj)
    {
        Transform boundsObj = obj.transform.Find("RoomBounds");
        Bounds newBounds;

        if (boundsObj != null)
        {
            var col = boundsObj.GetComponent<BoxCollider>();
            if (col)
            {
                col.enabled = false;
                newBounds = new Bounds(col.bounds.center, col.bounds.size);
            }
            else
            {
                newBounds = new Bounds(boundsObj.position, boundsObj.localScale);
            }
        }
        else
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                newBounds = renderers[0].bounds;
                foreach (var r in renderers)
                    newBounds.Encapsulate(r.bounds);
            }
            else
            {
                newBounds = new Bounds(obj.transform.position, Vector3.one * 2f);
            }
        }

        return newBounds;
    }

    Transform[] GetDoorways(GameObject obj)
    {
        List<Transform> doors = new List<Transform>();
        foreach (Transform t in obj.GetComponentsInChildren<Transform>())
        {
            if (t.CompareTag("Doorway"))
                doors.Add(t);
        }
        return doors.ToArray();
    }

    void ClearOldGeneration()
    {
        // Destroy everything except this generator itself
        for (int i = spawnedRooms.Count - 1; i >= 0; i--)
        {
            var room = spawnedRooms[i];
            if (room != null && room != mainRoom.gameObject)
                DestroyImmediate(room);
        }

        spawnedRooms.Clear();
        roomBounds.Clear();
        openDoorways.Clear();
    }

    void ClearPreviousGeneration()
    {
        foreach (var room in spawnedRooms)
        {
            if (room != null && room != mainRoom.gameObject)
                DestroyImmediate(room);
        }

        spawnedRooms.Clear();
        roomBounds.Clear();
        openDoorways.Clear();
    }
    bool IsHouseFullyConnected()
    {
        if (spawnedRooms.Count == 0) return false;

        // Build adjacency graph using doorway proximity and facing
        Dictionary<GameObject, List<GameObject>> graph = new Dictionary<GameObject, List<GameObject>>();
        foreach (var room in spawnedRooms)
            graph[room] = new List<GameObject>();

        foreach (var a in spawnedRooms)
        {
            foreach (var b in spawnedRooms)
            {
                if (a == b) continue;

                Transform[] doorsA = GetDoorways(a);
                Transform[] doorsB = GetDoorways(b);

                foreach (var da in doorsA)
                    foreach (var db in doorsB)
                    {
                        float dist = Vector3.Distance(da.position, db.position);
                        if (dist < doorwayConnectDistance && Vector3.Dot(da.forward, -db.forward) > 0.6f)
                        {
                            if (!graph[a].Contains(b))
                                graph[a].Add(b);
                            if (!graph[b].Contains(a))
                                graph[b].Add(a);
                        }
                    }
            }
        }

        // Breadth-first search from main room
        HashSet<GameObject> visited = new HashSet<GameObject>();
        Queue<GameObject> queue = new Queue<GameObject>();
        queue.Enqueue(mainRoom.gameObject);
        visited.Add(mainRoom.gameObject);

        while (queue.Count > 0)
        {
            GameObject current = queue.Dequeue();
            foreach (GameObject neighbor in graph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // If every room is visited, the layout is fully connected
        return visited.Count == spawnedRooms.Count;
    }


}
