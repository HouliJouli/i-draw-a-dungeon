using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 1f;
    [SerializeField] private float separationRadius = 1f;
    [SerializeField] private float separationForce = 2f;

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

    public bool IsKnockedBack { get; set; }

    private void RefreshTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closest = Mathf.Infinity;
        GameObject target = null;

        foreach (GameObject p in players)
        {
            if (!p.activeInHierarchy) continue;
            float dist = Vector2.Distance(rb.position, p.transform.position);
            if (dist < closest) { closest = dist; target = p; }
        }

        if (target == null) { player = null; return; }

        player = target.transform;
        playerDamageable = target.GetComponent<IDamageable>();
        playerMovement = target.GetComponent<PlayerMovement>();
        playerHitEffect = target.GetComponent<HitEffect>();
    }

    private void FixedUpdate()
    {
        RefreshTarget();
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

        Vector2 separation = GetSeparationVector();

        if (distance > stopDistance)
        {
            Vector2 direction = toPlayer / distance;
            rb.MovePosition(rb.position + (direction * moveSpeed + separation) * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + separation * Time.fixedDeltaTime);
        }
    }

    private Vector2 GetSeparationVector()
    {
        Vector2 separation = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(rb.position, separationRadius);
        foreach (Collider2D col in neighbors)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.TryGetComponent<Enemy>(out _)) continue;

            Vector2 away = rb.position - (Vector2)col.transform.position;
            float dist = away.magnitude;
            if (dist > 0f)
                separation += away.normalized * (separationRadius - dist);
        }
        return separation * separationForce;
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
