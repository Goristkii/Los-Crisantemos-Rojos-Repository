using UnityEngine;

public class BigStoneController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject stonePortrait;

    [Header("Dialogues")]
    [TextArea(2, 3)] public string[] normalEntryText;
    [TextArea(5, 10)] public string[] hugeStoneText;

    [Header("Panel Heights")]
    public float normalHeight = 250f;
    public float hugeHeight = 800f;

    [Header("Позиционирование (Y-координата)")]
    public float hugePosY = 0f; // Target Y anchored position when the panel is huge
    private float originalPosY; // To save the normal Y position dynamically
    private bool isYPosSaved = false;

    public void StartEncounter()
    {
        if (stonePortrait != null) stonePortrait.SetActive(true);

        if (MessageManager.Instance != null && MessageManager.Instance.panelGroup != null)
        {
            RectTransform rect = MessageManager.Instance.panelGroup.GetComponent<RectTransform>();
            if (rect != null && !isYPosSaved)
            {
                originalPosY = rect.anchoredPosition.y;
                isYPosSaved = true;
            }
        }

        // Apply normal size & position
        SetPanelHeightAndPos(normalHeight, originalPosY);
        MessageManager.Instance.ShowMessages(normalEntryText, ShowHugeText);
    }

    private void ShowHugeText()
    {
        SetPanelHeightAndPos(hugeHeight, hugePosY);
        MessageManager.Instance.ShowMessages(hugeStoneText, FinishEncounter);
    }

    private void FinishEncounter()
    {
        SetPanelHeightAndPos(normalHeight, originalPosY);
        if (stonePortrait != null) stonePortrait.SetActive(false);
    }

    private void SetPanelHeightAndPos(float newHeight, float newPosY)
    {
        if (MessageManager.Instance != null && MessageManager.Instance.panelGroup != null)
        {
            RectTransform rect = MessageManager.Instance.panelGroup.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Adjust the size and position on the Y axis
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, newPosY);
            }
        }
    }
}
