using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// UI manager singleton. Controls which HUD panels are visible.
/// Incomplete panels show placeholder messages until prefab/UI is wired in Editor.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public const string FirstSceneName = "Room1";

    private static bool resumeGameplayOnLoad;

    [Header("Panels (wired in Editor)")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject hudPanel;

    [Header("HUD Elements")]
    [SerializeField] private UnityEngine.UI.Slider healthSlider;
    [SerializeField] private UnityEngine.UI.Slider essenceSlider;
    [SerializeField] private TMPro.TextMeshProUGUI healthText;
    [SerializeField] private TMPro.TextMeshProUGUI essenceText;

    private bool isPaused;
    private bool isGameOver;
    private bool isMainMenu;

    // Subscribed HUD sources (unsubscribed on destroy)
    private HealthSystem playerHealth;
    private TransformationManager transformationManagerRef;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Destrói todo o Canvas duplicado da nova cena (incluindo o UICanvas pai)
            Destroy(transform.root.gameObject);
            return;
        }

        Instance = this;

        // Preserva o Canvas raiz (UICanvas) com todos os seus filhos entre as cenas
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        resumeGameplayOnLoad = false;
    }

    private void Start()
    {
        CheckInitialState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        WireHud();

        if (resumeGameplayOnLoad)
        {
            resumeGameplayOnLoad = false;
            StartGameplay();
        }
    }

    private void CheckInitialState()
    {
        WireHud();

        if (resumeGameplayOnLoad)
        {
            resumeGameplayOnLoad = false;
            StartGameplay();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void WireHud()
    {
        // Limpa inscrições anteriores para evitar duplicação de chamadas
        UnwireHud();

        transformationManagerRef = TransformationManager.Instance;
        if (transformationManagerRef != null)
        {
            transformationManagerRef.OnEssenceChanged += UpdateEssenceBar;
            UpdateEssenceBar(transformationManagerRef.CurrentEssence, transformationManagerRef.MaxEssence);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthBar;
                UpdateHealthBar(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
            }
        }
    }

    private void UnwireHud()
    {
        if (transformationManagerRef != null)
            transformationManagerRef.OnEssenceChanged -= UpdateEssenceBar;
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void OnDestroy()
    {
        UnwireHud();
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (isGameOver || isMainMenu) return;

        Keyboard kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    private void HideAll()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    // ---- HUD ----

    public void UpdateHealthBar(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText != null)
            healthText.text = $"{current}/{max}";
    }

    public void UpdateEssenceBar(int current, int max)
    {
        if (essenceSlider != null)
        {
            essenceSlider.maxValue = max;
            essenceSlider.value = current;
        }
        if (essenceText != null)
            essenceText.text = $"{current}/{max}";
    }

    // ---- Menus ----

    private void StartGameplay()
    {
        isMainMenu = false;
        isPaused = false;
        isGameOver = false;
        HideAll();
        if (hudPanel != null) hudPanel.SetActive(true);
        SetTimeScale(1f);
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel == null)
        {
            Debug.LogWarning("[UIManager] mainMenuPanel not wired - starting gameplay instead.");
            StartGameplay();
            return;
        }

        isMainMenu = true;
        isPaused = false;
        isGameOver = false;
        resumeGameplayOnLoad = false;
        HideAll();
        mainMenuPanel.SetActive(true);
        SetTimeScale(0f);

        if (SceneManager.GetActiveScene().name != FirstSceneName)
        {
            SceneManager.LoadScene(FirstSceneName);
        }
    }

    public void PauseGame()
    {
        if (isGameOver || isMainMenu) return;
        isPaused = true;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);
        SetTimeScale(0f);
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
        SetTimeScale(1f);
    }

    public void ShowGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        else Debug.Log("[UIManager] Game Over!");
        SetTimeScale(0f);
    }

    public void ShowVictory()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (hudPanel != null) hudPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(true);
        else Debug.Log("[UIManager] Victory!");
        SetTimeScale(0f);
    }

    // ---- Button callbacks (wired in Editor) ----

    public void OnStartButton()
    {
        PlayerProgress.Clear();
        SetTimeScale(1f);

        if (SceneManager.GetActiveScene().name == FirstSceneName)
        {
            // Se já está na Room1, apenas fecha o menu e inicia o gameplay sem recarregar a cena
            StartGameplay();
        }
        else
        {
            resumeGameplayOnLoad = true;
            SceneManager.LoadScene(FirstSceneName);
        }
    }

    public void OnRetryButton()
    {
        PlayerProgress.RestoreCheckpoint();
        resumeGameplayOnLoad = true;
        SetTimeScale(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenuButton()
    {
        PlayerProgress.Clear();
        ShowMainMenu();
    }

    public void OnResumeButton()
    {
        ResumeGame();
    }

    public void OnQuitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void SetTimeScale(float value)
    {
        Time.timeScale = value;
    }
}