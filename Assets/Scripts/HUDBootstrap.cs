using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Liga o MasterInfo ao texto de pontuação já existente no Canvas (CoinCount).
public static class HUDBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateHUD()
    {
        var master = Object.FindObjectOfType<MasterInfo>();

        if (master == null) return;

        var coinGO = GameObject.Find("CoinCount");
        if (coinGO == null) return;

        var scoreTMP = coinGO.GetComponent<TMP_Text>();
        if (scoreTMP == null) return;

        master.SetScoreText(scoreTMP);
    }
}
