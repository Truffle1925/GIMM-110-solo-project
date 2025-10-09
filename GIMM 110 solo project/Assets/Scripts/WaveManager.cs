using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WaveManagerTMP : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject prefab;  // Enemy prefab
        public int cost = 1;       // Cost from the wave budget
        [Range(0f, 1f)] public float weight = 0.33f; // Relative spawn chance
    }

    [Header("References")]
    [Tooltip("All active Spawners in the scene. Leave empty to auto-detect.")]
    public List<Spawner> spawners = new List<Spawner>();

    public TMP_Text waveText;              // TextMeshPro UI element (assign in Inspector)

    [Header("Enemy Settings")]
    public List<EnemyType> enemyTypes = new List<EnemyType>();

    [Header("Wave Settings")]
    public int startingBudget = 10;        // Starting enemy point budget
    public float budgetMultiplier = 1.5f;  // Budget growth per wave
    public float waveCooldown = 30f;       // Delay between waves
    public float spawnDelay = 0.5f;        // Time between each enemy spawn (per wave)

    private int currentWave = 0;
    private int currentBudget;
    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private bool waveInProgress = false;

    void Awake()
    {
        // Auto-detect all spawners in the scene if none are manually assigned
        if (spawners.Count == 0)
        {
            spawners.AddRange(FindObjectsOfType<Spawner>());
            Debug.Log($"WaveManager found {spawners.Count} spawners automatically.");
        }
    }

    void Start()
    {
        if (waveText != null)
            waveText.text = "Wave 0";
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            // Wait before starting the next wave
            if (currentWave > 0)
            {
                Debug.Log($"Wave {currentWave} complete! Next wave in {waveCooldown} seconds...");
                if (waveText != null)
                    waveText.text = $"Next Wave In: {waveCooldown}s";

                yield return new WaitForSeconds(waveCooldown);
            }

            StartNextWave();

            // Wait until all enemies in this wave are dead
            yield return new WaitUntil(() => activeEnemies.Count == 0);
        }
    }

    void StartNextWave()
    {
        currentWave++;
        currentBudget = Mathf.RoundToInt(startingBudget * Mathf.Pow(budgetMultiplier, currentWave - 1));
        Debug.Log($"Starting Wave {currentWave} with budget {currentBudget}");

        if (waveText != null)
            waveText.text = $"Wave {currentWave}";

        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        waveInProgress = true;

        while (currentBudget > 0)
        {
            if (enemyTypes.Count == 0 || spawners.Count == 0)
                yield break;

            // Choose random enemy and spawner
            EnemyType chosenEnemy = ChooseEnemyType();
            if (chosenEnemy == null || chosenEnemy.prefab == null)
                yield break;

            if (chosenEnemy.cost > currentBudget)
                break;

            Spawner chosenSpawner = spawners[Random.Range(0, spawners.Count)];

            // Spawn enemy
            GameObject newEnemy = Instantiate(chosenEnemy.prefab, chosenSpawner.transform.position, chosenSpawner.transform.rotation);
            currentBudget -= chosenEnemy.cost;

            // Track enemy
            activeEnemies.Add(newEnemy);
            EnemyDeathHandler deathHandler = newEnemy.AddComponent<EnemyDeathHandler>();
            deathHandler.manager = this;

            yield return new WaitForSeconds(spawnDelay);
        }

        waveInProgress = false;
    }

    EnemyType ChooseEnemyType()
    {
        float totalWeight = 0f;
        foreach (var e in enemyTypes)
            totalWeight += e.weight;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var e in enemyTypes)
        {
            cumulative += e.weight;
            if (roll <= cumulative)
                return e;
        }

        return enemyTypes[enemyTypes.Count - 1];
    }

    public void OnEnemyDeath(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
    }
}

/// <summary>
/// Helper that reports to WaveManager when an enemy dies.
/// You can replace this with a call in your enemy's health script.
/// </summary>
public class EnemyDeathHandler : MonoBehaviour
{
    public WaveManagerTMP manager;

    void OnDestroy()
    {
        if (manager != null)
            manager.OnEnemyDeath(gameObject);
    }
}
