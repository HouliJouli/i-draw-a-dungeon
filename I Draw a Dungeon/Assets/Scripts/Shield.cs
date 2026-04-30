using Sirenix.OdinInspector;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [BoxGroup("Drop"), Required]
    [Tooltip("Prefab do pickup que será spawnado no chão ao trocar o escudo.")]
    [SerializeField] private GameObject pickupPrefab;

    public GameObject PickupPrefab => pickupPrefab;

    [FoldoutGroup("Energy"), MinValue(0.1f)]
    [SerializeField] private float maxShieldTime = 3f;

    [FoldoutGroup("Energy"), MinValue(0.1f)]
    [Tooltip("Energia recuperada por segundo durante a recarga.")]
    [SerializeField] private float rechargeRate = 1f;

    [FoldoutGroup("Energy"), MinValue(0.1f)]
    [Tooltip("Tempo de penalidade após energia zerar antes de recarregar.")]
    [SerializeField] private float cooldownDuration = 2f;

    [FoldoutGroup("Durability"), MinValue(1)]
    [Tooltip("Número de hits que o escudo aguenta antes de quebrar.")]
    [SerializeField] private int maxHits = 5;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    public ShieldState State { get; private set; } = ShieldState.Recharge;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    public float EnergyRatio => maxShieldTime > 0f ? Mathf.Clamp01(_currentEnergy / maxShieldTime) : 0f;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    public int CurrentHits { get; private set; }

    private float _currentEnergy;
    private float _cooldownTimer;

    public enum ShieldState { Recharge, Active, Cooldown }

    private void Awake()
    {
        _currentEnergy = maxShieldTime;
        CurrentHits = maxHits;
    }

    public void TakeHit()
    {
        CurrentHits--;
        if (CurrentHits <= 0)
            Destroy(gameObject);
    }

    public void SetCurrentHits(int hits)
    {
        CurrentHits = Mathf.Max(1, hits);
    }

    public void Tick(bool holdingBlock)
    {
        switch (State)
        {
            case ShieldState.Recharge:
                _currentEnergy = Mathf.Min(_currentEnergy + rechargeRate * Time.deltaTime, maxShieldTime);
                if (holdingBlock && _currentEnergy > 0f)
                    State = ShieldState.Active;
                break;

            case ShieldState.Active:
                _currentEnergy -= Time.deltaTime;
                if (_currentEnergy <= 0f)
                {
                    _currentEnergy = 0f;
                    _cooldownTimer = cooldownDuration;
                    State = ShieldState.Cooldown;
                }
                else if (!holdingBlock)
                {
                    State = ShieldState.Recharge;
                }
                break;

            case ShieldState.Cooldown:
                _currentEnergy = Mathf.Min(_currentEnergy + rechargeRate * Time.deltaTime, maxShieldTime);
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0f)
                    State = ShieldState.Recharge;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (State != ShieldState.Active) return;

        if (other.TryGetComponent(out Projectile _))
        {
            Destroy(other.gameObject);
            TakeHit();
        }
    }
}
