using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

// Cria automaticamente uma tela inicial com "Party in Space"
// e um botão Jogar que dispara uma contagem de 5s antes do jogo começar.
public static class StartGameBootstrap
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
        CreateStartUI();
    }

    static void CreateStartUI()
    {
        // Hoje só garantimos que exista um EventSystem.
        // Toda a ligação com o Canvas é feita via StartGameUI no Inspector.
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
