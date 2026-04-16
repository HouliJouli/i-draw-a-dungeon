using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 1f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRange = 1.2f;

    private float currentHealth;
    private float attackTimer;
    private Rigidbody2D rb;
    private Transform player;
    private IDamageable playerDamageable;
    private PlayerMovement playerMovement;
    private HitEffect playerHitEffect;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerDamageable = playerObj.GetComponent<IDamageable>();
            playerMovement = playerObj.GetComponent<PlayerMovement>();
            playerHitEffect = playerObj.GetComponent<HitEffect>();
        }
    }

    public bool IsKnockedBack { get; set; }

    private void FixedUpdate()
    {
        if (player == null || IsKnockedBack) return;

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float distance = toPlayer.magnitude;

        attackTimer -= Time.fixedDeltaTime;

        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;

            if (attackTimer <= 0f)
            {
                bool hit = TryAttackPlayer();
                if (hit) playerHitEffect?.TriggerHit(rb.position);
                attackTimer = attackCooldown;
            }

            return;
        }

        if (distance > stopDistance)
        {
            Vector2 direction = toPlayer / distance;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private bool TryAttackPlayer()
    {
        if (playerMovement != null && playerMovement.IsInvincible) return false;
        playerDamageable?.TakeDamage(attackDamage);
        return true;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
            Destroy(gameObject);
    }
}
