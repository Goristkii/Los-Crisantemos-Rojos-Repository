using UnityEngine;

/// <summary>
/// Triggers location-specific ambient/music tracks with volume control or silences music when the location GameObject is enabled.
/// </summary>
public class LocationMusicTrigger : MonoBehaviour
{
    [Header("Location Music Settings")]
    public AudioClip locationMusic;
    [Range(0f, 1f)] public float locationVolume = 1f;
    public bool stopMusicInstead = false;

    private void OnEnable()
    {
        if (stopMusicInstead)
        {
            AudioManager.Instance?.StopMusic();
        }
        else if (locationMusic != null)
        {
            AudioManager.Instance?.PlayMusic(locationMusic, locationVolume);
        }
    }
}
