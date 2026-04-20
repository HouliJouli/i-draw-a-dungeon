using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class RangedWeapon : Weapon
{
    [BoxGroup("Ranged"), Required]
    [SerializeField] private GameObject projectilePrefab;

    [BoxGroup("Ranged"), Required]
    [SerializeField] private Transform firePoint;

    [FoldoutGroup("Recoil"), MinValue(0f)]
    [SerializeField] private float recoilDistance = 0.15f;

    [FoldoutGroup("Recoil"), MinValue(0.01f)]
    [SerializeField] private float recoilDuration = 0.08f;

    private GameObject nocked;
    private Vector3 originalLocalPosition;
    private Sequence _recoilSequence;

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

        HandsPivot handsPivot = GetComponentInParent<HandsPivot>();
        Vector2 aimDirection = handsPivot != null ? handsPivot.AimDirection : (Vector2)firePoint.right;

        Collider2D ownerCollider = GetComponentInParent<Collider2D>();
        GameObject obj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        obj.GetComponent<Projectile>().Init(aimDirection, ownerCollider);
        ConsumeUse();

        DOVirtual.DelayedCall(attackCooldown, SpawnNockedArrow);

        Vector3 recoilPosition = originalLocalPosition + Vector3.left * recoilDistance;
        float half = recoilDuration * 0.5f;

        _recoilSequence?.Kill();
        _recoilSequence = DOTween.Sequence();
        _recoilSequence.Append(
            transform.DOLocalMove(recoilPosition, half).SetEase(Ease.OutQuad));
        _recoilSequence.Append(
            transform.DOLocalMove(originalLocalPosition, half).SetEase(Ease.OutElastic));
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
