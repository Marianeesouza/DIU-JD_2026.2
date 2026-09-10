using UnityEngine;

/// <summary>
/// Interface for entities that can receive damage and be healed.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int damage);
    void Heal(int amount);
    int GetCurrentHealth();
    bool IsDead { get; }
}