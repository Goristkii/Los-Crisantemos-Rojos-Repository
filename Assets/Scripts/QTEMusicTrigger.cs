using UnityEngine;

/// <summary>
/// Plays tension music during QTE / mini-game encounters and restores previous music upon completion.
/// </summary>
public class QTEMusicTrigger : MonoBehaviour
{
    [Header("QTE Tension Music")]
    public AudioClip tensionMusic;
    [Range(0f, 1f)] public float tensionVolume = 1f;

    private void OnEnable()
    {
        AudioManager.Instance?.PlayTempMusic(tensionMusic, tensionVolume);
    }

    private void OnDisable()
    {
        AudioManager.Instance?.RestoreMusic();
    }
}
