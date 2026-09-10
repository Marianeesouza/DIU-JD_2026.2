using UnityEngine;

/// <summary>
/// Abstract base class for all enemies. Configures Rigidbody2D, Animator, player reference,
/// movement strategies, and obstacle avoidance via Rigidbody2D.Cast.
/// </summary>
public abstract class BaseEnemy : BaseCharacter
{
    [SerializeField] protected EnemyConfig config;

    protected Transform playerTransform;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected IMovementStrategy currentMovementStrategy;
    protected float currentSpeed;

    // Propriedades que leem do config (única fonte de verdade)
    public float ChaseSpeed => config != null ? config.chaseSpeed : 3f;
    public float ChaseDistance => config != null ? config.chaseDistance : 30f;
    public float AttackDistance => config != null ? config.attackDistance : 1f;
    public float ObstacleCheckDistance => config != null ? config.obstacleCheckDistance : 1.5f;
    public LayerMask ObstacleLayerMask => config != null ? config.obstacleLayerMask : ~0;
    public float StuckTimeThreshold => config != null ? config.stuckTimeThreshold : 1.5f;
    public float UnstuckForce => config != null ? config.unstuckForce : 2f;

    private ContactFilter2D obstacleFilter;
    private RaycastHit2D[] castHits = new RaycastHit2D[1];

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        obstacleFilter = new ContactFilter2D();
        obstacleFilter.SetLayerMask(ObstacleLayerMask);
        obstacleFilter.useTriggers = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
    }

    public void SetPlayerReference(Transform player)
    {
        playerTransform = player;
    }

    public void SetMovementStrategy(IMovementStrategy strategy)
    {
        currentMovementStrategy = strategy;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeed = ChaseSpeed * multiplier;
    }

    public EnemyConfig GetConfig()
    {
        return config;
    }

    protected Vector2 ApplyObstacleAvoidance(Vector2 desired)
    {
        int count = rb.Cast(desired, obstacleFilter, castHits, ObstacleCheckDistance);
        if (count == 0)
            return desired;

        float angleStep = 30f;
        Vector2 leftDir = RotateVector2(desired, angleStep);
        Vector2 rightDir = RotateVector2(desired, -angleStep);

        int leftCount = rb.Cast(leftDir, obstacleFilter, castHits, ObstacleCheckDistance);
        int rightCount = rb.Cast(rightDir, obstacleFilter, castHits, ObstacleCheckDistance);

        if (leftCount == 0) return leftDir;
        if (rightCount == 0) return rightDir;

        for (float angle = angleStep * 2; angle <= 90f; angle += angleStep)
        {
            leftDir = RotateVector2(desired, angle);
            rightDir = RotateVector2(desired, -angle);

            leftCount = rb.Cast(leftDir, obstacleFilter, castHits, ObstacleCheckDistance);
            rightCount = rb.Cast(rightDir, obstacleFilter, castHits, ObstacleCheckDistance);

            if (leftCount == 0)
                return leftDir;
            if (rightCount == 0)
                return rightDir;
        }

        return Vector2.zero;
    }

    protected Vector2 RotateVector2(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
