using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Weapon")]
    [SerializeField] private WeaponHolder weaponHolder;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private Vector2 lastDirection = Vector2.right;

    public float DashCooldownRatio => dashCooldown > 0f ? Mathf.Clamp01(1f - cooldownTimer / dashCooldown) : 1f;

    private bool isDashing;
    private float dashTimer;
    private float cooldownTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        inputDirection = value.Get<Vector2>();

        if (inputDirection != Vector2.zero)
            lastDirection = inputDirection;
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;
        weaponHolder?.CurrentWeapon?.TryAttack();
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed) return;
        if (isDashing) return;
        if (cooldownTimer > 0f) return;

        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;
        SetDashInvincibility(true);
    }

    private void SetDashInvincibility(bool active)
    {
        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemies");
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, active);
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = lastDirection.normalized * dashSpeed;

            if (dashTimer <= 0f)
            {
                isDashing = false;
                SetDashInvincibility(false);
            }

            return;
        }

        cooldownTimer -= Time.fixedDeltaTime;
        rb.linearVelocity = inputDirection * moveSpeed;
    }
}
