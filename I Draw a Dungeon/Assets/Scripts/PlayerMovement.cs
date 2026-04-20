using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour, IDamageable
{
    [BoxGroup("Health"), MinValue(1f)]
    [SerializeField] private float maxHealth = 100f;

    [BoxGroup("Movement"), MinValue(0.1f)]
    [SerializeField] private float moveSpeed = 5f;

    [BoxGroup("Weapon"), Required]
    [SerializeField] private WeaponHolder weaponHolder;

    [BoxGroup("Weapon"), MinValue(0f)]
    [SerializeField] private float collectRadius = 1.5f;

    [FoldoutGroup("Dash"), MinValue(0.1f)]
    [SerializeField] private float dashSpeed = 20f;

    [FoldoutGroup("Dash"), MinValue(0.05f)]
    [SerializeField] private float dashDuration = 0.15f;

    [FoldoutGroup("Dash"), MinValue(0.1f)]
    [SerializeField] private float dashCooldown = 1f;

    [FoldoutGroup("Dash"), MinValue(0f)]
    [SerializeField] private float postDashInvincibility = 0.5f;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private float currentHealth;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private bool isDashing;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private bool isInvincible;

    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private Vector2 lastDirection = Vector2.right;
    private float dashTimer;
    private float invincibilityTimer;
    private float cooldownTimer;

    public float DashCooldownRatio => dashCooldown > 0f ? Mathf.Clamp01(1f - cooldownTimer / dashCooldown) : 1f;
    public bool IsInvincible => isInvincible;
    public bool IsKnockedBack { get; set; }

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Projectiles"),
            LayerMask.NameToLayer("Default"),
            true);
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible) { Debug.Log("Damage blocked by invincibility."); return; }

        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Debug.Log("Player died.");
            gameObject.SetActive(false);
        }
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

    public void OnCollect(InputValue value)
    {
        if (!value.isPressed) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collectRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out WeaponPickup pickup))
            {
                pickup.Collect(weaponHolder);
                return;
            }
        }
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed) return;
        if (isDashing) return;
        if (cooldownTimer > 0f) return;

        isDashing = true;
        isInvincible = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;
        SetDashInvincibility(true);
    }

    private void SetDashInvincibility(bool active)
    {
        int playerLayer = gameObject.layer;
        Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("Enemies"), active);
        Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("Projectiles"), active);
        Physics2D.IgnoreLayerCollision(playerLayer, playerLayer, active);
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
                invincibilityTimer = postDashInvincibility;
            }

            return;
        }

        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.fixedDeltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                SetDashInvincibility(false);
            }
        }

        cooldownTimer -= Time.fixedDeltaTime;
        if (IsKnockedBack) return;
        rb.linearVelocity = inputDirection * moveSpeed;
    }
}
