using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the universal essence bar and transformation charges.
/// Singleton — access via TransformationManager.Instance.
/// Enemies call AddEssenceOnKill() when they die, passing their EnemyConfig.
/// </summary>
public class TransformationManager : MonoBehaviour
{
    public static TransformationManager Instance { get; private set; }

    [Header("Essence Bar")]
    [SerializeField] private int maxEssence = 100;
    [SerializeField] private int transformationCost = 50;

    [Header("Essence Rewards (fallback if enemy has no EnemyEssenceReward)")]
    [SerializeField] private List<EnemyEssenceReward> essenceRewards = new List<EnemyEssenceReward>();

    private int currentEssence;
    private Dictionary<EnemyType, int> essenceRewardLookup = new Dictionary<EnemyType, int>();

    // Events
    public event System.Action<int, int> OnEssenceChanged;   // (current, max)
    public event System.Action OnTransformationReady;         // fired when essence >= cost
    public event System.Action OnTransformationConsumed;      // fired when essence is spent

    public int CurrentEssence => currentEssence;
    public int MaxEssence => maxEssence;
    public int TransformationCost => transformationCost;
    public bool CanTransform => currentEssence >= transformationCost;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build lookup dictionary from the serialized list
        foreach (var reward in essenceRewards)
        {
            if (reward != null)
                essenceRewardLookup[reward.enemyType] = reward.essenceValue;
        }

        currentEssence = PlayerProgress.GetEssenceToApply();
    }

    private void Start()
    {
        OnEssenceChanged?.Invoke(currentEssence, maxEssence);
    }

    /// <summary>
    /// Adds essence when an enemy is killed. Uses the enemy's EnemyConfig to determine the reward.
    /// </summary>
    public void AddEssenceOnKill(EnemyConfig enemyConfig)
    {
        if (enemyConfig == null) return;

        int amount = 0;

        // Try to get reward from the enemy's config first (direct reference)
        // If not found, fall back to the lookup dictionary
        if (essenceRewardLookup.TryGetValue(enemyConfig.enemyType, out int lookupValue))
        {
            amount = lookupValue;
        }
        else
        {
            // Default fallback based on type
            amount = GetDefaultEssence(enemyConfig.enemyType);
        }

        AddEssence(amount);
    }

    /// <summary>
    /// Adds a raw amount of essence to the bar.
    /// </summary>
    public void AddEssence(int amount)
    {
        if (amount <= 0) return;

        int previousEssence = currentEssence;
        currentEssence = Mathf.Min(currentEssence + amount, maxEssence);

        if (currentEssence != previousEssence)
        {
            OnEssenceChanged?.Invoke(currentEssence, maxEssence);

            if (currentEssence >= transformationCost && previousEssence < transformationCost)
                OnTransformationReady?.Invoke();
        }
    }

    /// <summary>
    /// Consumes essence for a transformation. Returns true if successful.
    /// </summary>
    public bool ConsumeTransformation()
    {
        if (!CanTransform) return false;

        currentEssence -= transformationCost;
        OnEssenceChanged?.Invoke(currentEssence, maxEssence);
        OnTransformationConsumed?.Invoke();
        return true;
    }

    /// <summary>
    /// Resets the essence bar (e.g. on scene reload).
    /// </summary>
    public void ResetEssence()
    {
        currentEssence = 0;
        OnEssenceChanged?.Invoke(currentEssence, maxEssence);
    }

    private int GetDefaultEssence(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Zombie: return 10;
            case EnemyType.Wildfire: return 25;
            case EnemyType.Slime: return 8;
            case EnemyType.Orc: return 15;
            case EnemyType.Troll: return 100;
            default: return 10;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
