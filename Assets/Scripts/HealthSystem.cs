using UnityEngine;

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

    public int MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }

    private bool isDead;
    private float damageCooldown;
    private const float DAMAGE_COOLDOWN = 0.5f;

    private void Awake()
    {
        currentHealth = maxHealth;
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

        currentHealth = Mathf.Max(0, currentHealth - damage);
        damageCooldown = DAMAGE_COOLDOWN;
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
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

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
