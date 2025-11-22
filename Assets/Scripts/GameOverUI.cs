using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("Interface")]
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text scoreText;
    public Button restartButton;

    PlayerHealth player;
    bool shown;

    public void Initialize()
    {
        shown = false;

        if (panel != null)
            panel.SetActive(false);

        if (titleText != null)
            titleText.text = "Fim de jogo";

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    }

    public void BindPlayer(PlayerHealth newPlayer)
    {
        if (player != null)
        {
            player.DeathAnimationFinished -= OnDeathAnimationFinished;
        }
        player = newPlayer;
        if (player != null)
        {
            player.DeathAnimationFinished += OnDeathAnimationFinished;
        }
    }

    void OnDisable()
    {
        if (player != null)
        {
            player.DeathAnimationFinished -= OnDeathAnimationFinished;
        }
    }

    void OnDeathAnimationFinished()
    {
        ShowGameOver();
    }

    public void ShowGameOver()
    {
        if (shown) return;
        shown = true;

        // Desabilita pausa e HUD; congela o jogo.
        PauseMenu.canUsePause = false;
        HUDVisibility.SetHUDVisible(false);
        Time.timeScale = 0f;

        if (panel != null)
            panel.SetActive(true);

        if (scoreText != null)
        {
            scoreText.text = "Pontos: " + MasterInfo.ScoreInt;
        }
    }

    void OnRestartClicked()
    {
        Time.timeScale = 1f;
        PauseMenu.canUsePause = false; // StartGameUI cuidará do fluxo na nova cena
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
