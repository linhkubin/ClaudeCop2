using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClaudeCop.Core;

namespace ClaudeCop.UI
{
    /// <summary>View HUD thuan: diem, tim, dan, vu khi, nut Reload. T-211 se bind event vao cac Set*.</summary>
    public class HudView : MonoBehaviour
    {
        [SerializeField] UIConfig config;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] Image[] hearts;
        [SerializeField] TMP_Text ammoText;
        [SerializeField] TMP_Text weaponLabel;
        [SerializeField] Image weaponIcon;
        [Tooltip("Icon theo WeaponKind (Pistol, Shotgun, MachineGun); de trong thi chi hien nhan.")]
        [SerializeField] Sprite[] weaponSprites;
        [SerializeField] Button reloadButton;
        [SerializeField] TMP_Text reloadLabel;
        [SerializeField] RectTransform reticleRoot;
        [Tooltip("Chu combo (x2 COMBO...). Chi hien khi he so > 1.")]
        [SerializeField] TMP_Text comboText;

        UIConfig Cfg => config != null ? config : UIConfig.Fallback;

        /// <summary>Node cha de dat TargetReticle.</summary>
        public RectTransform ReticleRoot => reticleRoot;

        void Awake()
        {
            if (reloadButton != null) reloadButton.onClick.AddListener(OnReloadClicked);
        }

        float reloadPulse = -1f;

        void OnReloadClicked() => GameCommands.RequestReload();

        /// <summary>Nhap nhay nut Reload (het dan). Dung unscaled time.</summary>
        public void PulseReload() { reloadPulse = 0f; }

        float comboPunch = -1f;

        /// <summary>Hien he so combo; an khi multiplier &lt;= 1. Gay hieu ung nay nhe moi lan doi.</summary>
        public void SetCombo(int streak, float multiplier)
        {
            if (comboText == null) return;
            bool show = multiplier > 1.001f;
            comboText.gameObject.SetActive(show);
            if (!show) { comboText.rectTransform.localScale = Vector3.one; comboPunch = -1f; return; }
            comboText.SetText("x{0} COMBO", Mathf.Round(multiplier * 10f) / 10f);
            comboPunch = 0f;
        }

        public bool ComboVisible => comboText != null && comboText.gameObject.activeSelf;
        public string ComboString => comboText != null ? comboText.text : string.Empty;

        void Update()
        {
            if (comboPunch >= 0f && comboText != null)
            {
                comboPunch += Time.unscaledDeltaTime;
                const float cd = 0.25f;
                float k = comboPunch >= cd ? 0f : 1f - comboPunch / cd;
                float sc = 1f + 0.35f * k;
                comboText.rectTransform.localScale = new Vector3(sc, sc, 1f);
                if (comboPunch >= cd) comboPunch = -1f;
            }
            if (reloadPulse < 0f || reloadButton == null) return;
            reloadPulse += Time.unscaledDeltaTime;
            const float dur = 0.6f;
            if (reloadPulse >= dur) { reloadPulse = -1f; reloadButton.transform.localScale = Vector3.one; return; }
            float s = 1f + 0.25f * Mathf.Abs(Mathf.Sin(reloadPulse / dur * Mathf.PI * 3f));
            reloadButton.transform.localScale = new Vector3(s, s, 1f);
        }

        public void SetScore(int score) { if (scoreText != null) scoreText.text = score.ToString("N0"); }

        public void SetLives(int cur, int max)
        {
            if (hearts == null) return;
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null) continue;
                hearts[i].gameObject.SetActive(i < max);
                hearts[i].color = i < cur ? Cfg.heartFull : Cfg.heartEmpty;
            }
        }

        public void SetAmmo(int cur, int max)
        {
            if (ammoText == null) return;
            ammoText.text = cur + " / " + max;
            ammoText.color = cur <= 1 ? Cfg.ammoLow : Cfg.ammoNormal;
        }

        public void SetWeapon(WeaponKind weapon)
        {
            if (weaponLabel != null) weaponLabel.text = weapon.ToString();
            if (weaponIcon != null)
            {
                int i = (int)weapon;
                bool has = weaponSprites != null && i < weaponSprites.Length && weaponSprites[i] != null;
                if (has) weaponIcon.sprite = weaponSprites[i];
                weaponIcon.enabled = has;
            }
        }

        public void SetReloading(bool reloading)
        {
            if (reloadButton != null) reloadButton.interactable = !reloading;
            if (reloadLabel != null) reloadLabel.text = reloading ? "..." : "RELOAD";
        }
    }
}
