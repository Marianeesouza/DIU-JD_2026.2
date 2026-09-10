using UnityEngine;

/// <summary>
/// Chase strategy. Calculates normalized direction in a straight line toward the target.
/// </summary>
public class ChaseStrategy : IMovementStrategy
{
    public Vector2 CalculateDesiredDirection(Vector2 current, Vector2 target)
    {
        return (target - current).normalized;
    }
}
