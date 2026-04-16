using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attackRange = 0.5f;
    [SerializeField] protected float attackCooldown = 0.5f;

    [Header("Durability")]
    [Tooltip("0 = infinite (default weapon)")]
    [SerializeField] private int maxUses = 0;

    protected float cooldownTimer;
    private int usesLeft;

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
