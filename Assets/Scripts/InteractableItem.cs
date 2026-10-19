using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum RequiredItem { None, MetalPipe, BeerBottle, Flower, MoneyForFlowers }

[System.Serializable]
public struct MessageSequence
{
    [TextArea]
    public string[] messages;
}

/// <summary>
/// Attach to any interactive UI GameObject with a Button component.
/// Responds differently based on the action currently selected in ActionPanelController.
/// Each action has its own dialogue sequence array with independent progression tracking.
/// Optional UnityEvents trigger after the dialogue sequence has completed playing.
/// Supports item requirements for the 'Usar' action.
/// After triggering an action, the action panel is automatically closed and reset.
/// </summary>
[RequireComponent(typeof(Button))]
public class InteractableItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Mirar (Look)")]
    public MessageSequence[] mirarSequences;

    [Header("Cojer (Take)")]
    public MessageSequence[] cojerSequences;

    [Header("Usar (Use)")]
    public MessageSequence[] usarSequences;

    [Header("Abrir (Open)")]
    public MessageSequence[] abrirSequences;

    [Header("Usar Action Requirements")]
    public RequiredItem itemNeededToUse = RequiredItem.None;
    public UnityEvent onUsarSuccessEvent;

    [Header("Abrir Action Requirements")]
    public RequiredItem itemNeededToAbrir = RequiredItem.None;
    [TextArea(2, 4)] public string[] textIfAbrirFails;
    [Header("Abrir Action Blocking (If already have item)")]
    public RequiredItem itemThatBlocksAbrir = RequiredItem.None;
    [TextArea(2, 4)] public string[] textIfAlreadyHaveItem;

    [Header("Door / Location Transition (Optional for Abrir)")]
    public GameObject doorCurrentLocation;
    public GameObject doorTargetLocation;

    [Header("Custom Action Events (Optional - Fires after dialogue)")]
    public UnityEvent onMirarEvent;
    public UnityEvent onCojerEvent;
    public UnityEvent onUsarEvent;
    public UnityEvent onAbrirEvent;

    // Independent click progression index per action type
    private int mirarIndex = 0;
    private int cojerIndex = 0;
    private int usarIndex  = 0;
    private int abrirIndex = 0;

    [Header("Tutorial")]
    public bool isTutorialHobo = false; // Enable on the hobo object during the tutorial scene

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnItemClicked);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetHoveredItem(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ClearHoveredItem(this);
        }
    }

    private void OnDisable()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ClearHoveredItem(this);
        }
    }

    /// <summary>
    /// Checks if this item has any data, dialogues, requirements, or events configured for the given action.
    /// Used by CursorManager for context-sensitive smart cursor detection.
    /// </summary>
    public bool SupportsAction(ActionPanelController.ActionType action)
    {
        switch (action)
        {
            case ActionPanelController.ActionType.Mirar:
                return (mirarSequences != null && mirarSequences.Length > 0) ||
                       (onMirarEvent != null && onMirarEvent.GetPersistentEventCount() > 0) ||
                       isTutorialHobo;

            case ActionPanelController.ActionType.Cojer:
                return (cojerSequences != null && cojerSequences.Length > 0) ||
                       (onCojerEvent != null && onCojerEvent.GetPersistentEventCount() > 0);

            case ActionPanelController.ActionType.Usar:
                return (usarSequences != null && usarSequences.Length > 0) ||
                       itemNeededToUse != RequiredItem.None ||
                       isTutorialHobo ||
                       (onUsarEvent != null && onUsarEvent.GetPersistentEventCount() > 0) ||
                       (onUsarSuccessEvent != null && onUsarSuccessEvent.GetPersistentEventCount() > 0);

            case ActionPanelController.ActionType.Abrir:
                return (abrirSequences != null && abrirSequences.Length > 0) ||
                       itemNeededToAbrir != RequiredItem.None ||
                       itemThatBlocksAbrir != RequiredItem.None ||
                       (textIfAbrirFails != null && textIfAbrirFails.Length > 0) ||
                       (onAbrirEvent != null && onAbrirEvent.GetPersistentEventCount() > 0) ||
                       doorCurrentLocation != null ||
                       doorTargetLocation != null;

            default:
                return false;
        }
    }

    private void OnItemClicked()
    {
        if (ActionPanelController.Instance == null)
        {
            Debug.LogWarning("[InteractableItem] ActionPanelController instance not found.");
            return;
        }

        ActionPanelController.ActionType action = ActionPanelController.Instance.currentAction;

        // Do nothing if no action is currently selected
        if (action == ActionPanelController.ActionType.None) return;

        if (MessageManager.Instance == null)
        {
            Debug.LogWarning("[InteractableItem] MessageManager instance not found.");
            return;
        }

        switch (action)
        {
            case ActionPanelController.ActionType.Mirar:
                if (isTutorialHobo && TutorialManager.Instance != null)
                {
                    // Tutorial: notify manager, pass ShowUsarPrompt as extra post-dialogue callback
                    TutorialManager.Instance.OnHoboLookedAt();
                    PlaySequence(mirarSequences, ref mirarIndex, action, TutorialManager.Instance.ShowUsarPrompt);
                }
                else
                {
                    PlaySequence(mirarSequences, ref mirarIndex, action);
                }
                break;

            case ActionPanelController.ActionType.Cojer:
                PlaySequence(cojerSequences, ref cojerIndex, action);
                break;

            case ActionPanelController.ActionType.Usar:
                // Check if a specific inventory item is required
                if (itemNeededToUse != RequiredItem.None)
                {
                    if (GameManager.Instance == null || !GameManager.Instance.HasItem(itemNeededToUse))
                    {
                        return; // Ignore click completely if the required item is missing
                    }
                }

                if (isTutorialHobo && TutorialManager.Instance != null)
                {
                    // Tutorial: hide prompt, reset panel, start the QTE — skip normal dialogue
                    TutorialManager.Instance.OnHoboUsed();
                    ActionPanelController.Instance.ResetPanel();
                    FindAnyObjectByType<HoboEncounterController>()?.StartEncounter();
                    return; // Skip the ResetPanel call at the bottom
                }

                PlaySequence(usarSequences, ref usarIndex, action, () => onUsarSuccessEvent?.Invoke());
                break;

            case ActionPanelController.ActionType.Abrir:
                
                // 1. БЛОКЕР: Если у нас УЖЕ ЕСТЬ предмет (например, Букет Flower), из-за которого заходить больше не надо
                if (itemThatBlocksAbrir != RequiredItem.None && GameManager.Instance != null)
                {
                    if (GameManager.Instance.HasItem(itemThatBlocksAbrir))
                    {
                        if (textIfAlreadyHaveItem != null && textIfAlreadyHaveItem.Length > 0 && MessageManager.Instance != null)
                        {
                            MessageManager.Instance.ShowMessages(textIfAlreadyHaveItem);
                        }
                        ActionPanelController.Instance.ResetPanel();
                        return; // Прерываем клик, внутрь не пускаем
                    }
                }

                // 2. БЛОКЕР: Если нам НУЖЕН предмет (например, Деньги), чтобы зайти, а его нет
                if (itemNeededToAbrir != RequiredItem.None && GameManager.Instance != null)
                {
                    if (!GameManager.Instance.HasItem(itemNeededToAbrir))
                    {
                        if (textIfAbrirFails != null && textIfAbrirFails.Length > 0 && MessageManager.Instance != null)
                        {
                            MessageManager.Instance.ShowMessages(textIfAbrirFails);
                        }
                        ActionPanelController.Instance.ResetPanel();
                        return; // Прерываем клик, внутрь не пускаем
                    }
                }

                // Если мы прошли обе проверки (денег хватает, а букета еще нет) — запускаем переход!
                PlaySequence(abrirSequences, ref abrirIndex, action);
                break;
        }

        // Close and reset the action panel so the player must re-select
        // an action before interacting with the next object
        ActionPanelController.Instance.ResetPanel();
    }

    /// <summary>
    /// Shows the current sequence for the chosen action and advances its index.
    /// Fires the corresponding action's UnityEvent as a callback once the dialogue closes.
    /// </summary>
    private void PlaySequence(MessageSequence[] sequences, ref int index, ActionPanelController.ActionType action, System.Action extraCallback = null)
    {
        System.Action callback = () =>
        {
            // Invoke the corresponding UnityEvent after the dialogue finishes
            switch (action)
            {
                case ActionPanelController.ActionType.Mirar:
                    onMirarEvent?.Invoke();
                    break;
                case ActionPanelController.ActionType.Cojer:
                    onCojerEvent?.Invoke();
                    break;
                case ActionPanelController.ActionType.Usar:
                    onUsarEvent?.Invoke();
                    break;
                case ActionPanelController.ActionType.Abrir:
                    if (GlobalFader.Instance != null)
                    {
                        if (doorCurrentLocation != null && doorTargetLocation != null)
                        {
                            GlobalFader.Instance.FadeToLocation(doorCurrentLocation, doorTargetLocation, () => onAbrirEvent?.Invoke());
                        }
                        else
                        {
                            GlobalFader.Instance.FadeOutAndIn(() => onAbrirEvent?.Invoke());
                        }
                    }
                    else
                    {
                        if (doorCurrentLocation != null) doorCurrentLocation.SetActive(false);
                        if (doorTargetLocation != null) doorTargetLocation.SetActive(true);
                        onAbrirEvent?.Invoke();
                    }
                    break;
            }

            extraCallback?.Invoke();
        };

        // If there are no messages to display, trigger the callback immediately
        if (sequences == null || sequences.Length == 0)
        {
            callback?.Invoke();
            return;
        }

        MessageManager.Instance.ShowMessages(sequences[index].messages, callback);

        index = Mathf.Min(index + 1, sequences.Length - 1);
    }
}
