using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player attack system using a hitbox (trigger). Applies damage to enemies
/// with a HashSet to prevent duplicate hits per swing.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private GameObject hitbox;
    [SerializeField] private float attackOffset = 0.05f;

    [Header("Hit Window")]
    [SerializeField] private float hitboxActiveDuration = 0.15f;

    private bool isHitboxActive;
    private float hitboxActiveTimer;
    private int baseAttackDamage;
    private float baseAttackRange;
    private readonly HashSet<HealthSystem> hitTargetsInCurrentSwing = new HashSet<HealthSystem>();

    /// <summary>Dynamic attack damage (modified by transformations).</summary>
    public int AttackDamage
    {
        get => attackDamage;
        set => attackDamage = Mathf.Max(1, value);
    }

    /// <summary>Dynamic attack range (modified by transformations).</summary>
    public float AttackRange
    {
        get => attackRange;
        set => attackRange = Mathf.Max(0.1f, value);
    }

    /// <summary>Impulse force applied to enemies on hit (Warg leap). 0 = no knockback.</summary>
    public float KnockbackForce { get; set; }

    private void Start()
    {
        baseAttackDamage = attackDamage;
        baseAttackRange = attackRange;
        if (hitbox != null)
            hitbox.SetActive(false);
    }

    private void Update()
    {
        if (!isHitboxActive) return;

        hitboxActiveTimer -= Time.deltaTime;
        if (hitboxActiveTimer <= 0f)
            ForceDisableHitbox();
    }

    /// <summary>Restores damage/range to the values configured in the Inspector.</summary>
    public void ResetToBaseAttack()
    {
        attackDamage = baseAttackDamage;
        attackRange = baseAttackRange;
        KnockbackForce = 0f;
    }

    public void SetAttackDirection(Vector2 direction)
    {
        if (hitbox == null) return;

        if (direction.sqrMagnitude > 0.001f)
        {
            Vector2 norm = direction.normalized;
            hitbox.transform.localPosition = norm * attackOffset;
        }
    }

    public void EnableHitbox()
    {
        hitTargetsInCurrentSwing.Clear();
        if (hitbox != null)
        {
            hitbox.SetActive(true);
            isHitboxActive = true;
            hitboxActiveTimer = hitboxActiveDuration;
        }
    }

    /// <summary>
    /// Animation Event / OnAttackEnd path: does NOT cut the hit window short.
    /// The timer in Update owns the window length.
    /// </summary>
    public void DisableHitbox()
    {
        if (isHitboxActive && hitboxActiveTimer > 0f)
            return;
        ForceDisableHitbox();
    }

    /// <summary>
    /// Immediate close (player damaged, form swap, death) — ignores the timer.
    /// </summary>
    public void CancelHitbox()
    {
        ForceDisableHitbox();
    }

    private void ForceDisableHitbox()
    {
        if (hitbox != null)
            hitbox.SetActive(false);
        isHitboxActive = false;
        hitboxActiveTimer = 0f;
        hitTargetsInCurrentSwing.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        ProcessHit(other);
    }

    private void ProcessHit(Collider2D other)
    {
        if (!isHitboxActive) return;

        if (other.CompareTag("Enemy"))
        {
            HealthSystem enemyHealth = other.GetComponent<HealthSystem>();
            if (enemyHealth != null && !hitTargetsInCurrentSwing.Contains(enemyHealth))
            {
                hitTargetsInCurrentSwing.Add(enemyHealth);
                enemyHealth.TakeDamage(attackDamage);

                if (KnockbackForce > 0f)
                {
                    Rigidbody2D enemyRb = other.attachedRigidbody;
                    if (enemyRb == null)
                        enemyRb = other.GetComponentInParent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        Vector2 dir = ((Vector2)enemyRb.transform.position - (Vector2)transform.position).normalized;
                        if (dir.sqrMagnitude < 0.001f)
                            dir = Vector2.up;
                        enemyRb.AddForce(dir * KnockbackForce, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
}

