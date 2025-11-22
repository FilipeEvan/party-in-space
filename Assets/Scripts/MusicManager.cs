using UnityEngine;

// Gerencia a música de fundo usando o AudioSource
// do próprio GameObject (MusicaManager).
// Também expõe flags globais para habilitar/desabilitar
// música e efeitos sonoros (SFX).
public class MusicManager : MonoBehaviour
{
    static MusicManager instance;
    AudioSource audioSource;
    float initialVolume = 1f;

    public static bool MusicEnabled { get; private set; } = true;
    public static bool SfxEnabled { get; private set; } = true;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.ignoreListenerPause = true;

            initialVolume = audioSource.volume;

            if (MusicEnabled && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
            audioSource.mute = !MusicEnabled;
            audioSource.volume = MusicEnabled ? initialVolume : 0f;
        }
    }

    public static void SetMusicEnabled(bool enabled)
    {
        MusicEnabled = enabled;
        if (instance == null || instance.audioSource == null) return;
        instance.audioSource.mute = !enabled;
        instance.audioSource.volume = enabled ? instance.initialVolume : 0f;
        if (enabled && !instance.audioSource.isPlaying)
        {
            instance.audioSource.Play();
        }
        else if (!enabled && instance.audioSource.isPlaying)
        {
            // não pausa por tempo, apenas silencia
        }
    }

    public static void SetSfxEnabled(bool enabled)
    {
        SfxEnabled = enabled;
        // Não mexemos no AudioListener aqui; cada fonte de SFX
        // consulta esta flag para decidir se deve tocar.
    }
}
