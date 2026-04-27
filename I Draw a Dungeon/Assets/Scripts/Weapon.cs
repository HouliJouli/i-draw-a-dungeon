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
    [Tooltip("Distância do centro do player onde a arma chega no pico do thrust — deve coincidir com o raio do AimIndicator.")]
    [SerializeField] private float thrustTargetRadius = 0.8f;

    [FoldoutGroup("Attack Thrust"), MinValue(0.01f), ShowIf("thrustEnabled")]
    [SerializeField] private float thrustDuration = 0.08f;

    [FoldoutGroup("Attack Thrust"), MinValue(0.01f), ShowIf("thrustEnabled")]
    [SerializeField] private float thrustReturnDuration = 0.2f;

    public bool HasLimitedUses => maxUses > 0;

    // Direção da mira capturada no exato frame do input — usada por toda lógica de ataque
    protected Vector2 AttackAimDirection { get; private set; } = Vector2.right;

    protected float cooldownTimer;
    private Vector3 _originalLocalPosition;
    private Vector3 _slotOriginalLocalPos;
    private Sequence _thrustSequence;
    private Sequence _slotSequence;
    private AimController _aimController;

    protected virtual void Awake()
    {
        RemainingUses = maxUses;
        _originalLocalPosition = transform.localPosition;
        _aimController = transform.root.GetComponentInChildren<AimController>();

        if (transform.parent != null)
            _slotOriginalLocalPos = transform.parent.localPosition;
    }

    public void TryAttack()
    {
        if (cooldownTimer > 0f) return;
        cooldownTimer = attackCooldown;

        AttackAimDirection = _aimController != null ? _aimController.AimDirection : Vector2.right;

        AlignSlotForAttack();
        PerformAttack();
        PlayAttackThrust();
    }

    // Move o slot para a linha central antes do ataque e retorna depois
    private void AlignSlotForAttack()
    {
        if (transform.parent == null) return;

        _slotSequence?.Kill();

        // Snapa para linha central (y=0) mantendo x e z originais
        transform.parent.localPosition = new Vector3(
            _slotOriginalLocalPos.x, 0f, _slotOriginalLocalPos.z);

        // Retorna ao braço original após o cooldown
        _slotSequence = DOTween.Sequence();
        _slotSequence.AppendInterval(attackCooldown);
        _slotSequence.Append(
            transform.parent.DOLocalMove(_slotOriginalLocalPos, 0.2f).SetEase(Ease.OutElastic));
    }

    public virtual void OnAttackPressed() => TryAttack();
    public virtual void OnAttackReleased() { }

    protected abstract void PerformAttack();

    // Subclasses que já animam localPosition (Spear, RangedWeapon, Axe) fazem override vazio
    protected virtual void PlayAttackThrust()
    {
        if (!thrustEnabled) return;

        Vector3 playerPos   = transform.root.position;
        Vector3 targetWorld = playerPos + (Vector3)(AttackAimDirection * thrustTargetRadius);

        Vector3 thrustLocal = transform.parent != null
            ? transform.parent.InverseTransformPoint(targetWorld)
            : targetWorld;

        Vector3 backDir         = transform.parent != null
            ? transform.parent.InverseTransformDirection(-(Vector3)AttackAimDirection)
            : -(Vector3)AttackAimDirection;
        Vector3 anticipationPos = _originalLocalPosition + backDir * anticipationDistance;

        _thrustSequence?.Kill();
        transform.localPosition = _originalLocalPosition;

        _thrustSequence = DOTween.Sequence();
        _thrustSequence.Append(
            transform.DOLocalMove(anticipationPos, anticipationDuration).SetEase(Ease.OutQuad));
        _thrustSequence.Append(
            transform.DOLocalMove(thrustLocal, thrustDuration).SetEase(Ease.OutQuint));
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
