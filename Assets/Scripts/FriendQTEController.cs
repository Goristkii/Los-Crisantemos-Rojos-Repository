using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class FriendQTEController : MonoBehaviour
{
    [Header("QTE Settings")]
    public int keysToPress = 15;
    public float timeLimit = 10f;
    public string[] buttonKeys = { "Q", "D", "E", "A" };

    [Header("UI References")]
    public Slider timeSlider;
    public RectTransform spawnArea;
    public GameObject[] qteButtons; 
    public TextMeshProUGUI progressText;

    [Header("On Game End")]
    public UnityEvent onWin;
    public UnityEvent onLose;

    private int currentPresses;
    private float timer;
    private bool isPlaying = false;
    private int currentActiveIndex = -1;

    void OnEnable()
    {
        currentPresses = 0;
        timer = timeLimit;
        isPlaying = true;

        if (timeSlider != null)
        {
            timeSlider.maxValue = timeLimit;
            timeSlider.value = timeLimit;
        }

        GenerateNextKey();
    }

    void Update()
    {
        if (!isPlaying) return;

        timer -= Time.deltaTime;
        
        if (timeSlider != null) timeSlider.value = timer;

        if (timer <= 0)
        {
            LoseGame();
            return;
        }

        // РЕАГИРУЕМ ТОЛЬКО НА КЛАВИАТУРУ
        if (Input.anyKeyDown && currentActiveIndex != -1)
        {
            string inputStr = Input.inputString.ToUpper();
            string expectedKey = buttonKeys[currentActiveIndex].ToUpper();
            
            if (!string.IsNullOrEmpty(inputStr) && inputStr.Contains(expectedKey))
            {
                RegisterHit(); // Засчитываем правильное нажатие!
            }
        }
    }

    // Скрыли этот метод, чтобы нельзя было вызвать его мышкой
    private void RegisterHit()
    {
        currentPresses++;
            
        if (currentPresses >= keysToPress) WinGame();
        else GenerateNextKey();
    }

    private void GenerateNextKey()
    {
        for (int i = 0; i < qteButtons.Length; i++)
        {
            if (qteButtons[i] != null) qteButtons[i].SetActive(false);
        }

        currentActiveIndex = Random.Range(0, qteButtons.Length);
        
        if (qteButtons[currentActiveIndex] != null && spawnArea != null)
        {
            RectTransform btnRect = qteButtons[currentActiveIndex].GetComponent<RectTransform>();
            
            float paddingX = btnRect.rect.width / 2f;
            float paddingY = btnRect.rect.height / 2f;
            
            float randomX = Random.Range(-spawnArea.rect.width / 2f + paddingX, spawnArea.rect.width / 2f - paddingX);
            float randomY = Random.Range(-spawnArea.rect.height / 2f + paddingY, spawnArea.rect.height / 2f - paddingY);

            btnRect.anchoredPosition = new Vector2(randomX, randomY);
            qteButtons[currentActiveIndex].SetActive(true);
        }
        
        if (progressText != null) progressText.text = $"{currentPresses}/{keysToPress}";
    }

    private void WinGame()
    {
        isPlaying = false;
        HideAllButtons();
        onWin?.Invoke();
    }

    private void LoseGame()
    {
        isPlaying = false;
        HideAllButtons();
        onLose?.Invoke();
    }

    private void HideAllButtons()
    {
        for (int i = 0; i < qteButtons.Length; i++)
        {
            if (qteButtons[i] != null) qteButtons[i].SetActive(false);
        }
    }
}