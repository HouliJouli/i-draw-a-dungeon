using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private GameObject defaultWeaponPrefab;

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
