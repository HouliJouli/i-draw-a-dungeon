using Sirenix.OdinInspector;
using UnityEngine;

public class HandsPivot : MonoBehaviour
{
    [BoxGroup("References"), Required]
    [SerializeField] private AimController aimController;

    [BoxGroup("Weapon Slots")]
    [SerializeField] private Transform rightWeaponSlot;

    [BoxGroup("Weapon Slots")]
    [SerializeField] private Transform leftWeaponSlot;

    [FoldoutGroup("Rotation"), MinValue(0f)]
    [SerializeField] private float rotationSpeed = 4f;

    [FoldoutGroup("Sway/Rotation"), MinValue(0f)]
    [Tooltip("Sway baseado na velocidade angular da mira (rotação rápida).")]
    [SerializeField] private float rotationSwayAmount = 0.05f;

    [FoldoutGroup("Sway/Rotation"), MinValue(0f)]
    [SerializeField] private float rotationSwaySpeed = 8f;

    [FoldoutGroup("Sway/Movement"), MinValue(0f)]
    [Tooltip("Intensidade do atraso angular causado pelo movimento lateral (hand drag).")]
    [SerializeField] private float movementSwayScale = 4f;

    [FoldoutGroup("Sway/Movement"), MinValue(0f)]
    [Tooltip("Velocidade de suavização do movement sway.")]
    [SerializeField] private float movementSwaySpeed = 6f;

    [FoldoutGroup("Sway/Movement"), MinValue(0f)]
    [Tooltip("Offset máximo em graus que o movement sway pode aplicar.")]
    [SerializeField] private float movementSwayMaxDegrees = 15f;

    private float previousAngle;
    private float angularVelocity;
    private float rotationSwayOffset;
    private float movementSwayOffset;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponentInParent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        Vector2 aimDir = aimController.AimDirection;

        float lateralVelocity = 0f;
        if (_rb != null)
        {
            Vector2 perp = new Vector2(-aimDir.y, aimDir.x);
            lateralVelocity = Vector2.Dot(_rb.linearVelocity, perp);
        }

        float movementSwayTarget = Mathf.Clamp(lateralVelocity * movementSwayScale, -movementSwayMaxDegrees, movementSwayMaxDegrees);
        movementSwayOffset = Mathf.Lerp(movementSwayOffset, movementSwayTarget, movementSwaySpeed * Time.deltaTime);

        float targetAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg + movementSwayOffset;
        float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

        float deltaAngle = Mathf.DeltaAngle(previousAngle, smoothAngle);
        angularVelocity = deltaAngle / Time.deltaTime;
        rotationSwayOffset = Mathf.Lerp(
            rotationSwayOffset,
            Mathf.Clamp(angularVelocity * rotationSwayAmount, -rotationSwayAmount, rotationSwayAmount),
            rotationSwaySpeed * Time.deltaTime);
        previousAngle = smoothAngle;

        transform.localScale = Vector3.one;

        if (rightWeaponSlot != null)
            rightWeaponSlot.localPosition = new Vector2(rightWeaponSlot.localPosition.x, rotationSwayOffset);
        if (leftWeaponSlot != null)
            leftWeaponSlot.localPosition = new Vector2(leftWeaponSlot.localPosition.x, -rotationSwayOffset);
    }
}
