using System.Collections;
using UnityEngine;
using UnityEngine.UI;
// Using custom Game Manager class for identifying current game mode:
using static GameManager.ParticipantGroup;

/// <summary>
/// Controls the flashing of images for subliminal messaging studies:
/// It handles both car-related flashes (simulating car wheel shades)
/// and storm-related flashes (simulating lightning).
/// Caution: Contains no null-checks for optimised performance.
/// </summary>
public class ImageFlasher : MonoBehaviour
{
    // Singleton Instance:
    /// <summary>
    /// Public static reference to allow easy access from other scripts (e.g., ImageFlasher.Instance.TriggerCarFlash()).
    /// </summary>
    public static ImageFlasher Instance { get; private set; }

    [Header("Flash Images")]
    [Tooltip("RawImage for the car text to flash.")]
    [SerializeField] private RawImage carTextToFlash;
    [Tooltip("RawImage for the car arrow to flash.")]
    [SerializeField] private RawImage carArrowToFlash;
    [Tooltip("RawImage for the storm text to flash.")]
    [SerializeField] private RawImage stormTextToFlash;
    [Tooltip("RawImage for the storm arrow to flash.")]
    [SerializeField] private RawImage stormArrowToFlash;
    [Tooltip("RawImage used as a white screen overlay for lightning effects.")]
    [SerializeField] private RawImage whiteImg;
    [Tooltip("RawImage used as a black screen overlay for car shade effects.")]
    [SerializeField] private RawImage blackImg;
    [Tooltip("Score UI to hide during the car sequence.")]
    [SerializeField] private Text scoreText;

    [Header("Timing Settings")]
    [Tooltip("Duration of the subliminal flash in seconds (e.g., 0.033f for 33ms).")]
    [SerializeField] private float flashDuration = 0.033f;  // 33 milliseconds in seconds

    // Pre-cached WaitForSecondsRealtime for performance.
    private WaitForSecondsRealtime _flashWait;

    // A reference to the currently running flash coroutine, to prevent overlapping flashes.
    private Coroutine _currentFlashCoroutine;

    private void Awake()
    {
        // Singleton Enforcement:
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cache the WaitForSecondsRealtime object to avoid creating new objects.
        _flashWait = new WaitForSecondsRealtime(flashDuration);

        // Initial State Setup:
        // Ensure all flash images are disabled at the start.
        DisableImageState();    // set all to disabled

        // Ensure blackImg and whiteImg start with correct opacity.
        Color blackColor = blackImg.color;
        blackColor.a = 0.9f;    // set to 90% opacity
        blackImg.color = blackColor;
        Color whiteColor = whiteImg.color;
        whiteColor.a = 0.0f;    // set to 0% opacity
        whiteImg.color = whiteColor;
    }

    /// <summary>
    /// Helper to enable/disable all flash-related RawImages.
    /// </summary>
    private void DisableImageState()
    {
        carTextToFlash.enabled = false;
        carArrowToFlash.enabled = false;
        stormTextToFlash.enabled = false;
        stormArrowToFlash.enabled = false;
        whiteImg.enabled = false;
        blackImg.enabled = false;
    }

    /// <summary>
    /// Triggers a car-related subliminal flash:
    /// This should be called directly by the CarSpawner or similar script.
    /// </summary>
    public void TriggerCarFlash()
    {
        if (_currentFlashCoroutine != null)
        {
            StopCoroutine(_currentFlashCoroutine);
            DisableImageState();    // ensure any previous flash is cleared
        }
        _currentFlashCoroutine = StartCoroutine(HandleCarFlash());
    }

    /// <summary>
    /// Triggers a storm-related subliminal flash:
    /// This should be called directly by the LightningManager or similar script.
    /// </summary>
    public void TriggerStormFlash()
    {
        if (_currentFlashCoroutine != null)
        {
            StopCoroutine(_currentFlashCoroutine);
            DisableImageState();    // ensure any previous flash is cleared
        }
        _currentFlashCoroutine = StartCoroutine(HandleStormFlash());
    }

