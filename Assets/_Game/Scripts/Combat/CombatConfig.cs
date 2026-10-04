using UnityEngine;

namespace ClaudeCop.Combat
{
    /// <summary>So lieu chung cua TapShooter (khong gan voi mot vu khi).</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Combat/Combat Config", fileName = "CombatConfig")]
    public sealed class CombatConfig : ScriptableObject
    {
        [Tooltip("Ban kinh diem Justice (px chuan 1080p).")]
        [SerializeField, Min(1f)] float justiceRadiusPx = 35f;
        [SerializeField, Min(1f)] float referenceScreenHeight = 1080f;
        [Tooltip("Tu reload khi het dan. Mac dinh TAT (M1 chi reload bang nut HUD).")]
        [SerializeField] bool autoReloadWhenEmpty;
        [SerializeField] LayerMask environmentMask = ~0;
        [SerializeField, Min(1f)] float maxRayDistance = 100f;

        [Header("Combo")]
        [SerializeField, Min(1)] int comboMaxMultiplier = 5;
        [Tooltip("So phat trung lien tiep de tang 1 bac he so.")]
        [SerializeField, Min(1)] int comboHitsPerStep = 1;
        [Header("Vu khi")]
        [Tooltip("Het dan vu khi dac biet (Shotgun/MG) thi ve vu khi khoi dau (Pistol) day bang.")]
        [SerializeField] bool revertToStartingWeaponWhenEmpty = true;

        public int ComboMaxMultiplier => comboMaxMultiplier;
        public int ComboHitsPerStep => comboHitsPerStep;
        public bool RevertToStartingWeaponWhenEmpty => revertToStartingWeaponWhenEmpty;
        public float JusticeRadiusPx => justiceRadiusPx;
        public float ReferenceScreenHeight => referenceScreenHeight;
        public bool AutoReloadWhenEmpty => autoReloadWhenEmpty;
        public LayerMask EnvironmentMask => environmentMask;
        public float MaxRayDistance => maxRayDistance;
    }
}
