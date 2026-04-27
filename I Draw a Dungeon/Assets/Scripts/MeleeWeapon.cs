using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [BoxGroup("Attack"), Required]
    [SerializeField] private Transform attackPoint;

    [BoxGroup("Attack")]
    [SerializeField] private LayerMask enemyLayers;

    [FoldoutGroup("Swing Animation"), MinValue(0f)]
    [SerializeField] private float swingAngle = 80f;

    [FoldoutGroup("Swing Animation"), MinValue(0.05f)]
    [SerializeField] private float swingDuration = 0.2f;

    [FoldoutGroup("Swing Animation"), MinValue(0f)]
    [SerializeField] private float overshootAngle = 15f;

    [FoldoutGroup("Swing Animation"), MinValue(0f)]
    [SerializeField] private float squashAmount = 0.15f;

    [FoldoutGroup("Hit Window"), Range(0f, 1f)]
    [SerializeField] private float activeStart = 0.3f;

    [FoldoutGroup("Hit Window"), Range(0f, 1f)]
    [SerializeField] private float activeEnd = 0.7f;

    [BoxGroup("Hit Feedback"), MinValue(0f)]
    [SerializeField] private float hitStopDuration = 0.04f;

    [FoldoutGroup("Melee Feel"), MinValue(0f)]
    [SerializeField] private float meleeLungeForce = 5f;

    [FoldoutGroup("Melee Feel"), MinValue(0f)]
    [SerializeField] private float meleeRecoilForce = 2f;

    private readonly HashSet<Collider2D> hitTargets = new();
    private Vector3 originalScale;
    private bool hitRegistered;
    private Sequence _swingSequence;

    protected override void Awake()
    {
        base.Awake();
        originalScale = transform.localScale;
    }

    protected override void PerformAttack()
    {
        _swingSequence?.Kill();
        hitTargets.Clear();
        hitRegistered = false;

        float halfDuration     = swingDuration * 0.5f;
        float overshootDuration = swingDuration * 0.15f;

        Quaternion startRot     = Quaternion.Euler(0f, 0f, swingAngle);
        Quaternion endRot       = Quaternion.Euler(0f, 0f, -swingAngle);
        Quaternion overshootRot = Quaternion.Euler(0f, 0f, -swingAngle - overshootAngle);

        transform.localRotation = startRot;
        transform.localScale = originalScale;

        Vector3 squashedScale = new Vector3(
            originalScale.x * (1f + squashAmount),
            originalScale.y * (1f - squashAmount),
            originalScale.z);

        Vector2 aimDir = AttackAimDirection;
        PlayerMovement playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.LungeVelocity = aimDir.normalized * meleeLungeForce;

        _swingSequence = DOTween.Sequence();

        // Fase 1: swing + squash
        _swingSequence.Append(
            transform.DOLocalRotateQuaternion(endRot, halfDuration).SetEase(Ease.InOutSine));
        _swingSequence.Join(
            transform.DOScale(squashedScale, halfDuration * 0.5f)
                .SetEase(Ease.OutSine)
                .SetLoops(2, LoopType.Yoyo));

        // Fase 2: overshoot
        _swingSequence.Append(
            transform.DOLocalRotateQuaternion(overshootRot, overshootDuration).SetEase(Ease.OutQuad));

        // Fase 3: retorno
        _swingSequence.Append(
            transform.DOLocalRotateQuaternion(Quaternion.identity, halfDuration).SetEase(Ease.InOutSine));

        _swingSequence.OnComplete(() =>
        {
            transform.localRotation = Quaternion.identity;
            transform.localScale = originalScale;
            if (playerMovement != null)
                playerMovement.LungeVelocity = -aimDir.normalized * meleeRecoilForce;
        });

        _swingSequence.OnUpdate(() =>
        {
            float elapsed = _swingSequence.Elapsed();

            if (elapsed <= halfDuration)
            {
                float normalizedTime = elapsed / halfDuration;
                if (normalizedTime >= activeStart && normalizedTime <= activeEnd)
                    DetectHits();
            }

            if (hitRegistered)
            {
                hitRegistered = false;
                _swingSequence.Pause();
                DOVirtual.DelayedCall(hitStopDuration, () => _swingSequence?.Play(), ignoreTimeScale: true);
            }
        });
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
