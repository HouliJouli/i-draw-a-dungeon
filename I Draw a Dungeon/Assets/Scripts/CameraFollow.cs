using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private BoxCollider2D bounds;

    private Camera cam;
    private float halfHeight;
    private float halfWidth;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        if (bounds != null)
            smoothedPosition = ClampToBounds(smoothedPosition);

        transform.position = smoothedPosition;
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        halfHeight = cam.orthographicSize;
        halfWidth = cam.orthographicSize * cam.aspect;

        Bounds b = bounds.bounds;

        float clampedX = Mathf.Clamp(position.x, b.min.x + halfWidth, b.max.x - halfWidth);
        float clampedY = Mathf.Clamp(position.y, b.min.y + halfHeight, b.max.y - halfHeight);

        return new Vector3(clampedX, clampedY, position.z);
    }
}
