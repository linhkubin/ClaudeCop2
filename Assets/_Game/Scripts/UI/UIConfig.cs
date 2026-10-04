using UnityEngine;

namespace ClaudeCop.UI
{
    /// <summary>So lieu UI (mac dinh theo TASK_BOARD "So lieu"). Asset: Assets/_Game/UI/UIConfig.asset.</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/UI Config", fileName = "UIConfig")]
    public class UIConfig : ScriptableObject
    {
        [Header("Vong target")]
        [Tooltip("progress < nguong nay = xanh")] public float yellowThreshold = 0.4f;
        [Tooltip("progress >= nguong nay = do")] public float redThreshold = 0.75f;
        public Color reticleGreen = new Color(0.25f, 1f, 0.35f, 1f);
        public Color reticleYellow = new Color(1f, 0.9f, 0.15f, 1f);
        public Color reticleRed = new Color(1f, 0.2f, 0.15f, 1f);
        [Tooltip("Scale vong khi progress=0 (to)")] public float reticleStartScale = 2.2f;
        [Tooltip("Scale vong khi progress=1 (nho het)")] public float reticleEndScale = 0.55f;

        [Header("Nhay do")]
        public float flashDuration = 0.35f;
        public float flashPeakAlpha = 0.45f;

        [Header("HUD")]
        public Color heartFull = new Color(1f, 0.25f, 0.3f, 1f);
        public Color heartEmpty = new Color(0.25f, 0.1f, 0.12f, 0.6f);
        public Color ammoNormal = Color.white;
        public Color ammoLow = new Color(1f, 0.35f, 0.2f, 1f);

        [Header("Revive")]
        [Tooltip("Thoi gian dem nguoc popup Revive (giay) - TASK_BOARD: 10 s. Thoi luong quang cao gia nam o FakeRewardedAd (3 s).")]
        public float reviveCountdownSeconds = 10f;

        [Header("Chu diem bay")]
        public float floatingLife = 0.9f;
        public float floatingRise = 140f;
        public int floatingNormalSize = 54;
        public int floatingJusticeSize = 84;
        public Color floatingNormalColor = new Color(1f, 0.95f, 0.6f, 1f);
        public Color floatingJusticeColor = new Color(1f, 0.55f, 0.1f, 1f);

        [Header("Phase banner (khi khong co fade)")]
        public float bannerFadeIn = 0.25f;
        public float bannerHold = 1.2f;
        public float bannerFadeOut = 0.4f;

        [Header("Bang debug Jev (T-411)")]
        [Tooltip("Hien nut JEV trong Release build. Editor/Development build luon hien.")] public bool showJevDebugButton = false;
        public Color jevSourceOffline = new Color(0.45f, 0.8f, 1f, 1f);
        public Color jevSourceDefault = new Color(1f, 0.7f, 0.25f, 1f);
        public Color jevBarColor = new Color(0.35f, 0.75f, 0.45f, 1f);
        public Color jevBarChosenColor = new Color(1f, 0.9f, 0.2f, 1f);

        static UIConfig fallback;
        /// <summary>Config tam khi view khong duoc gan asset (gia tri mac dinh).</summary>
        public static UIConfig Fallback
        {
            get
            {
                if (fallback == null) { fallback = CreateInstance<UIConfig>(); fallback.hideFlags = HideFlags.HideAndDontSave; }
                return fallback;
            }
        }

        public Color ReticleColor(float progress)
        {
            if (progress < yellowThreshold) return reticleGreen;
            if (progress < redThreshold) return reticleYellow;
            return reticleRed;
        }
    }
}
