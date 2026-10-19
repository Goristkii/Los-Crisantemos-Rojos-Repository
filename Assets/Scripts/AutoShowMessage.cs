using System.Collections;
using UnityEngine;

/// <summary>
/// Attach this to any UI panel (scene) to automatically show a message
/// when the panel becomes active, after a configurable delay.
/// Ignores activation events that occur during the startup initialization phase.
/// </summary>
public class AutoShowMessage : MonoBehaviour
{
    [TextArea]
    public string[] startMessages;

    public float delay = 1.5f;

    private Coroutine showCoroutine;

    private void OnEnable()
    {
        // Ignore any OnEnable calls that happen during scene startup initialization
        if (Time.timeSinceLevelLoad < 0.2f) return;

        showCoroutine = StartCoroutine(ShowAfterDelay());
    }

    private void OnDisable()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        if (startMessages != null && startMessages.Length > 0)
        {
            if (MessageManager.Instance == null)
            {
                Debug.LogWarning("[AutoShowMessage] MessageManager instance not found in the scene.");
                yield break;
            }

            MessageManager.Instance.ShowMessages(startMessages);
        }
    }
}
