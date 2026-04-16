using System.Collections;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Scale Punch")]
    [SerializeField] private float punchScale = 1.3f;
    [SerializeField] private float punchDuration = 0.08f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.1f;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Color originalColor;
    private Vector3 originalScale;
    private Coroutine punchCoroutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (sr != null) originalColor = sr.color;
        originalScale = transform.localScale;
    }

    public void TriggerHit(Vector2 hitSourcePosition)
    {
        if (sr != null) StartCoroutine(FlashRoutine());
        if (rb != null) StartCoroutine(KnockbackRoutine(hitSourcePosition));

        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(ScalePunchRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
    }

    private IEnumerator ScalePunchRoutine()
    {
        float half = punchDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            transform.localScale = Vector3.LerpUnclamped(originalScale, originalScale * punchScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            transform.localScale = Vector3.LerpUnclamped(originalScale * punchScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        punchCoroutine = null;
    }

    private IEnumerator KnockbackRoutine(Vector2 hitSourcePosition)
    {
        Enemy enemy = GetComponent<Enemy>();
        PlayerMovement player = GetComponent<PlayerMovement>();
        if (enemy != null) enemy.IsKnockedBack = true;
        if (player != null) player.IsKnockedBack = true;

        Vector2 direction = ((Vector2)transform.position - hitSourcePosition).normalized;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        rb.linearVelocity = Vector2.zero;

        if (enemy != null) enemy.IsKnockedBack = false;
        if (player != null) player.IsKnockedBack = false;
    }
}
