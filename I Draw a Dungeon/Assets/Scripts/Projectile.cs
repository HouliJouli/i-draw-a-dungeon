using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float maxDistance = 15f;

    private Vector2 direction;
    private Vector2 spawnPosition;
    private bool launched;

    public void Init(Vector2 direction, Collider2D owner = null)
    {
        this.direction = direction.normalized;
        spawnPosition = transform.position;
        launched = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (owner != null)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) Physics2D.IgnoreCollision(col, owner);
        }
    }

    private void Update()
    {
        if (!launched) return;

        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        if (Vector2.Distance(transform.position, spawnPosition) >= maxDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!launched) return;

        if (other.TryGetComponent(out IDamageable damageable))
            damageable.TakeDamage(damage);

        if (other.TryGetComponent(out HitEffect hitEffect))
            hitEffect.TriggerHit(transform.position);

        Destroy(gameObject);
    }
}
