using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WaveManagerTMP : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject prefab;
        public int cost = 1;
        [Range(0f, 1f)] public float weight = 0.33f;
    }

    [Header("References")]
    [Tooltip("All active Spawners in the scene. Leave empty to auto-detect.")]
    public List<Spawner> spawners = new List<Spawner>();
    public TMP_Text waveText;
    public TMP_Text countdownText;

    [Header("Enemy Settings")]
    public List<EnemyType> enemyTypes = new List<EnemyType>();

    [Header("Wave Settings")]
    public int startingBudget = 10;
    public float budgetMultiplier = 1.5f;
    public float waveCooldown = 5f;
    public float spawnDelay = 0.5f;
    public float postSpawnCheckDelay = 1f;

    private int currentWave = 0;
    private int currentBudget;
    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private bool waveInProgress = false;
    private bool countdownRunning = false;

    public static bool IsBetweenWaves { get; private set; } = false;

    private static WaveManagerTMP instance;

    void Awake()
    {
        // ✅ Singleton enforcement — ensures only one runs
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Duplicate WaveManagerTMP detected, destroying this instance.");
            Destroy(gameObject);
            return;
        }
        instance = this;

        AutoDetectSpawners();
    }

    void Start()
    {
        if (waveText != null) waveText.text = "Wave 0";
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        // ✅ Start looping coroutine for waves
        StartCoroutine(WaveLoop());
    }

    void AutoDetectSpawners()
    {
#if UNITY_2023_2_OR_NEWER
        var found = Object.FindObjectsByType<Spawner>(FindObjectsSortMode.None);
#else
        var found = FindObjectsOfType<Spawner>();
#endif
        spawners.Clear();
        foreach (var s in found)
        {
            if (s == null || s.gameObject == this.gameObject) continue;
            spawners.Add(s);
        }

        Debug.Log($"WaveManager: found {spawners.Count} spawners in scene.");
    }

    IEnumerator WaveLoop()
    {
        //Run the initial countdown once at game start
        yield return StartCoroutine(InitialCountdown(waveCooldown));

        while (true) // Loop indefinitely for each wave
        {
            // 🔧 Explicitly mark combat phase start
            IsBetweenWaves = false;

            AutoDetectSpawners();
            yield return StartCoroutine(WaitUntilReady());

            StartNextWave();
            yield return new WaitUntil(() => waveInProgress == false);
            yield return new WaitForSeconds(postSpawnCheckDelay);
            yield return new WaitUntil(() => activeEnemies.Count == 0);

            // Now trigger countdown before next wave
            yield return StartCoroutine(CountdownCoroutine(waveCooldown));
        }

    }

    IEnumerator InitialCountdown(float duration)
    {
        IsBetweenWaves = true;
        countdownRunning = true; // Prevent other countdowns overlapping

        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        float remaining = duration;
        while (remaining > 0f)
        {
            if (countdownText != null)
                countdownText.text = $"Next Wave In: {Mathf.CeilToInt(remaining)}s";

            Debug.Log($"InitialCountdown: {Mathf.CeilToInt(remaining)} seconds remaining");
            yield return null;
            remaining -= Time.deltaTime;
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        Debug.Log("InitialCountdown finished.");
        countdownRunning = false;
        IsBetweenWaves = false;
    }

    IEnumerator WaitUntilReady()
    {
        float timeout = 5f;
        float timer = 0f;

        while ((spawners.Count == 0 || enemyTypes.Count == 0) && timer < timeout)
        {
            if (spawners.Count == 0)
                AutoDetectSpawners();

            timer += Time.deltaTime;
            yield return null;
        }

        if (spawners.Count == 0)
            Debug.LogWarning("WaveManager: No spawners found. Waves will not spawn until one exists in the scene.");

        if (enemyTypes.Count == 0)
            Debug.LogWarning("WaveManager: No enemy types assigned in Inspector. Add some under 'Enemy Settings'.");
    }

    IEnumerator CountdownCoroutine(float duration)
    {
        // ✅ Skip if one is already running
        if (countdownRunning) yield break;

        countdownRunning = true;

        // 🔧 Only mark between waves once enemies are really gone
        if (!waveInProgress && activeEnemies.Count == 0)
            IsBetweenWaves = true;

        Debug.Log($"CountdownCoroutine started with duration {duration} seconds.");

        float remaining = duration;
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            while (remaining > 0f)
            {
                countdownText.text = $"Next Wave In: {Mathf.CeilToInt(remaining)}s";
                Debug.Log($"CountdownCoroutine: {Mathf.CeilToInt(remaining)} seconds remaining");
                yield return null;
                remaining -= Time.deltaTime;
            }
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            while (remaining > 0f)
            {
                Debug.Log($"CountdownCoroutine (no UI): {Mathf.CeilToInt(remaining)} seconds remaining");
                yield return null;
                remaining -= Time.deltaTime;
            }
        }

        Debug.Log("CountdownCoroutine finished.");
        countdownRunning = false;
        IsBetweenWaves = false;

        // 🔧 Removed “IsBetweenWaves = false;” here — 
        // we now only set it false when a new wave actually starts
    }


    void StartNextWave()
    {
        if (waveInProgress) return;
        if (spawners.Count == 0 || enemyTypes.Count == 0)
        {
            Debug.LogWarning("Cannot start wave: missing spawners or enemy types.");
            return;
        }

        IsBetweenWaves = false;
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
        Debug.Log($"🚀 SpawnWave started for Wave {currentWave}.");

        while (currentBudget > 0)
        {
            EnemyType chosenEnemy = ChooseEnemyType();
            if (chosenEnemy == null || chosenEnemy.prefab == null) break;
            if (chosenEnemy.cost > currentBudget) break;

            Spawner chosenSpawner = GetRandomValidSpawner();
            if (chosenSpawner == null) break;

            Vector3 spawnPos = chosenSpawner.transform.position;
            spawnPos.z = 0f;

            GameObject newEnemy = Instantiate(chosenEnemy.prefab, spawnPos, chosenSpawner.transform.rotation);

            currentBudget -= chosenEnemy.cost;
            activeEnemies.Add(newEnemy);

            var deathHandler = newEnemy.AddComponent<EnemyDeathHandler>();
            deathHandler.manager = this;

            Debug.Log($"SpawnWave: Spawned {newEnemy.name} at {chosenSpawner.name}, Remaining Budget: {currentBudget}");
            yield return new WaitForSeconds(spawnDelay);
        }

        Debug.Log("SpawnWave finished spawning.");
        waveInProgress = false;
    }

    Spawner GetRandomValidSpawner()
    {
        var valid = spawners.FindAll(s => s != null && s.gameObject != this.gameObject);
        if (valid.Count == 0) return null;
        return valid[Random.Range(0, valid.Count)];
    }

    EnemyType ChooseEnemyType()
    {
        if (enemyTypes.Count == 0) return null;

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

    void Update()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
                activeEnemies.RemoveAt(i);
        }
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
    }
}

public class EnemyDeathHandler : MonoBehaviour
{
    public WaveManagerTMP manager;
    void OnDestroy()
    {
        if (manager != null)
            manager.OnEnemyDeath(gameObject);
    }
}
