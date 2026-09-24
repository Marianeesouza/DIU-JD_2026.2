using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gatilho de saída de cena: salva o estado do jogador e carrega a cena informada.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SceneExit : MonoBehaviour
{
    [Header("Scene flow")]
    [Tooltip("Nome da cena nas Build Settings.")]
    [SerializeField] private string nextSceneName = "Room2";

    private bool isTransitioning;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return;

        if (other != null && other.CompareTag("Player"))
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("[SceneExit] Nenhum nome de cena foi informado!");
                return;
            }

            isTransitioning = true;

            // Salva os dados atuais (Vida, Essência, etc.) antes de mudar de cena
            PlayerProgress.Capture();

            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void SetNextScene(string sceneName) => nextSceneName = sceneName;
}