using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArenaStateFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private Image overlayImage;
    [SerializeField] private TextMeshProUGUI stateLabel;

    [Header("Safe State")]
    [SerializeField] private Color safeColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private string safeText = "";

    [Header("Warning State")]
    [SerializeField] private Color warningColor = new Color(1f, 0.6f, 0f, 0.25f);
    [SerializeField] private string warningText = "Get Ready";

    [Header("Transition State")]
    [SerializeField] private Color transitionColor = new Color(1f, 0f, 0f, 0.4f);
    [SerializeField] private string transitionText = "RUN";

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 3f;

    private Color _targetColor;
    private string _targetText;

    private void Start()
    {
        if (arenaManager == null)
            arenaManager = FindAnyObjectByType<ArenaManager>();

        if (arenaManager != null)
            arenaManager.OnArenaStateChanged += OnArenaStateChanged;

        SetTargetForState(ArenaState.Safe);
        if (overlayImage != null) overlayImage.color = safeColor;
        ApplyText();
    }

    private void OnDisable()
    {
        if (arenaManager != null)
            arenaManager.OnArenaStateChanged -= OnArenaStateChanged;
    }

    private void Update()
    {
        if (overlayImage == null) return;
        overlayImage.color = Color.Lerp(overlayImage.color, _targetColor, fadeSpeed * Time.deltaTime);
    }

    private void OnArenaStateChanged(ArenaState newState)
    {
        SetTargetForState(newState);
        ApplyText();
        Debug.Log($"[ArenaStateFeedback] Estado: {newState}");
    }

    private void SetTargetForState(ArenaState state)
    {
        switch (state)
        {
            case ArenaState.Safe:
                _targetColor = safeColor;
                _targetText = safeText;
                break;
            case ArenaState.Warning:
                _targetColor = warningColor;
                _targetText = warningText;
                break;
            case ArenaState.Transition:
                _targetColor = transitionColor;
                _targetText = transitionText;
                break;
            case ArenaState.Completed:
                _targetColor = safeColor;
                _targetText = safeText;
                break;
        }
    }

    private void ApplyText()
    {
        if (stateLabel == null) return;
        stateLabel.text = _targetText;
        stateLabel.enabled = !string.IsNullOrEmpty(_targetText);
    }
}
