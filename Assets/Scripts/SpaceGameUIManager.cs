using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Gerencia as telas de Início, Pause e Fim de jogo
// usando os containers que você montou no Canvas:
// - containerInicio  (título + botão Jogar)
// - containerContador (texto Contador)
// - containerPonto   (HUD de Pontos)
// - containerVida    (HUD de Vida)
// - containerPause   (tela de pause)
// - containerFimJogo (tela de fim de jogo)
public class SpaceGameUIManager : MonoBehaviour
{
    [Header("Containers principais")]
    public GameObject containerInicio;
    public GameObject containerContador;
    public GameObject containerPonto;
    public GameObject containerVida;
    public GameObject containerPause;
    public GameObject containerFimJogo;

    [Header("Textos")]
    public TMP_Text textoContador;
    public TMP_Text textoPontosPause;
    public TMP_Text textoVidaPause;
    public TMP_Text textoPontosFim;

    [Header("Botões")]
    public Button btnJogar;
    public Button btnContinuar;
    public Button btnReiniciarPause;
    public Button btnReiniciarFim;

    [Header("Opções de áudio (telas de Pause)")]
    public Toggle toggleSomMusica; // SomMusica
    public Toggle toggleSomJogo;   // SomJogo

    [Header("Referências de jogo")]
    public PlayerHealth playerHealth;
    public int segundosContagem = 5;

    bool jogoRodando;
    bool pausado;
    bool gameOver;

    void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }
        if (playerHealth != null)
        {
            playerHealth.Died += OnPlayerDied;
        }

        // Liga eventos dos botões
        if (btnJogar != null)
        {
            btnJogar.onClick.RemoveAllListeners();
            btnJogar.onClick.AddListener(OnJogar);
        }
        if (btnContinuar != null)
        {
            btnContinuar.onClick.RemoveAllListeners();
            btnContinuar.onClick.AddListener(OnContinuar);
        }
        if (btnReiniciarPause != null)
        {
            btnReiniciarPause.onClick.RemoveAllListeners();
            btnReiniciarPause.onClick.AddListener(OnReiniciar);
        }
        if (btnReiniciarFim != null)
        {
            btnReiniciarFim.onClick.RemoveAllListeners();
            btnReiniciarFim.onClick.AddListener(OnReiniciar);
        }

        // Liga toggles de áudio
        if (toggleSomMusica != null)
        {
            toggleSomMusica.onValueChanged.RemoveAllListeners();
            toggleSomMusica.isOn = MusicManager.MusicEnabled;
            toggleSomMusica.onValueChanged.AddListener(OnToggleSomMusica);
        }
        if (toggleSomJogo != null)
        {
            toggleSomJogo.onValueChanged.RemoveAllListeners();
            toggleSomJogo.isOn = MusicManager.SfxEnabled;
            toggleSomJogo.onValueChanged.AddListener(OnToggleSomJogo);
        }
    }

    void Start()
    {
        IniciarTelaInicial();
    }

    void Update()
    {
        if (!jogoRodando || gameOver) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausado) FecharPause();
            else AbrirPause();
        }
    }

    void IniciarTelaInicial()
    {
        Time.timeScale = 0f;
        jogoRodando = false;
        pausado = false;
        gameOver = false;

        // Mostra apenas tela inicial
        SetActive(containerInicio, true);
        SetActive(containerContador, false);
        SetActive(containerPause, false);
        SetActive(containerFimJogo, false);

        // HUD de pontos/vida começa escondido
        SetActive(containerPonto, false);
        SetActive(containerVida, false);
    }

    void OnJogar()
    {
        if (gameOver) return;
        StartCoroutine(ContagemInicial());
    }

    IEnumerator ContagemInicial()
    {
        SetActive(containerInicio, false);
        SetActive(containerContador, true);

        int restante = Mathf.Max(1, segundosContagem);
        while (restante > 0)
        {
            if (textoContador != null)
            {
                textoContador.text = restante.ToString();
            }
            yield return new WaitForSecondsRealtime(1f);
            restante--;
        }

        if (textoContador != null)
        {
            textoContador.text = "Vai!";
            yield return new WaitForSecondsRealtime(0.6f);
        }

        SetActive(containerContador, false);

        // Mostra HUD e começa o jogo
        SetActive(containerPonto, true);
        SetActive(containerVida, true);

        Time.timeScale = 1f;
        jogoRodando = true;
        pausado = false;
    }

    void AbrirPause()
    {
        pausado = true;
        Time.timeScale = 0f;

        // Atualiza textos de pause
        if (textoPontosPause != null)
        {
            textoPontosPause.text = "Pontos: " + MasterInfo.ScoreInt;
        }
        if (textoVidaPause != null && playerHealth != null)
        {
            textoVidaPause.text = "Vida: " + playerHealth.currentHealth + " / " + playerHealth.maxHealth;
        }

        SetActive(containerPause, true);
        // Esconde HUD enquanto está no pause
        SetActive(containerPonto, false);
        SetActive(containerVida, false);
    }

    void FecharPause()
    {
        pausado = false;
        Time.timeScale = 1f;

        SetActive(containerPause, false);
        SetActive(containerPonto, true);
        SetActive(containerVida, true);
    }

    void OnContinuar()
    {
        if (!pausado) return;
        FecharPause();
    }

    void OnReiniciar()
    {
        Time.timeScale = 1f;
        var cena = SceneManager.GetActiveScene();
        SceneManager.LoadScene(cena.buildIndex);
    }

    void OnPlayerDied()
    {
        StartCoroutine(GameOverDepoisAnimacao());
    }

    IEnumerator GameOverDepoisAnimacao()
    {
        // espera um pouco (usa o mesmo delay que PlayerHealth já espera)
        yield return new WaitForSeconds(1.5f);

        gameOver = true;
        jogoRodando = false;
        pausado = false;

        Time.timeScale = 0f;

        // Atualiza texto de pontos finais
        if (textoPontosFim != null)
        {
            textoPontosFim.text = "Pontos: " + MasterInfo.ScoreInt;
        }

        // Mostra tela de fim de jogo e esconde HUD
        SetActive(containerFimJogo, true);
        SetActive(containerPonto, false);
        SetActive(containerVida, false);
    }

    void OnToggleSomMusica(bool ligado)
    {
        MusicManager.SetMusicEnabled(ligado);
    }

    void OnToggleSomJogo(bool ligado)
    {
        MusicManager.SetSfxEnabled(ligado);
    }

    void SetActive(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
        {
            go.SetActive(active);
        }
    }
}
