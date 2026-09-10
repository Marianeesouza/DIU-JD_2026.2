

public interface IDamageable
{
    void TakeDamage(int damage);
    void Heal(int amount);
    int GetCurrentHealth();
    bool IsDead { get; }
}