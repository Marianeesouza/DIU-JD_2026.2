using UnityEngine;

/// <summary>
/// Collectible essence pickup. Adds essence to TransformationManager on player touch.
/// </summary>
public class EssencePickup : MonoBehaviour
{
    [SerializeField] private int essenceValue = 10;
    [SerializeField] private float lifetime = 30f;
    [SerializeField] private AudioClip pickupSfx;

    private void Start()
    {
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (TransformationManager.Instance != null)
            TransformationManager.Instance.AddEssence(essenceValue);

        if (pickupSfx != null)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        Destroy(gameObject);
    }
}
