using UnityEngine;
using ClaudeCop.Enemy;

namespace ClaudeCop.Jev
{
    /// <summary>Cau hinh Jev (chi chay offline bang luat viet san, khong goi mang).</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Jev Config", fileName = "JevConfig")]
    public class JevConfig : ScriptableObject
    {
        public bool enabled = true;
        [Min(0.1f)] public float timeoutSeconds = 1.5f;
        [Range(0f, 1f)] public float confidenceThreshold = 0.6f;
        [Min(0.5f)] public float defaultReticleTime = 2.5f;
        /// <summary>Ung voi lua chon "short", "normal", "long".</summary>
        public float[] reticlePresets = { 2.0f, 2.5f, 3.0f };
        /// <summary>So phat toi thieu truoc khi tin so lieu.</summary>
        [Min(1)] public int minShotsForConfidence = 5;

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
        public static JevConfig CreateRuntimeDefault()
        {
            var c = CreateInstance<JevConfig>();
            c.reticlePresets = (float[])EnemyConfig.ReticlePresets.Clone();
            return c;
        }
    }
}
