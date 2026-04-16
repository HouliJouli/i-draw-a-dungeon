using UnityEngine;

/// <summary>
/// Adjusts the camera orthographic size to fit the entire level based on a BoxCollider2D bounds.
/// Centers the camera on the level and sizes it so the full area is visible.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFitLevel : MonoBehaviour
{
    [SerializeField] private BoxCollider2D levelBounds;
    [SerializeField] private float padding = 1f;
    [SerializeField] private float smoothSpeed = 3f;

    private Camera cam;
    private float targetOrthoSize;
    private Vector3 targetPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (levelBounds == null)
        {
            Debug.LogWarning("CameraFitLevel: levelBounds not assigned.");
            return;
        }

        SnapToLevel();
    }

    private void LateUpdate()
    {
        if (levelBounds == null) return;

        CalculateTarget();

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize, smoothSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    private void CalculateTarget()
    {
        Bounds b = levelBounds.bounds;

        float levelWidth = b.size.x + padding * 2f;
        float levelHeight = b.size.y + padding * 2f;

        float sizeByHeight = levelHeight / 2f;
        float sizeByWidth = levelWidth / (2f * cam.aspect);

        targetOrthoSize = Mathf.Max(sizeByHeight, sizeByWidth);
        targetPosition = new Vector3(b.center.x, b.center.y, transform.position.z);
    }

    /// <summary>
    /// Instantly snaps the camera to fit the level, skipping smoothing.
    /// Useful on scene load to avoid a jarring zoom-in animation.
    /// </summary>
    public void SnapToLevel()
    {
        CalculateTarget();
        cam.orthographicSize = targetOrthoSize;
        transform.position = targetPosition;
    }
}
