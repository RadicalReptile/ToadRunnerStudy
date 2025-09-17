using UnityEngine;

/// <summary>
/// Handles the endless spawning of new ground tiles.
/// </summary>
// Based on the 3D endless runner tutorial by Practical Programming on YouTube:
// Original source: https://www.youtube.com/playlist?list=PLvcJYjdXa962PHXFjQ5ugP59Ayia-rxM3
public class GroundSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [Tooltip("The ground tile prefab to be instantiated.")]
    [SerializeField] private GameObject groundTile;

    private Vector3 nextSpawnPoint;

    /// <summary>
    /// Spawns a new ground tile at the designated point.
    /// </summary>
    /// <param name="spawnItems">Controls whether collectible items (snails) are spawned on the new tile.</param>
    public void SpawnTile(bool spawnItems)
    {
        // Instantiate the ground tile prefab at the next spawn position.
        GameObject temp = Instantiate(groundTile, nextSpawnPoint, Quaternion.identity);

        // Get the position of the second child (index 1) which serves as the next spawn point.
        // This is a common pattern in endless runners to chain tiles together.
        Transform nextSpawnPointTransform = temp.transform.GetChild(1);
        nextSpawnPoint = nextSpawnPointTransform.position;

        // Check the boolean flag to determine if items should be spawned on this tile.
        if (spawnItems)
        {
            // Call the SpawnSnails method on the GroundTile component.
            temp.GetComponent<GroundTile>().SpawnSnails();
        }
    }

    /// <summary>
    /// Initializes the scene by spawning the first 15 ground tiles.
    /// The first 3 tiles are empty to give the player a clear start.
    /// </summary>
    private void Start()
    {
        // Loop to spawn a total of 15 ground tiles.
        for (int i = 0; i < 15; i++)
        {
            // The first 3 tiles have no collectibles.
            if (i < 3)
            {
                SpawnTile(false);
            }
            // All subsequent tiles can have collectibles.
            else
            {
                SpawnTile(true);
            }
        }
    }

}