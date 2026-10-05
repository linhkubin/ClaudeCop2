using UnityEngine;

namespace ClaudeCop.Enemy
{
    /// <summary>
    /// Bo cau hinh dot lam san (RankScore wave_preset: calm / standard / intense / hostage_heavy).
    /// EncounterWave.ApplyPreset ghi de len EnemyConfig theo tung dot (khong sua asset EnemyConfig).
    /// </summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Enemy Preset", fileName = "EnemyPreset")]
    public class EnemyPreset : ScriptableObject
    {
        [Min(0.1f), Tooltip("Thoi gian vong target (s). Preset: 2.0 / 2.5 / 3.0")] public float reticleTime = 2.5f;
        [Min(0f), Tooltip("Nap bao lau truoc khi lo lai (s)")] public float hideTime = 0.8f;
        [Min(0f)] public float staggerMin = 0.4f;
        [Min(0f)] public float staggerMax = 1.0f;
        [Tooltip("Bat Justice point cho enemy trong dot")] public bool justiceEnabled = true;
        [Range(0f, 1f)] public float justiceFraction = 0.34f;
        [Tooltip("Kich hoat con tin / thung vu khi da dat trong dot (tat de dot khong co con tin)")] public bool useHostages = true;
        [Range(0f, 1f), Tooltip("Ti le enemy mac giap trong dot (0 = khong co)")] public float armoredFraction = 0f;
        [Min(1), Tooltip("So phat giap do them (1 = can 2 phat de ha)")] public int armorHits = 1;
    }
}
