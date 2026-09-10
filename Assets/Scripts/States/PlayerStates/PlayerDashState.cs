using UnityEngine;

/// <summary>
/// Dash state. Duration managed by a timer in Player.cs.
/// </summary>
public class PlayerDashState : IPlayerState
{
    public void Enter(Player player)
    {
        player.Animator.SetBool(AnimationHashes.IsDashing, true);
        player.Animator.SetTrigger(AnimationHashes.Dash);
    }

    public void Update(Player player)
    {
        // Dash duration is managed by Player.Update() timer
    }

    public void Exit(Player player)
    {
        player.Animator.SetBool(AnimationHashes.IsDashing, false);
    }
}
