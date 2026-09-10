using UnityEngine;

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

    [Header("Speed Scaling")]
    [SerializeField] private float speedGrowthRate = 0.1f;
    [SerializeField] private float maxSpeedMultiplier = 3f;

    [Header("Spawn Area")]
    [SerializeField] private float spawnRadius = 1f;

    private float timer;
    private float currentInterval;

    private void Start()
    {
        currentInterval = baseInterval;
        timer = currentInterval;
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
    }

    private void SpawnOrc()
    {
        SpawnPoint point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 position = point.transform.position + (Vector3)offset;

        GameObject orc = Instantiate(orcPrefab, position, Quaternion.identity);
        EnemyChase chase = orc.GetComponent<EnemyChase>();
        if (chase != null)
            chase.SetSpeedMultiplier(GetSpeedMultiplier());
    }

    private float GetSpeedMultiplier()
    {
        float minutes = Time.time / 60f;
        return Mathf.Min(1f + speedGrowthRate * minutes, maxSpeedMultiplier);
    }

    private int CountOrcs()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}
