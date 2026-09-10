using UnityEngine;

public class WanderStrategy : IMovementStrategy
{
    private Vector2 wanderDirection;
    private float wanderTimer;
    private const float WANDER_CHANGE_INTERVAL = 2f;

    public Vector2 CalculateDesiredDirection(Vector2 current, Vector2 target)
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            wanderDirection = Random.insideUnitCircle.normalized;
            wanderTimer = WANDER_CHANGE_INTERVAL;
        }
        return wanderDirection;
    }
}
