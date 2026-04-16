using System.Collections;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.1f;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Color originalColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (sr != null) originalColor = sr.color;
    }

    public void TriggerHit(Vector2 hitSourcePosition)
    {
        if (sr != null) StartCoroutine(FlashRoutine());
        if (rb != null) StartCoroutine(KnockbackRoutine(hitSourcePosition));
    }

    private IEnumerator FlashRoutine()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
    }

    private IEnumerator KnockbackRoutine(Vector2 hitSourcePosition)
    {
        Vector2 direction = ((Vector2)transform.position - hitSourcePosition).normalized;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        rb.linearVelocity = Vector2.zero;
    }
}
