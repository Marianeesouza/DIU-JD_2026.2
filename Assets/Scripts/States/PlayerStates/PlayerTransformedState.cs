using UnityEngine;

/// <summary>
/// State active while the player is in a transformation form (Bat/Warg).
/// Movement, attack, and timers are still driven by Player; this state only
/// marks the transformed condition for the state machine.
/// </summary>
public class PlayerTransformedState : IPlayerState
{
    public void Enter(Player player)
    {
        // Visual/stat changes are handled by Player.ApplyForm.
    }

    public void Update(Player player)
    {
        // Transformation timer and movement handled in Player.Update/FixedUpdate.
    }

    public void Exit(Player player)
    {
        // Revert handled by Player.RevertForm.
    }
}
