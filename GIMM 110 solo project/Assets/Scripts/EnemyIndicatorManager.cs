using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIndicatorManager : MonoBehaviour
{
    [Header("References")]
    public WaveManagerTMP waveManager;
    public Transform player;
    public GameObject arrowPrefab;
    public Canvas worldCanvas;

    [Header("Arrow Settings")]
    public float arrowDistance = 2.5f;
    public float fadeSpeed = 5f;
    public float spawnDelayBeforeIndicators = 1f; // seconds to wait after enemy spawns

    private List<GameObject> arrows = new List<GameObject>();
    private PlayerRoomTracker playerTracker;

    private int lastEnemyCount = 0;
    private float spawnDelayTimer = 0f;

    private void Start()
    {
        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManagerTMP>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        playerTracker = player.GetComponent<PlayerRoomTracker>();

        for (int i = 0; i < 2; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab, worldCanvas.transform);
            arrow.SetActive(false);
            arrows.Add(arrow);
        }
    }

    private void Update()
    {
        if (waveManager == null || player == null) return;

        List<GameObject> enemies = GetValidEnemies();

        // Detect changes in enemy count to reset spawn delay
        if (enemies.Count != lastEnemyCount)
        {
            spawnDelayTimer = spawnDelayBeforeIndicators;
            lastEnemyCount = enemies.Count;
        }

        // Countdown timer before showing arrows
        if (spawnDelayTimer > 0f)
        {
            spawnDelayTimer -= Time.deltaTime;
            foreach (var arrow in arrows) HideArrow(arrow);
            return;
        }

        // Show arrows for 1 or 2 enemies
        for (int i = 0; i < arrows.Count; i++)
        {
            if (i < enemies.Count)
            {
                Enemy enemy = enemies[i].GetComponent<Enemy>();
                if (enemy != null && playerTracker != null)
                {
                    bool sameRoom = (enemy.currentRoom != null &&
                                     playerTracker.currentRoom != null &&
                                     enemy.currentRoom == playerTracker.currentRoom.gameObject);

                    if (!sameRoom)
                        UpdateArrow(arrows[i], enemy.transform);
                    else
                        HideArrow(arrows[i]);
                }
            }
            else
            {
                HideArrow(arrows[i]);
            }
        }
    }

    private List<GameObject> GetValidEnemies()
    {
        var activeEnemiesField = typeof(WaveManagerTMP)
            .GetField("activeEnemies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        List<GameObject> activeEnemies = activeEnemiesField?.GetValue(waveManager) as List<GameObject>;
        List<GameObject> valid = new List<GameObject>();

        if (activeEnemies != null)
        {
            foreach (var e in activeEnemies)
            {
                if (e != null) valid.Add(e);
            }
        }

        return valid;
    }

    private void UpdateArrow(GameObject arrow, Transform target)
    {
        if (arrow == null || target == null || player == null) return;

        arrow.SetActive(true);

        Vector2 dir = (target.position - player.position).normalized;
        arrow.transform.position = player.position + (Vector3)(dir * arrowDistance);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

        Image img = arrow.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = Mathf.MoveTowards(c.a, 1f, Time.deltaTime * fadeSpeed);
            img.color = c;
        }
    }

    private void HideArrow(GameObject arrow)
    {
        if (arrow == null) return;

        Image img = arrow.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * fadeSpeed);
            img.color = c;
            if (c.a <= 0.05f)
                arrow.SetActive(false);
        }
        else
        {
            arrow.SetActive(false);
        }
    }
}
