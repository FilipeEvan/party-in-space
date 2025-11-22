using System.Collections;
using UnityEngine;

// Utilitário global para rodar Coroutines em um objeto sempre ativo,
// mesmo que outros GameObjects da cena estejam desativados.
public class GlobalCoroutineRunner : MonoBehaviour
{
    static GlobalCoroutineRunner _instance;

    public static GlobalCoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("GlobalCoroutineRunner");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<GlobalCoroutineRunner>();
            }
            return _instance;
        }
    }

    public static Coroutine Run(IEnumerator routine)
    {
        return Instance.StartCoroutine(routine);
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

