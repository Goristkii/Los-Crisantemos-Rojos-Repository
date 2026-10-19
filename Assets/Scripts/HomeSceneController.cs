using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the delayed phone call sequence when returning to the home/street scene.
/// </summary>
public class HomeSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject phoneUI;

    [Header("Dialogues")]
    [TextArea] public string[] internalThoughts;
    [TextArea] public string[] phoneDialogue;

    private Coroutine callSequenceCoroutine;

    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.needToCallFriend)
        {
            if (callSequenceCoroutine != null)
            {
                StopCoroutine(callSequenceCoroutine);
            }

            callSequenceCoroutine = StartCoroutine(StartPhoneCallSequence());
        }
    }

    private void OnDisable()
    {
        if (callSequenceCoroutine != null)
        {
            StopCoroutine(callSequenceCoroutine);
            callSequenceCoroutine = null;
        }
    }

    private IEnumerator StartPhoneCallSequence()
    {
        yield return new WaitForSeconds(2f);

        if (MessageManager.Instance == null)
        {
            Debug.LogWarning("[HomeSceneController] MessageManager instance not found in the scene.");
            yield break;
        }

        MessageManager.Instance.ShowMessages(internalThoughts, OnThoughtsFinished);
    }

    private void OnThoughtsFinished()
    {
        if (phoneUI != null)
        {
            phoneUI.SetActive(true);
        }

        if (MessageManager.Instance == null)
        {
            Debug.LogWarning("[HomeSceneController] MessageManager instance not found in the scene.");
            return;
        }

        MessageManager.Instance.ShowMessages(phoneDialogue, OnPhoneCallFinished);
    }

    private void OnPhoneCallFinished()
    {
        if (phoneUI != null)
        {
            phoneUI.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.needToCallFriend = false;
            
            
            GameManager.Instance.friendCalled = true; 
        }
    }
}
