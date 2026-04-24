using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Spear : Weapon
{
    [BoxGroup("Attack"), Required]
    [SerializeField] private Transform tipPoint;

    [BoxGroup("Attack")]
    [SerializeField] private LayerMask enemyLayers;

    [FoldoutGroup("Thrust Animation"), MinValue(0f)]
    [SerializeField] private float thrustDistance = 0.8f;

    [FoldoutGroup("Thrust Animation"), MinValue(0.05f)]
    [SerializeField] private float thrustDuration = 0.15f;

    [FoldoutGroup("Thrust Animation"), MinValue(0.05f)]
    [SerializeField] private float returnDuration = 0.12f;

    [FoldoutGroup("Hit Window"), Range(0f, 1f)]
    [SerializeField] private float activeStart = 0.2f;

    [FoldoutGroup("Hit Window"), Range(0f, 1f)]
    [SerializeField] private float activeEnd = 0.85f;

    [BoxGroup("Hit Feedback"), MinValue(0f)]
    [SerializeField] private float hitStopDuration = 0.04f;

    [FoldoutGroup("Melee Feel"), MinValue(0f)]
    [SerializeField] private float meleeLungeForce = 5f;

    [FoldoutGroup("Melee Feel"), MinValue(0f)]
    [SerializeField] private float meleeRecoilForce = 2f;

    [FoldoutGroup("Throw Feel"), Range(0f, 1f)]
    [SerializeField] private float throwSpeedMultiplier = 0.35f;

    [FoldoutGroup("Throw Feel"), MinValue(0f)]
    [SerializeField] private float throwLungeForce = 7f;

    [FoldoutGroup("Throw"), Required]
    [SerializeField] private GameObject spearProjectilePrefab;

    [FoldoutGroup("Throw"), MinValue(0f)]
    [SerializeField] private float minHoldDuration = 0.25f;

    [FoldoutGroup("Throw"), ShowInInspector, ReadOnly]
    public bool IsChargingThrow { get; private set; }

    private readonly HashSet<Collider2D> hitTargets = new();
    private bool hitRegistered;
    private Sequence _thrustSequence;
    private Vector3 _originalLocalPosition;
    private float _pressTime;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        _originalLocalPosition = transform.localPosition;
    }

    public override void OnAttackPressed()
    {
        if (cooldownTimer > 0f) return;
        IsChargingThrow = true;
        _pressTime = Time.time;

        PlayerMovement player = GetComponentInParent<PlayerMovement>();
        if (player != null) player.SpeedMultiplier = throwSpeedMultiplier;
    }

    public override void OnAttackReleased()
    {
        if (!IsChargingThrow) return;
        IsChargingThrow = false;

        PlayerMovement player = GetComponentInParent<PlayerMovement>();
        if (player != null) player.SpeedMultiplier = 1f;

        if (Time.time - _pressTime >= minHoldDuration)
            PerformThrow();
        else
            TryAttack();
    }

    protected override void PerformAttack()
    {
        _thrustSequence?.Kill();
        hitTargets.Clear();
        hitRegistered = false;
        transform.localPosition = _originalLocalPosition;

        HandsPivot handsPivot = GetComponentInParent<HandsPivot>();
        Vector2 aimDir = handsPivot != null ? handsPivot.AimDirection : Vector2.right;
        Rigidbody2D playerRb = GetComponentInParent<Rigidbody2D>();

        PlayerMovement playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.LungeVelocity = aimDir.normalized * meleeLungeForce;

        Vector3 thrustTarget = _originalLocalPosition + Vector3.right * thrustDistance;

        _thrustSequence = DOTween.Sequence();

        _thrustSequence.Append(
            transform.DOLocalMove(thrustTarget, thrustDuration).SetEase(Ease.OutQuint));

        _thrustSequence.Append(
            transform.DOLocalMove(_originalLocalPosition, returnDuration).SetEase(Ease.InSine));

        _thrustSequence.OnComplete(() =>
        {
            transform.localPosition = _originalLocalPosition;
            if (playerMovement != null)
                playerMovement.LungeVelocity = -aimDir.normalized * meleeRecoilForce;
        });

        _thrustSequence.OnUpdate(() =>
        {
            float elapsed = _thrustSequence.Elapsed();

            if (elapsed <= thrustDuration)
            {
                float normalized = elapsed / thrustDuration;
                if (normalized >= activeStart && normalized <= activeEnd)
                    DetectHits();
            }

            if (hitRegistered)
            {
                hitRegistered = false;
                _thrustSequence.Pause();
                DOVirtual.DelayedCall(hitStopDuration, () => _thrustSequence?.Play(), ignoreTimeScale: true);
            }
        });
    }

    private void PerformThrow()
    {
        if (spearProjectilePrefab == null) return;

        cooldownTimer = attackCooldown;

        HandsPivot handsPivot = GetComponentInParent<HandsPivot>();
        Vector2 aimDirection = handsPivot != null ? handsPivot.AimDirection : Vector2.right;
        Collider2D ownerCollider = GetComponentInParent<Collider2D>();
        WeaponHolder weaponHolder = GetComponentInParent<WeaponHolder>();
        int uses = RemainingUses;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        PlayerMovement playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.LungeVelocity = aimDirection.normalized * throwLungeForce;

        Vector3 spawnPos = tipPoint.position;
        weaponHolder?.BreakCurrentWeapon();

        GameObject obj = Instantiate(spearProjectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
        obj.GetComponent<SpearProjectile>()?.Init(aimDirection, ownerCollider, uses);
    }

    private void DetectHits()
    {
        Collider2D ownerCollider = GetComponentInParent<Collider2D>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(tipPoint.position, attackRange, enemyLayers);

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
                hitEffect.TriggerHit(tipPoint.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (tipPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(tipPoint.position, attackRange);
    }
}
