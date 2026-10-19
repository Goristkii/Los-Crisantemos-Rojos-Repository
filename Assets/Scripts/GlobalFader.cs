using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton manager for smooth black screen transitions during physical scene/location changes.
/// Never triggers during regular dialogue or text scrolling in MessageManager.
/// </summary>
public class GlobalFader : MonoBehaviour
{
    public static GlobalFader Instance { get; private set; }

    [Header("Fader Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;

    public bool IsFading { get; private set; } = false;

    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
    }

    /// <summary>
    /// Fades out to black, switches active GameObjects, executes mid-fade logic, and fades back in.
    /// </summary>
    public void FadeToLocation(GameObject currentLocation, GameObject targetLocation, Action onMiddleOfFade = null)
    {
        if (fadeCanvasGroup == null)
        {
            if (currentLocation != null) currentLocation.SetActive(false);
            if (targetLocation != null) targetLocation.SetActive(true);
            onMiddleOfFade?.Invoke();
            return;
        }

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeToLocationRoutine(currentLocation, targetLocation, onMiddleOfFade));
    }

    /// <summary>
    /// Fades out to black, executes mid-fade logic, and fades back in.
    /// Used for scripted transitions (e.g. dialogues triggering scene changes).
    /// </summary>
    public void FadeOutAndIn(Action onMiddleOfFade)
    {
        if (fadeCanvasGroup == null)
        {
            onMiddleOfFade?.Invoke();
            return;
        }

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeOutAndInRoutine(onMiddleOfFade));
    }

    private IEnumerator FadeToLocationRoutine(GameObject currentLocation, GameObject targetLocation, Action onMiddleOfFade)
    {
        IsFading = true;
        fadeCanvasGroup.blocksRaycasts = true;

        // Smooth fade to black
        yield return StartCoroutine(FadeRoutine(fadeCanvasGroup.alpha, 1f, fadeDuration));

        // Location swap in pure darkness
        if (currentLocation != null) currentLocation.SetActive(false);
        if (targetLocation != null) targetLocation.SetActive(true);

        onMiddleOfFade?.Invoke();

        // Smooth fade to clear
        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeDuration));

        fadeCanvasGroup.blocksRaycasts = false;
        IsFading = false;
        currentFadeCoroutine = null;
    }

    private IEnumerator FadeOutAndInRoutine(Action onMiddleOfFade)
    {
        IsFading = true;
        fadeCanvasGroup.blocksRaycasts = true;

        // Smooth fade to black
        yield return StartCoroutine(FadeRoutine(fadeCanvasGroup.alpha, 1f, fadeDuration));

        // Execute logic in pure darkness
        onMiddleOfFade?.Invoke();

        // Smooth fade to clear
        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeDuration));

        fadeCanvasGroup.blocksRaycasts = false;
        IsFading = false;
        currentFadeCoroutine = null;
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, float duration)
    {
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
