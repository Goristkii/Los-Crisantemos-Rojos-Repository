using UnityEngine;

/// <summary>
/// Singleton that manages navigation between location GameObjects in a single scene.
/// Locations are ordered left to right: 0=Home, 1=Station, 2=Block, 3=Trec, 4=Friend.
/// Handles story-based movement locks using GameManager flags.
/// </summary>
public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }

    [Header("Locations (Left to Right)")]
    public GameObject[] locations;
    public int currentIndex = 1; // Game starts at the Station

    [Header("Navigation Buttons")]
    public GameObject leftButton;
    public GameObject rightButton;



    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Deactivate all locations and navigation arrows until the intro finishes
        for (int i = 0; i < locations.Length; i++)
        {
            if (locations[i] != null)
            {
                locations[i].SetActive(false);
            }
        }

        if (leftButton != null) leftButton.SetActive(false);
        if (rightButton != null) rightButton.SetActive(false);
    }

    /// <summary>
    /// Initializes the starting location and navigation buttons.
    /// Called by IntroSequenceController after the intro sequence completes.
    /// </summary>
    public void InitializeGame()
    {
        currentIndex = 1; // Start at the Station
        ChangeLocation();
    }

    // -------------------------------------------------------------------------
    // Navigation — Linked to UI Arrow Buttons
    // -------------------------------------------------------------------------

    /// <summary>
    /// Moves the player to the location on the left.
    /// Wire to the left arrow button's OnClick event.
    /// </summary>
    public void MoveLeft()
    {
        if (GlobalFader.Instance != null && GlobalFader.Instance.IsFading) return;

        if (currentIndex > 0)
        {
            int prevIndex = currentIndex;
            currentIndex--;
            ChangeLocation(prevIndex);
        }
    }

    /// <summary>
    /// Moves the player to the location on the right.
    /// Wire to the right arrow button's OnClick event.
    /// </summary>
    public void MoveRight()
    {
        if (GlobalFader.Instance != null && GlobalFader.Instance.IsFading) return;

        if (currentIndex < locations.Length - 1)
        {
            int prevIndex = currentIndex;
            currentIndex++;
            ChangeLocation(prevIndex);
        }
    }

    // -------------------------------------------------------------------------
    // Location Switch
    // -------------------------------------------------------------------------

    /// <summary>
    /// Deactivates all locations and activates the one at currentIndex.
    /// Uses GlobalFader when transitioning from a previous location.
    /// </summary>
    private void ChangeLocation(int prevIndex = -1)
    {
        if (prevIndex >= 0 && prevIndex < locations.Length && GlobalFader.Instance != null)
        {
            GameObject prevLoc = locations[prevIndex];
            GameObject targetLoc = locations[currentIndex];

            GlobalFader.Instance.FadeToLocation(prevLoc, targetLoc, () =>
            {
                for (int i = 0; i < locations.Length; i++)
                {
                    if (locations[i] != null)
                    {
                        locations[i].SetActive(i == currentIndex);
                    }
                }
                UpdateNavigationButtons();
            });
        }
        else
        {
            for (int i = 0; i < locations.Length; i++)
            {
                if (locations[i] != null)
                {
                    locations[i].SetActive(i == currentIndex);
                }
            }

            UpdateNavigationButtons();
        }
    }

    // -------------------------------------------------------------------------
    // Navigation Button Visibility
    // -------------------------------------------------------------------------

    /// <summary>
    /// Refreshes navigation arrows. Delegates to NavigationController as the master
    /// visibility controller, or executes fallback logic if not present.
    /// </summary>
    public void UpdateNavigationButtons()
    {
        if (NavigationController.Instance != null)
        {
            NavigationController.Instance.UpdateArrowVisibility();
            return;
        }

        if (leftButton != null)
        {
            bool canGoLeft = currentIndex > 0;
            if (currentIndex == 1 && GameManager.Instance != null)
            {
                canGoLeft = GameManager.Instance.hoboQTEFinished;
            }

            leftButton.SetActive(canGoLeft);
        }

        if (rightButton != null)
        {
            bool canGoRight = currentIndex < (locations != null ? locations.Length - 1 : 0);
            if (currentIndex == 1 && GameManager.Instance != null)
            {
                canGoRight = GameManager.Instance.friendCalled;
            }

            rightButton.SetActive(canGoRight);
        }
    }
}
