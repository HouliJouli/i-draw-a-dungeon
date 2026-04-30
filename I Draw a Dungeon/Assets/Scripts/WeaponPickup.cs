using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [BoxGroup("Setup"), Required]
    [SerializeField] private GameObject weaponPrefab;

    [FoldoutGroup("Highlight"), MinValue(1f)]
    [SerializeField] private float pulseScale = 1.2f;

    [FoldoutGroup("Highlight"), MinValue(0f)]
    [SerializeField] private float pulseSpeed = 3f;

    [FoldoutGroup("Highlight")]
    [SerializeField] private Color highlightColor = Color.yellow;

    public bool IsAxe => weaponPrefab != null && weaponPrefab.GetComponent<Axe>() != null;
    public bool IsRanged => weaponPrefab != null && weaponPrefab.GetComponent<RangedWeapon>() != null;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Color originalColor;
    private Tween _scaleTween;
    private Tween _colorTween;
    private int _remainingUsesOverride = -1;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
        if (sr != null) originalColor = sr.color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        float cycleDuration = 1f / pulseSpeed;

        _scaleTween = transform
            .DOScale(originalScale * pulseScale, cycleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        if (sr != null)
            _colorTween = sr
                .DOColor(highlightColor, cycleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _scaleTween?.Kill();
        _colorTween?.Kill();
        transform.localScale = originalScale;
        if (sr != null) sr.color = originalColor;
    }

    public void InitWithUses(int uses)
    {
        _remainingUsesOverride = uses;
    }

    public void Collect(WeaponHolder holder)
    {
        Debug.Log($"[WeaponPickup.Collect] weaponPrefab={weaponPrefab?.name ?? "NULL"}");
        holder.EquipWeapon(weaponPrefab);

        if (_remainingUsesOverride >= 0 && holder.CurrentWeapon != null)
            holder.CurrentWeapon.SetRemainingUses(_remainingUsesOverride);

        Destroy(gameObject);
    }
}
