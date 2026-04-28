using Sirenix.OdinInspector;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [BoxGroup("Stats"), MinValue(0.1f)]
    [SerializeField] private float speed = 12f;

    [BoxGroup("Stats"), MinValue(0f)]
    [SerializeField] private float damage = 15f;

    [BoxGroup("Stats"), MinValue(1f)]
    [SerializeField] private float maxDistance = 15f;

    private Vector2 direction;
    private Vector2 spawnPosition;
    private bool launched;
    private string targetTag;
    private Collider2D _ownerCollider;

    public void Init(Vector2 direction, Collider2D owner = null, string targetTag = "")
    {
        this.targetTag = targetTag;
        this.direction = direction.normalized;
        spawnPosition = transform.position;
        launched = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = direction * speed;
        }

        if (owner != null)
        {
            _ownerCollider = owner;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) Physics2D.IgnoreCollision(col, owner);
        }
    }

    public void Reflect(Vector2 newDirection, Collider2D newOwner, string newTargetTag = "")
    {
        targetTag = newTargetTag;
        direction = newDirection.normalized;
        spawnPosition = transform.position;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null && _ownerCollider != null)
            Physics2D.IgnoreCollision(col, _ownerCollider, false);

        if (col != null && newOwner != null)
            Physics2D.IgnoreCollision(col, newOwner);

        _ownerCollider = newOwner;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    private void Update()
    {
        if (!launched) return;

        if (Vector2.Distance(transform.position, spawnPosition) >= maxDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!launched) return;

        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
        {
            Destroy(gameObject);
            return;
        }

        ShieldController shield = other.GetComponentInParent<ShieldController>();
        bool blockedByShield = shield != null && shield.IsBlocking;

        if (blockedByShield)
        {
            shield.TakeHit();
        }
        else
        {
            if (other.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(damage);

            HitEffect hitEffect = other.GetComponentInParent<HitEffect>();
            if (hitEffect != null)
                hitEffect.TriggerHit(transform.position);
        }

        Destroy(gameObject);
    }
}
