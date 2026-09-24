using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Victory panel buttons. Delegates to UIManager.
/// </summary>
public class VictoryUI : MonoBehaviour
{
    [Header("Buttons (optional - can be wired via OnClick instead)")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    private void OnEnable()
    {
        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    private void OnDisable()
    {
        if (retryButton != null) retryButton.onClick.RemoveListener(OnRetry);
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenu);
    }

    public void OnRetry()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnRetryButton();
    }

    public void OnMainMenu()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnMainMenuButton();
    }
}
