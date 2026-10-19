using UnityEngine;

/// <summary>
/// Master controller for location navigation arrows (B_NextLocation_Left & Right).
/// Ensures arrows appear ONLY when on an active street location and when no dialogue,
/// tutorial, or story lock is blocking navigation.
/// </summary>
public class NavigationController : MonoBehaviour
{
    public static NavigationController Instance { get; private set; }

    [Header("Navigation Buttons")]
    public GameObject leftArrow;
    public GameObject rightArrow;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void LateUpdate()
    {
        UpdateArrowVisibility();
    }

    /// <summary>
    /// Evaluates all game systems and updates navigation arrow visibility.
    /// Can also be called directly by other scripts when story states change.
    /// </summary>
    public void UpdateArrowVisibility()
    {
        if (LocationManager.Instance == null)
        {
            SetArrows(false, false);
            return;
        }

        // 1. Check if the player is currently on an active street location
        bool onStreet = IsOnStreet();

        // 2. Check if a dialogue is currently playing in MessageManager
        bool inDialogue = MessageManager.Instance != null && MessageManager.Instance.isDialogueActive;

        // 3. Check if the initial tutorial is still in progress
        bool inTutorial = TutorialManager.Instance != null && !TutorialManager.Instance.IsTutorialDone;

        // 4. Check if a scene transition fade is currently in progress
        bool isFading = GlobalFader.Instance != null && GlobalFader.Instance.IsFading;

        // Master lock: If not on street, or dialogue is open, in tutorial, or fading -> hide both arrows
        if (!onStreet || inDialogue || inTutorial || isFading)
        {
            SetArrows(false, false);
            return;
        }

        // 4. Directional & story locks from LocationManager and GameManager
        int currentIndex = LocationManager.Instance.currentIndex;
        int totalLocations = LocationManager.Instance.locations != null ? LocationManager.Instance.locations.Length : 0;

        // Left Arrow conditions:
        bool canGoLeft = currentIndex > 0;
        if (currentIndex == 1 && GameManager.Instance != null)
        {
            // At Station (index 1), left is blocked until the Hobo QTE is finished
            canGoLeft = GameManager.Instance.hoboQTEFinished;
        }

        // Right Arrow conditions:
        bool canGoRight = currentIndex < totalLocations - 1;
        if (currentIndex == 1 && GameManager.Instance != null)
        {
            // At Station (index 1), right is blocked until the friend is called
            canGoRight = GameManager.Instance.friendCalled;
        }

        SetArrows(canGoLeft, canGoRight);
    }

    /// <summary>
    /// Whitelist check: Returns true only if the current street GameObject in LocationManager is active in hierarchy.
    /// When entering interior locations (like FlowerShop or FriendHome), the street location is deactivated,
    /// so this automatically returns false without needing a manual blacklist array.
    /// </summary>
    private bool IsOnStreet()
    {
        var locManager = LocationManager.Instance;
        if (locManager == null || locManager.locations == null || locManager.locations.Length == 0) return false;
        if (locManager.currentIndex < 0 || locManager.currentIndex >= locManager.locations.Length) return false;

        GameObject currentStreet = locManager.locations[locManager.currentIndex];
        return currentStreet != null && currentStreet.activeInHierarchy;
    }

    private void SetArrows(bool showLeft, bool showRight)
    {
        if (leftArrow != null && leftArrow.activeSelf != showLeft)
        {
            leftArrow.SetActive(showLeft);
        }

        if (rightArrow != null && rightArrow.activeSelf != showRight)
        {
            rightArrow.SetActive(showRight);
        }
    }
}