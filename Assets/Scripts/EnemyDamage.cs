using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageCooldown = 1f;

    private float cooldownTimer;
    private HealthSystem enemyHealthSystem;

    private void Awake()
    {
        enemyHealthSystem = GetComponent<HealthSystem>();
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
