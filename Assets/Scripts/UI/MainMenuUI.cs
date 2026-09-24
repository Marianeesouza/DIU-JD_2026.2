using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu panel buttons. Delegates to UIManager.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons (optional - can be wired via OnClick instead)")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void OnEnable()
    {
        if (startButton != null) startButton.onClick.AddListener(OnStart);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
    }

    private void OnDisable()
    {
        if (startButton != null) startButton.onClick.RemoveListener(OnStart);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuit);
    }

    public void OnStart()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnStartButton();
    }

    public void OnQuit()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnQuitButton();
    }
}
