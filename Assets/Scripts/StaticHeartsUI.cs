using UnityEngine;
using UnityEngine.UI;

// Usa pares de imagens (cheio/vazio) já existentes no Canvas
// para representar a vida do jogador.
//
// Exemplo de Hierarchy esperado (dentro de containerVida):
//   painelCoracao
//   Coracao_1
//   Coracao_Sem_Vida_1
//   Coracao_2
//   Coracao_Sem_Vida_2
//   Coracao_3
//   Coracao_Sem_Vida_3
//
// Configure no Inspector:
//   playerHealth  -> Player com PlayerHealth
//   fullHearts    -> Coracao_1, Coracao_2, Coracao_3
//   emptyHearts   -> Coracao_Sem_Vida_1, _2, _3
public class StaticHeartsUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public GameObject[] fullHearts;
    public GameObject[] emptyHearts;

    void OnEnable()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.HealthChanged += OnHealthChanged;
            playerHealth.Died += OnDied;
            OnHealthChanged(playerHealth.currentHealth, playerHealth.maxHealth);
        }
        else
        {
            // Se não achar PlayerHealth, ainda assim garante um estado inicial
            UpdateHearts(0, fullHearts != null ? fullHearts.Length : 0);
        }
    }

    void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= OnHealthChanged;
            playerHealth.Died -= OnDied;
        }
    }

    void OnHealthChanged(int current, int max)
    {
        UpdateHearts(current, max);
    }

    void OnDied()
    {
        int max = playerHealth != null ? playerHealth.maxHealth : fullHearts.Length;
        UpdateHearts(0, max);
    }

    void UpdateHearts(int current, int max)
    {
        if (fullHearts == null && emptyHearts == null) return;

        int slots = max;
        if (fullHearts != null) slots = Mathf.Min(slots, fullHearts.Length);
        if (emptyHearts != null && emptyHearts.Length > 0)
            slots = Mathf.Min(slots, emptyHearts.Length);

        current = Mathf.Clamp(current, 0, slots);

        for (int i = 0; i < slots; i++)
        {
            bool isFull = i < current;

            if (fullHearts != null && i < fullHearts.Length && fullHearts[i] != null)
            {
                fullHearts[i].SetActive(isFull);
            }
            if (emptyHearts != null && i < emptyHearts.Length && emptyHearts[i] != null)
            {
                emptyHearts[i].SetActive(!isFull);
            }
        }

        // Se houver slots extras nas arrays além de "max", desativa todos
        if (fullHearts != null)
        {
            for (int i = slots; i < fullHearts.Length; i++)
            {
                if (fullHearts[i] != null) fullHearts[i].SetActive(false);
            }
        }
        if (emptyHearts != null)
        {
            for (int i = slots; i < emptyHearts.Length; i++)
            {
                if (emptyHearts[i] != null) emptyHearts[i].SetActive(false);
            }
        }
    }
}

