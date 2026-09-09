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

        Instantiate(orcPrefab, position, Quaternion.identity);
    }

    private int CountOrcs()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}
