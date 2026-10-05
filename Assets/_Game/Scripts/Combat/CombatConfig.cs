using UnityEngine;

namespace ClaudeCop.Combat
{
    /// <summary>So lieu chung cua TapShooter (khong gan voi mot vu khi).</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Combat/Combat Config", fileName = "CombatConfig")]
    public sealed class CombatConfig : ScriptableObject
    {
        public const float DefaultJusticeRadiusPx = 35f;
        public const float DefaultReferenceScreenHeight = 1080f;
        public const float DefaultMaxRayDistance = 100f;
        public const float DefaultEmptyClickCooldown = 0.2f;
        public const float DefaultBodyHitPaddingPx = 8f;
        public const int DefaultComboMaxMultiplier = 5;
        public const int DefaultComboHitsPerStep = 1;

        [Tooltip("Ban kinh diem Justice (px chuan 1080p).")]
        [SerializeField, Min(1f)] float justiceRadiusPx = DefaultJusticeRadiusPx;
        [SerializeField, Min(1f)] float referenceScreenHeight = DefaultReferenceScreenHeight;
        [Tooltip("Tu reload khi het dan. Mac dinh TAT (M1 chi reload bang nut HUD).")]
        [SerializeField] bool autoReloadWhenEmpty;
        [SerializeField] LayerMask environmentMask = ~0;
        [SerializeField, Min(1f)] float maxRayDistance = DefaultMaxRayDistance;

        [Tooltip("Thoi gian cho (s) giua hai lan bam khi het dan, tranh spam.")]
        [SerializeField, Min(0f)] float emptyClickCooldown = DefaultEmptyClickCooldown;

        [Header("Trung than (HIT-BODY)")]
        [Tooltip("Tap vao bat ky phan nao cua collider enemy (than/dau/chan) deu trung, khong can trung tam.")]
        [SerializeField] bool enemyBodyHit = true;
        [Tooltip("Mo rong hinh chu nhat than enemy tren man hinh (px chuan 1080p).")]
        [SerializeField, Min(0f)] float bodyHitPaddingPx = DefaultBodyHitPaddingPx;

        [Tooltip("Dan khong xuyen: vu khi 1 muc tieu/phat (Pistol/MG) chon muc tieu GAN CAMERA NHAT khi nhieu enemy cung trung (Justice van uu tien nhat).")]
        [SerializeField] bool nearestTargetFirst = true;

        [Header("Combo")]
        [SerializeField, Min(1)] int comboMaxMultiplier = DefaultComboMaxMultiplier;
        [Tooltip("So phat trung lien tiep de tang 1 bac he so.")]
        [SerializeField, Min(1)] int comboHitsPerStep = DefaultComboHitsPerStep;
        [Header("Vu khi")]
        [Tooltip("Het dan vu khi dac biet (Shotgun/MG) thi ve vu khi khoi dau (Pistol) day bang.")]
        [SerializeField] bool revertToStartingWeaponWhenEmpty = true;

        public float EmptyClickCooldown => emptyClickCooldown;
        public int ComboMaxMultiplier => comboMaxMultiplier;
        public int ComboHitsPerStep => comboHitsPerStep;
        public bool RevertToStartingWeaponWhenEmpty => revertToStartingWeaponWhenEmpty;
        public bool EnemyBodyHit => enemyBodyHit;
        public bool NearestTargetFirst => nearestTargetFirst;
        public float BodyHitPaddingPx => bodyHitPaddingPx;
        public float JusticeRadiusPx => justiceRadiusPx;
        public float ReferenceScreenHeight => referenceScreenHeight;
        public bool AutoReloadWhenEmpty => autoReloadWhenEmpty;
        public LayerMask EnvironmentMask => environmentMask;
        public float MaxRayDistance => maxRayDistance;
    }
}
