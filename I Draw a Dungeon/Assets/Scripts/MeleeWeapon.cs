using System.Collections;
using System.Collections.Generic;
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

    [Header("Hit Window")]
    [SerializeField] [Range(0f, 1f)] private float activeStart = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float activeEnd   = 0.7f;

    [Header("Hit Feedback")]
    [SerializeField] private float hitStopDuration = 0.04f;

    private readonly HashSet<Collider2D> hitTargets = new();
    private Vector3 originalScale;
    private bool hitRegistered;

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
        hitTargets.Clear();
        hitRegistered = false;
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

            float squashT = Mathf.Sin(t * Mathf.PI);
            transform.localScale = new Vector3(
                originalScale.x * (1f + squashAmount * squashT),
                originalScale.y * (1f - squashAmount * squashT),
                originalScale.z);

            float normalizedTime = elapsed / halfDuration;
            if (normalizedTime >= activeStart && normalizedTime <= activeEnd)
                DetectHits();

            if (hitRegistered)
            {
                hitRegistered = false;
                yield return new WaitForSecondsRealtime(hitStopDuration);
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
        Collider2D ownerCollider = GetComponentInParent<Collider2D>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D hit in hits)
        {
            if (hit == ownerCollider) continue;
            if (hitTargets.Contains(hit)) continue;
            hitTargets.Add(hit);
            hitRegistered = true;

            if (hit.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(damage);
            if (hit.TryGetComponent(out HitEffect hitEffect))
                hitEffect.TriggerHit(attackPoint.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
