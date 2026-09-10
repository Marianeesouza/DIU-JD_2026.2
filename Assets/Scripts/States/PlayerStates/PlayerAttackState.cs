using UnityEngine;

public class PlayerAttackState : IPlayerState
{
    public void Enter(Player player)
    {
        player.Animator.SetBool("IsAttacking", true);
        player.Animator.SetTrigger(AnimationHashes.Attack);
    }

    public void Update(Player player)
    {
        // Aguardar Animation Event chamar OnAttackEnd
    }

    public void Exit(Player player)
    {
        player.Animator.SetBool("IsAttacking", false);
    }
}
