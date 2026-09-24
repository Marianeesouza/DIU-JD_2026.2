using UnityEngine;

/// <summary>
/// Generic health system for all entities. Implements IDamageable and IKillable.
/// Manages damage, healing, death, and temporary invulnerability.
/// </summary>
public class HealthSystem : MonoBehaviour, IDamageable, IKillable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;
    public System.Action<int> OnDamageTaken;
    public System.Action<int, int> OnHealthChanged;
    public event System.Action OnDeath;

    public bool IsInvulnerable { get; set; }
    public bool IsDead => isDead;

    /// <summary>Multiplier applied to incoming damage (e.g. 1.2 = +20% damage taken).</summary>
    public float IncomingDamageMultiplier { get; set; } = 1f;

    public int MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }

    private bool isDead;
    private float damageCooldown;
    private float damageCooldownDuration = DAMAGE_COOLDOWN;
    private const float DAMAGE_COOLDOWN = 0.5f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>Sets the i-frame window after taking damage (wired from EnemyConfig.damageImmunityDuration).</summary>
    public void SetDamageCooldown(float duration)
    {
        damageCooldownDuration = Mathf.Max(0f, duration);
    }

    public void Initialize()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || IsInvulnerable) return;
        if (damageCooldown > 0f) return;

        int applied = Mathf.Max(1, Mathf.RoundToInt(damage * IncomingDamageMultiplier));
        currentHealth = Mathf.Max(0, currentHealth - applied);
        damageCooldown = damageCooldownDuration;
        OnDamageTaken?.Invoke(applied);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Applies damage ignoring i-frames and the post-hit cooldown (e.g. kamikaze explosion).
    /// Still respects death.
    /// </summary>
    public void ForceTakeDamage(int damage)
    {
        if (isDead) return;

        IsInvulnerable = false;
        damageCooldown = 0f;

        int applied = Mathf.Max(1, Mathf.RoundToInt(damage * IncomingDamageMultiplier));
        currentHealth = Mathf.Max(0, currentHealth - applied);
        damageCooldown = damageCooldownDuration;
        OnDamageTaken?.Invoke(applied);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Update()
    {
        if (damageCooldown > 0f)
            damageCooldown -= Time.deltaTime;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Sets max health and clamps current health if it exceeds the new maximum.
    /// </summary>
    public void SetMaxHealth(int newMax)
    {
        maxHealth = Mathf.Max(1, newMax);
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Directly sets current health, clamped to [0, maxHealth].
    /// Used to restore HP lost to max-health clamps (e.g. transformation cycles).
    /// </summary>
    public void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
