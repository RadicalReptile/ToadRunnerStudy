using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random; // explicitly use UnityEngine.Random to avoid ambiguity with System.Random

/// <summary>
/// Manages the overall game state, score, player speed progression,
/// and determines the participant's experimental group (game mode).
/// </summary>
// Based on the 3D endless runner tutorial by Practical Programming on YouTube:
// Original source: https://www.youtube.com/playlist?list=PLvcJYjdXa962PHXFjQ5ugP59Ayia-rxM3
public class GameManager : MonoBehaviour
{
    // Singleton Instance:
    /// <summary>
    /// Public static reference allows for easy access from other scripts (e.g., GameManager.Instance.IncrementScore()).
    /// </summary>
    public static GameManager Instance { get; private set; }

    // Experiment Settings:
    [Header("Experiment Settings")]
    [Tooltip("Overrides the randomized game mode for specific testing. Set to 'No Override' for randomization.")]
    [SerializeField] private ParticipantGroup gameModeOverride = ParticipantGroup.NoOverride;

    /// <summary>
    /// Defines the different participant groups/game modes for the experiment:
    /// Used by various scripts to adjust behavior based on the current mode.
    /// </summary>
    public enum ParticipantGroup
    {
        NoOverride = 0,
        TestGroup1Text = 1,
        TestGroup2Arrows = 2,
        ControlGroupBlank = 3
    }

    /// <summary>
    /// Stores the determined game mode for the current session.
    /// </summary>
    public static ParticipantGroup CurrentGameMode { get; private set; }

    // Misc Settings:
    [Header("Score & UI Settings")]
    [Tooltip("Reference to the UI Text component that displays the player's score.")]
    [SerializeField] private Text scoreText;
    [Tooltip("Reference to the crunch sound that plays the player eats a snail.")]
    [SerializeField] private AudioClip crunchSound;
    [Header("Player References")]
    [Tooltip("Reference to the player's ToadController script.")]
    [SerializeField] private ToadController playerController;
    [Header("Game Progression")]
    [Tooltip("The amount by which player speed increases for each point scored.")]
    [SerializeField] private float speedIncreasePerPoint = 0.1f;

    // The current score of the player.
    private int _score;

    private void Awake()
    {
        // Singleton Enforcement:
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initial Setup:
        Application.targetFrameRate = 60;

        if (playerController == null)
        {
            Debug.LogWarning("GameManager: Player Controller not assigned! Cannot increase player speed.", this);
        }
        if (scoreText == null)
        {
            Debug.LogWarning("GameManager: Score Text UI component not assigned! Score will not be displayed.", this);
        }

        UpdateScoreDisplay();
    }

    private void Start()
    {
#if UNITY_EDITOR
        Debug.Log("GameManager.cs: Editor detected. Initializing game mode directly.");
        SetGameMode(gameModeOverride);
#elif UNITY_WEBGL
        Debug.Log("GameManager.cs: WebGL build detected. Waiting for Firebase assignment.");
        StartCoroutine(WaitForFirebaseGroupAssignment());
#else
        // Fallback for other platforms.
        SetGameMode(gameModeOverride);
#endif
    }

    private System.Collections.IEnumerator WaitForFirebaseGroupAssignment()
    {
        // Ensure the FirebaseWebGLManager exists and is ready.
        while (FirebaseWebGLManager.Instance == null || !FirebaseWebGLManager.Instance.IsReady)
        {
            yield return null;
        }

        // Get the assigned group from Firebase.
        int firebaseGroup = FirebaseWebGLManager.Instance.GetCurrentGroup();
        SetGameMode((ParticipantGroup)firebaseGroup);
    }

    /// <summary>
    /// Sets the game mode based on a provided ParticipantGroup.
    /// </summary>
    /// <param name="group">The participant group to assign.</param>
    private void SetGameMode(ParticipantGroup group)
    {
        if (gameModeOverride != ParticipantGroup.NoOverride)
        {
            CurrentGameMode = gameModeOverride;
            Debug.Log($"Game Mode overridden to: {CurrentGameMode}");
        }
        else
        {
            // If the group from Firebase is 0 (NoOverride), assign a random group.
            if (group == ParticipantGroup.NoOverride)
            {
                // Randomly assign group 1, 2, or 3.
                CurrentGameMode = (ParticipantGroup)Random.Range(1, 4);
                Debug.Log("Firebase assignment failed or was 'NoOverride', randomized assignment.");
            }
            else
            {
                CurrentGameMode = group;
                Debug.Log("Assigned Firebase group.");
            }
        }
    }

    /// <summary>
    /// Increments the player's score and updates the UI:
    /// Also increases the player's forward speed and plays a crunch sound effect.
    /// </summary>
    public void IncrementScore()
    {
        _score++;
        UpdateScoreDisplay();

        if (playerController != null)
        {
            playerController.ForwardSpeed += speedIncreasePerPoint;
        }

        if (crunchSound != null)
        {
            GetComponent<AudioSource>().PlayOneShot(crunchSound);
        }
    }

    /// <summary>
    /// Updates the score display UI Text.
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "x " + _score;
        }
    }

}