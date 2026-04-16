using UnityEngine;
using UnityEngine.InputSystem;

public class HandsPivot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;

    [Header("Weapon Slots")]
    [SerializeField] private Transform rightWeaponSlot;
    [SerializeField] private Transform leftWeaponSlot;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Sway")]
    [SerializeField] private float swayAmount = 0.05f;
    [SerializeField] private float swaySpeed = 8f;

    private float previousAngle;
    private float angularVelocity;
    private float swayOffset;

    private void Update()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        // Rotação
        Vector2 direction = (mouseWorld - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

        // Velocidade angular para o sway
        float deltaAngle = Mathf.DeltaAngle(previousAngle, smoothAngle);
        angularVelocity = deltaAngle / Time.deltaTime;
        swayOffset = Mathf.Lerp(swayOffset, Mathf.Clamp(angularVelocity * swayAmount, -swayAmount, swayAmount), swaySpeed * Time.deltaTime);
        previousAngle = smoothAngle;

        // Flip quando mouse está à esquerda
        bool facingLeft = mouseWorld.x < transform.position.x;
        transform.localScale = new Vector3(1f, facingLeft ? -1f : 1f, 1f);

        // Sway nos slots
        if (rightWeaponSlot != null)
            rightWeaponSlot.localPosition = new Vector2(rightWeaponSlot.localPosition.x, swayOffset);
        if (leftWeaponSlot != null)
            leftWeaponSlot.localPosition = new Vector2(leftWeaponSlot.localPosition.x, -swayOffset);
    }
}
