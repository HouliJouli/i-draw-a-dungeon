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

    private void Update()
    {
        if (CurrentWeapon == null)
            EquipWeapon(defaultWeaponPrefab);
    }

    public void EquipWeapon(GameObject prefab)
    {
        if (prefab == null) return;

        if (CurrentWeapon != null)
            Destroy(CurrentWeapon.gameObject);

        GameObject instance = Instantiate(prefab, weaponSlot);

        CurrentWeapon = instance.GetComponent<Weapon>();
    }
}
