using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Referências")]
    public PlayerHealth player;
    public Image[] heartImages; // opcional: assigne 3 imagens de coração
    public TMP_Text healthText; // opcional: exibir ♥♥♡ ou 2/3
    [Header("Opções de Corações no HUD")]
    public bool showHeartsAsSymbols = false; // padrão numérico para evitar quadrados em fontes sem ♥/♡
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    void OnEnable()
    {
        if (player == null) player = FindObjectOfType<PlayerHealth>();
        if (player != null)
        {
            player.HealthChanged += OnHealthChanged;
            player.Died += OnDied;
            OnHealthChanged(player.currentHealth, player.maxHealth);
        }
    }

    void OnDisable()
    {
        if (player != null)
        {
            player.HealthChanged -= OnHealthChanged;
            player.Died -= OnDied;
        }
    }

    void OnHealthChanged(int current, int max)
    {
        // Atualiza imagens
        if (heartImages != null && heartImages.Length > 0)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] == null) continue;
                bool filled = i < current;
                heartImages[i].enabled = filled || fullHeartSprite != null || emptyHeartSprite != null;
                if (fullHeartSprite != null && emptyHeartSprite != null)
                {
                    heartImages[i].sprite = filled ? fullHeartSprite : emptyHeartSprite;
                }
            }
        }

        if (healthText != null)
        {
            if (showHeartsAsSymbols)
            {
                // Monta string de corações: ♥ cheio, ♡ vazio
                System.Text.StringBuilder sb = new System.Text.StringBuilder(max);
                for (int i = 0; i < current; i++) sb.Append('♥');
                for (int i = current; i < max; i++) sb.Append('♡');
                healthText.text = sb.ToString();
            }
            else
            {
                healthText.text = current + "/" + max;
            }
        }
    }

    void OnDied()
    {
        OnHealthChanged(0, player != null ? player.maxHealth : 3);
    }
}
