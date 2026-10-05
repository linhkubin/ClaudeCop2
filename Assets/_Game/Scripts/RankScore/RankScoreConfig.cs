using UnityEngine;
using ClaudeCop.Enemy;

namespace ClaudeCop.RankScore
{
    /// <summary>Cau hinh RankScore (chi chay offline bang luat viet san, khong goi mang).</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/RankScore Config", fileName = "RankScoreConfig")]
    public class RankScoreConfig : ScriptableObject
    {
        public bool enabled = true;
        [Min(0.1f)] public float timeoutSeconds = 1.5f;
        [Range(0f, 1f)] public float confidenceThreshold = 0.6f;
        [Min(0.5f)] public float defaultReticleTime = 2.5f;
        /// <summary>Ung voi lua chon "short", "normal", "long".</summary>
        public float[] reticlePresets = { 2.0f, 2.5f, 3.0f };
        /// <summary>So phat toi thieu truoc khi tin so lieu.</summary>
        [Min(1)] public int minShotsForConfidence = 5;

        [Header("Cong thuc ky nang (Skill)")]
        [Range(0f, 1f)] public float weightAccuracy = 0.45f;
        [Range(0f, 1f)] public float weightReaction = 0.3f;
        [Range(0f, 1f)] public float weightSafety = 0.25f;
        [Range(0f, 1f), Tooltip("Tru vao diem ky nang cho moi lan trung con tin")] public float hostagePenaltyPerHit = 0.15f;
        [Min(0f), Tooltip("Phan ung tot nhat (s) => diem 1")] public float reactionBestSeconds = 0.8f;
        [Min(0.01f), Tooltip("Do rong (s) tu diem 1 xuong diem 0")] public float reactionSpanSeconds = 1.2f;
        [Range(0f, 1f), Tooltip("Diem phan ung/ky nang trung tinh khi chua co mau")] public float neutralScore = 0.5f;
        [Min(1), Tooltip("So mang mat de diem an toan = 0")] public int livesLostForZeroSafety = 2;

        [Header("reticle_time")]
        [Tooltip("Tam ky nang cua short / normal / long")] public float[] reticleCenters = { 0.85f, 0.5f, 0.15f };
        [Min(0.01f)] public float reticleFalloff = 0.6f;
        [Range(0f, 1f)] public float reticleMinWeight = 0.05f;
        [Range(0f, 1f)] public float reticleConfidenceBase = 0.45f;
        [Min(0f)] public float reticleConfidenceMarginGain = 1.5f;

        [Header("wave_preset / weapon_drop (luat theo ky nang ca man)")]
        [Range(0f, 1f)] public float presetIntenseMinSkill = 0.75f;
        [Range(0f, 1f)] public float presetCalmMaxSkill = 0.40f;
        [Range(0f, 1f)] public float presetHostageHeavyMinSkill = 0.60f;
        [Min(0), Tooltip("Chi so Phase (tinh tu 0) som nhat cho hostage_heavy; Phase 2 = 1")] public int presetHostageHeavyMinPhaseIndex = 1;
        [Range(0f, 1f)] public float dropMachineGunMaxSkill = 0.40f;
        [Range(0f, 1f)] public float dropShotgunMaxSkill = 0.70f;
        [Range(0f, 1f), Tooltip("Do tin cay co ban cua mot luat khi ky nang sat nguong")] public float ruleConfidenceBase = 0.7f;
        [Min(0f), Tooltip("Do tin cay tang them theo khoang cach toi nguong gan nhat")] public float ruleConfidenceGain = 2f;

        [Header("Rank cuoi man")]
        [Range(0f, 1f)] public float rankSMinSkill = 0.85f;
        [Range(0f, 1f)] public float rankAMinSkill = 0.70f;
        [Range(0f, 1f)] public float rankBMinSkill = 0.50f;
        [Range(0f, 1f), Tooltip("Chi so yeu nhat van >= nguong nay => khong co diem yeu")] public float weaknessOkThreshold = 0.8f;
        [Range(0f, 1f), Tooltip("Diem con tin giam moi lan trung (de tim diem yeu)")] public float hostageWeaknessPerHit = 0.25f;

        public const string ChoiceDefault = "default";
        public const string PresetCalm = "calm", PresetStandard = "standard", PresetIntense = "intense", PresetHostageHeavy = "hostage_heavy";
        public static readonly string[] PresetNames = { PresetCalm, PresetStandard, PresetIntense, PresetHostageHeavy };
        public const string DropNone = "none", DropShotgun = "shotgun", DropMachineGun = "machinegun";
        public static readonly string[] DropNames = { DropNone, DropShotgun, DropMachineGun };

        static RankScoreConfig fallback;
        /// <summary>Config mac dinh dung khi tham so config = null.</summary>
        public static RankScoreConfig Fallback
        {
            get
            {
                if (fallback == null)
                {
                    fallback = CreateRuntimeDefault();
                    fallback.hideFlags = HideFlags.HideAndDontSave;
                }
                return fallback;
            }
        }

        public const string ChoiceShort = "short";
        public const string ChoiceNormal = "normal";
        public const string ChoiceLong = "long";
        public static readonly string[] ChoiceNames = { ChoiceShort, ChoiceNormal, ChoiceLong };

        public float TimeFor(string choice)
        {
            int i = System.Array.IndexOf(ChoiceNames, choice);
            if (i < 0 || reticlePresets == null || i >= reticlePresets.Length) return defaultReticleTime;
            return reticlePresets[i];
        }

        /// <summary>Config tam trong code (test, khi chua co asset).</summary>
        public static RankScoreConfig CreateRuntimeDefault()
        {
            var c = CreateInstance<RankScoreConfig>();
            c.reticlePresets = (float[])EnemyConfig.ReticlePresets.Clone();
            return c;
        }
    }
}
