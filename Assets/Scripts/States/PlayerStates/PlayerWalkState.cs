using UnityEngine;

/// <summary>
/// Walk state. Transitions to Idle when movement input ceases.
/// </summary>
public class PlayerWalkState : IPlayerState
{
    public void Enter(Player player)
    {
        player.Animator.SetBool("IsMoving", true);
    }

    public void Update(Player player)
    {
        if (player.MoveInput.sqrMagnitude <= 0.01f)
            player.SetState(PlayerStates.Idle); // static readonly
    }

    public void Exit(Player player) { }
}
