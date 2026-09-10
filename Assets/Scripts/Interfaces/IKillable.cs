

public interface IKillable
{
    void Die();
    event System.Action OnDeath;
}
