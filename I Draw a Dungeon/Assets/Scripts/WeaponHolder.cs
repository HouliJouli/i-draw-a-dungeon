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

        bool hasPickupPrefab = (object)CurrentWeapon.PickupPrefab != null;
        int uses = CurrentWeapon.RemainingUses;

        if (hasPickupPrefab && uses > 0)
        {
            GameObject pickupPrefab = CurrentWeapon.PickupPrefab;

            Destroy(CurrentWeapon.gameObject);
            CurrentWeapon = null;

            GameObject go = Instantiate(pickupPrefab, position, Quaternion.identity);
            if (go.TryGetComponent(out WeaponPickup pickup))
            {
                pickup.enabled = true;
                pickup.InitWithUses(uses);
            }
        }
        else
        {
            Destroy(CurrentWeapon.gameObject);
            CurrentWeapon = null;
        }

        EquipWeapon(defaultWeaponPrefab);
    }

    public void EquipWeapon(GameObject prefab)
    {
        if (prefab == null) return;

        if (CurrentWeapon != null)
            Destroy(CurrentWeapon.gameObject);

        // Garante que o slot está na posição correta antes de o Awake da nova arma capturar _slotOriginalLocalPos
        weaponSlot.DOComplete();

        GameObject instance = Instantiate(prefab, weaponSlot);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        CurrentWeapon = instance.GetComponent<Weapon>();

        if (instance.TryGetComponent(out WeaponPickup wp))
            wp.enabled = false;
    }

    public void BreakCurrentWeapon()
    {
        EquipWeapon(defaultWeaponPrefab);
    }
}
