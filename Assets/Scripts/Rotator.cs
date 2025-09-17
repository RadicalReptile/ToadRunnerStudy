using UnityEngine;

/// <summary>
/// Simple script to continuously rotate a GameObject around specified axes:
/// The rotation speed and direction are controlled by serialized fields.
/// </summary>
// Original concept from Unity forum post by starmind001: https://forum.unity.com/threads/simple-rotation-script-free.510303/
public class Rotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("The speed of rotation in degrees per second.")]
    [SerializeField] private float rotationSpeed = 50.0f;

    [Tooltip("Defines the axis of rotation (e.g., (1,0,0) for X-axis, (0,1,0) for Y-axis). " +
             "Use negative values for reverse direction (e.g., (-1,0,0) for reverse X).")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // default to Y-axis (up) rotation

    [Tooltip("Defines the space relative to which the object rotates.")]
    [SerializeField] private Space rotationSpace = Space.Self;  // rotate relative to itself (local space)

    private void Update()
    {
        // Calculate the rotation amount for this frame.
        // Normalize the axis to ensure consistent speed regardless of vector magnitude.
        // Multiply by Time.deltaTime to make rotation frame-rate independent.
        Vector3 rotationAmount = rotationSpeed * Time.deltaTime * rotationAxis.normalized;

        // Apply the rotation.
        transform.Rotate(rotationAmount, rotationSpace);
    }

}