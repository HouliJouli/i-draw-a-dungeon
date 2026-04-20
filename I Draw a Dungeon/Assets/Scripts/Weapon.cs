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
    private int usesLeft;

    protected float cooldownTimer;

    protected virtual void Awake()
    {
        usesLeft = maxUses;
    }

    public void TryAttack()
    {
        if (cooldownTimer > 0f) return;
        cooldownTimer = attackCooldown;
        PerformAttack();
    }

    protected abstract void PerformAttack();

    protected void ConsumeUse()
    {
        if (maxUses == 0) return;

        usesLeft--;
        if (usesLeft <= 0)
            GetComponentInParent<WeaponHolder>()?.BreakCurrentWeapon();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }
}
