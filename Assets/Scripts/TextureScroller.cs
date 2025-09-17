using UnityEngine;

/// <summary>
/// Scrolls a texture on a MeshRenderer's material to create a moving background effect.
/// </summary>
// Based on code by bhavinbhai2707: https://discussions.unity.com/t/scroll-using-material-maintextureoffset-makes-the-texture-very-distorted/235441
public class TextureScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("The speed at which the texture scrolls horizontally.")]
    [SerializeField] private float scrollSpeedX;
    [Tooltip("The speed at which the texture scrolls vertically.")]
    [SerializeField] private float scrollSpeedY;

    private MeshRenderer meshRenderer;
    private float offsetX;
    private float offsetY;

    private void Awake()
    {
        // Cache the MeshRenderer component for better performance.
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        // Calculate the offset for the next frame using Time.deltaTime.
        // This makes the scroll speed frame-rate independent.
        offsetX += (Time.deltaTime * scrollSpeedX) / 10.0f;
        offsetY += (Time.deltaTime * scrollSpeedY) / 10.0f;

        // Apply the new offset to the material's main texture.
        meshRenderer.material.mainTextureOffset = new Vector2(offsetX, offsetY);
    }

}