using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Cursor Textures")]
    public Texture2D defaultCursor;
    public Texture2D hoverCursor;
    public Texture2D clickCursor;

    [Header("Settings")]
    public Vector2 hotspot = Vector2.zero;

    private InteractableItem currentHoveredItem;
    private Texture2D currentCursorTexture = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplyCursor(defaultCursor);
    }

    public void SetHoveredItem(InteractableItem item)
    {
        currentHoveredItem = item;
    }

    public void ClearHoveredItem(InteractableItem item)
    {
        if (currentHoveredItem == item)
        {
            currentHoveredItem = null;
        }
    }

    private void Update()
    {
        bool canInteract = currentHoveredItem != null && 
                           ActionPanelController.Instance != null && 
                           ActionPanelController.Instance.currentAction != ActionPanelController.ActionType.None &&
                           currentHoveredItem.SupportsAction(ActionPanelController.Instance.currentAction);

        Texture2D targetCursor;

        if (Input.GetMouseButton(0))
        {
            targetCursor = clickCursor != null ? clickCursor : defaultCursor;
        }
        else if (canInteract)
        {
            targetCursor = hoverCursor != null ? hoverCursor : defaultCursor;
        }
        else
        {
            targetCursor = defaultCursor;
        }

        if (targetCursor != currentCursorTexture)
        {
            ApplyCursor(targetCursor);
        }
    }

    private void ApplyCursor(Texture2D cursorTexture)
    {
        currentCursorTexture = cursorTexture;
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }

    private void OnDisable()
    {
        currentHoveredItem = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyCursor(currentCursorTexture != null ? currentCursorTexture : defaultCursor);
        }
    }
}
