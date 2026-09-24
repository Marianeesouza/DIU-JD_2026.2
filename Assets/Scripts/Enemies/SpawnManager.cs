using UnityEngine;

/// <summary>
/// Enemy spawn manager. Controls spawn interval, max limit, and gradual health
/// scaling over time. Supports multiple enemy prefab types (weighted random).
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public struct EnemySpawnEntry
    {
        public GameObject prefab;
        public int weight;
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject orcPrefab;
    [SerializeField] private EnemySpawnEntry[] enemyPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private SpawnPoint[] spawnPoints;

    [Header("Timing")]
    [SerializeField] private float baseInterval = 3f;
    [SerializeField] private float minInterval = 0.1f;
    [SerializeField] private float intervalDecreaseRate = 0.5f;

    [Header("Limits")]
    [SerializeField] private int maxEnemies = -1;

    [Header("Health Scaling")]
    [SerializeField] private float healthInterval = 30f;
    [SerializeField] private int healthIncrement = 1;

    [Header("Spawn Area")]
    [SerializeField] private float spawnRadius = 1f;

    private float timer;
    private float currentInterval;
    private int bonusHealth;
    private float sceneStartTime;
    private int totalWeight;

    private void Start()
    {
        currentInterval = baseInterval;
        timer = currentInterval;
        sceneStartTime = Time.time;
        CalculateTotalWeight();
    }

    private void CalculateTotalWeight()
    {
        totalWeight = 0;
        if (enemyPrefabs != null)
        {
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (enemyPrefabs[i].prefab != null)
                    totalWeight += Mathf.Max(1, enemyPrefabs[i].weight);
            }
        }
        if (orcPrefab != null && totalWeight == 0)
            totalWeight = 1;
    }

    private bool HasAnyPrefab()
    {
        if (orcPrefab != null) return true;
        if (enemyPrefabs != null)
        {
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (enemyPrefabs[i].prefab != null) return true;
            }
        }
        return false;
    }

    private void Update()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (!HasAnyPrefab()) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            bool spawned = false;
            if (maxEnemies < 0 || CountEnemies() < maxEnemies)
            {
                spawned = SpawnEnemy();
            }

            // Only ramp difficulty when an enemy actually spawned (no phantom ramp at cap).
            if (spawned)
            {
                currentInterval = Mathf.Max(minInterval,
                    currentInterval - intervalDecreaseRate * Time.deltaTime / 60f);
            }

            timer = currentInterval;
        }

        bonusHealth = Mathf.FloorToInt((Time.time - sceneStartTime) / healthInterval) * healthIncrement;
    }

    private GameObject PickPrefab()
    {
        // Se não houver array, usar orcPrefab como fallback
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || totalWeight == 0)
            return orcPrefab;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i].prefab == null) continue;
            cumulative += Mathf.Max(1, enemyPrefabs[i].weight);
            if (roll < cumulative)
                return enemyPrefabs[i].prefab;
        }

        // Fallback: primeiro prefab não-nulo
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i].prefab != null)
                return enemyPrefabs[i].prefab;
        }

        return orcPrefab;
    }

    private bool SpawnEnemy()
    {
        GameObject prefab = PickPrefab();
        if (prefab == null) return false;

        SpawnPoint point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 position = point.transform.position + (Vector3)offset;

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);

        HealthSystem health = enemy.GetComponent<HealthSystem>();
        if (health != null)
        {
            EnemyConfig config = prefab.GetComponent<BaseEnemy>()?.GetConfig();
            int baseHealth = config != null ? config.maxHealth : 1;
            health.MaxHealth = baseHealth + bonusHealth;
            health.Initialize();
        }
        return true;
    }

    private int CountEnemies()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}
