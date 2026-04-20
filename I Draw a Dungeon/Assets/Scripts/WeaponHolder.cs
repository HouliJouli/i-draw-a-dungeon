using Sirenix.OdinInspector;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [BoxGroup("Setup"), Required]
    [SerializeField] private Transform weaponSlot;

    [BoxGroup("Setup"), Required]
    [SerializeField] private GameObject defaultWeaponPrefab;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    public Weapon CurrentWeapon { get; private set; }

    private void Start()
    {
        EquipWeapon(defaultWeaponPrefab);
    }

    public void EquipWeapon(GameObject prefab)
    {
        if (prefab == null) return;

        if (CurrentWeapon != null)
            Destroy(CurrentWeapon.gameObject);

        GameObject instance = Instantiate(prefab, weaponSlot);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        CurrentWeapon = instance.GetComponent<Weapon>();
    }

    public void BreakCurrentWeapon()
    {
        EquipWeapon(defaultWeaponPrefab);
    }
}
