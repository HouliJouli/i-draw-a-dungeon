using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    [BoxGroup("Setup"), Required]
    [SerializeField] private GameObject shieldPrefab;

    [FoldoutGroup("Highlight"), MinValue(1f)]
    [SerializeField] private float pulseScale = 1.2f;

    [FoldoutGroup("Highlight"), MinValue(0f)]
    [SerializeField] private float pulseSpeed = 3f;

    [FoldoutGroup("Highlight")]
    [SerializeField] private Color highlightColor = Color.cyan;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Color originalColor;
    private Tween _scaleTween;
    private Tween _colorTween;
    private int _hitsOverride = -1;

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

    public void InitWithHits(int hits)
    {
        _hitsOverride = hits;
    }

    public void Collect(ShieldController shieldController)
    {
        if (shieldController == null) return;

        GameObject instance = Instantiate(shieldPrefab, shieldController.LeftWeaponSlot);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        Shield shield = instance.GetComponent<Shield>();
        if (_hitsOverride >= 1) shield.SetCurrentHits(_hitsOverride);
        shieldController.SetShield(shield);

        Destroy(gameObject);
    }
}
