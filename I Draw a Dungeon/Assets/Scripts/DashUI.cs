using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class DashUI : MonoBehaviour
{
    [BoxGroup("References"), Required]
    [SerializeField] private PlayerMovement playerMovement;

    [BoxGroup("References"), Required]
    [SerializeField] private Image fillImage;

    [FoldoutGroup("Fade"), MinValue(0f)]
    [SerializeField] private float fadeInSpeed = 8f;

    [FoldoutGroup("Fade"), MinValue(0f)]
    [SerializeField] private float fadeOutSpeed = 3f;

    [FoldoutGroup("Fade"), Range(0f, 1f)]
    [SerializeField] private float fadeOutThreshold = 0.85f;

    private CanvasGroup canvasGroup;

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

        bool onCooldown = ratio < 1f;
        bool nearEnd = ratio >= fadeOutThreshold;

        if (onCooldown && !nearEnd)
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, fadeInSpeed * Time.deltaTime);
        else
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeOutSpeed * Time.deltaTime);
    }
}
