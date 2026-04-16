using System.Collections;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Swing Animation")]
    [SerializeField] private float swingAngle = 80f;
    [SerializeField] private float swingDuration = 0.2f;
    [SerializeField] private float overshootAngle = 15f;
    [SerializeField] private float squashAmount = 0.15f;

    private bool hitDetected;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    protected override void PerformAttack()
    {
        StopAllCoroutines();
        StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        hitDetected = false;
        float halfDuration = swingDuration * 0.5f;
        float elapsed = 0f;

        Quaternion startRot     = Quaternion.Euler(0f, 0f, swingAngle);
        Quaternion endRot       = Quaternion.Euler(0f, 0f, -swingAngle);
        Quaternion overshootRot = Quaternion.Euler(0f, 0f, -swingAngle - overshootAngle);

        transform.localRotation = startRot;
        transform.localScale = originalScale;

        // Swing forward
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            transform.localRotation = Quaternion.Lerp(startRot, endRot, t);

            // Squash peaks at midpoint (compress length, widen breadth)
            float squashT = Mathf.Sin(t * Mathf.PI);
            transform.localScale = new Vector3(
                originalScale.x * (1f + squashAmount * squashT),
                originalScale.y * (1f - squashAmount * squashT),
                originalScale.z);

            if (!hitDetected && elapsed >= halfDuration * 0.5f)
            {
                DetectHits();
                hitDetected = true;
            }

            yield return null;
        }

        // Overshoot
        elapsed = 0f;
        float overshootDuration = swingDuration * 0.15f;
        while (elapsed < overshootDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / overshootDuration;
            transform.localRotation = Quaternion.Lerp(endRot, overshootRot, t);
            yield return null;
        }

        // Return to original
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            transform.localRotation = Quaternion.Lerp(overshootRot, Quaternion.identity, t);
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, t);
            yield return null;
        }

        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;
    }

    private void DetectHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
