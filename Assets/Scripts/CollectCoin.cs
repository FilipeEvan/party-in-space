using UnityEngine;

public class CollectCoin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        MasterInfo.coinCount += 1;
        this.gameObject.SetActive(false);
    }
}
