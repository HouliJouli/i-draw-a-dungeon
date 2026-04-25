using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Axe : Weapon
{
    [BoxGroup("Attack"), Required]
    [SerializeField] private Transform attackPoint;

    [BoxGroup("Attack")]
    [SerializeField] private LayerMask enemyLayers;

    [BoxGroup("Reflection")]
    [SerializeField] private LayerMask projectileLayers;

    [BoxGroup("Reflection"), MinValue(0f)]
    [Tooltip("Raio de detecção para reflexão. Pode ser maior que o attackRange para cobrir toda a lâmina.")]
    [SerializeField] private float reflectionRange = 1.2f;

    [BoxGroup("Reflection")]
    [Tooltip("Tag atribuída ao projétil refletido. Vazio = atinge qualquer alvo.")]
    [SerializeField] private string reflectedTargetTag = "";

    [FoldoutGroup("Swing Animation"), MinValue(0f)]
    [SerializeField] private float swingAngle = 130f;

    [FoldoutGroup("Swing Animation"), MinValue(0.05f)]
    [SerializeField] private float swingDuration = 0.45f;

    [FoldoutGroup("Swing Animation"), MinValue(0f)]
    [SerializeField] private float overshootAngle = 20f;

    [FoldoutGroup("Swing Animation"), MinValue(0f)]
    [SerializeField] private float squashAmount = 0.2f;

    [FoldoutGroup("Hit Window"), Range(0f, 1f)]
    [SerializeField] private float activeStart = 0.2f;

    [FoldoutGroup("Hit Window"), Range(0f, 1f)]
    [SerializeField] private float activeEnd = 0.8f;

    [BoxGroup("Hit Feedback"), MinValue(0f)]
    [SerializeField] private float hitStopDuration = 0.07f;

    [FoldoutGroup("Axe Feel"), MinValue(0f)]
    [SerializeField] private float meleeLungeForce = 6f;

    [FoldoutGroup("Axe Feel"), MinValue(0f)]
    [SerializeField] private float meleeRecoilForce = 3f;

    [FoldoutGroup("Axe Feel"), Range(0f, 1f)]
    [Tooltip("Velocidade do player enquanto o machado está equipado.")]
    [SerializeField] private float equippedSpeedMultiplier = 0.7f;

    [FoldoutGroup("Axe Feel"), Range(0f, 1f)]
    [Tooltip("Velocidade do player durante o swing.")]
    [SerializeField] private float swingSpeedMultiplier = 0.3f;

    [FoldoutGroup("Hit Window"), ShowInInspector, ReadOnly]
    public bool IsInActiveWindow { get; private set; }

    private readonly HashSet<Collider2D> hitTargets = new();
    private readonly HashSet<Projectile> reflectedProjectiles = new();
    private Vector3 originalScale;
    private bool hitRegistered;
    private Sequence _swingSequence;
    private Vector2 _swingAimDir;

    protected override void Awake()
    {
        base.Awake();
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        PlayerMovement playerMov = GetComponentInParent<PlayerMovement>();
        if (playerMov != null) playerMov.SpeedMultiplier = equippedSpeedMultiplier;
    }

    private void OnDisable()
    {
        PlayerMovement playerMov = GetComponentInParent<PlayerMovement>();
        if (playerMov != null) playerMov.SpeedMultiplier = 1f;
    }

    protected override void PerformAttack()
    {
        _swingSequence?.Kill();
        hitTargets.Clear();
        reflectedProjectiles.Clear();
        hitRegistered = false;
        IsInActiveWindow = false;

        float halfDuration      = swingDuration * 0.5f;
        float overshootDuration = swingDuration * 0.15f;

        Quaternion startRot     = Quaternion.Euler(0f, 0f, swingAngle);
        Quaternion endRot       = Quaternion.Euler(0f, 0f, -swingAngle);
        Quaternion overshootRot = Quaternion.Euler(0f, 0f, -swingAngle - overshootAngle);

        transform.localRotation = startRot;
        transform.localScale    = originalScale;

        Vector3 squashedScale = new Vector3(
            originalScale.x * (1f + squashAmount),
            originalScale.y * (1f - squashAmount),
            originalScale.z);

        HandsPivot handsPivot    = GetComponentInParent<HandsPivot>();
        Vector2 aimDir           = handsPivot != null ? handsPivot.AimDirection : Vector2.right;
        _swingAimDir             = aimDir.normalized;
        PlayerMovement playerMov = GetComponentInParent<PlayerMovement>();
        if (playerMov != null)
        {
            playerMov.SpeedMultiplier = swingSpeedMultiplier;
            playerMov.LungeVelocity = _swingAimDir * meleeLungeForce;
        }

        _swingSequence = DOTween.Sequence();

        _swingSequence.Append(
            transform.DOLocalRotateQuaternion(endRot, halfDuration).SetEase(Ease.InOutSine));
        _swingSequence.Join(
            transform.DOScale(squashedScale, halfDuration * 0.4f)
                .SetEase(Ease.OutSine)
                .SetLoops(2, LoopType.Yoyo));

        _swingSequence.Append(
            transform.DOLocalRotateQuaternion(overshootRot, overshootDuration).SetEase(Ease.OutQuad));

        _swingSequence.Append(
            transform.DOLocalRotateQuaternion(Quaternion.identity, halfDuration).SetEase(Ease.InOutSine));

        _swingSequence.OnComplete(() =>
        {
            IsInActiveWindow = false;
            transform.localRotation = Quaternion.identity;
            transform.localScale    = originalScale;
            if (playerMov != null)
            {
                playerMov.SpeedMultiplier = equippedSpeedMultiplier;
                playerMov.LungeVelocity = -_swingAimDir * meleeRecoilForce;
            }
        });

        _swingSequence.OnUpdate(() =>
        {
            float elapsed        = _swingSequence.Elapsed();
            float normalizedTime = elapsed / halfDuration;
            bool inWindow        = elapsed <= halfDuration && normalizedTime >= activeStart && normalizedTime <= activeEnd;

            IsInActiveWindow = inWindow;

            if (inWindow)
            {
                DetectHits();
                ReflectProjectiles();
            }

            if (hitRegistered)
            {
                hitRegistered = false;
                _swingSequence.Pause();
                DOVirtual.DelayedCall(hitStopDuration, () => _swingSequence?.Play(), ignoreTimeScale: true);
            }
        });
    }

    private void ReflectProjectiles()
    {
        Collider2D ownerCollider = GetComponentInParent<Collider2D>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, reflectionRange, projectileLayers);

        foreach (Collider2D hit in hits)
        {
            if (!hit.TryGetComponent(out Projectile projectile)) continue;
            if (reflectedProjectiles.Contains(projectile)) continue;

            reflectedProjectiles.Add(projectile);
            projectile.Reflect(_swingAimDir, ownerCollider, reflectedTargetTag);
        }
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

            if (hitTargets.Count == 1)
                ConsumeUse();

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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, reflectionRange);
    }
}
