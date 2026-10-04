using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>C4. Thong tin mot phat ban. Combat tao, IShootable/ITapTarget nhan.</summary>
    public struct ShotInfo
    {
        /// <summary>Vi tri tap tren man hinh (pixel).</summary>
        public Vector2 ScreenPosition;
        /// <summary>Diem trung (world).</summary>
        public Vector3 HitPoint;
        /// <summary>Phap tuyen tai diem trung.</summary>
        public Vector3 HitNormal;
        /// <summary>Huong vien dan (chuan hoa).</summary>
        public Vector3 Direction;
        public WeaponKind Weapon;
        /// <summary>He so luc day (1 = chuan), do WeaponData quyet dinh.</summary>
        public float ImpulseScale;
    }

    /// <summary>C5. Vat the moi truong nhan dan. Props cai dat, TapShooter goi.</summary>
    public interface IShootable
    {
        void OnShot(ShotInfo shot);
    }
}
