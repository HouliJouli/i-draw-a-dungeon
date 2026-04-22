using Sirenix.OdinInspector;
using UnityEngine;

public class WeaponDrop : MonoBehaviour
{
    [BoxGroup("Drop"), Required]
    [SerializeField] private GameObject weaponPickupPrefab;

    [BoxGroup("Drop"), Range(0f, 1f)]
    [SerializeField] private float dropChance = 0.5f;

    private void Start()
    {
        if (TryGetComponent(out Enemy enemy))
            enemy.OnDeath += TryDrop;
        else if (TryGetComponent(out RangedEnemy rangedEnemy))
            rangedEnemy.OnDeath += TryDrop;
    }

    private void TryDrop()
    {
        if (weaponPickupPrefab == null) return;
        if (Random.value <= dropChance)
            Instantiate(weaponPickupPrefab, transform.position, Quaternion.identity);
    }
}
