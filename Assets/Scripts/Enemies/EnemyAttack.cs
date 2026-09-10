using UnityEngine;

/// <summary>
/// Enemy damage system to the player via trigger. Reads parameters from EnemyConfig.
/// Respects damage cooldown to prevent rapid hits.
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    private float cooldownTimer;
    private HealthSystem enemyHealthSystem;
    private EnemyConfig config;
    private int damage;
    private float damageCooldown;

    private void Awake()
    {
        enemyHealthSystem = GetComponent<HealthSystem>();

        BaseEnemy baseEnemy = GetComponent<BaseEnemy>();
        if (baseEnemy != null)
            config = baseEnemy.GetConfig();

        if (config != null)
        {
            damage = config.damage;
            damageCooldown = config.damageCooldown;
        }
        else
        {
            damage = 1;
            damageCooldown = 1f;
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (cooldownTimer > 0f) return;

        if (enemyHealthSystem != null && enemyHealthSystem.GetCurrentHealth() <= 0)
            return;

        if (other.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                cooldownTimer = damageCooldown;
            }
        }
    }
}
