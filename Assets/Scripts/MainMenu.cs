using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the main menu UI and scene transitions.
/// </summary>
public class MainMenu : MonoBehaviour
{
    // A single parent GameObject for all UI panels.
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject sourcesPanel;

    private void Start()
    {
        // Ensure the main menu is the first panel visible on scene load.
        GoToSources();
    }

    /// <summary>
    /// Activates the main menu panel and deactivates others.
    /// </summary>
    public void GoToMain()
    {
        mainPanel.SetActive(true);
        sourcesPanel.SetActive(false);
    }

    /// <summary>
    /// Activates the sources panel and deactivates others.
    /// </summary>
    public void GoToSources()
    {
        mainPanel.SetActive(false);
        sourcesPanel.SetActive(true);
    }

    /// <summary>
    /// Loads the main game scene:
    /// This method is typically called by a "Play" or "Start" button.
    /// </summary>
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

}