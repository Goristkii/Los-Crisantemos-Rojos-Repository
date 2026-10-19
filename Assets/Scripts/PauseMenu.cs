using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;

    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // CRITICAL: Unfreeze time before loading the menu!
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        // Safety check to ensure timeScale is always restored if this object or scene is destroyed
        Time.timeScale = 1f;
    }
}
