using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalCutsceneController : MonoBehaviour
{
    [Header("Screens/Shots")]
    public GameObject shotElevator;
    public GameObject shotEntrance;
    public GameObject shotDoor;
    public GameObject blackScreenPanel;

    [Header("Fader")]
    public CanvasGroup fadeCanvasGroup;

    [Header("Timings")]
    public float elevatorDuration = 3f;
    public float entranceDuration = 3f;
    public float darkWalkDuration = 4f;
    public float momSpeechDuration = 4f;
    public float delayBeforeMomSpeaks = 1.5f;
    public float typingSpeed = 0.05f;
    public float delayBeforeMainMenu = 2f;
    public float fadeDuration = 1f;

    [Header("Dialogues")]
    [TextArea(2, 4)] public string[] protagonistThoughts;
    [TextArea] public string momSpeechText;
    public TextMeshProUGUI momSpeechTextUI;

    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Global UI to Hide")]
    public GameObject[] uiToHide;

    public void StartFinalCutscene()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        // Loop through and hide all specified global UI elements
        if (uiToHide != null)
        {
            foreach (var ui in uiToHide)
            {
                if (ui != null)
                {
                    ui.SetActive(false);
                }
            }
        }

        // Ensure fader starts fully transparent
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
        }

        // Initialize shots and screens
        if (shotEntrance != null) shotEntrance.SetActive(false);
        if (shotDoor != null) shotDoor.SetActive(false);
        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);

        if (shotElevator != null) shotElevator.SetActive(true);

        // Start the main sequence
        StartCoroutine(MainCutsceneSequence());
    }

    private IEnumerator MainCutsceneSequence()
    {
        // 1. Elevator shot
        yield return new WaitForSeconds(elevatorDuration);

        // Fade to black
        yield return StartCoroutine(FadeRoutine(0f, 1f, fadeDuration));

        // Switch to entrance shot
        if (shotElevator != null) shotElevator.SetActive(false);
        if (shotEntrance != null) shotEntrance.SetActive(true);

        // Fade to clear
        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeDuration));

        // 2. Entrance shot
        yield return new WaitForSeconds(entranceDuration);

        // Fade to black
        yield return StartCoroutine(FadeRoutine(0f, 1f, fadeDuration));

        // Wait in pure darkness between corridor and door
        yield return new WaitForSeconds(darkWalkDuration);

        // Switch to door shot
        if (shotEntrance != null) shotEntrance.SetActive(false);
        if (shotDoor != null) shotDoor.SetActive(true);

        // Fade to clear
        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeDuration));

        // 3. Dialogue
        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(protagonistThoughts, OnProtagonistThoughtsFinished);
        }
        else
        {
            OnProtagonistThoughtsFinished();
        }
    }

    private void OnProtagonistThoughtsFinished()
    {
        StartCoroutine(TransitionToMomSpeech());
    }

    private IEnumerator TransitionToMomSpeech()
    {
        // Smoothly fade to black
        yield return StartCoroutine(FadeRoutine(0f, 1f, fadeDuration));

        // Safely activate black screen panel
        if (blackScreenPanel != null) blackScreenPanel.SetActive(true);
        if (momSpeechTextUI != null) momSpeechTextUI.text = "";

        // Start the typewriter effect
        StartCoroutine(TypewriterMomSpeech());
    }

    private IEnumerator TypewriterMomSpeech()
    {
        yield return new WaitForSeconds(delayBeforeMomSpeaks);

        if (momSpeechTextUI != null && !string.IsNullOrEmpty(momSpeechText))
        {
            foreach (char c in momSpeechText)
            {
                momSpeechTextUI.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        yield return new WaitForSeconds(momSpeechDuration);

        // Clear text to leave the screen in pure darkness
        if (momSpeechTextUI != null) momSpeechTextUI.text = string.Empty;

        // Wait in pure darkness
        yield return new WaitForSeconds(delayBeforeMainMenu);

        // Load Main Menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.alpha = startAlpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
