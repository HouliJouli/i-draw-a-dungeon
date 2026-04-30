using DG.Tweening;
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

    public void DropCurrentWeapon(Vector3 position)
    {
        if (CurrentWeapon == null || !CurrentWeapon.HasLimitedUses) return;

        WeaponPickup wp = CurrentWeapon.GetPickupComponent();
        GameObject dropPrefab = wp != null ? wp.DropPickupPrefab : null;
        int uses = CurrentWeapon.RemainingUses;

        Destroy(CurrentWeapon.gameObject);
        CurrentWeapon = null;

        if (dropPrefab != null && uses > 0)
        {
            GameObject go = Instantiate(dropPrefab, position, Quaternion.identity);
            if (go.TryGetComponent(out WeaponPickup pickup))
            {
                pickup.enabled = true;
                pickup.InitWithUses(uses);
            }
        }

        EquipWeapon(defaultWeaponPrefab);
    }

    public void EquipWeapon(GameObject prefab)
    {
        if (prefab == null) return;

        if (CurrentWeapon != null)
            Destroy(CurrentWeapon.gameObject);

        weaponSlot.DOComplete();

        GameObject instance = Instantiate(prefab, weaponSlot);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        CurrentWeapon = instance.GetComponent<Weapon>();
        if (CurrentWeapon != null)
            CurrentWeapon.enabled = true;

        if (instance.TryGetComponent(out WeaponPickup wp))
            wp.enabled = false;
    }

    public void BreakCurrentWeapon()
    {
        EquipWeapon(defaultWeaponPrefab);
    }
}
