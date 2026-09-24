using System.Collections;
using UnityEngine;

/// <summary>
/// Slime: slow wander, contact damage via EnemyAttack.
/// First death: respawns once at the death location after a delay.
/// Second death: permanent (no more respawn).
/// </summary>
public class EnemySlime : BaseEnemy, IDetectable
{
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 5f;

    private float lastMoveX;
    private float lastMoveY;
    private bool isTakingDamage;
    private float damageAnimTimer;
    private Vector3 deathPosition;
    private bool hasRespawned;

    private bool isRespawning;

    /// <summary>Slime only counts as permanently dead after the second death.</summary>
    public override bool IsPermanentlyDead => hasRespawned && isDead;

    protected override void Awake()
    {
        base.Awake();
        currentSpeed = ChaseSpeed * 0.5f; // wander lento
        currentMovementStrategy = new WanderStrategy();
    }

    private void FixedUpdate()
    {
        if (isDead || isRespawning) return;

        if (isTakingDamage)
        {
            damageAnimTimer -= Time.fixedDeltaTime;
            if (damageAnimTimer <= 0f)
                isTakingDamage = false;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Slime apenas faz wander (não persegue)
        Vector2 current = transform.position;
        Vector2 desired = currentMovementStrategy.CalculateDesiredDirection(current, current);

        if (desired.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 finalDir = ApplyObstacleAvoidance(desired);
        rb.linearVelocity = finalDir * currentSpeed;
        lastMoveX = finalDir.x;
        lastMoveY = finalDir.y;
    }

    private void Update()
    {
        if (isDead) return;
        animator.SetFloat(AnimationHashes.MoveX, lastMoveX);
        animator.SetFloat(AnimationHashes.MoveY, lastMoveY);
        animator.SetBool(AnimationHashes.IsMoving, rb.linearVelocity.sqrMagnitude > 0.01f);
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
        deathPosition = transform.position;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger(AnimationHashes.Death);
        if (TransformationManager.Instance != null)
            TransformationManager.Instance.AddEssenceOnKill(GetConfig());
        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        // Only the first death schedules a respawn; the second is permanent.
        if (!hasRespawned)
            StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);

        // Reappear where the first death happened (not the original spawn).
        transform.position = deathPosition;
        hasRespawned = true;

        // Reativa
        isDead = false;
        isRespawning = false;
        isTakingDamage = false;
        currentMovementStrategy = new WanderStrategy();

        if (healthSystem != null)
            healthSystem.Initialize();

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = true;

        // Reset animator so the slime doesn't stay stuck in the death state/pose.
        if (animator != null)
        {
            animator.ResetTrigger(AnimationHashes.Death);
            animator.ResetTrigger(AnimationHashes.Damage);
            animator.Rebind();
            animator.Update(0f);
        }

        gameObject.SetActive(true);
    }

    public GameObject GetGameObject() => gameObject;
    public Vector2 GetPosition() => transform.position;
}