    /// <summary>
    /// Handles the car-related flash sequence, including the black screen overlay.
    /// </summary>
    private IEnumerator HandleCarFlash()
    {
        scoreText.enabled = false;

        // Determine which image to flash based on game mode.
        RawImage imageToFlash = null;
        switch (GameManager.CurrentGameMode)
        {
            case TestGroup1Text:
                imageToFlash = carTextToFlash;
                break;
            case TestGroup2Arrows:
                imageToFlash = carArrowToFlash;
                break;
            case ControlGroupBlank:
                // Ensures no text/arrow is shown For control groups.
                // The blackImg will still simulate the "shade".
                break;
        }

        // Simulate the shade of the first set of wheels of the car (black mask).
        // Cover screen with a black image mask for 3 frames, then wait for 4 frames.
        blackImg.enabled = true;
        yield return CoroutineUtils.WaitForFrames(3);
        blackImg.enabled = false;
        yield return CoroutineUtils.WaitForFrames(4);

        // Subliminal Flash:
        if (imageToFlash != null)       // only show if valid image for the mode
        {
            imageToFlash.enabled = true;
            yield return _flashWait;    // use the cached WaitForSecondsRealtime
            imageToFlash.enabled = false;
        }

        // Wait for 4 frames, then cover screen with a black image mask for 3 frames.
        // This simulates the shade of the second set of wheels of the car.
        yield return CoroutineUtils.WaitForFrames(4);
        blackImg.enabled = true;
        yield return CoroutineUtils.WaitForFrames(3);
        blackImg.enabled = false;

        scoreText.enabled = true;
    }

    /// <summary>
    /// Handles the storm-related flash sequence, including the white screen fade.
    /// </summary>
    private IEnumerator HandleStormFlash()
    {
        // Determine which image to flash based on game mode.
        RawImage imageToFlash = null;
        switch (GameManager.CurrentGameMode)
        {
            case TestGroup1Text:
                imageToFlash = stormTextToFlash;
                break;
            case TestGroup2Arrows:
                imageToFlash = stormArrowToFlash;
                break;
            case ControlGroupBlank:
                // Ensures no text/arrow is shown For control groups.
                // The whiteImg will still simulate the "lightning".
                break;
        }

        // Fade screen into a white image mask for 3 frames, then fade out for 4 frames.
        // Simulates flashes of lightning.
        whiteImg.enabled = true;
        yield return StartCoroutine(FadeImageAlpha(whiteImg, 0.0f, 1.0f, 3));   // fade in
        yield return StartCoroutine(FadeImageAlpha(whiteImg, 1.0f, 0.0f, 4));   // fade out
        whiteImg.enabled = false;

        // Subliminal Flash:
        if (imageToFlash != null)       // only show if valid image for the mode
        {
            imageToFlash.enabled = true;
            yield return _flashWait;    // use the cached WaitForSecondsRealtime
            imageToFlash.enabled = false;
        }

        // Fade screen into a white image mask for 4 frames, then fade out for 3 frames.
        // Simulates flashes of lightning.
        whiteImg.enabled = true;
        yield return StartCoroutine(FadeImageAlpha(whiteImg, 0.0f, 1.0f, 4));   // fade in
        yield return StartCoroutine(FadeImageAlpha(whiteImg, 1.0f, 0.0f, 3));   // fade out

        // Simulate slow fade after lightning.
        yield return StartCoroutine(FadeImageAlpha(whiteImg, 0.5f, 0.0f, 30));
        whiteImg.enabled = false;
    }

    /// <summary>
    /// Coroutine to smoothly fade an image's alpha over a specified number of frames.
    /// </summary>
    /// <param name="image">The RawImage to fade.</param>
    /// <param name="startAlpha">The starting alpha value (0.0 to 1.0).</param>
    /// <param name="endAlpha">The ending alpha value (0.0 to 1.0).</param>
    /// <param name="frameCount">The number of frames over which to perform the fade.</param>
    private IEnumerator FadeImageAlpha(RawImage image, float startAlpha, float endAlpha, int frameCount)
    {
        Color color = image.color;
        for (int i = 0; i <= frameCount; i++)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, (float)i / frameCount);
            color.a = alpha;
            image.color = color;
            yield return null;
        }
    }

}

/// <summary>
/// Utility class for common coroutine helpers.
/// </summary>
public static class CoroutineUtils
{
    /// <summary>
    /// Waits for a specified number of frames.
    /// </summary>
    /// <param name="frames">The number of frames to wait.</param>
    public static IEnumerator WaitForFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return null;
        }
    }

}