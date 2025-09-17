using UnityEngine;

/// <summary>
/// Controls the movement and behavior of a car object in the scene:
/// The car moves towards the player, triggers a subliminal flash upon collision,
/// plays a drive-by sound, and self-destructs when out of range.
/// </summary>
[RequireComponent(typeof(AudioSource))] // ensures an AudioSource component is present
public class Car : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The speed at which the car moves. A positive value will be negated to move towards the player.")]
    [SerializeField] private float speed = 100.0f;
    private float _currentSpeed;                        // internal speed variable, accounts for direction

    [Header("Audio Settings")]
    [Tooltip("The AudioClip to play when the car drives by (e.g., on collision with player).")]
    [SerializeField] private AudioClip driveBySound;

    [Header("Cleanup Settings")]
    [Tooltip("Distance behind the player at which the car will be destroyed.")]
    [SerializeField] private float destroyDistanceBehindPlayer = 250.0f;
    [Tooltip("Reference to the player's Transform.")]
    [SerializeField] private Transform playerTransform; // direct reference for player's position

    private void Awake()
    {
        // Negate speed to have the car move towards the player.
        _currentSpeed = -speed;
    }

    void FixedUpdate()
    {
        // FixedUpdate for physics and movement updates:
        // Move the car along its forward (local Z) axis.
        transform.Translate(0.0f, 0.0f, _currentSpeed * Time.fixedDeltaTime);
    }

    void Update()
    {
        // Update for non-physics-related updates like checking distances:
        // Destroy the object once it is behind the player and out of hearing range.
        if (transform.position.z < playerTransform.position.z - destroyDistanceBehindPlayer)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ensure the player GameObject has the tag "Player".
        if (other.CompareTag("Player"))
        {
            // Trigger the subliminal image flash.
            ImageFlasher.Instance.TriggerCarFlash();
            
            // Play the drive-by sound using the AudioSource.
            GetComponent<AudioSource>().PlayOneShot(driveBySound);

            // Disable collider after the first trigger to prevent multiple flashes/sounds.
            GetComponent<Collider>().enabled = false;
        }
    }
    
}