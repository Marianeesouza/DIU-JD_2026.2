using UnityEngine;

/// <summary>
/// Interface for entities detectable by AI. Provides position and GameObject reference.
/// </summary>
public interface IDetectable
{
    GameObject GetGameObject();
    Vector2 GetPosition();
}
