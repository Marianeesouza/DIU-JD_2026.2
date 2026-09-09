using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthSlider;

    private void Start()
    {
        if (healthSystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                healthSystem = player.GetComponent<HealthSystem>();
        }

        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += OnHealthChanged;
            healthSystem.OnDeath += OnHealthChanged;
            UpdateHealthDisplay();
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= OnHealthChanged;
            healthSystem.OnDeath -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        UpdateHealthDisplay();
    }

    private void OnHealthChanged()
    {
        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (healthSystem == null) return;

        int current = healthSystem.GetCurrentHealth();
        int max = healthSystem.GetMaxHealth();

        if (healthText != null)
        {
            healthText.text = $"HP: {current}/{max}";
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }
}
