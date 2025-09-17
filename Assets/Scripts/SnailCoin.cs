using UnityEngine;

/// <summary>
/// Manages the behavior of a collectible "Snail" object:
/// Detects when the player collects it to increment the game score,
/// and handles rotation for visual effect.
/// </summary>
// Based on the 3D endless runner tutorial by Practical Programming on YouTube: 
// Original source: https://www.youtube.com/playlist?list=PLvcJYjdXa962PHXFjQ5ugP59Ayia-rxM3
public class SnailCoin : MonoBehaviour
{
    [Header("Behavior Settings")]
    [Tooltip("The speed at which the snail rotates around the Z-axis.")]
    [SerializeField] private float turnSpeed = 90.0f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "Player" tag.
        if (other.gameObject.CompareTag("Player"))
        {
            // Add to the player's score.
            GameManager.Instance.IncrementScore();

            // Destroy this snail object.
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Rotate the snail on its Z-axis at a constant speed.
        transform.Rotate(0, 0, turnSpeed * Time.deltaTime);
    }

}