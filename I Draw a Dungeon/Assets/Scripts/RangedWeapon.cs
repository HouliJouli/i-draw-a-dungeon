using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RangedWeapon : Weapon
{
    [Header("Ranged")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Recoil")]
    [SerializeField] private float recoilDistance = 0.15f;
    [SerializeField] private float recoilDuration = 0.08f;

    private GameObject nocked;
    private Vector3 originalLocalPosition;
    private Coroutine recoilCoroutine;

    private void Start()
    {
        originalLocalPosition = transform.localPosition;
        SpawnNockedArrow();
    }

    protected override void PerformAttack()
    {
if (projectilePrefab == null || firePoint == null) return;

        if (nocked != null)
        {
            nocked.SetActive(false);
            Destroy(nocked);
            nocked = null;
        }

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 aimDirection = (mouseWorld - (Vector2)firePoint.position).normalized;

        Collider2D ownerCollider = GetComponentInParent<Collider2D>();
        GameObject obj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        obj.GetComponent<Projectile>().Init(aimDirection, ownerCollider);
        ConsumeUse();

        StartCoroutine(RenockAfterCooldown());

        if (recoilCoroutine != null) StopCoroutine(recoilCoroutine);
        recoilCoroutine = StartCoroutine(RecoilRoutine());
    }

    private IEnumerator RenockAfterCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        SpawnNockedArrow();
    }

    private IEnumerator RecoilRoutine()
    {
        Vector3 recoilPosition = originalLocalPosition + Vector3.left * recoilDistance;
        float half = recoilDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalLocalPosition, recoilPosition, elapsed / half);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(recoilPosition, originalLocalPosition, elapsed / half);
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        recoilCoroutine = null;
    }

    private void SpawnNockedArrow()
    {
        if (projectilePrefab == null || firePoint == null) return;
        nocked = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation, firePoint);
        nocked.transform.localPosition = Vector3.zero;

        Rigidbody2D rb = nocked.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        Collider2D col = nocked.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }
}
