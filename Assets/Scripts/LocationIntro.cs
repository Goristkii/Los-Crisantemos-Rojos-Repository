using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class LocationIntro : MonoBehaviour
{
    [Header("Location Intro Text")]
    [TextArea(2, 4)]
    public string[] introText;

    [Header("Trigger after intro")]
    public UnityEvent onIntroFinished;

    private bool hasPlayed = false;

    private void OnEnable()
    {
        if (!hasPlayed && introText != null && introText.Length > 0)
        {
            hasPlayed = true;
            StartCoroutine(PlayIntroRoutine());
        }
    }

    private IEnumerator PlayIntroRoutine()
    {
        // Small delay to ensure the scene and MessageManager are fully ready
        yield return new WaitForSeconds(0.2f);
        
        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(introText, () =>
            {
                onIntroFinished?.Invoke();
            });
        }
    }
}
