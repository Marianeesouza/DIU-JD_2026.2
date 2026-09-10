using UnityEngine;

public class FleeStrategy : IMovementStrategy
{
    public Vector2 CalculateDesiredDirection(Vector2 current, Vector2 target)
    {
        return (current - target).normalized;
    }
}
