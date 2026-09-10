using UnityEngine;

/// <summary>
/// Wander strategy with move-pause pattern. Wanders for a duration,
/// stops briefly, then wanders again in a new random direction.
/// </summary>
public class WanderStrategy : IMovementStrategy
{
    private Vector2 wanderDirection;
    private float stateTimer;
    private bool isMoving;

    private const float MIN_MOVE_DURATION = 1.5f;
    private const float MAX_MOVE_DURATION = 4f;
    private const float PAUSE_DURATION = 1f;

    public Vector2 CalculateDesiredDirection(Vector2 current, Vector2 target)
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (isMoving)
            {
                isMoving = false;
                stateTimer = PAUSE_DURATION;
                return Vector2.zero;
            }
            else
            {
                wanderDirection = Random.insideUnitCircle.normalized;
                stateTimer = Random.Range(MIN_MOVE_DURATION, MAX_MOVE_DURATION);
                isMoving = true;
                return wanderDirection;
            }
        }

        return isMoving ? wanderDirection : Vector2.zero;
    }
}
