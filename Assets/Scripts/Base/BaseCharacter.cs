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

    // IKillable
    public event System.Action OnDeath;

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDeath?.Invoke();
    }
}
