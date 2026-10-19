using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages a step-by-step tutorial sequence.
/// Controls a top UI panel with instructions and listens for story events
/// (panel opened, hobo looked at, hobo used) to advance through the steps.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Tutorial Step Tracking
    // -------------------------------------------------------------------------

    private enum TutorialStep
    {
        Intro,          // Showing introductory inner thoughts
        WaitOpenPanel,  // Prompting player to open the action panel
        WaitMirar,      // Prompting player to use Mirar on the hobo
        WaitUsar,       // Prompting player to use Usar on the hobo
        Done            // Tutorial complete
    }

    private TutorialStep currentStep = TutorialStep.Intro;
    public bool IsTutorialDone => currentStep == TutorialStep.Done;

    // -------------------------------------------------------------------------
    // UI References
    // -------------------------------------------------------------------------

    [Header("UI References")]
    public GameObject topTextPanel;
    public TextMeshProUGUI topText;

    // -------------------------------------------------------------------------
    // Text Content
    // -------------------------------------------------------------------------

    [Header("Intro Thoughts")]
    [TextArea] public string[] introThoughts;

    [Header("Tutorial Prompt Texts")]
    [TextArea] public string promptOpenPanel;
    [TextArea] public string promptMirar;
    [TextArea] public string promptUsar;

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    public GameObject actionPanelRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Make sure the top panel starts hidden
        if (topTextPanel != null)
        {
            topTextPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Waiting for IntroSequenceController to trigger StartTutorialSequence()
    }

    /// <summary>
    /// Starts the tutorial sequence with the introductory inner thoughts.
    /// Called by IntroSequenceController once the bus arrival sequence concludes.
    /// </summary>
    public void StartTutorialSequence()
    {
        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(introThoughts, ShowOpenPanelPrompt);
        }
        else
        {
            Debug.LogWarning("[TutorialManager] MessageManager instance not found.");
        }
    }

    // -------------------------------------------------------------------------
    // Tutorial Steps
    // -------------------------------------------------------------------------

    /// <summary>
    /// Step 1 callback: intro thoughts finished.
    /// Show the "open action panel" prompt.
    /// </summary>
    private void ShowOpenPanelPrompt()
    {
        currentStep = TutorialStep.WaitOpenPanel;
        ShowTopText(promptOpenPanel);

        if (actionPanelRoot != null)
        {
            actionPanelRoot.SetActive(true);
        }
        
    }

    /// <summary>
    /// Called by ActionPanelController.TogglePanel() when the panel is opened.
    /// Only advances if the player is on the correct step.
    /// </summary>
    public void OnPanelOpened()
    {
        if (currentStep != TutorialStep.WaitOpenPanel) return;

        currentStep = TutorialStep.WaitMirar;
        ShowTopText(promptMirar);
    }

    /// <summary>
    /// Called by InteractableItem after the tutorial hobo is looked at (Mirar).
    /// Hides the top panel while the dialogue plays.
    /// </summary>
    public void OnHoboLookedAt()
    {
        HideTopText();
    }

    /// <summary>
    /// Called by InteractableItem as the callback after the Mirar dialogue closes.
    /// Shows the "use Usar on the hobo" prompt.
    /// </summary>
    public void ShowUsarPrompt()
    {
        currentStep = TutorialStep.WaitUsar;
        ShowTopText(promptUsar);
    }

    /// <summary>
    /// Called by InteractableItem when the tutorial hobo is interacted with using Usar.
    /// Hides the top panel permanently (tutorial complete).
    /// </summary>
    public void OnHoboUsed()
    {
        currentStep = TutorialStep.Done;
        HideTopText();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void ShowTopText(string text)
    {
        if (topText != null)
        {
            topText.text = text;
        }

        if (topTextPanel != null)
        {
            topTextPanel.SetActive(true);
        }
    }

    private void HideTopText()
    {
        if (topTextPanel != null)
        {
            topTextPanel.SetActive(false);
        }
    }
}
