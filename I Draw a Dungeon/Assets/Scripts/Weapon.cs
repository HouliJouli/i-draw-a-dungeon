using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attackRange = 0.5f;
    [SerializeField] protected float attackCooldown = 0.5f;

    [Header("Hands Preset")]
    [SerializeField] private int weaponPresetIndex = 0;

    protected float cooldownTimer;

    private void Awake()
    {
        HandsPivot pivot = GetComponentInParent<HandsPivot>();
        if (pivot != null)
            pivot.SetPreset(weaponPresetIndex);
    }

    public void TryAttack()
    {
        if (cooldownTimer > 0f) return;
        cooldownTimer = attackCooldown;
        PerformAttack();
    }

    protected abstract void PerformAttack();

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }
}
