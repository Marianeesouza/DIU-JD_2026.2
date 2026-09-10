using UnityEngine;

/// <summary>
/// Static container with readonly instances of all player states.
/// Centralizes state access to prevent unnecessary allocations.
/// </summary>
public static class PlayerStates
{
    public static readonly PlayerIdleState Idle = new PlayerIdleState();
    public static readonly PlayerWalkState Walk = new PlayerWalkState();
    public static readonly PlayerAttackState Attack = new PlayerAttackState();
    public static readonly PlayerDashState Dash = new PlayerDashState();
    public static readonly PlayerDeadState Dead = new PlayerDeadState();
}
