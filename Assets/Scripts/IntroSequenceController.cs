using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the introductory bus sequence before main gameplay and tutorial start:
/// 1. Plays bus inner thoughts dialogue in scene0Bus.
/// 2. Cycles through bus arrival image panels sequentially (BusComes -> DoorsOpen -> Exit).
/// 3. Hands off control to LocationManager and TutorialManager.
/// </summary>
public class IntroSequenceController : MonoBehaviour
{

    public GameObject actionPanelRoot;

    [Header("Developer Settings")]
    public bool skipIntroAndTutorial = false;


    [Header("Scene 0 — Initial Bus")]
    public GameObject scene0Bus;
    [TextArea] public string[] busDialogue;

    [Header("Timings")]
    public float startDelay = 3f;

    [Header("Scene 1 — Bus Arrival Sequence")]
    public GameObject scene1BusComing;
    public GameObject[] busPanels; // BusComes, DoorsOpen, Exit
    public float delayBetweenPanels = 2f;

    private void Start()
    {

        if (skipIntroAndTutorial)
        {
            
            if (scene0Bus != null) scene0Bus.SetActive(false);
            
            if (scene1BusComing != null) scene1BusComing.SetActive(false); 

            
            if (actionPanelRoot != null) actionPanelRoot.SetActive(true);

            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.hoboQTEFinished = true;
            }

            
            if (LocationManager.Instance != null)
            {
                LocationManager.Instance.InitializeGame();
                LocationManager.Instance.UpdateNavigationButtons();
            }

            
            return; 
        }

        if (actionPanelRoot != null)
        {
            actionPanelRoot.SetActive(false); 
        }


        if (scene1BusComing != null)
        {
            scene1BusComing.SetActive(false);
        }

        if (scene0Bus != null)
        {
            scene0Bus.SetActive(true);
        }

        StartCoroutine(StartInitialDialogueRoutine());
    }

    private IEnumerator StartInitialDialogueRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(busDialogue, OnBusDialogueFinished);
        }
        else
        {
            Debug.LogWarning("[IntroSequenceController] MessageManager instance not found in scene.");
        }
    }

    private void OnBusDialogueFinished()
    {
        StartCoroutine(PlayBusArrivalSequence());
    }

    private IEnumerator PlayBusArrivalSequence()
    {
        // 1. Transition from initial bus (scene0Bus) to the bus arrival sequence (scene1BusComing)
        if (GlobalFader.Instance != null)
        {
            GlobalFader.Instance.FadeOutAndIn(() =>
            {
                if (scene0Bus != null) scene0Bus.SetActive(false);
                if (scene1BusComing != null) scene1BusComing.SetActive(true);

                if (busPanels != null)
                {
                    for (int j = 0; j < busPanels.Length; j++)
                    {
                        if (busPanels[j] != null) busPanels[j].SetActive(j == 0);
                    }
                }
            });

            while (GlobalFader.Instance.IsFading)
            {
                yield return null;
            }
        }
        else
        {
            if (scene0Bus != null) scene0Bus.SetActive(false);
            if (scene1BusComing != null) scene1BusComing.SetActive(true);

            if (busPanels != null)
            {
                for (int j = 0; j < busPanels.Length; j++)
                {
                    if (busPanels[j] != null) busPanels[j].SetActive(j == 0);
                }
            }
        }

        // 2. Cycle through the arrival panels: Show Frame -> Wait -> Fade Out -> Swap -> Fade In
        if (busPanels != null && busPanels.Length > 0)
        {
            // Frame 1 is currently active and visible
            yield return new WaitForSeconds(delayBetweenPanels);

            for (int i = 1; i < busPanels.Length; i++)
            {
                int nextIndex = i;
                int prevIndex = i - 1;

                if (GlobalFader.Instance != null)
                {
                    GlobalFader.Instance.FadeOutAndIn(() =>
                    {
                        if (busPanels[prevIndex] != null) busPanels[prevIndex].SetActive(false);
                        if (busPanels[nextIndex] != null) busPanels[nextIndex].SetActive(true);
                    });

                    while (GlobalFader.Instance.IsFading)
                    {
                        yield return null;
                    }
                }
                else
                {
                    if (busPanels[prevIndex] != null) busPanels[prevIndex].SetActive(false);
                    if (busPanels[nextIndex] != null) busPanels[nextIndex].SetActive(true);
                }

                yield return new WaitForSeconds(delayBetweenPanels);
            }
        }

        // 3. Final transition: Fade from last panel into the main gameplay station
        if (GlobalFader.Instance != null)
        {
            GlobalFader.Instance.FadeOutAndIn(() =>
            {
                if (busPanels != null)
                {
                    for (int j = 0; j < busPanels.Length; j++)
                    {
                        if (busPanels[j] != null) busPanels[j].SetActive(false);
                    }
                }

                if (scene1BusComing != null) scene1BusComing.SetActive(false);

                if (LocationManager.Instance != null)
                {
                    LocationManager.Instance.InitializeGame();
                }

                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.StartTutorialSequence();
                }
            });

            while (GlobalFader.Instance.IsFading)
            {
                yield return null;
            }
        }
        else
        {
            if (busPanels != null)
            {
                for (int j = 0; j < busPanels.Length; j++)
                {
                    if (busPanels[j] != null) busPanels[j].SetActive(false);
                }
            }

            if (scene1BusComing != null) scene1BusComing.SetActive(false);

            if (LocationManager.Instance != null)
            {
                LocationManager.Instance.InitializeGame();
            }

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.StartTutorialSequence();
            }
        }
    }
}
