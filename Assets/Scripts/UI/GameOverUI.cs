using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Game over panel buttons. Delegates to UIManager.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Buttons (optional - can be wired via OnClick instead)")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    private void OnEnable()
    {
        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
    }

    private void OnDisable()
    {
        if (retryButton != null) retryButton.onClick.RemoveListener(OnRetry);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuit);
    }

    public void OnRetry()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnRetryButton();
    }

    public void OnQuit()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnQuitButton();
    }
}
