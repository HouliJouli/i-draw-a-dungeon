using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [FoldoutGroup("Stats"), MinValue(0f)]
    [SerializeField] protected float damage = 10f;

    [FoldoutGroup("Stats"), MinValue(0f)]
    [SerializeField] protected float attackRange = 0.5f;

    [FoldoutGroup("Stats"), MinValue(0.05f)]
    [SerializeField] protected float attackCooldown = 0.5f;

    [FoldoutGroup("Durability")]
    [Tooltip("0 = infinito (arma padrão)"), MinValue(0)]
    [SerializeField] private int maxUses = 0;

    [FoldoutGroup("Durability"), ShowInInspector, ReadOnly]
    public int RemainingUses { get; private set; }

    [FoldoutGroup("Attack Thrust")]
    [Tooltip("Ativa o thrust visual durante o ataque.")]
    [SerializeField] private bool thrustEnabled = true;

    [FoldoutGroup("Attack Thrust"), MinValue(0f), ShowIf("thrustEnabled")]
    [Tooltip("Distância de recuo de antecipação antes do avanço.")]
    [SerializeField] private float anticipationDistance = 0.05f;

    [FoldoutGroup("Attack Thrust"), MinValue(0.01f), ShowIf("thrustEnabled")]
    [SerializeField] private float anticipationDuration = 0.06f;

    [FoldoutGroup("Attack Thrust"), MinValue(0f), ShowIf("thrustEnabled")]
    [Tooltip("Distância de avanço na AimDirection durante o ataque.")]
    [SerializeField] private float thrustDistance = 0.2f;

    [FoldoutGroup("Attack Thrust"), MinValue(0.01f), ShowIf("thrustEnabled")]
    [SerializeField] private float thrustDuration = 0.08f;

    [FoldoutGroup("Attack Thrust"), MinValue(0.01f), ShowIf("thrustEnabled")]
    [SerializeField] private float thrustReturnDuration = 0.2f;

    public bool HasLimitedUses => maxUses > 0;

    protected float cooldownTimer;
    private Vector3 _originalLocalPosition;
    private Sequence _thrustSequence;

    protected virtual void Awake()
    {
        RemainingUses = maxUses;
        _originalLocalPosition = transform.localPosition;
    }

    public void TryAttack()
    {
        if (cooldownTimer > 0f) return;
        cooldownTimer = attackCooldown;
        PerformAttack();
        PlayAttackThrust();
    }

    public virtual void OnAttackPressed() => TryAttack();
    public virtual void OnAttackReleased() { }

    protected abstract void PerformAttack();

    // Subclasses que já animam localPosition (Spear, RangedWeapon) fazem override vazio
    protected virtual void PlayAttackThrust()
    {
        if (!thrustEnabled) return;

        AimController aimController = GetComponentInParent<AimController>();
        Vector2 aimDir = aimController != null ? aimController.AimDirection : Vector2.right;

        // Converte AimDirection de world space para local space do parent
        Vector3 thrustWorld     = transform.parent != null
            ? transform.parent.InverseTransformDirection(aimDir)
            : (Vector3)aimDir;

        Vector3 anticipationPos = _originalLocalPosition - thrustWorld * anticipationDistance;
        Vector3 thrustPos       = _originalLocalPosition + thrustWorld * thrustDistance;

        _thrustSequence?.Kill();
        transform.localPosition = _originalLocalPosition;

        _thrustSequence = DOTween.Sequence();
        _thrustSequence.Append(
            transform.DOLocalMove(anticipationPos, anticipationDuration).SetEase(Ease.OutQuad));
        _thrustSequence.Append(
            transform.DOLocalMove(thrustPos, thrustDuration).SetEase(Ease.OutQuint));
        _thrustSequence.Append(
            transform.DOLocalMove(_originalLocalPosition, thrustReturnDuration).SetEase(Ease.OutElastic));
    }

    public void SetRemainingUses(int uses)
    {
        RemainingUses = Mathf.Max(0, uses);
    }

    protected void ConsumeUse()
    {
        if (maxUses == 0) return;

        RemainingUses = Mathf.Max(0, RemainingUses - 1);
        if (RemainingUses <= 0)
            GetComponentInParent<WeaponHolder>()?.BreakCurrentWeapon();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }
}
