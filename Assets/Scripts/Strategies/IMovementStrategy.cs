using UnityEngine;

/// <summary>
/// Interface for enemy movement strategies (Strategy Pattern).
/// </summary>
public interface IMovementStrategy
{
    Vector2 CalculateDesiredDirection(Vector2 current, Vector2 target);
}
