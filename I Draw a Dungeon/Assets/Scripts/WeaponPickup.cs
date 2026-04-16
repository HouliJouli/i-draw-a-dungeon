using System.Collections;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private GameObject weaponPrefab;

    [Header("Highlight")]
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private Color highlightColor = Color.yellow;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Color originalColor;
    private bool playerNearby;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        if (sr != null) originalColor = sr.color;
    }

    private void Update()
    {
        if (!playerNearby) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, t);
        if (sr != null) sr.color = Color.Lerp(originalColor, highlightColor, t);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNearby = false;
        transform.localScale = originalScale;
        if (sr != null) sr.color = originalColor;
    }

    public void Collect(WeaponHolder holder)
    {
        holder.EquipWeapon(weaponPrefab);
        Destroy(gameObject);
    }
}
