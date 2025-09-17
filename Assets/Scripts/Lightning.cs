using UnityEngine;

/// <summary>
/// Controls the behavior of a lightning trigger object in the scene:
/// It plays a thunder sound and triggers a subliminal image flash when the player enters its trigger zone,
/// and self-destructs when out of range.
/// </summary>
[RequireComponent(typeof(AudioSource))] // ensures an AudioSource component is present on this GameObject
public class Lightning : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The AudioClip for the thunder sound to play when the player triggers the lightning.")]
    [SerializeField] private AudioClip thunderCrackSound;

    [Header("Cleanup Settings")]
    [Tooltip("Distance behind the player (Camera.main.transform.position.z) at which the lightning object will be destroyed.")]
    [SerializeField] private float destroyDistanceBehindPlayer = 50.0f;
    [Tooltip("Reference to the player's Transform.")]
    [SerializeField] private Transform playerTransform; // direct reference for player's position

    private void Update()
    {
        // Destroy the object once it is behind the player and out of range.
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
            // Trigger the subliminal image flash for storm text/arrow.
            ImageFlasher.Instance.TriggerStormFlash();

            // Play the thunder sound using the AudioSource.
            GetComponent<AudioSource>().PlayOneShot(thunderCrackSound);

            // Disable collider after the first trigger to prevent multiple flashes/sounds.
            GetComponent<Collider>().enabled = false;
        }
    }

}