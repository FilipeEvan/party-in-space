using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    // Controla se o ESC pode pausar o jogo (desativado enquanto a contagem inicial roda)
    public static bool canUsePause = true;

    [Header("Interface")]
    public GameObject pausePanel;
    public TMP_Text pausedTitleText;
    public TMP_Text scoreText;
    public TMP_Text lifeText;
    public Button resumeButton;
    public Button restartButton;

    [Header("Referências")]
    public PlayerHealth playerHealth;

    bool isPaused;

    void Awake()
    {
        // Awake pode ser chamado antes de o Bootstrap preencher as referências.
        // A inicialização real é feita em Initialize(), chamada pelo Bootstrap.
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        if (!canUsePause)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pausedTitleText != null)
            pausedTitleText.text = "Jogo Pausado";

        // Pausado: não mostrar HUD
        HUDVisibility.SetHUDVisible(false);

        UpdateInfoTexts();
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Voltou a rodar: mostra HUD novamente
        HUDVisibility.SetHUDVisible(true);
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    // Chamado pelo PauseMenuBootstrap depois de atribuir todos os campos públicos
    public void Initialize()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
            resumeButton.onClick.AddListener(ResumeGame);
        }
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    void UpdateInfoTexts()
    {
        if (scoreText != null)
        {
            scoreText.text = "Pontos: " + MasterInfo.ScoreInt;
        }

        if (lifeText != null && playerHealth != null)
        {
            lifeText.text = "Vida: " + playerHealth.currentHealth + " / " + playerHealth.maxHealth;
        }
    }
}
