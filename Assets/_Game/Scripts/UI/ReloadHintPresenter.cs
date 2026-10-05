using UnityEngine;
using TMPro;
using ClaudeCop.Core;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Goi y "SWIPE DOWN TO RELOAD" o cuoi man hinh khi het dan (va chua dang thay dan). Vuot xuong do TapShooter xu ly.
    /// UI tu dung bang code (khong can prefab).
    /// </summary>
    public class ReloadHintPresenter : MonoBehaviour
    {
        GameObject root;
        TMP_Text text;
        bool empty, reloading;

        void Awake()
        {
            root = new GameObject("ReloadHintCanvas", typeof(RectTransform), typeof(Canvas));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 15;
            var go = new GameObject("Hint", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(root.transform, false);
            text = go.GetComponent<TextMeshProUGUI>();
            text.text = "SWIPE DOWN TO RELOAD";
            text.fontSize = 56; text.fontStyle = FontStyles.Bold; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            text.color = new Color(1f, 0.9f, 0.3f, 1f);
            var r = text.rectTransform; r.anchorMin = new Vector2(0.5f, 0f); r.anchorMax = new Vector2(0.5f, 0f); r.pivot = new Vector2(0.5f, 0f);
            r.anchoredPosition = new Vector2(0f, 330f); r.sizeDelta = new Vector2(1000f, 90f);
            root.SetActive(false);
        }

        void OnEnable()
        {
            CombatEvents.AmmoChanged += OnAmmo;
            CombatEvents.ReloadStateChanged += OnReloading;
            var c = CombatEvents.Current;
            empty = c.Ammo <= 0; reloading = c.Reloading;
            Refresh();
        }

        void OnDisable()
        {
            CombatEvents.AmmoChanged -= OnAmmo;
            CombatEvents.ReloadStateChanged -= OnReloading;
            if (root != null) root.SetActive(false);
        }

        void OnAmmo(int ammo, int magazine, WeaponKind kind) { empty = ammo <= 0; Refresh(); }
        void OnReloading(bool value) { reloading = value; Refresh(); }

        void Refresh() { if (root != null) root.SetActive(empty && !reloading); }

        void Update()
        {
            if (root == null || !root.activeSelf) return;
            float a = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 6f);
            var c = text.color; c.a = a; text.color = c;
        }
    }
}
