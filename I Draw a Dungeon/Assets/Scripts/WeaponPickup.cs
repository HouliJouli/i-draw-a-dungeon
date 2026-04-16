using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private GameObject weaponPrefab;

    public void Collect(WeaponHolder holder)
    {
        holder.EquipWeapon(weaponPrefab);
        Destroy(gameObject);
    }
}
