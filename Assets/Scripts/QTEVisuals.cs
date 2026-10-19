using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct QTEKeyBinding
{
    public RectTransform keyUI; //
    public KeyCode targetKey;   //
}

public class QTEVisuals : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider timerSlider;
    public RectTransform spawnArea;
    
    [Header("QTE Settings")]
    public float qteDuration = 5f;
    public QTEKeyBinding[] availableKeys;

    private RectTransform currentActiveUI;
    private KeyCode currentActiveKey;
    private Coroutine sliderCoroutine;

    private void OnEnable()
    {
        // 
        foreach (var binding in availableKeys)
        {
            if (binding.keyUI != null)
                binding.keyUI.gameObject.SetActive(false);
        }

        // 
        if (sliderCoroutine != null) StopCoroutine(sliderCoroutine);
        sliderCoroutine = StartCoroutine(AnimateSlider());

        // 
        ShowRandomKey();
    }

    private void Update()
    {
        // 1. 
        if (Input.anyKeyDown)
        {
            Debug.Log("KeyPressed!");
        }

        if (currentActiveUI != null && currentActiveUI.gameObject.activeSelf)
        {
            // 2. 
            if (Input.GetKeyDown(currentActiveKey))
            {
                Debug.Log("BINGO " + currentActiveKey);
                ShowRandomKey(); 
            }
        }
    }

    private void ShowRandomKey()
    {
        //
        if (currentActiveUI != null)
        {
            currentActiveUI.gameObject.SetActive(false);
        }

        if (availableKeys == null || availableKeys.Length == 0) return;

        // 
        int randomIndex = Random.Range(0, availableKeys.Length);
        QTEKeyBinding newBinding = availableKeys[randomIndex];

        currentActiveUI = newBinding.keyUI;
        currentActiveKey = newBinding.targetKey;

        // 
        if (currentActiveUI != null && spawnArea != null)
        {
            float randomX = Random.Range(-spawnArea.rect.width / 2f, spawnArea.rect.width / 2f);
            float randomY = Random.Range(-spawnArea.rect.height / 2f, spawnArea.rect.height / 2f);
            
            currentActiveUI.anchoredPosition = new Vector2(randomX, randomY);
            currentActiveUI.gameObject.SetActive(true);
        }
    }

    private IEnumerator AnimateSlider()
    {
        float elapsed = 0f;
        if (timerSlider == null) yield break;

        while (elapsed < qteDuration)
        {
            elapsed += Time.deltaTime;
            timerSlider.value = Mathf.Lerp(timerSlider.maxValue, timerSlider.minValue, elapsed / qteDuration);
            yield return null;
        }
    }
}
