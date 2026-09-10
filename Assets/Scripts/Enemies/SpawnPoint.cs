using UnityEngine;

/// <summary>
/// Spawn point marker. Draws a red gizmo in the Editor for position visualization.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
