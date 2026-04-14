using UnityEngine;
using UnityEngine.UI;

public class DashUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Image fillImage;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInSpeed = 8f;
    [SerializeField] private float fadeOutSpeed = 3f;
    [SerializeField] [Range(0f, 1f)] private float fadeOutThreshold = 0.85f;

    private CanvasGroup canvasGroup;
    private float previousRatio = 1f;

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

        previousRatio = ratio;
    }
}
