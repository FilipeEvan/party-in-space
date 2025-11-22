using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Adicione este componente a um GameObject dentro do Canvas (por exemplo, um Panel).
// Atribua o Player (PlayerHealth) e os sprites de coração cheio/vazio no Inspector.
// Ele cria e atualiza automaticamente os ícones conforme a vida do jogador.
public class HeartsHUD : MonoBehaviour
{
    [Header("Referências")]
    public PlayerHealth player;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    [Header("Layout")]
    public Vector2 heartSize = new Vector2(40f, 40f);
    public float spacing = 6f;
    public TextAnchor alignment = TextAnchor.UpperRight; // vida no topo-direita
    [Tooltip("Ajuste fino vertical em relação ao Score (valores positivos sobem).")]
    public float topNudge = 0f;

    readonly List<Image> _hearts = new List<Image>();
    TextMeshProUGUI _textFallback;
    HorizontalLayoutGroup _layout;

    void OnEnable()
    {
        if (player == null) player = FindObjectOfType<PlayerHealth>();
        if (fullHeartSprite == null) fullHeartSprite = Resources.Load<Sprite>("Hearts/full");
        if (emptyHeartSprite == null) emptyHeartSprite = Resources.Load<Sprite>("Hearts/empty");
        EnsureLayout();
        RebuildHearts();
        AlignWithScore();
        if (player != null)
        {
            player.HealthChanged += OnHealthChanged;
            player.Died += OnDied;
            OnHealthChanged(player.currentHealth, player.maxHealth);
        }
    }

    void Start()
    {
        // Garante alinhamento mesmo se o ScoreText for criado após este componente
        StartCoroutine(AlignNextFrame());
    }

    void OnDisable()
    {
        if (player != null)
        {
            player.HealthChanged -= OnHealthChanged;
            player.Died -= OnDied;
        }
    }

    void EnsureLayout()
    {
        _layout = GetComponent<HorizontalLayoutGroup>();
        if (_layout == null)
        {
            _layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            _layout.spacing = spacing;
            _layout.childAlignment = alignment;
            _layout.childForceExpandWidth = false;
            _layout.childForceExpandHeight = false;
        }
        else
        {
            _layout.spacing = spacing;
            _layout.childAlignment = alignment;
        }
    }

    void RebuildHearts()
    {
        // Limpa antigos
        for (int i = 0; i < _hearts.Count; i++)
        {
            if (_hearts[i] != null)
                Destroy(_hearts[i].gameObject);
        }
        _hearts.Clear();
        if (_textFallback != null)
        {
            Destroy(_textFallback.gameObject);
            _textFallback = null;
        }

        int count = (player != null) ? Mathf.Max(0, player.maxHealth) : 3;

        bool haveAnySprite = (fullHeartSprite != null || emptyHeartSprite != null);
        if (!haveAnySprite)
        {
            // Fallback: usa TMP texto com ♥/♡
            var go = new GameObject("HeartsText", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(heartSize.x * count + spacing * (count - 1), heartSize.y);
            _textFallback = go.GetComponent<TextMeshProUGUI>();
            _textFallback.fontSize = heartSize.y;
            _textFallback.alignment =
                alignment == TextAnchor.UpperRight ? TextAlignmentOptions.TopRight :
                alignment == TextAnchor.UpperCenter ? TextAlignmentOptions.Top :
                alignment == TextAnchor.UpperLeft ? TextAlignmentOptions.TopLeft :
                alignment == TextAnchor.MiddleRight ? TextAlignmentOptions.Right :
                alignment == TextAnchor.MiddleCenter ? TextAlignmentOptions.Center :
                alignment == TextAnchor.MiddleLeft ? TextAlignmentOptions.Left :
                alignment == TextAnchor.LowerRight ? TextAlignmentOptions.BottomRight :
                alignment == TextAnchor.LowerCenter ? TextAlignmentOptions.Bottom :
                TextAlignmentOptions.BottomLeft;
            _textFallback.enableWordWrapping = false;
            _textFallback.color = Color.red;
            UpdateHearts();
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = heartSize;
            var img = go.GetComponent<Image>();
            img.preserveAspect = true;
            var sprite = emptyHeartSprite != null ? emptyHeartSprite : fullHeartSprite;
            if (sprite == null)
                sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            img.sprite = sprite;
            _hearts.Add(img);
        }
        UpdateHearts();
    }

    public void AlignWithScore()
    {
        // Tenta alinhar o Y com o ScoreText se existir
        var scoreGO = GameObject.Find("ScoreText");
        if (scoreGO == null) return;
        var scoreRt = scoreGO.GetComponent<RectTransform>();
        var rt = GetComponent<RectTransform>();
        if (scoreRt != null && rt != null)
        {
            var pos = rt.anchoredPosition;
            pos.y = scoreRt.anchoredPosition.y + topNudge; // positivo sobe
            rt.anchoredPosition = pos;
        }
    }

    IEnumerator AlignNextFrame()
    {
        yield return null; // espera 1 frame
        AlignWithScore();
    }

    void UpdateHearts()
    {
        if (_textFallback != null)
        {
            int curHearts = player != null ? Mathf.Clamp(player.currentHealth, 0, (player != null ? player.maxHealth : 3)) : 0;
            int maxHearts = player != null ? player.maxHealth : 3;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(maxHearts);
            for (int i = 0; i < curHearts; i++) sb.Append('♥');
            for (int i = curHearts; i < maxHearts; i++) sb.Append('♡');
            _textFallback.text = sb.ToString();
            return;
        }
        if (_hearts.Count == 0) return;
        int current = player != null ? Mathf.Clamp(player.currentHealth, 0, _hearts.Count) : 0;
        for (int i = 0; i < _hearts.Count; i++)
        {
            var img = _hearts[i];
            if (img == null) continue;
            if (fullHeartSprite != null || emptyHeartSprite != null)
            {
                Sprite full = fullHeartSprite != null ? fullHeartSprite : emptyHeartSprite;
                Sprite empty = emptyHeartSprite != null ? emptyHeartSprite : fullHeartSprite;
                img.sprite = i < current ? full : empty;
                img.color = Color.white;
            }
            else
            {
                // Sem sprites: usa cor como feedback
                img.color = i < current ? Color.red : new Color(1f, 1f, 1f, 0.3f);
            }
        }
    }

    void OnHealthChanged(int current, int max)
    {
        if (max != _hearts.Count)
        {
            RebuildHearts();
        }
        else
        {
            UpdateHearts();
        }
    }

    void OnDied()
    {
        UpdateHearts();
    }
}
