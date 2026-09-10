using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private GameObject hitbox;
    [SerializeField] private float attackOffset = 0.05f;

    private bool isHitboxActive;
    private readonly HashSet<HealthSystem> hitTargetsInCurrentSwing = new HashSet<HealthSystem>();

    private void Start()
    {
        if (hitbox != null)
            hitbox.SetActive(false);
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
        }
    }

    public void DisableHitbox()
    {
        if (hitbox != null)
        {
            hitbox.SetActive(false);
            isHitboxActive = false;
        }
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
            }
        }
    }
}

