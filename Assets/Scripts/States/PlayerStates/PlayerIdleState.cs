using UnityEngine;

public class PlayerIdleState : IPlayerState
{
    public void Enter(Player player)
    {
        player.Animator.SetBool("IsMoving", false);
    }

    public void Update(Player player)
    {
        // Usa propriedade somente leitura (input capturado por PlayerMovement)
        if (player.MoveInput.sqrMagnitude > 0.01f)
            player.SetState(PlayerStates.Walk); // static readonly, sem new
    }

    public void Exit(Player player) { }
}
