using UnityEngine;

// Coins no longer add score. They are just collectibles/visuals.
public class CollectCoin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Desativa a moeda ao tocar, sem alterar o score.
        this.gameObject.SetActive(false);
    }
}
