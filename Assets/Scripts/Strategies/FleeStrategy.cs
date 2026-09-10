using UnityEngine;

/// <summary>
/// Flee strategy. Calculates normalized direction opposite to the target.
/// </summary>
public class FleeStrategy : IMovementStrategy
{
    public Vector2 CalculateDesiredDirection(Vector2 current, Vector2 target)
    {
        return (current - target).normalized;
    }
}
