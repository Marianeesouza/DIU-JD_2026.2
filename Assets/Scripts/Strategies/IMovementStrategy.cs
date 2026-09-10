using UnityEngine;

public interface IMovementStrategy
{
    Vector2 CalculateDesiredDirection(Vector2 current, Vector2 target);
}
