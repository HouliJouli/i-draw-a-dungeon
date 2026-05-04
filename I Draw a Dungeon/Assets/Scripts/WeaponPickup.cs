using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [BoxGroup("Setup"), Required]
    [SerializeField] private GameObject weaponPrefab;

    [BoxGroup("Setup")]
    [Tooltip("Prefab de pickup que será instanciado no chão ao dropar esta arma. Deve referenciar um prefab do Project (não a própria arma).")]
    [SerializeField] private GameObject dropPickupPrefab;

    public GameObject DropPickupPrefab => dropPickupPrefab;

    [FoldoutGroup("Highlight"), MinValue(1f)]
    [SerializeField] private float pulseScale = 1.2f;

    [FoldoutGroup("Highlight"), MinValue(0f)]
    [SerializeField] private float pulseSpeed = 3f;

    [FoldoutGroup("Highlight")]
    [SerializeField] private Color highlightColor = Color.yellow;

    [FoldoutGroup("Highlight"), MinValue(0f)]
    [Tooltip("Distância máxima para ativar o highlight. Deve ser igual ao collectRadius do PlayerMovement.")]
    [SerializeField] private float highlightRadius = 1.5f;

    public bool IsAxe => weaponPrefab != null && weaponPrefab.GetComponent<Axe>() != null;
    public bool IsRanged => weaponPrefab != null && weaponPrefab.GetComponent<RangedWeapon>() != null;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Color originalColor;
    private Tween _scaleTween;
    private Tween _colorTween;
    private int _remainingUsesOverride = -1;
    private bool _isHighlighted;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
        if (sr != null) originalColor = sr.color;
    }

    private void Update()
    {
        bool playerNear = IsPlayerNear();

        if (playerNear && !_isHighlighted)
            StartHighlight();
        else if (!playerNear && _isHighlighted)
            StopHighlight();
    }

    private bool IsPlayerNear()
    {
        PlayerMovement[] players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (PlayerMovement p in players)
        {
            if (!p.gameObject.activeSelf) continue;
            if (Vector2.Distance(transform.position, p.transform.position) <= highlightRadius)
                return true;
        }
        return false;
    }

    private void StartHighlight()
    {
        _isHighlighted = true;
        _scaleTween?.Kill();
        _colorTween?.Kill();

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

    private void StopHighlight()
    {
        _isHighlighted = false;
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
        holder.EquipWeapon(weaponPrefab);

        if (_remainingUsesOverride >= 0 && holder.CurrentWeapon != null)
            holder.CurrentWeapon.SetRemainingUses(_remainingUsesOverride);

        Destroy(gameObject);
    }
}
