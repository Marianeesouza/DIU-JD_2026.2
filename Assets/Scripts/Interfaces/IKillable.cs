using UnityEngine;

/// <summary>
/// Interface for entities that can be killed. Exposes an OnDeath event.
/// </summary>
public interface IKillable
{
    void Die();
    event System.Action OnDeath;
}
