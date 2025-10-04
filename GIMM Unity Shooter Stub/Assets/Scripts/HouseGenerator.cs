using UnityEngine;
using System.Collections.Generic;

public class HouseGenerator : MonoBehaviour
{
    [Header("Prefabs (Global)")]
    public GameObject doorPrefab;
    public GameObject spawnerPrefab;
    public GameObject playerPrefab;

    [System.Serializable]
    public class TemplateSet
    {
        public string templateName = "Template";
        public GameObject floorPrefab;
        public GameObject wallPrefab;
    }

    [Header("Main Room Templates")]
    public List<TemplateSet> mainRoomTemplates = new List<TemplateSet>();

    [Header("Side Room Templates")]
    public List<TemplateSet> sideRoomTemplates = new List<TemplateSet>();

    [Header("Settings")]
    [SerializeField] private int houseWidth = 40;
    [SerializeField] private int houseHeight = 30;
    [SerializeField] private int minRoomSize = 6;
    [SerializeField] private int maxRoomSize = 12;
    [SerializeField] private int maxSpawners = 4;

    private int[,] map; // 0 = empty, >0 = room ID, -1 = wall, -2 = door, -3 = spawner
    private Dictionary<int, TemplateSet> roomTemplateAssignments = new Dictionary<int, TemplateSet>();
    private List<RectInt> rooms = new List<RectInt>();
    private int mainRoomIndex = -1;

    void Start()
    {
        GenerateHouse();
        BuildMap();
    }

