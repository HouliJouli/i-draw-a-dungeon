using UnityEngine;
using UnityEngine.InputSystem;

public class HandsPivot : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float rotationSpeed = 20f;

    private void Update()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        Vector2 direction = (mouseWorld - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);
    }
}
