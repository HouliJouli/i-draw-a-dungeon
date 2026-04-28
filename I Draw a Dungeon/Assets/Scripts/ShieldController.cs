using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldController : MonoBehaviour
{
    [BoxGroup("References"), Required]
    [SerializeField] private AimController aimController;

    [BoxGroup("References"), Required]
    [SerializeField] private Transform leftWeaponSlot;

    public Transform LeftWeaponSlot => leftWeaponSlot;

    [BoxGroup("Positioning"), MinValue(0f)]
    [SerializeField] private float shieldDistance = 0.5f;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    public bool IsBlocking { get; private set; }

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    public bool HasShield => _shieldTransform != null;

    private InputAction _blockAction;
    private Transform _shieldTransform;

    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        _blockAction = playerInput?.actions["Block"];
    }

    private void Update()
    {
        bool wasBlocking = IsBlocking;
        IsBlocking = HasShield && (_blockAction?.IsPressed() ?? false);

        if (IsBlocking)
            UpdateShieldTransform();
        else if (wasBlocking)
            ResetShieldToSlot();
    }

    public void SetShield(Transform shieldTransform)
    {
        _shieldTransform = shieldTransform;
    }

    private void UpdateShieldTransform()
    {
        Vector2 dir = aimController.AimDirection;
        _shieldTransform.position = transform.position + (Vector3)(dir * shieldDistance);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _shieldTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void ResetShieldToSlot()
    {
        _shieldTransform.localPosition = Vector3.zero;
        _shieldTransform.localRotation = Quaternion.identity;
    }
}
