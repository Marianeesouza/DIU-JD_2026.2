using UnityEngine;

/// <summary>
/// Abstract base class for all characters (player and enemies).
/// Implements IDamageable and IKillable, delegating to HealthSystem.
/// </summary>
public abstract class BaseCharacter : MonoBehaviour, IDamageable, IKillable
{
    [SerializeField] protected int maxHealth = 100;
    protected HealthSystem healthSystem;
    protected bool isDead;

    protected virtual void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        // Apply the Inspector maxHealth to the HealthSystem (previously ignored).
        // Initialize() also syncs currentHealth regardless of Awake execution order.
        if (healthSystem != null && maxHealth > 0)
        {
            healthSystem.MaxHealth = maxHealth;
            healthSystem.Initialize();
        }
    }

    protected virtual void OnEnable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken += HandleDamageTaken;
            healthSystem.OnDeath += HandleDeath;
        }
    }

    protected virtual void OnDisable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken -= HandleDamageTaken;
            healthSystem.OnDeath -= HandleDeath;
        }
    }

    protected abstract void HandleDamageTaken(int damage);
    protected abstract void HandleDeath();

    // IDamageable
    public void TakeDamage(int damage) => healthSystem?.TakeDamage(damage);
    public void Heal(int amount) => healthSystem?.Heal(amount);
    public int GetCurrentHealth() => healthSystem?.GetCurrentHealth() ?? 0;
    public bool IsDead => isDead;

    /// <summary>
    /// True when the character is dead and will not come back (room-clear check).
    /// Slimes override: first death is temporary until they respawn and die again.
    /// </summary>
    public virtual bool IsPermanentlyDead => isDead;

    // IKillable
    public event System.Action OnDeath;

    public void Die()
    {
        if (isDead) return;
        if (healthSystem != null && !healthSystem.IsDead)
        {
            healthSystem.Die();
            if (isDead)
            {
                OnDeath?.Invoke();
                return;
            }
        }
        HandleDeath();
        OnDeath?.Invoke();
    }
}
