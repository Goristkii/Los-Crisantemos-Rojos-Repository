using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionPanelController : MonoBehaviour
{
    public static ActionPanelController Instance { get; private set; }

    public enum ActionType { None, Mirar, Cojer, Usar, Abrir }

    public ActionType currentAction = ActionType.None;
    private bool isOpen = false;
    private RectTransform panelRectTransform;
    private Canvas parentCanvas;

    [Header("Animator")]
    public Animator panelAnimator;

    [Header("Action Buttons")]
    public Button btnMirar;
    public Button btnCojer;
    public Button btnUsar;
    public Button btnAbrir;

    [Header("Button Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    [Header("Inventory UI")]
    public GameObject moneyIcon;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI bottlesText;
    public GameObject pipeIcon;
    public GameObject bouquetIcon;
    public GameObject bottleIcon;

    [Header("Inventory Descriptions (For Mirar)")]
    [TextArea(2, 3)] public string[] moneyDescription = { "Несколько монет. Не густо, но хоть что-то." };
    [TextArea(2, 3)] public string[] bottlesDescription = { "Пустые стеклянные бутылки. Можно сдать или применить." };
    [TextArea(2, 3)] public string[] pipeDescription = { "Тяжелая металлическая труба. Выглядит надежно." };
    [TextArea(2, 3)] public string[] bouquetDescription = { "Красивый букет цветов." };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        panelRectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        UpdateInventoryUI();
        CheckAutoClose();
    }

    private void CheckAutoClose()
    {
        // Если панель открыта и выбрано действие — следим, не ушла ли мышка с панели
        if (isOpen && currentAction != ActionType.None && panelRectTransform != null)
        {
            Camera cam = null;
            if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = parentCanvas.worldCamera;
            }

            bool isMouseInside = RectTransformUtility.RectangleContainsScreenPoint(
                panelRectTransform, 
                Input.mousePosition, 
                cam);

            // Как только курсор вышел за пределы прямоугольника панели — закрываем её!
            if (!isMouseInside)
            {
                isOpen = false;
                if (panelAnimator != null)
                {
                    panelAnimator.SetBool("IsOpen", false);
                }
            }
        }
    }

    public void TogglePanel()
    {
        isOpen = !isOpen;
        if (panelAnimator != null)
        {
            panelAnimator.SetBool("IsOpen", isOpen);
        }

        if (isOpen)
        {
            TutorialManager.Instance?.OnPanelOpened();
        }
        else
        {
            currentAction = ActionType.None;
            UpdateButtonVisuals();
        }
    }

    public void ResetPanel()
    {
        isOpen = false;
        if (panelAnimator != null)
        {
            panelAnimator.SetBool("IsOpen", false);
        }
        currentAction = ActionType.None;
        UpdateButtonVisuals();
    }

    public void SelectAction(int actionIndex)
    {
        ActionType selected = actionIndex switch
        {
            1 => ActionType.Mirar,
            2 => ActionType.Cojer,
            3 => ActionType.Usar,
            4 => ActionType.Abrir,
            _ => ActionType.None
        };

        if (currentAction == selected)
            currentAction = ActionType.None;
        else
            currentAction = selected;

        UpdateButtonVisuals();
    }

    public void InspectInventoryItem(int itemIndex)
    {
        if (currentAction != ActionType.Mirar) return;

        string[] descToShow = itemIndex switch
        {
            1 => moneyDescription,
            2 => bottlesDescription,
            3 => pipeDescription,
            4 => bouquetDescription,
            _ => null
        };

        if (descToShow != null && descToShow.Length > 0 && MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(descToShow);
            ResetPanel();
        }
    }

    public void UpdateButtonVisuals()
    {
        SetButtonColor(btnMirar, currentAction == ActionType.Mirar);
        SetButtonColor(btnCojer, currentAction == ActionType.Cojer);
        SetButtonColor(btnUsar,  currentAction == ActionType.Usar);
        SetButtonColor(btnAbrir, currentAction == ActionType.Abrir);
    }

    private void SetButtonColor(Button button, bool isSelected)
    {
        if (button == null) return;
        Image img = button.GetComponent<Image>();
        if (img != null) img.color = isSelected ? selectedColor : normalColor;
    }

    public void UpdateInventoryUI()
    {
        if (GameManager.Instance == null) return;

        bool hasMoney = GameManager.Instance.playerMoney > 0;
        if (moneyIcon != null) moneyIcon.SetActive(hasMoney);
        if (moneyText != null)
        {
            moneyText.gameObject.SetActive(hasMoney);
            if (hasMoney) moneyText.text = GameManager.Instance.playerMoney.ToString();
        }

        bool hasBottles = GameManager.Instance.bottleCount > 0;
        if (bottleIcon != null) bottleIcon.SetActive(hasBottles);
        if (bottlesText != null)
        {
            bottlesText.gameObject.SetActive(hasBottles);
            if (hasBottles) bottlesText.text = GameManager.Instance.bottleCount.ToString();
        }

        if (pipeIcon != null) pipeIcon.SetActive(GameManager.Instance.hasMetalPipe);
        if (bouquetIcon != null) bouquetIcon.SetActive(GameManager.Instance.hasFlower);
    }
}