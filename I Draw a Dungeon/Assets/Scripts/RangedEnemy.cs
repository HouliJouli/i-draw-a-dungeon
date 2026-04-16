using UnityEngine;

public class RangedEnemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 80f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float minDistance = 4f;
    [SerializeField] private float maxDistance = 7f;

    [Header("Aim")]
    [SerializeField] private Transform aimPivot;
    [SerializeField] private float aimSpeed = 15f;

    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 1f;
    [SerializeField] private float separationForce = 2f;

    private float currentHealth;
    private float attackTimer;
    private Rigidbody2D rb;
    private Collider2D col;
    private Transform player;
    private PlayerMovement playerMovement;

    public bool IsKnockedBack { get; set; }

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

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
        playerMovement = target.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        RefreshTarget();
        if (player == null || aimPivot == null) return;

        Vector2 dir = (Vector2)player.position - (Vector2)aimPivot.position;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.LerpAngle(aimPivot.eulerAngles.z, targetAngle, aimSpeed * Time.deltaTime);
        aimPivot.rotation = Quaternion.Euler(0f, 0f, smoothAngle);
    }

    private void FixedUpdate()
    {
        if (player == null || IsKnockedBack) return;

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float distance = toPlayer.magnitude;
        Vector2 dirToPlayer = toPlayer / distance;

        attackTimer -= Time.fixedDeltaTime;

        Vector2 separation = GetSeparationVector();
        Vector2 move = Vector2.zero;

        if (distance < minDistance)
        {
            move = -dirToPlayer * moveSpeed;
        }
        else if (distance > maxDistance)
        {
            move = dirToPlayer * moveSpeed;
        }
        else
        {
            if (attackTimer <= 0f)
            {
                TryShoot(dirToPlayer);
                attackTimer = attackCooldown;
            }
        }

        rb.MovePosition(rb.position + (move + separation) * Time.fixedDeltaTime);
    }

    private void TryShoot(Vector2 direction)
    {
        if (projectilePrefab == null || firePoint == null) return;
        if (playerMovement != null && playerMovement.IsInvincible) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        if (proj.TryGetComponent(out Projectile projectile))
            projectile.Init(direction, col, "Player");
    }

    private Vector2 GetSeparationVector()
    {
        Vector2 separation = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(rb.position, separationRadius);
        foreach (Collider2D neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;
            if (!neighbor.TryGetComponent<RangedEnemy>(out _) && !neighbor.TryGetComponent<Enemy>(out _)) continue;

            Vector2 away = rb.position - (Vector2)neighbor.transform.position;
            float dist = away.magnitude;
            if (dist > 0f)
                separation += away.normalized * (separationRadius - dist);
        }
        return separation * separationForce;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
            Destroy(gameObject);
    }
}
