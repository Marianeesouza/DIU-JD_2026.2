using UnityEngine;

/// <summary>
/// Dead state. Plays a random death animation (SpinDeath or SoulDeath).
/// </summary>
public class PlayerDeadState : IPlayerState
{
    public void Enter(Player player)
    {
        player.Animator.SetBool("IsDead", true);
        player.Animator.SetTrigger(AnimationHashes.Death);
        if (Random.Range(0, 2) == 0)
            player.Animator.Play(AnimationHashes.SpinDeath);
        else
            player.Animator.Play(AnimationHashes.SoulDeath);
    }

    public void Update(Player player) { }

    public void Exit(Player player) { }
}
