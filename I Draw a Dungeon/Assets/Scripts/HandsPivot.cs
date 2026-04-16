using UnityEngine;
using UnityEngine.InputSystem;

public class HandsPivot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerInput playerInput;

    [Header("Weapon Slots")]
    [SerializeField] private Transform rightWeaponSlot;
    [SerializeField] private Transform leftWeaponSlot;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Sway")]
    [SerializeField] private float swayAmount = 0.05f;
    [SerializeField] private float swaySpeed = 8f;

    private Vector2 aimInput;
    private Vector2 lastAimDirection = Vector2.right;

    /// <summary>Direção de mira atual em world space, normalizada.</summary>
    public Vector2 AimDirection => lastAimDirection;

    private float previousAngle;
    private float angularVelocity;
    private float swayOffset;

    // Called by PlayerInput (Send Messages) when the Look action fires.
    public void OnLook(InputValue value)
    {
        aimInput = value.Get<Vector2>();
    }

    private void Update()
    {
        Vector2 direction = ResolveAimDirection();
        if (direction == Vector2.zero) return;

        lastAimDirection = direction;

        // Rotation
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

        // Angular velocity for sway
        float deltaAngle = Mathf.DeltaAngle(previousAngle, smoothAngle);
        angularVelocity = deltaAngle / Time.deltaTime;
        swayOffset = Mathf.Lerp(swayOffset, Mathf.Clamp(angularVelocity * swayAmount, -swayAmount, swayAmount), swaySpeed * Time.deltaTime);
        previousAngle = smoothAngle;

        // Flip when aiming left
        bool facingLeft = direction.x < 0f;
        transform.localScale = new Vector3(1f, facingLeft ? -1f : 1f, 1f);

        // Sway on slots
        if (rightWeaponSlot != null)
            rightWeaponSlot.localPosition = new Vector2(rightWeaponSlot.localPosition.x, swayOffset);
        if (leftWeaponSlot != null)
            leftWeaponSlot.localPosition = new Vector2(leftWeaponSlot.localPosition.x, -swayOffset);
    }

    private Vector2 ResolveAimDirection()
    {
        bool isMouseScheme = playerInput == null ||
                             playerInput.currentControlScheme == "Keyboard&Mouse";

        if (isMouseScheme)
        {
            // Quando playerInput não está configurado ou scheme é mouse,
            // lê a posição atual do mouse diretamente para garantir que nunca fique em zero.
            Vector2 screenPos = (playerInput != null && aimInput != Vector2.zero)
                ? aimInput
                : Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

            if (float.IsNaN(screenPos.x) || float.IsNaN(screenPos.y)) return Vector2.zero;
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            worldPos.z = 0f;
            return (worldPos - transform.position).normalized;
        }
        else
        {
            // aimInput é a direção do stick (já normalizado, -1..1)
            if (aimInput.magnitude < 0.1f) return Vector2.zero; // stick em repouso — mantém ângulo
            return aimInput.normalized;
        }
    }
}
