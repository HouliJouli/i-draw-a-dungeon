using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class DashUI : MonoBehaviour
{
    [BoxGroup("References"), Required]
    [SerializeField] private PlayerMovement playerMovement;

    [BoxGroup("References"), Required]
    [SerializeField] private Image fillImage;

    [FoldoutGroup("Fade"), MinValue(0.01f)]
    [SerializeField] private float fadeInDuration = 0.15f;

    [FoldoutGroup("Fade"), MinValue(0.01f)]
    [SerializeField] private float fadeOutDuration = 0.35f;

    [FoldoutGroup("Fade"), Range(0f, 1f)]
    [SerializeField] private float fadeOutThreshold = 0.85f;

    private CanvasGroup canvasGroup;
    private bool _visible;
    private Tween _fadeTween;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        fillImage.fillAmount = 1f;
    }

    private void Update()
    {
        float ratio = playerMovement.DashCooldownRatio;
        fillImage.fillAmount = ratio;

        bool shouldShow = ratio < 1f && ratio < fadeOutThreshold;

        if (shouldShow == _visible) return;

        _visible = shouldShow;
        _fadeTween?.Kill();
        _fadeTween = shouldShow
            ? canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad)
            : canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad);
    }
}
