using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Cria automaticamente um Canvas de HUD com corações e placar se não existir.
public static class HUDBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateHUD()
    {
        // Evita duplicar caso já tenha um configurado
        var existingHearts = Object.FindObjectOfType<HeartsHUD>();
        var existingScoreGO = GameObject.Find("ScoreText");
        var master = Object.FindObjectOfType<MasterInfo>();
        bool hasScore = (existingScoreGO != null) || (master != null && master.HasScoreTextBound());

        // Pega um Canvas existente ou cria um novo
        Canvas targetCanvas = Object.FindObjectOfType<Canvas>();
        if (targetCanvas == null)
        {
            var canvasGO = new GameObject("HUD Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = canvasGO.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Hearts panel (top-right)
        if (existingHearts == null)
        {
            var heartsGO = new GameObject("Hearts", typeof(RectTransform));
            heartsGO.transform.SetParent(targetCanvas.transform, false);
            var rt = heartsGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-24f, -24f);

            var hearts = heartsGO.AddComponent<HeartsHUD>();
            hearts.heartSize = new Vector2(88, 88);
            hearts.spacing = 12f;
            hearts.alignment = TextAnchor.UpperRight;
            hearts.topNudge = 6f;
            hearts.player = Object.FindObjectOfType<PlayerHealth>();
        }
        else
        {
            var rt = existingHearts.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-24f, -24f);
            }
            existingHearts.alignment = TextAnchor.UpperRight;
        }

        // Score text (top-left)
        if (!hasScore)
        {
            var scoreGO = new GameObject("ScoreText", typeof(RectTransform), typeof(TextMeshProUGUI));
            scoreGO.transform.SetParent(targetCanvas.transform, false);
            var rt = scoreGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 80);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(24f, -24f);

            var tmp = scoreGO.GetComponent<TextMeshProUGUI>();
            tmp.text = "SCORE: 0";
            tmp.fontSize = 36f;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.enableWordWrapping = false;
            tmp.color = Color.white;

            if (master != null)
            {
                master.SetScoreText(tmp);
            }
        }
        else
        {
            // Reposiciona score existente para o topo-esquerda
            TMP_Text boundBefore = master != null ? master.GetScoreText() : null;
            TMP_Text tmp = null;
            if (existingScoreGO != null)
            {
                tmp = existingScoreGO.GetComponent<TMP_Text>();
            }
            if (tmp == null && boundBefore != null)
            {
                tmp = boundBefore;
            }
            if (tmp != null)
            {
                var rt = tmp.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(400, 80);
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(24f, -24f);
                tmp.alignment = TextAlignmentOptions.TopLeft;
                if (master != null) master.SetScoreText(tmp);

                // Se havia um texto antigo diferente do atual, desativa para evitar duplicidade
                if (boundBefore != null && boundBefore != tmp)
                {
                    boundBefore.gameObject.SetActive(false);
                }
            }
        }

        // Garantir que os corações estejam no Canvas e alinhados na mesma altura do score
        var heartsHUD = Object.FindObjectOfType<HeartsHUD>();
        if (heartsHUD != null)
        {
            if (heartsHUD.transform.parent != targetCanvas.transform)
            {
                heartsHUD.transform.SetParent(targetCanvas.transform, false);
            }

            RectTransform heartsRT = heartsHUD.GetComponent<RectTransform>();
            TMP_Text scoreText = master != null ? master.GetScoreText() : null;
            RectTransform scoreRT = null;
            if (scoreText != null) scoreRT = scoreText.GetComponent<RectTransform>();
            if (scoreRT == null)
            {
                var scoreGO2 = GameObject.Find("ScoreText");
                if (scoreGO2 != null) scoreRT = scoreGO2.GetComponent<RectTransform>();
            }
            if (heartsRT != null && scoreRT != null)
            {
                var pos = heartsRT.anchoredPosition;
                pos.y = scoreRT.anchoredPosition.y + heartsHUD.topNudge;
                heartsRT.anchoredPosition = pos;
            }
            heartsHUD.alignment = TextAnchor.UpperRight;
        }
    }
}
