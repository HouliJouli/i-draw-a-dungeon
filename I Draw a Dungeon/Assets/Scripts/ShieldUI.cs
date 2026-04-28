using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ShieldUI : MonoBehaviour
{
    [BoxGroup("References"), Required]
    [SerializeField] private ShieldController shieldController;

    [BoxGroup("References"), Required]
    [SerializeField] private Image fillImage;

    [FoldoutGroup("Colors")]
    [SerializeField] private Color colorActive = new Color(0.2f, 0.6f, 1f);

    [FoldoutGroup("Colors")]
    [SerializeField] private Color colorRecharge = new Color(0.8f, 0.8f, 0.8f);

    [FoldoutGroup("Colors")]
    [SerializeField] private Color colorCooldown = new Color(1f, 0.3f, 0.3f);

    [FoldoutGroup("Fade"), MinValue(0.01f)]
    [SerializeField] private float fadeInDuration = 0.15f;

    [FoldoutGroup("Fade"), MinValue(0.01f)]
    [SerializeField] private float fadeOutDuration = 0.35f;

    [FoldoutGroup("Fade"), Range(0f, 1f)]
    [SerializeField] private float fadeOutThreshold = 0.95f;

    private CanvasGroup _canvasGroup;
    private bool _visible;
    private Tween _fadeTween;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        fillImage.fillAmount = 1f;
    }

    private void Update()
    {
        if (!shieldController.HasShield)
        {
            SetVisible(false);
            return;
        }

        float ratio = shieldController.ShieldEnergyRatio;
        Shield.ShieldState state = shieldController.State;

        fillImage.fillAmount = ratio;
        fillImage.color = state switch
        {
            Shield.ShieldState.Active   => colorActive,
            Shield.ShieldState.Cooldown => colorCooldown,
            _                           => colorRecharge
        };

        bool shouldShow = state == Shield.ShieldState.Active
                       || state == Shield.ShieldState.Cooldown
                       || ratio < fadeOutThreshold;

        SetVisible(shouldShow);
    }

    private void SetVisible(bool show)
    {
        if (show == _visible) return;
        _visible = show;
        _fadeTween?.Kill();
        _fadeTween = show
            ? _canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad)
            : _canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad);
    }
}
