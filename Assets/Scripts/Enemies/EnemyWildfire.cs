using UnityEngine;

/// <summary>
/// Wildfire (Kamikaze): chases the player, damages on contact, then dies.
/// Does NOT chase if the player is transformed (Player.IsTransformed).
/// </summary>
public class EnemyWildfire : BaseEnemy, IDetectable
{
    private float lastMoveX;
    private float lastMoveY;
    private bool isTakingDamage;
    private float damageAnimTimer;
    private bool hasCollided;

    private Player playerComponent;

    protected override void Awake()
    {
        base.Awake();
        currentSpeed = ChaseSpeed;
        currentMovementStrategy = new ChaseStrategy();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (playerTransform != null)
            playerComponent = playerTransform.GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        if (isDead || hasCollided) return;
        if (playerTransform == null) return;

        if (isTakingDamage)
        {
            damageAnimTimer -= Time.fixedDeltaTime;
            if (damageAnimTimer <= 0f)
                isTakingDamage = false;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // NÃO persegue se o jogador estiver transformado
        if (playerComponent != null && playerComponent.IsTransformed)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= ChaseDistance)
        {
            Vector2 current = transform.position;
            Vector2 target = playerTransform.position;
            Vector2 desired = currentMovementStrategy.CalculateDesiredDirection(current, target);
            Vector2 finalDir = ApplyObstacleAvoidance(desired);
            rb.linearVelocity = finalDir * currentSpeed;
            lastMoveX = finalDir.x;
            lastMoveY = finalDir.y;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || hasCollided) return;
        if (collision.gameObject.CompareTag("Player"))
            ExplodeOnPlayer(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || hasCollided) return;
        if (other.CompareTag("Player"))
            ExplodeOnPlayer(other.gameObject);
    }

    private void ExplodeOnPlayer(GameObject playerGo)
    {
        hasCollided = true;
        rb.linearVelocity = Vector2.zero;

        // Kamikaze trade: always land the hit (bypass dash i-frames / damage cooldown),
        // otherwise the wildfire would die for free.
        HealthSystem playerHealth = playerGo.GetComponent<HealthSystem>();
        if (playerHealth != null && config != null)
            playerHealth.ForceTakeDamage(config.damage);

        Die();
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
        if (TransformationManager.Instance != null)
            TransformationManager.Instance.AddEssenceOnKill(GetConfig());
        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;
        Destroy(gameObject, 1.5f);
    }

    public GameObject GetGameObject() => gameObject;
    public Vector2 GetPosition() => transform.position;
}
