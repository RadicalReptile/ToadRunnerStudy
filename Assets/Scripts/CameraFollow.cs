using UnityEngine;

/// <summary>
/// Manages how the camera follows the player.
/// </summary>
// Based on the 3D endless runner tutorial by Practical Programming on YouTube: 
// Original source: https://www.youtube.com/playlist?list=PLvcJYjdXa962PHXFjQ5ugP59Ayia-rxM3
public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("The player's transform to follow.")]
    [SerializeField] private Transform player;

    private Vector3 offset;

    private void Awake()
    {
        // Calculate the initial offset based on the camera's starting position.
        offset = transform.position - player.position;
    }

    private void LateUpdate()
    {
        // Calculate the new camera position based on the player's position and the offset.
        Vector3 targetPos = player.position + offset;

        // Keep the camera's X and Y position constant.
        targetPos.x = 0;
        targetPos.y = 1.75f;

        // Update the camera's position.
        transform.position = targetPos;
    }

}