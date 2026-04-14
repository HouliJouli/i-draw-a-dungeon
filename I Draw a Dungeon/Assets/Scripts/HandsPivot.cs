using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class WeaponPreset
{
    public string presetName;
    public Vector2 rightHandLocalPos;
    public Vector2 leftHandLocalPos;
}

public class HandsPivot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform leftHand;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Sway")]
    [SerializeField] private float swayAmount = 0.05f;
    [SerializeField] private float swaySpeed = 8f;

    [Header("Weapon Presets")]
    [SerializeField] private WeaponPreset[] presets = new WeaponPreset[]
    {
        new() { presetName = "Melee",     rightHandLocalPos = new(0.2f, -0.1f),  leftHandLocalPos = new(-0.2f, -0.1f) },
        new() { presetName = "Ranged",    rightHandLocalPos = new(0.4f,  0.0f),  leftHandLocalPos = new(-0.1f,  0.15f) },
        new() { presetName = "DualWield", rightHandLocalPos = new(0.35f, -0.15f), leftHandLocalPos = new(-0.35f, 0.15f) }
    };
    [SerializeField] private int currentPreset = 0;

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

        // Posições das mãos
        if (presets.Length == 0 || currentPreset >= presets.Length) return;
        WeaponPreset preset = presets[currentPreset];

        if (rightHand != null)
            rightHand.localPosition = preset.rightHandLocalPos + new Vector2(0f, swayOffset);

        if (leftHand != null)
            leftHand.localPosition = preset.leftHandLocalPos + new Vector2(0f, -swayOffset);
    }

    public void SetPreset(int index)
    {
        if (index >= 0 && index < presets.Length)
            currentPreset = index;
    }
}
