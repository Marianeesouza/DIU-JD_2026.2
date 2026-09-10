using UnityEngine;

// CORREÇÃO CRÍTICA 5: Strings de animação via StringToHash
// Evita erros de digitação e melhora performance (comparação de int vs string)
public static class AnimationHashes
{
    public static readonly int IsMoving = Animator.StringToHash("IsMoving");
    public static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    public static readonly int IsDashing = Animator.StringToHash("IsDashing");
    public static readonly int IsTakingDamage = Animator.StringToHash("IsTakingDamage");
    public static readonly int IsDead = Animator.StringToHash("IsDead");
    public static readonly int Attack = Animator.StringToHash("Attack");
    public static readonly int Dash = Animator.StringToHash("Dash");
    public static readonly int Death = Animator.StringToHash("Death");
    public static readonly int Damage = Animator.StringToHash("Damage");
    public static readonly int SpinDeath = Animator.StringToHash("SpinDeath");
    public static readonly int SoulDeath = Animator.StringToHash("SoulDeath");
    public static readonly int MoveX = Animator.StringToHash("MoveX");
    public static readonly int MoveY = Animator.StringToHash("MoveY");
}
