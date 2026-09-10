using UnityEngine;

/// <summary>
/// ScriptableObject configuration for enemies. Single source of truth for health,
/// speed, chase/attack distances, damage, and obstacle avoidance parameters.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Game/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 1;

    [Header("Movement")]
    public float chaseSpeed = 1f;
    public float chaseDistance = 30f;
    public float attackDistance = 0.5f;

    [Header("Obstacle Avoidance")]
    public float obstacleCheckDistance = 1.5f;
    public LayerMask obstacleLayerMask = ~0;

    [Header("Stuck Detection")]
    public float stuckTimeThreshold = 1.5f;
    public float unstuckForce = 2f;

    [Header("Damage")]
    public int damage = 1;
    public float damageCooldown = 1f;

    [Header("Defense")]
    public float damageImmunityDuration = 0.5f;
}
