using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Manages the two-part cutscene at the end of the game, including visual fades
/// and UI interactions for the player's choices.
/// </summary>
public class CutsceneHandler : MonoBehaviour
{
    // --- Public UI References ---
    [Header("Cutscene Visuals & Text")]
    [Tooltip("Panel containing the two tunnel choices.")]
    public GameObject tunnelUIPanel;
    [Tooltip("Panel containing the survey prompt.")]
    public GameObject exitUIPanel;
    [Tooltip("Image shown for the tunnel cutscene.")]
    public RawImage TunnelCG;
    [Tooltip("Image shown for the exit/survey cutscene.")]
    public RawImage ExitCG;
    [Tooltip("Text element for the tunnel choice prompt.")]
    public Text tunnelPromptText;
    [Tooltip("Text element for the final survey prompt.")]
    public Text exitPromptText;

    // --- Button References ---
    [Header("Cutscene Buttons")]
    [Tooltip("The 'Left' choice button.")]
    public Button leftButton;
    [Tooltip("The 'Right' choice button.")]
    public Button rightButton;
    [Tooltip("The 'Open Survey' button.")]
    public Button surveyButton;

    // --- Private Components ---
    private CanvasGroup tunnelCanvasGroup;
    private CanvasGroup exitCanvasGroup;
    private const int FADE_FRAME_COUNT = 90; // The number of frames for the fade effect.

    private void Awake()
    {
        // Get references to CanvasGroups for controlling UI visibility and interactivity.
        tunnelCanvasGroup = tunnelUIPanel.GetComponent<CanvasGroup>();
        exitCanvasGroup = exitUIPanel.GetComponent<CanvasGroup>();

        // Ensure panels are initially hidden and not blocking clicks.
        tunnelCanvasGroup.alpha = 0;
        tunnelCanvasGroup.interactable = false;
        tunnelCanvasGroup.blocksRaycasts = false;
        leftButton.enabled = false;
        rightButton.enabled = false;

        exitCanvasGroup.alpha = 0;
        exitCanvasGroup.interactable = false;
        exitCanvasGroup.blocksRaycasts = false;
        surveyButton.enabled = false;
    }

    /// <summary>
    /// Starts the cutscene sequence when the player hits the drain.
    /// This method should be called from your ToadController.
    /// </summary>
    public void StartCutscene()
    {
        StartCoroutine(LoadTunnelCG());
    }

    /// <summary>
    /// Coroutine to fade into the tunnel image and enable interaction.
    /// </summary>
    private IEnumerator LoadTunnelCG()
    {
        yield return new WaitForSeconds(0.75f);

        tunnelUIPanel.SetActive(true);

        // Perform simultaneous alpha and color fade in.
        yield return StartCoroutine(FadeAll(
            TunnelCG, tunnelCanvasGroup,
            Color.black, new Color(1, 1, 1, 1),
            0, 1,
            FADE_FRAME_COUNT
        ));

        tunnelPromptText.text = "Oh no! Click which tunnel you want to go down.";

        // When done, have two clickable buttons (left/right).
        leftButton.enabled = true;
        rightButton.enabled = true;
        leftButton.onClick.AddListener(() => OnChoiceSelected("left"));
        rightButton.onClick.AddListener(() => OnChoiceSelected("right"));
    }

    /// <summary>
    /// Handles the player's tunnel choice and starts the next cutscene phase.
    /// </summary>
    /// <param name="direction">The player's chosen direction ("left" or "right").</param>
    private void OnChoiceSelected(string direction)
    {
        // Disable the buttons immediately to prevent multiple clicks.
        leftButton.interactable = false;
        rightButton.interactable = false;

        // Start the crossfade to the next screen.
        StartCoroutine(LoadExitCG(direction));
    }

    /// <summary>
    /// Coroutine to crossfade from the tunnel image to the exit image and enable the survey button.
    /// </summary>
    /// <param name="direction">The player's chosen direction.</param>
    private IEnumerator LoadExitCG(string direction)
    {
        exitUIPanel.SetActive(true);

        // Fade out the tunnel panel, fading the image from white to black.
        StartCoroutine(FadeAll(
            TunnelCG, tunnelCanvasGroup,
            Color.white, Color.black,
            1, 1,
            FADE_FRAME_COUNT
        ));

        tunnelPromptText.enabled = false;
        yield return new WaitForSeconds(0.75f);

        // Wait for the exit panel to finish fading in before continuing.
        yield return StartCoroutine(FadeAll(
            ExitCG, exitCanvasGroup,
            Color.black, Color.white,
            0, 1,
            FADE_FRAME_COUNT
        ));

        // Deactivate the tunnel panel to save resources once the new panel is visible.
        tunnelUIPanel.SetActive(false);

        exitPromptText.text = "Thank you for playing this very short game. This game was made for a behavioral study. If you wish to participate in the study, please click the toad to be redirected to the survey.";

        // When done, have one clickable button that opens the survey.
        surveyButton.enabled = true;
        surveyButton.onClick.AddListener(() => SurveyManager.Instance.OpenSurvey(GameManager.CurrentGameMode.ToString(), direction));

        // Make the survey button interactive.
        surveyButton.interactable = true;
    }

    /// <summary>
    /// Coroutine to smoothly fade an image's color and a CanvasGroup's alpha simultaneously.
    /// </summary>
    private IEnumerator FadeAll(RawImage image, CanvasGroup canvasGroup, Color startColor, Color endColor, float startAlpha, float endAlpha, int frameCount)
    {
        canvasGroup.interactable = (endAlpha > 0);
        canvasGroup.blocksRaycasts = (endAlpha > 0);

        for (int i = 0; i <= frameCount; i++)
        {
            float t = (float)i / frameCount;
            Color newColor = Color.Lerp(startColor, endColor, t);
            image.color = newColor;

            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, t);
            canvasGroup.alpha = newAlpha;
            yield return null;
        }
    }
}