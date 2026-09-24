using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD panel display. Subscribes to essence and health events and keeps the
/// sliders/texts in sync; polls the current form name and transformation timer.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Bars")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider essenceSlider;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI essenceText;
    [SerializeField] private TextMeshProUGUI formNameText;
    [SerializeField] private TextMeshProUGUI timerText;

    private TransformationManager transformationManager;
    private HealthSystem playerHealth;
    private Player player;
    private PlayerFormController forms;

    private void Start()
    {
        transformationManager = TransformationManager.Instance;
        if (transformationManager != null)
        {
            transformationManager.OnEssenceChanged += UpdateEssence;
            UpdateEssence(transformationManager.CurrentEssence, transformationManager.MaxEssence);
        }

        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null)
        {
            player = playerGo.GetComponent<Player>();
            forms = playerGo.GetComponent<PlayerFormController>();
            playerHealth = playerGo.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealth;
                UpdateHealth(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
            }
        }
    }

    private void OnDestroy()
    {
        if (transformationManager != null)
            transformationManager.OnEssenceChanged -= UpdateEssence;
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealth;
    }

    private void Update()
    {
        if (player == null || forms == null) return;

        if (formNameText != null)
        {
            TransformationData data = forms.CurrentFormData;
            formNameText.text = player.IsTransformed && data != null ? data.formName : "Humano";
        }

        if (timerText != null)
        {
            if (player.IsTransformed)
            {
                timerText.gameObject.SetActive(true);
                timerText.text = $"{player.TransformationTimeRemaining:0.0}s";
            }
            else
            {
                timerText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateEssence(int current, int max)
    {
        if (essenceSlider != null)
        {
            essenceSlider.maxValue = max;
            essenceSlider.value = current;
        }
        if (essenceText != null)
            essenceText.text = $"{current}/{max}";
    }

    private void UpdateHealth(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText != null)
            healthText.text = $"{current}/{max}";
    }
}
