using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause menu panel buttons. Delegates to UIManager.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Buttons (optional - can be wired via OnClick instead)")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    private void OnEnable()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResume);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
    }

    private void OnDisable()
    {
        if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResume);
        if (restartButton != null) restartButton.onClick.RemoveListener(OnRestart);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuit);
    }

    public void OnResume()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnResumeButton();
    }

    public void OnRestart()
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
