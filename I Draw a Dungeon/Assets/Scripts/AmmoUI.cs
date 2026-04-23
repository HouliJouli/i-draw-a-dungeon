using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    [BoxGroup("References"), Required]
    [SerializeField] private WeaponHolder weaponHolder;

    [BoxGroup("References"), Required]
    [Tooltip("Image do ícone já existente no AmmoIndicator — sprite, cor e tamanho são lidos daqui")]
    [SerializeField] private Image iconTemplate;

    [BoxGroup("Settings"), MinValue(0f)]
    [SerializeField] private float iconSpacing = 0.05f;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private Weapon trackedWeapon;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private int trackedUses = -1;

    private readonly List<Image> pool = new();
    private Sprite cachedSprite;
    private Color cachedColor;
    private Vector2 cachedSize;

    private void Awake()
    {
        cachedSprite = iconTemplate.sprite;
        cachedColor = iconTemplate.color;
        cachedSize = iconTemplate.rectTransform.sizeDelta;
        iconTemplate.gameObject.SetActive(false);
    }

    private void Update()
    {
        Weapon current = weaponHolder.CurrentWeapon;

        bool weaponChanged = current != trackedWeapon;
        bool usesChanged = current != null && current.RemainingUses != trackedUses;

        if (weaponChanged || usesChanged)
        {
            trackedWeapon = current;
            trackedUses = current != null ? current.RemainingUses : -1;
            Refresh(current);
        }
    }

    private void Refresh(Weapon weapon)
    {
        int count = (weapon != null && weapon.HasLimitedUses) ? weapon.RemainingUses : 0;

        if (count == 0)
        {
            foreach (var img in pool) img.gameObject.SetActive(false);
            return;
        }

        EnsurePoolSize(count);

        float step = cachedSize.x + iconSpacing;
        float totalWidth = count * cachedSize.x + Mathf.Max(0, count - 1) * iconSpacing;
        float startX = -totalWidth / 2f + cachedSize.x / 2f;

        for (int i = 0; i < pool.Count; i++)
        {
            bool active = i < count;
            pool[i].gameObject.SetActive(active);
            if (active)
                pool[i].rectTransform.anchoredPosition = new Vector2(startX + i * step, 0f);
        }
    }

    private void EnsurePoolSize(int count)
    {
        while (pool.Count < count)
        {
            var go = new GameObject("AmmoIcon", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var img = go.AddComponent<Image>();
            img.sprite = cachedSprite;
            img.color = cachedColor;
            img.rectTransform.sizeDelta = cachedSize;
            img.gameObject.SetActive(false);
            pool.Add(img);
        }
    }

    [Button("Force Show 3 Icons")]
    private void ForceShow()
    {
        cachedSprite = iconTemplate.sprite;
        cachedColor = iconTemplate.color;
        cachedSize = iconTemplate.rectTransform.sizeDelta;

        EnsurePoolSize(3);

        float step = cachedSize.x + iconSpacing;
        float totalWidth = 3 * cachedSize.x + 2 * iconSpacing;
        float startX = -totalWidth / 2f + cachedSize.x / 2f;

        for (int i = 0; i < 3; i++)
        {
            pool[i].gameObject.SetActive(true);
            pool[i].rectTransform.anchoredPosition = new Vector2(startX + i * step, 0f);
        }

        Debug.Log($"[AmmoUI] cachedSize={cachedSize} | step={step} | startX={startX}");
    }
}
