using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class AimIndicatorUI : MonoBehaviour
{
    [BoxGroup("References"), Required]
    [SerializeField] private AimController aimController;

    [BoxGroup("References"), Required]
    [Tooltip("Transform do player — o indicador orbita ao redor dele.")]
    [SerializeField] private Transform playerTransform;

    [BoxGroup("References"), Required]
    [Tooltip("Transform da seta — posicionada e rotacionada em world space.")]
    [SerializeField] private Transform arrow;

    [BoxGroup("Settings"), MinValue(0f)]
    [Tooltip("Distância em unidades de mundo entre a seta e o centro do player.")]
    [SerializeField] private float radius = 0.6f;

    [FoldoutGroup("Pulse")]
    [SerializeField] private bool pulseEnabled = true;

    [FoldoutGroup("Pulse"), MinValue(0f), ShowIf("pulseEnabled")]
    [SerializeField] private float pulseScale = 0.08f;

    [FoldoutGroup("Pulse"), MinValue(0f), ShowIf("pulseEnabled")]
    [SerializeField] private float pulseDuration = 0.7f;

    private Tween _pulseTween;
    private Vector3 _baseScale;

    private void Awake()
    {
        if (arrow != null)
            _baseScale = arrow.localScale;
    }

    private void OnEnable()
    {
        if (!pulseEnabled || arrow == null) return;

        _pulseTween = arrow
            .DOScale(_baseScale * (1f + pulseScale), pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDisable()
    {
        _pulseTween?.Kill();
        if (arrow != null) arrow.localScale = _baseScale;
    }

    private void LateUpdate()
    {
        if (playerTransform == null || arrow == null) return;

        Vector2 dir = aimController.AimDirection;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arrow.position = playerTransform.position + (Vector3)(dir * radius);
        arrow.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    [Button("Preview at Right")]
    private void PreviewRight()
    {
        if (arrow == null || playerTransform == null) return;
        arrow.position = playerTransform.position + Vector3.right * radius;
        arrow.rotation = Quaternion.identity;
    }
}
