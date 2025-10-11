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

    // Wave number UI (assign in Inspector)
    public TMP_Text waveText;

    // New: separate countdown UI element (assign in Inspector). Only shown between waves.
    public TMP_Text countdownText;

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

    // Public read-only state for other systems to check whether we are in the between-waves countdown
    public static bool IsBetweenWaves { get; private set; } = false;

    // Tracks number spawned in the current wave (reset each wave)
    private int spawnedThisWave = 0;

    void Awake()
    {
        // Auto-detect all spawners in the scene if none are manually assigned,
        // but exclude any Spawner that is attached to the same GameObject as this WaveManager.
        if (spawners.Count == 0)
        {
#if UNITY_2023_2_OR_NEWER
            var found = Object.FindObjectsByType<Spawner>(FindObjectsSortMode.None);
#else
            var found = FindObjectsOfType<Spawner>();
#endif
            foreach (var s in found)
            {
                if (s == null) continue;
                if (s.gameObject == this.gameObject) continue; // exclude self
                spawners.Add(s);
            }
            Debug.Log($"WaveManager found {spawners.Count} spawners automatically (self excluded).");
        }
    }

    // Start now defers initialization until spawners are present (or timeout)
    void Start()
    {
        StartCoroutine(InitializeAndRun());
    }

    // Waits briefly for spawners to be assigned (inspector or runtime), then starts wave loop.
    IEnumerator InitializeAndRun()
    {
        // small grace period to allow other systems to populate spawners (e.g. scene setup)
        const float waitTimeout = 2f; // seconds to wait for spawners to appear
        float waited = 0f;

        // If none assigned, try to auto-detect repeatedly for up to timeout
        while (spawners.Count == 0 && waited < waitTimeout)
        {
#if UNITY_2023_2_OR_NEWER
            var found = Object.FindObjectsByType<Spawner>(FindObjectsSortMode.None);
#else
            var found = FindObjectsOfType<Spawner>();
#endif
            spawners.Clear();
            foreach (var s in found)
            {
                if (s == null) continue;
                if (s.gameObject == this.gameObject) continue;
                spawners.Add(s);
            }

            if (spawners.Count > 0) break;

            yield return null;
            waited += Time.deltaTime;
        }

        if (spawners.Count == 0)
            Debug.LogWarning("WaveManager: No spawners found after waiting. Assign spawners in Inspector or ensure they exist at start.");

        // UI init (was previously in Start)
        if (waveText != null)
            waveText.text = "Wave 0";

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // Start wave loop after ensuring we attempted auto-detect
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            // Always run the between-wave countdown BEFORE starting the next wave.
            // This enforces the same 30s wait before the first wave and between waves,
            // and ensures SpawnWave does not start until the countdown finishes.
            yield return StartCoroutine(CountdownCoroutine(waveCooldown));

            // Start the next wave immediately after countdown finishes 
            StartNextWave();

            // Wait until spawn phase finishes (SpawnWave sets waveInProgress = false when done)
            yield return new WaitUntil(() => waveInProgress == false);

            // If no enemies were spawned this wave, log and run the countdown (no enemies to kill)
            if (spawnedThisWave == 0)
            {
                Debug.Log("Wave completed with zero spawns. Starting countdown.");
                yield return StartCoroutine(CountdownCoroutine(waveCooldown));
                continue;
            }

            // Wait until all enemies spawned for this wave are dead
            yield return new WaitUntil(() => activeEnemies.Count == 0);

            // After the wave is cleared, show countdown UI and wait the cooldown
            yield return StartCoroutine(CountdownCoroutine(waveCooldown));
        }
    }

    IEnumerator CountdownCoroutine(float duration)
    {
        // mark between-waves state for other systems
        IsBetweenWaves = true;

        if (countdownText == null)
        {
            // fallback: just wait if no UI assigned, but still set the flag while waiting
            yield return new WaitForSeconds(duration);
            IsBetweenWaves = false;
            yield break;
        }

        countdownText.gameObject.SetActive(true);
        float remaining = duration;

        // Update each frame for smooth/accurate countdown
        while (remaining > 0f)
        {
            int seconds = Mathf.CeilToInt(remaining);
            countdownText.text = $"Next Wave In: {seconds}s";
            yield return null;
            remaining -= Time.deltaTime;
        }

        countdownText.gameObject.SetActive(false);
        IsBetweenWaves = false;
    }

    void StartNextWave()
    {
        // prevent starting another wave while one is already in progress
        if (waveInProgress) return;

        // ensure we are not considered between waves when a wave starts
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
        spawnedThisWave = 0; // reset counter

        while (currentBudget > 0)
        {
            if (enemyTypes.Count == 0 || spawners.Count == 0)
            {
                Debug.LogWarning("SpawnWave aborted: no enemyTypes or no spawners assigned.");
                break;
            }

            EnemyType chosenEnemy = ChooseEnemyType();
            if (chosenEnemy == null || chosenEnemy.prefab == null)
            {
                Debug.LogWarning("SpawnWave aborted: chosen enemy invalid.");
                break;
            }

            if (chosenEnemy.cost > currentBudget)
            {
                // no affordable enemy left
                break;
            }

            Spawner chosenSpawner = GetRandomValidSpawner();
            if (chosenSpawner == null)
            {
                Debug.LogWarning("No valid spawner available to spawn enemies.");
                break;
            }

            GameObject newEnemy = Instantiate(chosenEnemy.prefab, chosenSpawner.transform.position, chosenSpawner.transform.rotation);
            Debug.Log($"Spawned enemy '{newEnemy.name}' instanceID={newEnemy.GetInstanceID()} at {Time.time}", newEnemy);
            currentBudget -= chosenEnemy.cost;

            // Track enemy
            activeEnemies.Add(newEnemy);
            spawnedThisWave++;
            EnemyDeathHandler deathHandler = newEnemy.AddComponent<EnemyDeathHandler>();
            deathHandler.manager = this;

            yield return new WaitForSeconds(spawnDelay);
        }

        waveInProgress = false;
    }

    // Helper: returns a random spawner that is not on the same GameObject as this manager
    Spawner GetRandomValidSpawner()
    {
        var valid = spawners.FindAll(s => s != null && s.gameObject != this.gameObject);
        if (valid.Count == 0) return null;
        return valid[Random.Range(0, valid.Count)];
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

public class EnemyDeathHandler : MonoBehaviour
{
    public WaveManagerTMP manager;

    void OnDestroy()
    {
        if (manager != null)
            manager.OnEnemyDeath(gameObject);
    }
}
