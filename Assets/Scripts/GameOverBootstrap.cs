using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

// Cria automaticamente uma tela de Game Over:
// - Jogo congela ao morrer
// - Tela acinzentada cobrindo tudo
// - Mostra a pontuação final e um botão de Reiniciar
public static class GameOverBootstrap
{
    static bool registered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        if (registered) return;
        registered = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CreateGameOverUI();
    }

    static void CreateGameOverUI()
    {
        var player = Object.FindObjectOfType<PlayerHealth>();

        // Garante EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // Usa Canvas existente (você já montou o Canvas e os painéis de fim de jogo)
        Canvas targetCanvas = Object.FindObjectOfType<Canvas>();
        if (targetCanvas == null)
        {
            return;
        }

        // Objetos criados por você no Canvas:
        // FadeOut/containerFimJogo
        //    painelPause -> imagem de fundo
        //    Pause       -> título "Fim de jogo"
        //    Pontuacao   -> texto de pontos finais
        //    Vida        -> (opcional) texto de vida
        //    btnReiniciar-> botão Reiniciar

        GameObject containerFimJogo = GameObject.Find("containerFimJogo");
        if (containerFimJogo == null)
        {
            return;
        }

        TMP_Text titleText = null;
        TMP_Text scoreText = null;
        Button restartButton = null;

        var root = containerFimJogo.transform;
        var titleTf = root.Find("Pause");
        if (titleTf != null) titleText = titleTf.GetComponent<TMP_Text>();

        var scoreTf = root.Find("Pontuacao");
        if (scoreTf != null) scoreText = scoreTf.GetComponent<TMP_Text>();

        var btnReiniciarTf = root.Find("btnReiniciar");
        if (btnReiniciarTf != null) restartButton = btnReiniciarTf.GetComponent<Button>();

        // Garante que exista um controlador GameOverUI
        var ui = Object.FindObjectOfType<GameOverUI>();
        if (ui == null)
        {
            var controllerGO = new GameObject("GameOverController", typeof(GameOverUI));
            controllerGO.transform.SetParent(targetCanvas.transform, false);
            ui = controllerGO.GetComponent<GameOverUI>();
        }

        ui.panel = containerFimJogo;
        ui.titleText = titleText;
        ui.scoreText = scoreText;
        ui.restartButton = restartButton;

        ui.Initialize();
        ui.BindPlayer(player);
    }
}
