using UnityEngine;

public class ChaseStrategy : IMovementStrategy
{
    public Vector2 CalculateDesiredDirection(Vector2 current, Vector2 target)
    {
        return (target - current).normalized;
    }
}
