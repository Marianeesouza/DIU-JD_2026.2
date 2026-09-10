using UnityEngine;

/// <summary>
/// Interface for player states. Defines the Enter/Update/Exit lifecycle (State Pattern).
/// </summary>
public interface IPlayerState
{
    void Enter(Player player);
    void Update(Player player);
    void Exit(Player player);
}
