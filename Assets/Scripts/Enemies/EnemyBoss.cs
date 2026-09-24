using UnityEngine;

/// <summary>
/// Troll Boss: high HP, slow, high damage. Summons 3-5 Orcs when HP drops below 50%.
/// </summary>
public class EnemyBoss : BaseEnemy, IDetectable
{
    [Header("Minion Summoning")]
    [SerializeField] private GameObject orcPrefab;
    [SerializeField] private int minOrcs = 3;
    [SerializeField] private int maxOrcs = 5;
    [SerializeField] private float summonRadius = 2f;
    [SerializeField] private float summonCooldown = 3f;

    private bool hasSummoned;
    private float summonTimer;
    private bool isTakingDamage;
    private float damageAnimTimer;
    private float lastMoveX;
    private float lastMoveY;

    protected override void Awake()
    {
        base.Awake();
        
        // Garante que a referência do HealthSystem seja pega no próprio GameObject
        if (healthSystem == null)
            healthSystem = GetComponent<HealthSystem>();

        currentSpeed = ChaseSpeed;
        currentMovementStrategy = new ChaseStrategy();
        summonTimer = summonCooldown;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // 1. Invocação de Orcs (Executa mesmo se playerTransform for nulo no momento)
        CheckSummonMinions();

        if (isTakingDamage)
        {
            damageAnimTimer -= Time.fixedDeltaTime;
            if (damageAnimTimer <= 0f)
                isTakingDamage = false;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (playerTransform == null) return;

        // Cooldown para eventuais re-invocações
        if (summonTimer > 0f)
            summonTimer -= Time.fixedDeltaTime;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= ChaseDistance && distance > AttackDistance)
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

    private void CheckSummonMinions()
    {
        if (!hasSummoned && healthSystem != null && healthSystem.GetMaxHealth() > 0)
        {
            float hpPercent = (float)healthSystem.GetCurrentHealth() / healthSystem.GetMaxHealth();
            if (hpPercent < 0.5f)
            {
                SummonOrcs();
                hasSummoned = true;
            }
        }
    }

    private void Update()
    {
        if (isDead) return;
        animator.SetFloat(AnimationHashes.MoveX, lastMoveX);
        animator.SetFloat(AnimationHashes.MoveY, lastMoveY);
        animator.SetBool(AnimationHashes.IsMoving, rb.linearVelocity.sqrMagnitude > 0.01f);
    }

    private void SummonOrcs()
    {
        if (orcPrefab == null)
        {
            Debug.LogWarning("[EnemyBoss] orcPrefab não foi atribuído no Inspector!");
            return;
        }

        int count = Random.Range(minOrcs, maxOrcs + 1);
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * summonRadius;
            Vector3 pos = transform.position + (Vector3)offset;
            GameObject orc = Instantiate(orcPrefab, pos, Quaternion.identity);

            HealthSystem orcHealth = orc.GetComponent<HealthSystem>();
            if (orcHealth != null)
                orcHealth.Initialize();
        }
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

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Chama o painel de vitória (pode usar Invoke para esperar 1.5s a animação de morte)
        Invoke(nameof(TriggerVictoryUI), 1.5f);

        Destroy(gameObject, 2f);
    }

    private void TriggerVictoryUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictory();
        }
    }

    public GameObject GetGameObject() => gameObject;
    public Vector2 GetPosition() => transform.position;
}