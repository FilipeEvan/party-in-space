using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

// Cria automaticamente o Canvas e a UI de pausa
public static class PauseMenuBootstrap
{
    static bool registered;

    // Garante que registramos apenas uma vez o callback de cena carregada
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        if (registered) return;
        registered = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CreatePauseMenu();
    }

    static void CreatePauseMenu()
    {
        // Garante que exista um EventSystem para os botões
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var eventSystemGO = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // Usa um Canvas existente (você já montou o Canvas na cena)
        Canvas targetCanvas = Object.FindObjectOfType<Canvas>();
        if (targetCanvas == null)
        {
            return;
        }

        // Objetos criados por você no Canvas:
        // FadeOut/containerPause
        //    Painel       -> imagem de fundo
        //    Pontuacao    -> texto de pontos
        //    Vida         -> texto de vida
        //    Pause        -> texto "Jogo Pausado"
        //    btnContinuar -> botão Continuar
        //    btnReiniciar -> botão Reiniciar

        GameObject containerPause = GameObject.Find("containerPause");
        if (containerPause == null)
        {
            return;
        }

        TMP_Text titleText = null;
        TMP_Text scoreText = null;
        TMP_Text lifeText = null;
        Button resumeButton = null;
        Button restartButton = null;

        var root = containerPause.transform;
        var titleTf = root.Find("Pause");
        if (titleTf != null) titleText = titleTf.GetComponent<TMP_Text>();

        var scoreTf = root.Find("Pontuacao");
        if (scoreTf != null) scoreText = scoreTf.GetComponent<TMP_Text>();

        var lifeTf = root.Find("Vida");
        if (lifeTf != null) lifeText = lifeTf.GetComponent<TMP_Text>();

        var btnContinuarTf = root.Find("btnContinuar");
        if (btnContinuarTf != null) resumeButton = btnContinuarTf.GetComponent<Button>();

        var btnReiniciarTf = root.Find("btnReiniciar");
        if (btnReiniciarTf != null) restartButton = btnReiniciarTf.GetComponent<Button>();

        // Garante que exista um controlador PauseMenu
        var pauseMenu = Object.FindObjectOfType<PauseMenu>();
        if (pauseMenu == null)
        {
            var controllerGO = new GameObject("PauseMenuController", typeof(PauseMenu));
            controllerGO.transform.SetParent(targetCanvas.transform, false);
            pauseMenu = controllerGO.GetComponent<PauseMenu>();
        }

        pauseMenu.pausePanel = containerPause;
        pauseMenu.pausedTitleText = titleText;
        pauseMenu.scoreText = scoreText;
        pauseMenu.lifeText = lifeText;
        pauseMenu.resumeButton = resumeButton;
        pauseMenu.restartButton = restartButton;
        pauseMenu.playerHealth = Object.FindObjectOfType<PlayerHealth>();

        pauseMenu.Initialize();

        // Começa desativado
        containerPause.SetActive(false);
    }
}
