using UnityEngine;

/// <summary>
/// Enemy spawn manager. Controls spawn interval, max limit, and gradual health
/// scaling over time (increases by 1 every 30 seconds).
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject orcPrefab;

    [Header("Spawn Points")]
    [SerializeField] private SpawnPoint[] spawnPoints;

    [Header("Timing")]
    [SerializeField] private float baseInterval = 3f;
    [SerializeField] private float minInterval = 0.1f;
    [SerializeField] private float intervalDecreaseRate = 0.5f;

    [Header("Limits")]
    [SerializeField] private int maxOrcs = -1;

    [Header("Health Scaling")]
    [SerializeField] private float healthInterval = 30f;
    [SerializeField] private int healthIncrement = 1;

    [Header("Spawn Area")]
    [SerializeField] private float spawnRadius = 1f;

    private float timer;
    private float currentInterval;
    private int bonusHealth;
    private float sceneStartTime;

    private void Start()
    {
        currentInterval = baseInterval;
        timer = currentInterval;
        sceneStartTime = Time.time;
    }

    private void Update()
    {
        if (spawnPoints.Length == 0 || orcPrefab == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (maxOrcs < 0 || CountOrcs() < maxOrcs)
            {
                SpawnOrc();
            }

            currentInterval = Mathf.Max(minInterval,
                currentInterval - intervalDecreaseRate * Time.deltaTime / 60f);

            timer = currentInterval;
        }

        bonusHealth = Mathf.FloorToInt((Time.time - sceneStartTime) / healthInterval) * healthIncrement;
    }

    private void SpawnOrc()
    {
        SpawnPoint point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 position = point.transform.position + (Vector3)offset;

        GameObject orc = Instantiate(orcPrefab, position, Quaternion.identity);

        HealthSystem health = orc.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.MaxHealth = GetTotalHealth();
            health.Initialize();
        }
    }

    private int GetTotalHealth()
    {
        EnemyConfig config = orcPrefab.GetComponent<BaseEnemy>()?.GetConfig();
        int baseHealth = config != null ? config.maxHealth : 1;
        return baseHealth + bonusHealth;
    }

    private int CountOrcs()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}
