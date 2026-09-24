using UnityEngine;

/// <summary>
/// Zombie/Skeleton: idle until the player is within detection range, then
/// permanently awakens and chases. Uses EnemyConfig for speed/damage/distances.
/// </summary>
public class EnemyZombie : BaseEnemy, IDetectable
{
    private float lastMoveX;
    private float lastMoveY;
    private bool isTakingDamage;
    private float damageAnimTimer;
    private bool isChasing;
    private bool isAwake;

    private Vector3 lastPosition;
    private float stuckTimer;

    protected override void Awake()
    {
        base.Awake();
        lastPosition = transform.position;
        currentSpeed = ChaseSpeed;
        currentMovementStrategy = new ChaseStrategy();
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

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (!isAwake)
        {
            rb.linearVelocity = Vector2.zero;
            if (distance <= ChaseDistance)
                isAwake = true;
            else
                return;
        }

        if (distance > AttackDistance)
        {
            if (!isChasing)
            {
                currentMovementStrategy = new ChaseStrategy();
                isChasing = true;
            }
            ChasePlayer();
            CheckStuck();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
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
        Vector2 desired = currentMovementStrategy.CalculateDesiredDirection(current, target);
        Vector2 finalDir = ApplyObstacleAvoidance(desired);
        rb.linearVelocity = finalDir * currentSpeed;
        lastMoveX = finalDir.x;
        lastMoveY = finalDir.y;
    }

    private void CheckStuck()
    {
        if (isDead || isTakingDamage) return;
        float moved = Vector3.Distance(transform.position, lastPosition);
        if (moved < 0.01f)
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
        Vector2 dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        rb.linearVelocity = dir * UnstuckForce;
        lastMoveX = dir.x;
        lastMoveY = dir.y;
    }

    protected override void HandleDamageTaken(int damage)
    {
        if (isDead) return;
        isAwake = true;
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
        if (TransformationManager.Instance != null)
            TransformationManager.Instance.AddEssenceOnKill(GetConfig());
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        Destroy(gameObject, 1.5f);
    }

    public GameObject GetGameObject() => gameObject;
    public Vector2 GetPosition() => transform.position;
}
