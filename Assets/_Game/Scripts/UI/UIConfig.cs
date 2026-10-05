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

        [Header("Vong target - lua dan")]
        [Tooltip("Ti le kich thuoc so voi vong enemy")] public float grenadeReticleScale = 0.6f;
        public Color grenadeReticleColor = new Color(1f, 0.55f, 0.1f, 1f);
        [Tooltip("Mau thu hai khi nhap nhay")] public Color grenadeReticleColorAlt = new Color(1f, 0.85f, 0.4f, 1f);
        [Tooltip("Tan so nhap nhay vong lua dan (Hz); Giam chuyen dong -> khong nhay")] public float grenadeReticleBlinkHz = 4f;

        [Header("Canh bao LUU DAN")]
        public Color grenadeWarnColor = new Color(1f, 0.45f, 0.1f, 1f);
        [Tooltip("Tan so nhap nhay (Hz)")] public float grenadeWarnBlinkHz = 4f;
        [Range(0f, 1f)] public float grenadeWarnMinAlpha = 0.35f;

        [Header("Nhay do")]
        public float flashDuration = 0.35f;
        public float flashPeakAlpha = 0.45f;
        [Tooltip("Ti le thoi gian de len dinh")] public float flashRiseFraction = 0.15f;

        [Header("Hieu ung HUD")]
        public float comboPunchDuration = 0.25f;
        public float comboPunchScale = 0.35f;
        public float reloadPulseDuration = 0.6f;
        public float reloadPulseScale = 0.25f;
        public float reloadPulseCycles = 3f;

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
        [Tooltip("Ti le doi song de chu 'pop' luc dau")] public float floatingPopFraction = 0.15f;
        public float floatingPopScale = 1.5f;
        [Tooltip("Ti le doi song bat dau mo dan")] public float floatingFadeStart = 0.6f;
        public Color floatingJusticeColor = new Color(1f, 0.55f, 0.1f, 1f);

        [Header("Phase banner (khi khong co fade)")]
        public float bannerFadeIn = 0.25f;
        public float bannerHold = 1.2f;
        public float bannerFadeOut = 0.4f;
        [Tooltip("Thoi gian chu tieu de hien ra khi fade den")] public float phaseTitleFadeIn = 0.2f;

        [Header("Bang debug RankScore (T-411)")]
        [Tooltip("Hien nut RANKSCORE trong Release build. Editor/Development build luon hien.")] public bool showRankScoreDebugButton = false;
        public Color rankScoreSourceOffline = new Color(0.45f, 0.8f, 1f, 1f);
        public Color rankScoreSourceDefault = new Color(1f, 0.7f, 0.25f, 1f);
        public Color rankScoreBarColor = new Color(0.35f, 0.75f, 0.45f, 1f);
        public Color rankScoreBarChosenColor = new Color(1f, 0.9f, 0.2f, 1f);

        [Header("Man Win - rank (T-711)")]
        public Color rankColorS = new Color(1f, 0.85f, 0.15f, 1f);
        public Color rankColorA = new Color(0.3f, 0.9f, 0.4f, 1f);
        public Color rankColorB = new Color(0.3f, 0.6f, 1f, 1f);
        public Color rankColorC = new Color(0.65f, 0.65f, 0.65f, 1f);
        [Tooltip("Thoi gian dong dau (s, unscaled)")] public float rankStampDuration = 0.35f;
        [Tooltip("Scale luc bat dau dong dau")] public float rankStampStartScale = 2.6f;
        [Tooltip("Cho rank toi da (s, unscaled); qua thi an khoi rank")] public float rankWaitSeconds = 1f;

        [Header("Chu bay NO! (T-711)")]
        public int blastTextSize = 72;
        public Color blastTextColor = new Color(1f, 0.5f, 0.1f, 1f);
        [Tooltip("Day chu NO! len tren (px) de khong de len chu diem")] public float blastTextOffsetY = 110f;

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

        public Color RankColor(ClaudeCop.RankScore.ScoreRank r)
        {
            switch (r)
            {
                case ClaudeCop.RankScore.ScoreRank.S: return rankColorS;
                case ClaudeCop.RankScore.ScoreRank.A: return rankColorA;
                case ClaudeCop.RankScore.ScoreRank.B: return rankColorB;
                default: return rankColorC;
            }
        }

        public static string RankLetter(ClaudeCop.RankScore.ScoreRank r)
        {
            switch (r)
            {
                case ClaudeCop.RankScore.ScoreRank.S: return "S";
                case ClaudeCop.RankScore.ScoreRank.A: return "A";
                case ClaudeCop.RankScore.ScoreRank.B: return "B";
                default: return "C";
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
