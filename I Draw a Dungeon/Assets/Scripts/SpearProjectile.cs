using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class SpearProjectile : MonoBehaviour
{
    [BoxGroup("Stats"), MinValue(0.1f)]
    [SerializeField] private float speed = 14f;

    [BoxGroup("Stats"), MinValue(0f)]
    [SerializeField] private float damage = 25f;

    [BoxGroup("Stats"), MinValue(1f)]
    [SerializeField] private float maxDistance = 15f;

    [BoxGroup("Pickup"), Required]
    [SerializeField] private GameObject pickupPrefab;

    [FoldoutGroup("Impact Feel"), MinValue(0f)]
    [SerializeField] private float impactBounceStrength = 0.2f;

    [FoldoutGroup("Impact Feel"), MinValue(0f)]
    [SerializeField] private float impactBounceDuration = 0.2f;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private bool stuck;

    private Vector2 spawnPosition;
    private bool launched;
    private int remainingUses;
    private Rigidbody2D rb;
    private Collider2D col;

    public void Init(Vector2 direction, Collider2D owner = null, int remainingUses = 0)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spawnPosition = transform.position;
        launched = true;
        this.remainingUses = remainingUses;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = direction.normalized * speed;
        }

        if (owner != null && col != null)
            Physics2D.IgnoreCollision(col, owner);
    }

    private void Update()
    {
        if (!launched || stuck) return;

        if (Vector2.Distance(transform.position, spawnPosition) >= maxDistance)
            Stick();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!launched || stuck) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable == null && other.isTrigger) return;

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            remainingUses = 0;
        }

        HitEffect hitEffect = other.GetComponentInParent<HitEffect>();
        if (hitEffect != null)
            hitEffect.TriggerHit(transform.position);

        Stick();
    }

    private void Stick()
    {
        stuck = true;
        launched = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (col != null)
            col.enabled = false;

        if (remainingUses <= 0)
        {
            Destroy(gameObject);
            return;
        }

        transform.DOPunchPosition(
            transform.up * impactBounceStrength,
            impactBounceDuration, vibrato: 2, elasticity: 0.5f)
            .OnComplete(SpawnPickup);
    }

    private void SpawnPickup()
    {
        if (pickupPrefab == null) return;

        GameObject pickupObj = Instantiate(pickupPrefab, transform.position, transform.rotation);

        if (pickupObj.TryGetComponent(out WeaponPickup pickup))
            pickup.InitWithUses(remainingUses);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPosition, 0.1f);
        Gizmos.DrawLine(spawnPosition, spawnPosition + (Vector2.right * maxDistance));
    }
}
