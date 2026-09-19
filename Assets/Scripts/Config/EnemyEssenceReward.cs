using UnityEngine;

/// <summary>
/// ScriptableObject that defines how much essence a specific enemy type rewards when killed.
/// Create assets via menu: Game > Enemy Essence Reward.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyEssenceReward", menuName = "Game/Enemy Essence Reward")]
public class EnemyEssenceReward : ScriptableObject
{
    public EnemyType enemyType;
    [Tooltip("Amount of essence granted to the player when this enemy is killed.")]
    public int essenceValue = 10;
}
