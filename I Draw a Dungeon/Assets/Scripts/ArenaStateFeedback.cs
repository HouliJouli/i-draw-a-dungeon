using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArenaStateFeedback : MonoBehaviour
{
    [BoxGroup("References"), Required]
    [SerializeField] private ArenaManager arenaManager;

    [BoxGroup("References"), Required]
    [SerializeField] private Image overlayImage;

    [BoxGroup("References")]
    [SerializeField] private TextMeshProUGUI stateLabel;

    [BoxGroup("References"), Required]
    [SerializeField] private CameraFitLevel cameraFitLevel;

    [FoldoutGroup("Safe State")]
    [SerializeField] private Color safeColor = new Color(0f, 0f, 0f, 0f);

    [FoldoutGroup("Safe State")]
    [SerializeField] private string safeText = "";

    [FoldoutGroup("Warning State")]
    [SerializeField] private Color warningColor = new Color(1f, 0.6f, 0f, 0.25f);

    [FoldoutGroup("Warning State")]
    [SerializeField] private string warningText = "Get Ready";

    [FoldoutGroup("Warning State"), MinValue(0f)]
    [SerializeField] private float warningShakeAmplitude = 0.03f;

    [FoldoutGroup("Warning State"), MinValue(0f)]
    [SerializeField] private float warningShakeFrequency = 30f;

    [FoldoutGroup("Transition State")]
    [SerializeField] private Color transitionColor = new Color(1f, 0f, 0f, 0.4f);

    [FoldoutGroup("Transition State")]
    [SerializeField] private string transitionText = "RUN";

    [FoldoutGroup("Transition State"), MinValue(0f)]
    [SerializeField] private float transitionShakeAmplitude = 0.08f;

    [FoldoutGroup("Transition State"), MinValue(0f)]
    [SerializeField] private float transitionShakeFrequency = 60f;

    [BoxGroup("Fade"), MinValue(0.01f)]
    [SerializeField] private float fadeDuration = 0.4f;

    private Tween _fadeTween;

    private void Start()
    {
        if (arenaManager == null)
            arenaManager = FindAnyObjectByType<ArenaManager>();

        if (arenaManager != null)
            arenaManager.OnArenaStateChanged += OnArenaStateChanged;

        if (overlayImage != null) overlayImage.color = safeColor;
        ApplyText();
    }

    private void OnDisable()
    {
        if (arenaManager != null)
            arenaManager.OnArenaStateChanged -= OnArenaStateChanged;
    }

    private void OnArenaStateChanged(ArenaState newState)
    {
        FadeToStateColor(newState);
        ApplyText();

        if (newState == ArenaState.Warning)
            cameraFitLevel?.StartShake(warningShakeAmplitude, warningShakeFrequency);
        else if (newState == ArenaState.Transition)
            cameraFitLevel?.StartShake(transitionShakeAmplitude, transitionShakeFrequency);
        else
            cameraFitLevel?.StopShake();

        Debug.Log($"[ArenaStateFeedback] Estado: {newState}");
    }

    private void FadeToStateColor(ArenaState state)
    {
        if (overlayImage == null) return;

        Color target = state switch
        {
            ArenaState.Warning    => warningColor,
            ArenaState.Transition => transitionColor,
            _                     => safeColor
        };

        _fadeTween?.Kill();
        _fadeTween = overlayImage.DOColor(target, fadeDuration).SetEase(Ease.InOutSine);
    }

    private void ApplyText()
    {
        if (stateLabel == null) return;

        string text = arenaManager?.CurrentState switch
        {
            ArenaState.Warning    => warningText,
            ArenaState.Transition => transitionText,
            _                     => safeText
        };

        stateLabel.text = text;
        stateLabel.enabled = !string.IsNullOrEmpty(text);
    }
}
