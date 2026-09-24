using UnityEngine;

/// <summary>
/// Dead state. Plays the Spin_Death animation.
/// </summary>
public class PlayerDeadState : IPlayerState
{
    public void Enter(Player player)
    {
        player.Animator.SetBool(AnimationHashes.IsDead, true);
        player.Animator.SetTrigger(AnimationHashes.Death);
        player.Animator.Play(AnimationHashes.SpinDeath);
    }

    public void Update(Player player) { }

    public void Exit(Player player) { }
}
