using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    private float currentHealth;
    private Rigidbody2D rb;
    private Transform player;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    public bool IsKnockedBack { get; set; }

    private void FixedUpdate()
    {
        if (player == null || IsKnockedBack) return;

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
            Destroy(gameObject);
    }
}
