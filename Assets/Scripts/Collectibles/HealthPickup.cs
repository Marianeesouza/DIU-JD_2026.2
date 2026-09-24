using UnityEngine;

/// <summary>
/// Collectible health pickup. Heals the player on touch.
/// </summary>
public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healAmount = 25;
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

        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
            health.Heal(healAmount);

        if (pickupSfx != null)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        Destroy(gameObject);
    }
}
