using UnityEngine;

/// <summary>
/// This script manages a single ground tile in the endless runner environment.
/// It's responsible for spawning snails and triggering the creation of a new ground tile.
/// </summary>
// Based on the 3D endless runner tutorial by Practical Programming on YouTube:
// Original source: https://www.youtube.com/playlist?list=PLvcJYjdXa962PHXFjQ5ugP59Ayia-rxM3
public class GroundTile : MonoBehaviour
{
    [Header("Tile References")]
    [Tooltip("The prefab for the collectible snail.")]
    [SerializeField] private GameObject snailPrefab;

    [Header("End-Game Boundaries")]
    [Tooltip("The Z-position at which tiles will stop being destroyed.")]
    [SerializeField] private float tileDestroyThresholdZ = 168.0f;
    [Tooltip("The Z-position at which snails will stop spawning.")]
    [SerializeField] private float snailSpawnThresholdZ = 180.0f;

    [Header("Snail Spawn Settings")]
    [Tooltip("The x-axis offset from the edge of the tile's bounds for snail spawning.")]
    [SerializeField] private float snailSpawnXOffset = 3.75f;
    private const float SNAIL_SPAWN_Y_POSITION = 0.5f;

    private GroundSpawner groundSpawner;

    private void Start()
    {
        groundSpawner = FindAnyObjectByType<GroundSpawner>();
    }

    /// <summary>
    /// Triggers when another collider exits the tile's boundary:
    /// It's used to spawn a new tile and eventually destroy this one.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Request the GroundSpawner to create a new tile for the player to run on.
            groundSpawner.SpawnTile(true);

            // Check if the player is close to the end of the level before destroying the tile.
            // This prevents a noticeable gap in the ground during the final cutscene.
            if (transform.position.z < tileDestroyThresholdZ)
            {
                // Destroy this tile after a delay to ensure it's off-screen.
                Destroy(gameObject, 2);
            }
        }
    }

    /// <summary>
    /// Spawns a single snail at a random position on the ground tile:
    /// This method is called by the GroundSpawner.
    /// </summary>
    public void SpawnSnails()
    {
        // Only spawn snails if the tile is before the final end point.
        if (transform.position.z < snailSpawnThresholdZ)
        {
            // Get the collider component.
            Collider tileCollider = GetComponent<Collider>();
            if (tileCollider == null)
            {
                Debug.LogWarning("GroundTile is missing a Collider component. Cannot spawn snails.");
                return;
            }

            // Get a random position within the tile's collider to place the snail.
            Vector3 spawnPoint = GetRandomPointInCollider(tileCollider);

            // Instantiate the snail prefab at the calculated position with a correct, upright rotation.
            // Using Quaternion.Euler(-90, 0, 0) makes the snail stand upright on the tile.
            Instantiate(snailPrefab, spawnPoint, Quaternion.Euler(-90, 0, 0), transform);
        }
    }

    /// <summary>
    /// Calculates a random position within the bounds of the ground tile's collider.
    /// </summary>
    /// <param name="collider">The collider of the ground tile.</param>
    /// <returns>A random Vector3 position for the snail.</returns>
    private Vector3 GetRandomPointInCollider(Collider collider)
    {
        Vector3 boundsMin = collider.bounds.min;
        Vector3 boundsMax = collider.bounds.max;

        // Use Random.Range to get a random position within the collider's bounds.
        // The x-axis range is constrained to prevent snails from spawning on the edges.
        // The y-position is fixed to ensure the snail is at a consistent height.
        Vector3 point = new Vector3(
            Random.Range(boundsMin.x + snailSpawnXOffset, boundsMax.x - snailSpawnXOffset),
            SNAIL_SPAWN_Y_POSITION,
            Random.Range(boundsMin.z, boundsMax.z)
        );

        return point;
    }

}