using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null || sfxSource == null)
        {
            Debug.LogError(
                "[AudioManager] Assign Music Source and SFX Source in the Inspector.",
                this
            );
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayUISFX(AudioClip clip, float volume = 1f)
    {
        PlayGlobalSFX(clip, volume);
    }

    public void PlayGlobalSFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void PlayWorldSFX(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(volume));
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
}
