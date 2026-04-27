using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimController : MonoBehaviour
{
    [BoxGroup("References")]
    [SerializeField] private PlayerInput playerInput;

    private Camera cam;

    [ShowInInspector, ReadOnly]
    public Vector2 AimDirection { get; private set; } = Vector2.right;

    private Vector2 _aimInput;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    public void OnLook(InputValue value)
    {
        _aimInput = value.Get<Vector2>();
    }

    private void Update()
    {
        Vector2 resolved = ResolveAimDirection();
        if (resolved != Vector2.zero)
            AimDirection = resolved;
    }

    private Vector2 ResolveAimDirection()
    {
        bool isMouseScheme = playerInput == null ||
                             playerInput.currentControlScheme == "Keyboard&Mouse";

        if (isMouseScheme)
        {
            Vector2 screenPos = (_aimInput != Vector2.zero)
                ? _aimInput
                : Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

            if (float.IsNaN(screenPos.x) || float.IsNaN(screenPos.y)) return Vector2.zero;
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            worldPos.z = 0f;
            return (worldPos - transform.position).normalized;
        }
        else
        {
            if (_aimInput.magnitude < 0.1f) return Vector2.zero;
            return _aimInput.normalized;
        }
    }
}
