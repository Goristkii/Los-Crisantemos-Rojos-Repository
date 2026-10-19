using UnityEngine;

/// <summary>
/// Controls the branching dialogue encounter with the "Dude" NPC.
/// Strictly handles dialogue UI, the dude's question, choice branching, and rewards.
/// </summary>
public class DudeDialogueController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogUI;
    public GameObject portraitBasic;
    public GameObject portraitAngry;
    public GameObject portraitSmile;
    public GameObject optionsGrid;
    public GameObject actionPanelRoot;

    [Header("Navigation Lock")]
    public GameObject navArrowLeft;
    public GameObject navArrowRight;

    [Header("Dialogues")]
    [TextArea] public string[] dudeQuestionText;
    [TextArea] public string[] aggressiveResponseDialogue;
    [TextArea] public string[] friendlyResponseDialogue;

    private bool isEncounterFinished = false;

    // -------------------------------------------------------------------------
    // Encounter Flow
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts the Dude encounter. If already finished, does nothing.
    /// Wire to the Dude's InteractableItem event or click trigger.
    /// </summary>
    public void StartEncounter()
    {
        if (isEncounterFinished) return;

        if (actionPanelRoot != null)
        {
            actionPanelRoot.SetActive(false);
        }

        if (dialogUI != null)
        {
            dialogUI.SetActive(true);
        }

        SetPortrait(portraitBasic);

        if (optionsGrid != null)
        {
            optionsGrid.SetActive(false);
        }

        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(dudeQuestionText, ShowOptions);
        }
        else
        {
            Debug.LogWarning("[DudeDialogueController] MessageManager instance not found.");
            ShowOptions();
        }
    }

    private void ShowOptions()
    {
        if (navArrowLeft != null) navArrowLeft.SetActive(false);
        if (navArrowRight != null) navArrowRight.SetActive(false);

        if (optionsGrid != null)
        {
            optionsGrid.SetActive(true);
        }
    }

    // -------------------------------------------------------------------------
    // Player Choices (Wire to UI Buttons inside optionsGrid)
    // -------------------------------------------------------------------------

    public void ChooseAggressive()
    {
        if (optionsGrid != null)
        {
            optionsGrid.SetActive(false);
        }

        SetPortrait(portraitAngry);

        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(aggressiveResponseDialogue, CloseDialogue);
        }
        else
        {
            CloseDialogue();
        }
    }

    public void ChooseFriendly()
    {
        if (optionsGrid != null)
        {
            optionsGrid.SetActive(false);
        }

        SetPortrait(portraitSmile);

        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(friendlyResponseDialogue, GiveMoneyAndClose);
        }
        else
        {
            GiveMoneyAndClose();
        }
    }

    // -------------------------------------------------------------------------
    // Resolution
    // -------------------------------------------------------------------------

    private void GiveMoneyAndClose()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(10);
        }

        CloseDialogue();
    }

    private void CloseDialogue()
    {
        if (dialogUI != null)
        {
            dialogUI.SetActive(false);
        }

        if (actionPanelRoot != null)
        {
            actionPanelRoot.SetActive(true);
        }

        if (navArrowLeft != null) navArrowLeft.SetActive(true);
        if (navArrowRight != null) navArrowRight.SetActive(true);

        isEncounterFinished = true;
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private void SetPortrait(GameObject activePortrait)
    {
        if (portraitBasic != null) portraitBasic.SetActive(portraitBasic == activePortrait);
        if (portraitAngry != null) portraitAngry.SetActive(portraitAngry == activePortrait);
        if (portraitSmile != null) portraitSmile.SetActive(portraitSmile == activePortrait);
    }

    private void LateUpdate()
    {
        // ЖЕСТКИЙ КОСТЫЛЬ: Если сетка вариантов ответа активна, 
        // беспощадно выключаем стрелки каждый кадр, перебивая глобальный навигатор.
        if (optionsGrid != null && optionsGrid.activeSelf)
        {
            if (navArrowLeft != null) navArrowLeft.SetActive(false);
            if (navArrowRight != null) navArrowRight.SetActive(false);
        }
    }
}
