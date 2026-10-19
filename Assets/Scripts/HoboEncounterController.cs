using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the hobo encounter sequence tailored to the scene environment and UI hierarchy:
/// Dialogue -> Fake QTE -> Sprite/Portrait Swap & Item Spawns -> Final Thoughts -> Cleanup.
/// </summary>
public class HoboEncounterController : MonoBehaviour
{
    [Header("Environment (Scene) Objects")]
    public GameObject sleepingManButton;
    public GameObject manOnFloorSprite;
    public GameObject droppedBottle;
    public GameObject droppedCoins;

    [Header("UI Portrait Objects")]
    public GameObject dialogPortraitPanel;
    public GameObject portraitAngry;
    public GameObject portraitFallen;
    public GameObject mainCharacter;

    [Header("QTE Settings")]
    public GameObject qtePanel;
    public float qteDuration = 5f;
    public GameObject actionPanelRoot;

    [Header("Dialogues")]
    [TextArea] public string[] hoboYells;
    [TextArea] public string[] playerThoughts;

    private Coroutine qteCoroutine;

    private void OnDisable()
    {
        if (qteCoroutine != null)
        {
            StopCoroutine(qteCoroutine);
            qteCoroutine = null;
        }
    }

    /// <summary>
    /// Triggered by clicking sleepingManButton.
    /// </summary>
    public void StartEncounter()
    {
        mainCharacter.SetActive(true);


        if (actionPanelRoot != null)
        {
            actionPanelRoot.SetActive(false);
        }

        if (sleepingManButton != null)
        {
            sleepingManButton.SetActive(false);
        }

        if (dialogPortraitPanel != null)
        {
            dialogPortraitPanel.SetActive(true);
        }

        if (portraitAngry != null)
        {
            portraitAngry.SetActive(true);
        }

        if (portraitFallen != null)
        {
            portraitFallen.SetActive(false);
        }

        if (MessageManager.Instance == null)
        {
            Debug.LogWarning("[HoboEncounterController] MessageManager instance not found in the scene.");
            return;
        }

        MessageManager.Instance.ShowMessages(hoboYells, StartQTEPhase);
    }

    private void StartQTEPhase()
    {
        Action startAction = () =>
        {
            if (qtePanel != null)
            {
                qtePanel.SetActive(true);
            }

            if (qteCoroutine != null)
            {
                StopCoroutine(qteCoroutine);
            }

            qteCoroutine = StartCoroutine(RunQTE());
        };

        if (GlobalFader.Instance != null)
        {
            GlobalFader.Instance.FadeOutAndIn(startAction);
        }
        else
        {
            startAction();
        }
    }

    private IEnumerator RunQTE()
    {
        yield return new WaitForSeconds(qteDuration);

        Action endQTEAction = () =>
        {
            // Hide QTE panel
            if (qtePanel != null)
            {
                qtePanel.SetActive(false);
            }

            // Swap UI Portraits
            if (portraitAngry != null)
            {
                portraitAngry.SetActive(false);
            }

            if (portraitFallen != null)
            {
                mainCharacter.SetActive(false);
                portraitFallen.SetActive(true);
            }

            // Show Environment Objects
            if (manOnFloorSprite != null)
            {
                manOnFloorSprite.SetActive(true);
            }

            if (droppedBottle != null)
            {
                droppedBottle.SetActive(true);
            }

            if (droppedCoins != null)
            {
                droppedCoins.SetActive(true);
            }

            // Show player's aftermath thoughts and clean up portraits on completion
            if (MessageManager.Instance != null)
            {
                MessageManager.Instance.ShowMessages(playerThoughts, EndEncounter);
            }
            else
            {
                EndEncounter();
            }
        };

        if (GlobalFader.Instance != null)
        {
            GlobalFader.Instance.FadeOutAndIn(endQTEAction);
        }
        else
        {
            endQTEAction();
        }
    }

    private void EndEncounter()
    {
        if (dialogPortraitPanel != null)
        {
            dialogPortraitPanel.SetActive(false);
        }

        if (actionPanelRoot != null)
        {
            actionPanelRoot.SetActive(true);
        }

        // Hobo is defeated — refresh navigation so the left arrow appears
        if (GameManager.Instance != null) 
        {
            GameManager.Instance.hoboQTEFinished = true;  
        }
        
        if (LocationManager.Instance != null) 
        {
            LocationManager.Instance.UpdateNavigationButtons(); 
        }
    }
}