    void GenerateHouse()
    {
        map = new int[houseWidth, houseHeight];
        rooms.Clear();
        roomTemplateAssignments.Clear();

        // Step 1: Start with one big rectangle (the house bounds)
        RectInt houseRect = new RectInt(1, 1, houseWidth - 2, houseHeight - 2);

        // Step 2: Subdivide into rooms using BSP
        Subdivide(houseRect, 4); // split into ~4–8 rooms

        // Step 3: Pick the largest central room as the main room
        Vector2Int houseCenter = new Vector2Int(houseWidth / 2, houseHeight / 2);
        float closestDist = float.MaxValue;
        int centralRoomIndex = 0;
        int largestArea = 0;

        for (int i = 0; i < rooms.Count; i++)
        {
            int area = rooms[i].width * rooms[i].height;
            Vector2Int rc = new Vector2Int((int)rooms[i].center.x, (int)rooms[i].center.y);
            float dist = Vector2Int.Distance(houseCenter, rc);

            if (area > largestArea || (area == largestArea && dist < closestDist))
            {
                largestArea = area;
                closestDist = dist;
                centralRoomIndex = i;
            }
        }
        mainRoomIndex = centralRoomIndex;

        // Step 4: Fill rooms in map + assign templates
        for (int i = 0; i < rooms.Count; i++)
        {
            RectInt r = rooms[i];
            for (int x = r.xMin; x < r.xMax; x++)
                for (int y = r.yMin; y < r.yMax; y++)
                    map[x, y] = i + 1;

            roomTemplateAssignments[i] = (i == mainRoomIndex)
                ? mainRoomTemplates[Random.Range(0, mainRoomTemplates.Count)]
                : sideRoomTemplates[Random.Range(0, sideRoomTemplates.Count)];
        }

        // Step 5: Add walls around rooms
        for (int x = 0; x < houseWidth; x++)
        {
            for (int y = 0; y < houseHeight; y++)
            {
                if (map[x, y] == 0)
                {
                    if ((x > 0 && map[x - 1, y] > 0) ||
                        (x < houseWidth - 1 && map[x + 1, y] > 0) ||
                        (y > 0 && map[x, y - 1] > 0) ||
                        (y < houseHeight - 1 && map[x, y + 1] > 0))
                    {
                        map[x, y] = -1; // wall
                    }
                }
            }
        }

        // Step 6: Add doors between touching rooms
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                RectInt a = rooms[i];
                RectInt b = rooms[j];
                if (a.Overlaps(b)) continue;

                if (a.xMin <= b.xMax + 1 && a.xMax + 1 >= b.xMin &&
                    a.yMin <= b.yMax + 1 && a.yMax + 1 >= b.yMin)
                {
                    Vector2Int pos = new Vector2Int(
                        (int)((a.center.x + b.center.x) / 2f),
                        (int)((a.center.y + b.center.y) / 2f)
                    );
                    map[pos.x, pos.y] = -2; // door
                }
            }
        }

        // Step 7: Place spawners on outer walls
        List<Vector2Int> outsideWalls = new List<Vector2Int>();
        for (int x = 0; x < houseWidth; x++)
        {
            for (int y = 0; y < houseHeight; y++)
            {
                if (map[x, y] == -1 &&
                    (x == 0 || x == houseWidth - 1 || y == 0 || y == houseHeight - 1))
                {
                    outsideWalls.Add(new Vector2Int(x, y));
                }
            }
        }

        int spawnersPlaced = 0;
        while (outsideWalls.Count > 0 && spawnersPlaced < maxSpawners)
        {
            int index = Random.Range(0, outsideWalls.Count);
            Vector2Int pos = outsideWalls[index];
            outsideWalls.RemoveAt(index);
            map[pos.x, pos.y] = -3;
            spawnersPlaced++;
        }
    }

    void BuildMap()
    {
        for (int x = 0; x < houseWidth; x++)
        {
            for (int y = 0; y < houseHeight; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                int val = map[x, y];

                if (val > 0)
                {
                    TemplateSet template = roomTemplateAssignments[val - 1];
                    Instantiate(template.floorPrefab, pos, Quaternion.identity, transform);
                }
                else if (val == -1)
                {
                    TemplateSet template = ClosestRoomTemplate(new Vector2Int(x, y));
                    Instantiate(template.wallPrefab, pos, Quaternion.identity, transform);
                }
                else if (val == -2)
                {
                    Instantiate(doorPrefab, pos, Quaternion.identity, transform);
                }
                else if (val == -3)
                {
                    Instantiate(spawnerPrefab, pos, Quaternion.identity, transform);
                }
            }
        }

        // Step 8: Spawn player in center of main room
        if (playerPrefab && mainRoomIndex >= 0)
        {
            Vector3 playerPos = new Vector3(rooms[mainRoomIndex].center.x, rooms[mainRoomIndex].center.y, 0);
            Instantiate(playerPrefab, playerPos, Quaternion.identity);
        }
    }

    TemplateSet ClosestRoomTemplate(Vector2Int pos)
    {
        float bestDist = float.MaxValue;
        TemplateSet closest = sideRoomTemplates.Count > 0 ? sideRoomTemplates[0] : mainRoomTemplates[0];

        for (int i = 0; i < rooms.Count; i++)
        {
            Vector2Int roomCenter = new Vector2Int((int)rooms[i].center.x, (int)rooms[i].center.y);
            float dist = Vector2Int.Distance(pos, roomCenter);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = roomTemplateAssignments[i];
            }
        }
        return closest;
    }

    // Recursive BSP split
    void Subdivide(RectInt space, int depth)
    {
        if (depth <= 0 || space.width < minRoomSize * 2 || space.height < minRoomSize * 2)
        {
            int w = Mathf.Clamp(space.width - 1, minRoomSize, maxRoomSize);
            int h = Mathf.Clamp(space.height - 1, minRoomSize, maxRoomSize);
            int x = space.x + Random.Range(0, space.width - w);
            int y = space.y + Random.Range(0, space.height - h);
            rooms.Add(new RectInt(x, y, w, h));
            return;
        }

        bool splitVertical = (space.width > space.height);
        if (splitVertical)
        {
            int splitX = Random.Range(minRoomSize, space.width - minRoomSize);
            Subdivide(new RectInt(space.x, space.y, splitX, space.height), depth - 1);
            Subdivide(new RectInt(space.x + splitX, space.y, space.width - splitX, space.height), depth - 1);
        }
        else
        {
            int splitY = Random.Range(minRoomSize, space.height - minRoomSize);
            Subdivide(new RectInt(space.x, space.y, space.width, splitY), depth - 1);
            Subdivide(new RectInt(space.x, space.y + splitY, space.width, space.height - splitY), depth - 1);
        }
    }
}


