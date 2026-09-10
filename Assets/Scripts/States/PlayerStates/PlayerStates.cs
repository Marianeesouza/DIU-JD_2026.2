using UnityEngine;

public static class PlayerStates
{
    public static readonly PlayerIdleState Idle = new PlayerIdleState();
    public static readonly PlayerWalkState Walk = new PlayerWalkState();
    public static readonly PlayerAttackState Attack = new PlayerAttackState();
    public static readonly PlayerDashState Dash = new PlayerDashState();
    public static readonly PlayerDeadState Dead = new PlayerDeadState();
}
