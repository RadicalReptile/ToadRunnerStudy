using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the movement and behavior of the player character (toad) in an endless runner:
/// Handles forward and horizontal movement, out-of-bounds detection,
/// and a special cutscene sequence for the storm drain.
/// </summary>
// Based on the 3D endless runner tutorial by Practical Programming on YouTube:
// Original source: https://www.youtube.com/playlist?list=PLvcJYjdXa962PHXFjQ5ugP59Ayia-rxM3
[RequireComponent(typeof(Rigidbody))]   // ensures a Rigidbody component is present on this GameObject.
public class ToadController : MonoBehaviour
{
    // Core Movement Settings:
    [Header("Movement Settings")]
    [Tooltip("The base forward speed of the toad.")]
    [SerializeField] private float forwardSpeed = 10.0f;
    [Tooltip("The multiplier for horizontal input, controlling sideway movement speed.")]
    [SerializeField] private float horizontalMovementMultiplier = 1.75f;
    [Tooltip("The speed increase applied per 'point' or interval in the game.")]
    [SerializeField] private float speedIncreasePerPoint = 0.1f;    // used in GameManager.cs when the score is increased
    [Tooltip("The maximum absolute X-coordinate the player can move to (e.g., 3.25 means between -3.25 and +3.25).")]
    [SerializeField] private float horizontalBounds = 3.25f;

    // References & Components:
    [Header("Component References")]
    [Tooltip("Reference to the Animator component controlling toad animations.")]
    [SerializeField] private Animator toadAnimator;
    [Tooltip("Reference to the script that handles rotation animation during the cutscene (e.g., a Rotator.cs script).")]
    [SerializeField] private MonoBehaviour cutsceneRotationScript;
    [Tooltip("The Transform representing the entrance of the storm drain for the cutscene.")]
    [SerializeField] private Transform stormDrainEntrance;
    [Tooltip("Reference to the script that handles the transition to the cutscene.")]
    [SerializeField] private CutsceneHandler cutsceneHandler;
    // Player's Rigidbody is Automatically assigned in Awake() because [RequireComponent] guarantees its presence.
    private Rigidbody playerRigidbody;

    // Internal State:
    private bool _isAlive = true;
    private bool _cutsceneActive = false;
    private float _horizontalInput;

    // Public properties to access the speed values:
    public float ForwardSpeed 
    { 
        get => forwardSpeed; 
        set => forwardSpeed = value; 
    }
    public float SpeedIncreasePerPoint 
    { 
        get => speedIncreasePerPoint; 
        set => speedIncreasePerPoint = value; 
    }

    private void Awake()
    {
        // Cache the Rigidbody component. Guaranteed to be found due to [RequireComponent].
        playerRigidbody = GetComponent<Rigidbody>();
        // Ensure that the rotation is frozen by default.
        playerRigidbody.freezeRotation = true;

        // Ensure the Animator is assigned.
        if (toadAnimator == null)
        {
            Debug.LogWarning("ToadController: Animator component not found. Animations may not work.", this);
        }

        // Ensure rotation script is disabled at start, as it's for the cutscene only.
        if (cutsceneRotationScript != null)
        {
            cutsceneRotationScript.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (!_isAlive) return;

        if (_cutsceneActive)
        {
            HandleCutsceneMovement();
        }
        else
        {
            HandlePlayerMovement();
        }
    }

    private void Update()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
    }

    /// <summary>
    /// Handles the continuous forward and horizontal movement of the toad.
    /// </summary>
    private void HandlePlayerMovement()
    {
        Vector3 forwardMove = transform.forward * ForwardSpeed * Time.fixedDeltaTime;
        Vector3 horizontalMove = transform.right * _horizontalInput * ForwardSpeed * Time.fixedDeltaTime * horizontalMovementMultiplier;

        Vector3 targetPosition = playerRigidbody.position + forwardMove + horizontalMove;
        targetPosition.x = Mathf.Clamp(targetPosition.x, -horizontalBounds, horizontalBounds);

        playerRigidbody.MovePosition(targetPosition);
    }

    /// <summary>
    /// Handles the player's movement during the storm drain cutscene.
    /// </summary>
    private void HandleCutsceneMovement()
    {
        if (stormDrainEntrance == null)
        {
            Debug.LogError("ToadController: Storm Drain Entrance Transform is not assigned for cutscene!", this);
            return;
        }

        Vector3 newPosition = Vector3.MoveTowards(playerRigidbody.position,
                                                  stormDrainEntrance.position,
                                                  ForwardSpeed * Time.fixedDeltaTime);
        newPosition.y = 0.75f;  // magic number that looks best for the cutscene (otherwise the toad sinks into the ground instantly)

        playerRigidbody.MovePosition(newPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DrainTrigger"))
        {
            StartDrainCutscene();
        }
    }

    /// <summary>
    /// Initiates the storm drain cutscene sequence for the player:
    /// This method is called once when the drain is triggered.
    /// </summary>
    public void StartDrainCutscene()
    {
        if (_cutsceneActive) return;

        _cutsceneActive = true;
        _isAlive = true;

        playerRigidbody.isKinematic = true;
        playerRigidbody.freezeRotation = false;

        if (toadAnimator != null)
        {
            toadAnimator.enabled = false;
        }
        else
        {
            Debug.LogWarning("ToadController: Animator not assigned for cutscene transition!", this);
        }

        if (cutsceneRotationScript != null)
        {
            cutsceneRotationScript.enabled = true;
        }
        else
        {
            Debug.LogWarning("ToadController: Cutscene Rotation Script not assigned!", this);
        }

        if (cutsceneHandler != null)
        {
            cutsceneHandler.StartCutscene();
        }
        else
        {
            Debug.LogWarning("ToadController: Cutscene Handler not assigned!", this);
        }
    }

}