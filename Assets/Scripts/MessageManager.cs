using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    public bool isDialogueActive = false;

    [Header("UI References")]
    [SerializeField] public CanvasGroup panelGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Optional UI Visibility")]
    public GameObject actionPanelRoot;
    private bool wasPanelActive = false;

    private string[] currentLines;
    private int currentIndex;
    private Coroutine fadeCoroutine;
    private Action onCompleteCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start hidden
        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        // Hook up buttons
        nextButton.onClick.AddListener(OnNextClicked);
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    public void ShowMessages(string[] lines, Action onComplete = null)
    {
        // Сначала проверяем, есть ли вообще текст
        if (lines == null || lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Текст есть — включаем режим диалога
        isDialogueActive = true;

        // Save whether the action panel was active, then hide it while dialogue is shown
        if (actionPanelRoot != null)
        {
            wasPanelActive = actionPanelRoot.activeSelf;
            actionPanelRoot.SetActive(false);
        }

        currentLines = lines;
        currentIndex = 0;
        onCompleteCallback = onComplete;

        DisplayCurrentLine();
        StartFade(1f);
    }

    private void DisplayCurrentLine()
    {
        messageText.text = currentLines[currentIndex];
        RefreshButtonVisibility();
    }

    private void RefreshButtonVisibility()
    {
        bool isLastLine = currentIndex >= currentLines.Length - 1;
        nextButton.gameObject.SetActive(!isLastLine);
        closeButton.gameObject.SetActive(isLastLine);
    }

    private void OnNextClicked()
    {
        if (currentLines == null) return;

        if (currentIndex < currentLines.Length - 1)
        {
            currentIndex++;
            DisplayCurrentLine();
        }
    }

    private void OnCloseClicked()
    {
        StartFade(0f);
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = panelGroup.alpha;
        float elapsed = 0f;

        if (targetAlpha > 0f)
        {
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            panelGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        panelGroup.alpha = targetAlpha;

        if (targetAlpha <= 0f)
        {
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;

            if (actionPanelRoot != null && wasPanelActive)
            {
                actionPanelRoot.SetActive(true);
            }

            // Выключаем флаг перед самым вызовом коллбэка (если коллбэк запустит новый диалог, он тут же снова станет true)
            isDialogueActive = false;

            Action callback = onCompleteCallback;
            onCompleteCallback = null;
            callback?.Invoke();
        }
    }
}