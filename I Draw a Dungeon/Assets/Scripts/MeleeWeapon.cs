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

    private bool hitDetected;

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

        Quaternion startRot = Quaternion.Euler(0f, 0f, swingAngle);
        Quaternion endRot   = Quaternion.Euler(0f, 0f, -swingAngle);

        transform.localRotation = startRot;

        // Swing forward
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            transform.localRotation = Quaternion.Lerp(startRot, endRot, t);

            if (!hitDetected && elapsed >= halfDuration * 0.5f)
            {
                DetectHits();
                hitDetected = true;
            }

            yield return null;
        }

        transform.localRotation = endRot;

        // Return to original
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            transform.localRotation = Quaternion.Lerp(endRot, Quaternion.identity, t);
            yield return null;
        }

        transform.localRotation = Quaternion.identity;
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
