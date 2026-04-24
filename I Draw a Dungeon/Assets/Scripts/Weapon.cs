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

    public bool HasLimitedUses => maxUses > 0;

    protected float cooldownTimer;

    protected virtual void Awake()
    {
        RemainingUses = maxUses;
    }

    public void TryAttack()
    {
        if (cooldownTimer > 0f) return;
        cooldownTimer = attackCooldown;
        PerformAttack();
    }

    public virtual void OnAttackPressed() => TryAttack();
    public virtual void OnAttackReleased() { }

    protected abstract void PerformAttack();

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
