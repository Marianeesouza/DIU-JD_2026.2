using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float chaseDistance = 30f;
    [SerializeField] private float attackDistance = 1f;

    [Header("Wall Avoidance")]
    [SerializeField] private float obstacleCheckDistance = 1.5f;
    [SerializeField] private LayerMask obstacleLayerMask = ~0;

    [Header("Stuck Detection")]
    [SerializeField] private float stuckTimeThreshold = 1.5f;
    [SerializeField] private float unstuckForce = 2f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private Animator animator;
    private HealthSystem healthSystem;

    private float lastMoveX;
    private float lastMoveY;
    private bool isDead;
    private bool isTakingDamage;
    private float damageAnimTimer;

    private Vector3 lastPosition;
    private float stuckTimer;
    private float currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<HealthSystem>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        lastPosition = transform.position;
        currentSpeed = chaseSpeed;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeed = chaseSpeed * multiplier;
    }

    private void OnEnable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken += OnEnemyDamageTaken;
            healthSystem.OnDeath += OnEnemyDeath;
        }
    }

    private void OnDisable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken -= OnEnemyDamageTaken;
            healthSystem.OnDeath -= OnEnemyDeath;
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        if (playerTransform == null) return;

        if (isTakingDamage)
        {
            damageAnimTimer -= Time.fixedDeltaTime;
            if (damageAnimTimer <= 0f)
                isTakingDamage = false;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= chaseDistance && distanceToPlayer > attackDistance)
        {
            ChasePlayer();
        }
        else
        {
            StopChasing();
        }

        CheckStuck();
    }

    private void Update()
    {
        if (isDead) return;

        animator.SetFloat("MoveX", lastMoveX);
        animator.SetFloat("MoveY", lastMoveY);
        animator.SetBool("IsMoving", rb.linearVelocity.sqrMagnitude > 0.01f);
    }

    private void ChasePlayer()
    {
        Vector2 desiredDirection = (playerTransform.position - transform.position).normalized;
        Vector2 finalDirection = GetAvoidedDirection(desiredDirection);

        rb.linearVelocity = finalDirection * currentSpeed;
        lastMoveX = finalDirection.x;
        lastMoveY = finalDirection.y;
    }

    private Vector2 GetAvoidedDirection(Vector2 desired)
    {
        if (!Physics2D.Raycast(transform.position, desired, obstacleCheckDistance, obstacleLayerMask))
            return desired;

        float angleStep = 30f;
        Vector2 leftDir = RotateVector2(desired, angleStep);
        Vector2 rightDir = RotateVector2(desired, -angleStep);

        bool leftBlocked = Physics2D.Raycast(transform.position, leftDir, obstacleCheckDistance, obstacleLayerMask);
        bool rightBlocked = Physics2D.Raycast(transform.position, rightDir, obstacleCheckDistance, obstacleLayerMask);

        if (!leftBlocked) return leftDir;
        if (!rightBlocked) return rightDir;

        for (float angle = angleStep * 2; angle <= 90f; angle += angleStep)
        {
            leftDir = RotateVector2(desired, angle);
            rightDir = RotateVector2(desired, -angle);

            if (!Physics2D.Raycast(transform.position, leftDir, obstacleCheckDistance, obstacleLayerMask))
                return leftDir;
            if (!Physics2D.Raycast(transform.position, rightDir, obstacleCheckDistance, obstacleLayerMask))
                return rightDir;
        }

        return Vector2.zero;
    }

    private Vector2 RotateVector2(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    private void StopChasing()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void CheckStuck()
    {
        if (isDead || isTakingDamage) return;

        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved < 0.01f)
        {
            stuckTimer += Time.fixedDeltaTime;

            if (stuckTimer >= stuckTimeThreshold)
            {
                ApplyRandomPush();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    private void ApplyRandomPush()
    {
        Vector2 randomDirection = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        rb.linearVelocity = randomDirection * unstuckForce;
        lastMoveX = randomDirection.x;
        lastMoveY = randomDirection.y;
    }

    private void OnEnemyDamageTaken(int damage)
    {
        if (isDead) return;
        isTakingDamage = true;
        damageAnimTimer = 0.5f;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Damage");
    }

    private void OnEnemyDeath()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Death");
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 1.5f);
    }
}
