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
    public bool HasShield => _shield != null;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    public float ShieldEnergyRatio => _shield != null ? _shield.EnergyRatio : 0f;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    public int ShieldCurrentHits => _shield != null ? _shield.CurrentHits : 0;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    public Shield.ShieldState State => _shield != null ? _shield.State : Shield.ShieldState.Recharge;

    private InputAction _blockAction;
    private Shield _shield;
    private Transform _shieldTransform;

    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        _blockAction = playerInput?.actions["Block"];
    }

    private void Update()
    {
        if (!HasShield)
        {
            IsBlocking = false;
            return;
        }

        bool holdingBlock = _blockAction?.IsPressed() ?? false;
        _shield.Tick(holdingBlock);

        bool wasBlocking = IsBlocking;
        IsBlocking = _shield.State == Shield.ShieldState.Active;

        if (IsBlocking)
            UpdateShieldTransform();
        else if (wasBlocking)
            ResetShieldToSlot();
    }

    public void TakeHit()
    {
        _shield?.TakeHit();
    }

    public void DropShield(Vector3 position)
    {
        if (_shield == null) return;

        GameObject pickupPrefab = _shield.PickupPrefab;
        int hits = _shield.CurrentHits;

        Destroy(_shield.gameObject);
        _shield = null;
        IsBlocking = false;

        if (pickupPrefab == null || hits <= 0) return;

        GameObject go = Instantiate(pickupPrefab, position, Quaternion.identity);
        if (go.TryGetComponent(out ShieldPickup pickup))
        {
            pickup.enabled = true;
            pickup.InitWithHits(hits);
        }
    }

    public void SetShield(Shield shield)
    {
        _shield = shield;
        _shieldTransform = shield.transform;
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
