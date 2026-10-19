using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class FriendStreetController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject friendDialogUI;
    public GameObject friendPortrait;

    [Header("Dialogue")]
    [FormerlySerializedAs("linesBeforeCash")]
    [TextArea(2, 4)]
    public string[] dialogueLines;

    [Header("On Dialogue End")]
    public UnityEvent onDialogueFinished;

    public void StartEncounter()
    {
        if (ActionPanelController.Instance != null)
        {
            ActionPanelController.Instance.ResetPanel();
        }

        if (friendDialogUI != null) friendDialogUI.SetActive(true);
        if (friendPortrait != null) friendPortrait.SetActive(true);

        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(dialogueLines, FinishEncounter);
        }
        else
        {
            FinishEncounter();
        }
    }

    private void FinishEncounter()
    {
        if (GlobalFader.Instance != null)
        {
            GlobalFader.Instance.FadeOutAndIn(() =>
            {
                if (friendPortrait != null) friendPortrait.SetActive(false);
                if (friendDialogUI != null) friendDialogUI.SetActive(false);

                onDialogueFinished?.Invoke();
            });
        }
        else
        {
            if (friendPortrait != null) friendPortrait.SetActive(false);
            if (friendDialogUI != null) friendDialogUI.SetActive(false);

            onDialogueFinished?.Invoke();
        }
    }
}