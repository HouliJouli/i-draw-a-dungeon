using UnityEngine;
using UnityEngine.UI;

public class DoorIndicator : MonoBehaviour
{
    [SerializeField] private DoorController door;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RectTransform arrowRect;
    [SerializeField] private Image arrowImage;

    [Header("Edge Padding")]
    [SerializeField] private float screenEdgePadding = 60f;

    [Header("Pulse")]
    [SerializeField] private float pulseDistance = 12f;
    [SerializeField] private float pulseSpeed = 3f;

    [Header("Rotation")]
    [Tooltip("Ajuste se a ponta da seta não estiver apontando para a porta. 0 = ponta para cima no sprite.")]
    [SerializeField] private float rotationOffset = 0f;

    private bool _doorOpen;
    private Vector3 _doorWorldPosition;

    private void OnEnable()
    {
        if (door != null)
        {
            door.OnDoorOpened += HandleDoorOpened;
            door.OnDoorClosed += HandleDoorClosed;
        }
    }

    private void OnDisable()
    {
        if (door != null)
        {
            door.OnDoorOpened -= HandleDoorOpened;
            door.OnDoorClosed -= HandleDoorClosed;
        }
    }

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        _doorWorldPosition = door != null ? door.transform.position : Vector3.zero;

        SetArrowVisible(false);
    }

    private void Update()
    {
        if (!_doorOpen) return;

        Vector3 viewport = targetCamera.WorldToViewportPoint(_doorWorldPosition);
        bool isOffScreen = viewport.x < 0f || viewport.x > 1f ||
                           viewport.y < 0f || viewport.y > 1f ||
                           viewport.z < 0f;

        SetArrowVisible(isOffScreen);

        if (!isOffScreen) return;

        Vector2 direction = GetEdgePositionAndDirection(viewport, out Vector2 edgePos);

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseDistance;
        arrowRect.position = edgePos + direction * pulse;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrowRect.rotation = Quaternion.Euler(0f, 0f, angle - 90f + rotationOffset);
    }

    private Vector2 GetEdgePositionAndDirection(Vector3 viewport, out Vector2 edgePos)
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 screenPos = new Vector2(viewport.x * Screen.width, viewport.y * Screen.height);

        if (viewport.z < 0f)
            screenPos = screenCenter + (screenCenter - screenPos);

        Vector2 direction = (screenPos - screenCenter).normalized;

        float halfW = Screen.width * 0.5f - screenEdgePadding;
        float halfH = Screen.height * 0.5f - screenEdgePadding;

        float scaleX = halfW / Mathf.Abs(direction.x);
        float scaleY = halfH / Mathf.Abs(direction.y);

        edgePos = screenCenter + direction * Mathf.Min(scaleX, scaleY);
        return direction;
    }

    private void SetArrowVisible(bool visible)
    {
        if (arrowImage != null)
            arrowImage.enabled = visible;
    }

    private void HandleDoorOpened()
    {
        _doorOpen = true;
        _doorWorldPosition = door.transform.position;
    }

    private void HandleDoorClosed()
    {
        _doorOpen = false;
        SetArrowVisible(false);
    }
}
