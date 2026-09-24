using UnityEngine;

/// <summary>
/// Cross-scene player progress (HP, essence) and per-room checkpoint.
/// Static so it survives LoadScene without DontDestroyOnLoad.
/// Cleared on Start/Main Menu; restored on Retry.
/// </summary>
public static class PlayerProgress
{
    public static bool HasProgress { get; private set; }
    public static int CurrentHealth { get; private set; }
    public static int MaxHealth { get; private set; }
    public static int Essence { get; private set; }

    public static bool HasCheckpoint { get; private set; }
    public static int CheckpointHealth { get; private set; }
    public static int CheckpointEssence { get; private set; }

    /// <summary>Snapshots live HP/essence before loading the next scene.</summary>
    public static void Capture()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        HealthSystem hs = player != null ? player.GetComponent<HealthSystem>() : null;
        if (hs != null)
        {
            CurrentHealth = hs.GetCurrentHealth();
            MaxHealth = hs.GetMaxHealth();
        }

        TransformationManager tm = TransformationManager.Instance;
        Essence = tm != null ? tm.CurrentEssence : 0;

        HasProgress = true;
    }

    /// <summary>Applies persisted HP to the player's HealthSystem (call after init).</summary>
    public static void ApplyHealth(HealthSystem hs)
    {
        if (!HasProgress || hs == null) return;
        hs.SetCurrentHealth(CurrentHealth);
    }

    /// <summary>Returns essence to restore, or 0 when starting fresh.</summary>
    public static int GetEssenceToApply()
    {
        return HasProgress ? Essence : 0;
    }

    /// <summary>Saves the current scene-entry state (used as Retry restore point).</summary>
    public static void SaveCheckpoint(int health, int essence)
    {
        HasCheckpoint = true;
        CheckpointHealth = health;
        CheckpointEssence = essence;
    }

    /// <summary>Restores live progress from the scene-entry checkpoint (Retry).</summary>
    public static void RestoreCheckpoint()
    {
        if (!HasCheckpoint) return;
        CurrentHealth = CheckpointHealth;
        Essence = CheckpointEssence;
        HasProgress = true;
    }

    /// <summary>Full reset (Start / Main Menu).</summary>
    public static void Clear()
    {
        HasProgress = false;
        CurrentHealth = 0;
        MaxHealth = 0;
        Essence = 0;
        HasCheckpoint = false;
        CheckpointHealth = 0;
        CheckpointEssence = 0;
    }
}
