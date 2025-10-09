using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;       // Enemy prefab to spawn
    public float spawnInterval = 2f;     // Time between spawns
    public int maxEnemies = 10;          // Maximum enemies allowed to spawn (-1 = infinite)
    public bool randomizeSpawn = false;  // If true, adds random delay variation
    public float randomDelay = 1f;       // Max random delay variation

    private int enemiesSpawned = 0;
    private float nextSpawnTime = 0f;

    //void Start()
    //{
    //    ScheduleNextSpawn();
    //}

    //void Update()
    //{
    //    if (Time.time >= nextSpawnTime)
     //   {
    //        SpawnEnemy();
     //       ScheduleNextSpawn();
     //   }
   // }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Spawner has no enemyPrefab assigned!", this);
            return;
        }

        // Stop if max enemies reached (unless infinite)
        if (maxEnemies > 0 && enemiesSpawned >= maxEnemies)
            return;

        Instantiate(enemyPrefab, transform.position, transform.rotation);
        enemiesSpawned++;
    }

    void ScheduleNextSpawn()
    {
        float delay = spawnInterval;

        if (randomizeSpawn)
        {
            delay += Random.Range(-randomDelay, randomDelay);
            delay = Mathf.Max(0.1f, delay); // Prevent negative or zero delays
        }

        nextSpawnTime = Time.time + delay;
    }
}
