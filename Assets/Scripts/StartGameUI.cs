using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartGameUI : MonoBehaviour
{
    [Header("Interface")]
    public GameObject startPanel;
    public TMP_Text titleText;
    public Button playButton;
    public TMP_Text countdownText;

    [Header("Configuração")]
    public int countdownSeconds = 5;

    bool gameStarted;
    bool countdownRunning;
    bool initialized;

    void Start()
    {
        // Inicializa automaticamente quando a cena começa.
        // A ligação dos campos é feita pelo Inspector.
        Initialize();
    }

    public void Initialize()
    {
        if (initialized) return;
        // Chamado pelo Bootstrap ao carregar a cena
        initialized = true;
        gameStarted = false;
        countdownRunning = false;

        PauseMenu.canUsePause = false;
        Time.timeScale = 0f;

        // Enquanto a tela inicial está ativa, esconde o HUD (score + vida)
        HUDVisibility.SetHUDVisible(false);

        // Garante que os containers de HUD estejam ativos (mesmo que desativados no editor),
        // pois o HUDVisibility controla a visibilidade interna.
        var pontosContainer = GameObject.Find("containerPonto");
        if (pontosContainer != null && !pontosContainer.activeSelf)
            pontosContainer.SetActive(true);
        var vidaContainer = GameObject.Find("containerVida");
        if (vidaContainer != null && !vidaContainer.activeSelf)
            vidaContainer.SetActive(true);
        var contadorContainer = GameObject.Find("containerContador");
        if (contadorContainer != null && !contadorContainer.activeSelf)
            contadorContainer.SetActive(true);

        if (startPanel != null)
            startPanel.SetActive(true);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
            countdownText.text = "";
        }

        if (titleText != null)
        {
            titleText.text = "Party in Space";
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
            playButton.onClick.AddListener(OnPlayClicked);
        }
    }

    void OnPlayClicked()
    {
        if (!initialized || countdownRunning || gameStarted)
            return;

        countdownRunning = true;

        // Some com a tela inicial imediatamente ao clicar em Jogar
        if (startPanel != null)
            startPanel.SetActive(false);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        // Usa um runner global sempre ativo para evitar erro
        // caso o GameObject deste componente esteja desativado na hierarquia.
        GlobalCoroutineRunner.Run(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        int remaining = Mathf.Max(1, countdownSeconds);

        while (remaining > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = remaining.ToString();
            }
            yield return new WaitForSecondsRealtime(1f);
            remaining--;
        }

        if (countdownText != null)
        {
            countdownText.text = "Vai!";
            yield return new WaitForSecondsRealtime(0.5f);
            countdownText.gameObject.SetActive(false);
        }

        // Depois da contagem, some com o painel inicial inteiro
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        PauseMenu.canUsePause = true;

        // Jogo começou de verdade: mostra HUD
        HUDVisibility.SetHUDVisible(true);

        countdownRunning = false;
        gameStarted = true;
    }
}
