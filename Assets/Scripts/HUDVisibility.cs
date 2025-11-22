using UnityEngine;

// Controla se o HUD (Score + Vida) deve estar visível
public static class HUDVisibility
{
    static bool _visible;

    public static bool IsVisible => _visible;

    public static void SetHUDVisible(bool visible)
    {
        _visible = visible;
        ApplyVisibility();
    }

    // Aplica o estado atual de visibilidade aos elementos de HUD conhecidos
    public static void ApplyVisibility()
    {
        // Precisamos encontrar inclusive objetos desativados,
        // então usamos Resources.FindObjectsOfTypeAll e filtramos por cena válida.

        // Score do HUD:
        //  1) Qualquer GameObject chamado "ScoreText" (criado pelo HUDBootstrap)
        //  2) Qualquer texto de pontuação que esteja ligado ao MasterInfo (ex.: CoinCount)

        var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allGOs.Length; i++)
        {
            var go = allGOs[i];
            if (go == null) continue;
            if (!go.scene.IsValid()) continue; // ignora prefabs/assets
            if (go.name == "ScoreText")
            {
                go.SetActive(_visible);
            }
        }

        var masters = Resources.FindObjectsOfTypeAll<MasterInfo>();
        for (int i = 0; i < masters.Length; i++)
        {
            var m = masters[i];
            if (m == null) continue;
            var txt = m.GetScoreText();
            if (txt == null) continue;
            var go = txt.gameObject;
            if (!go.scene.IsValid()) continue;
            go.SetActive(_visible);
        }

        // Corações HUD automáticos
        var allHearts = Resources.FindObjectsOfTypeAll<HeartsHUD>();
        for (int i = 0; i < allHearts.Length; i++)
        {
            var hearts = allHearts[i];
            if (hearts == null) continue;
            var go = hearts.gameObject;
            if (!go.scene.IsValid()) continue;
            go.SetActive(_visible);
        }

        // Qualquer outro HUD de vida baseado em PlayerHealthUI
        var healthUIs = Resources.FindObjectsOfTypeAll<PlayerHealthUI>();
        for (int i = 0; i < healthUIs.Length; i++)
        {
            var ui = healthUIs[i];
            if (ui == null) continue;
            var go = ui.gameObject;
            if (!go.scene.IsValid()) continue;
            go.SetActive(_visible);
        }
    }
}
