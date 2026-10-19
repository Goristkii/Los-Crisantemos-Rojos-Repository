using UnityEngine;

/// <summary>
/// Global manager for handling sound effects and background music with smooth volume fading.
/// Persists across all scenes using the Singleton pattern.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Volume Fading")]
    public float volumeFadeSpeed = 2f;
    private float targetMusicVolume = 1f;

    private AudioClip savedMusic;
    private float savedVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, targetMusicVolume, volumeFadeSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Plays a sound effect without cutting off other sounds.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Plays background music with a target volume. If the track is already playing,
    /// it smoothly fades to the new target volume without restarting.
    /// </summary>
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null || musicSource == null) return;

        targetMusicVolume = volume;

        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.volume = 0f;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Stops the currently playing background music and clears the clip reference.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            musicSource.volume = 0f;
        }
        targetMusicVolume = 0f;
    }

    /// <summary>
    /// Temporarily plays a music track (e.g. during a QTE or encounter), saving the previous track and volume.
    /// </summary>
    public void PlayTempMusic(AudioClip tempClip, float tempVolume = 1f)
    {
        if (musicSource != null)
        {
            savedMusic = musicSource.clip;
            savedVolume = targetMusicVolume;
        }

        PlayMusic(tempClip, tempVolume);
    }

    /// <summary>
    /// Restores the previously saved music track and volume.
    /// </summary>
    public void RestoreMusic()
    {
        if (savedMusic != null)
        {
            PlayMusic(savedMusic, savedVolume);
            savedMusic = null;
        }
    }
}
