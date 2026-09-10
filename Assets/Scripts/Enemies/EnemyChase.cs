using UnityEngine;

public class EnemyChase : BaseEnemy, IDetectable
{
    private float lastMoveX;
    private float lastMoveY;
    private bool isTakingDamage;
    private float damageAnimTimer;

    private Vector3 lastPosition;
    private float stuckTimer;

    protected override void Awake()
    {
        base.Awake();
        lastPosition = transform.position;
        currentSpeed = ChaseSpeed;
        currentMovementStrategy = new ChaseStrategy();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
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

        if (distanceToPlayer <= ChaseDistance && distanceToPlayer > AttackDistance)
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

        animator.SetFloat(AnimationHashes.MoveX, lastMoveX);
        animator.SetFloat(AnimationHashes.MoveY, lastMoveY);
        animator.SetBool(AnimationHashes.IsMoving, rb.linearVelocity.sqrMagnitude > 0.01f);
    }

    private void ChasePlayer()
    {
        Vector2 current = transform.position;
        Vector2 target = playerTransform.position;

        Vector2 desiredDirection = currentMovementStrategy.CalculateDesiredDirection(current, target);
        Vector2 finalDirection = ApplyObstacleAvoidance(desiredDirection);

        rb.linearVelocity = finalDirection * currentSpeed;
        lastMoveX = finalDirection.x;
        lastMoveY = finalDirection.y;
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

            if (stuckTimer >= StuckTimeThreshold)
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

        rb.linearVelocity = randomDirection * UnstuckForce;
        lastMoveX = randomDirection.x;
        lastMoveY = randomDirection.y;
    }

    protected override void HandleDamageTaken(int damage)
    {
        if (isDead) return;
        isTakingDamage = true;
        damageAnimTimer = 0.5f;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger(AnimationHashes.Damage);
    }

    protected override void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger(AnimationHashes.Death);
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 1.5f);
    }

    public GameObject GetGameObject() => gameObject;
    public Vector2 GetPosition() => transform.position;
}
