using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Combat
{
    /// <summary>So lieu mot loai vu khi. Ban kinh tinh bang pixel chuan 1080p (quy doi theo Screen.height / 1080).</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Combat/Weapon Data", fileName = "WeaponData")]
    public sealed class WeaponData : ScriptableObject
    {
        [SerializeField] WeaponKind kind = WeaponKind.Pistol;
        [SerializeField, Min(1)] int magazineSize = 6;
        [Tooltip("Ban kinh trung (px chuan 1080p) quanh AimPoint.")]
        [SerializeField, Min(1f)] float hitRadiusPx = 90f;
        [Tooltip("So muc tieu toi da trung moi phat.")]
        [SerializeField, Min(1)] int maxTargetsPerShot = 1;
        [Tooltip("Phat/giay (chi dung khi holdToFire).")]
        [SerializeField, Min(0.1f)] float shotsPerSecond = 10f;
        [SerializeField, Min(0f)] float reloadTime = 0.5f;
        [Tooltip("He so luc day len vat the (1 = chuan).")]
        [SerializeField, Min(0f)] float impulseScale = 1f;
        [SerializeField] bool holdToFire;

        public WeaponKind Kind => kind;
        public int MagazineSize => magazineSize;
        public float HitRadiusPx => hitRadiusPx;
        public int MaxTargetsPerShot => maxTargetsPerShot;
        public float ShotsPerSecond => shotsPerSecond;
        public float FireInterval => 1f / Mathf.Max(0.1f, shotsPerSecond);
        public float ReloadTime => reloadTime;
        public float ImpulseScale => impulseScale;
        public bool HoldToFire => holdToFire;

        /// <summary>Dung cho tao asset/test bang code.</summary>
        public void Configure(WeaponKind k, int mag, float radiusPx, int maxTargets, float sps, float reload, float impulse, bool hold)
        {
            kind = k; magazineSize = mag; hitRadiusPx = radiusPx; maxTargetsPerShot = maxTargets;
            shotsPerSecond = sps; reloadTime = reload; impulseScale = impulse; holdToFire = hold;
        }
    }
}
