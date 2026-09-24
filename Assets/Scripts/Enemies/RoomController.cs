using UnityEngine;

/// <summary>
/// Tracks living enemies in the room and toggles the doors GameObject.
/// Doors stay active while any enemy is not permanently dead (Slime counts
/// until its second death). When the room is clear, doors are deactivated.
/// </summary>
public class RoomController : MonoBehaviour
{
    [Header("Doors (auto-finds tag \"Doors\" when empty)")]
    [SerializeField] private GameObject doors;

    [Header("Check Interval")]
    [SerializeField] private float checkInterval = 0.25f;

    private float timer;
    private bool doorsOpen;

    public bool IsClear { get; private set; }

    private void Awake()
    {
        if (doors == null)
            doors = GameObject.FindGameObjectWithTag("Doors");
    }

    private void Start()
    {
        EvaluateRoom();
        ApplyDoors();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = checkInterval;
        EvaluateRoom();
        ApplyDoors();
    }

    private void EvaluateRoom()
    {
        // Continuous spawner keeps the room locked until it is disabled/removed.
        SpawnManager spawner = FindAnyObjectByType<SpawnManager>(FindObjectsInactive.Include);
        if (spawner != null && spawner.isActiveAndEnabled)
        {
            IsClear = false;
            return;
        }

        bool anyAlive = false;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject go = enemies[i];
            if (go == null || !go.activeInHierarchy) continue;

            BaseCharacter character = go.GetComponent<BaseCharacter>();
            if (character == null)
            {
                anyAlive = true;
                continue;
            }

            if (!character.IsPermanentlyDead)
            {
                anyAlive = true;
                break;
            }
        }

        IsClear = !anyAlive;
    }

    private void ApplyDoors()
    {
        // Doors active = room locked. Open (deactivate) only when clear.
        doorsOpen = IsClear;
        if (doors != null)
        {
            bool wantActive = !doorsOpen;
            if (doors.activeSelf != wantActive)
                doors.SetActive(wantActive);
        }
    }
}
