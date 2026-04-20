using System.Collections;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [FoldoutGroup("Feel"), Required]
    [SerializeField] private MMF_Player hitFeedbacks;

    [FoldoutGroup("Knockback"), MinValue(0f)]
    [SerializeField] private float knockbackForce = 5f;

    [FoldoutGroup("Knockback"), MinValue(0f)]
    [SerializeField] private float knockbackDuration = 0.1f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void TriggerHit(Vector2 hitSourcePosition)
    {
        hitFeedbacks?.PlayFeedbacks(transform.position);
        if (rb != null) StartCoroutine(KnockbackRoutine(hitSourcePosition));
    }

    private IEnumerator KnockbackRoutine(Vector2 hitSourcePosition)
    {
        Enemy enemy = GetComponent<Enemy>();
        PlayerMovement player = GetComponent<PlayerMovement>();
        if (enemy != null) enemy.IsKnockedBack = true;
        if (player != null) player.IsKnockedBack = true;

        Vector2 direction = ((Vector2)transform.position - hitSourcePosition).normalized;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        rb.linearVelocity = Vector2.zero;

        if (enemy != null) enemy.IsKnockedBack = false;
        if (player != null) player.IsKnockedBack = false;
    }
}
