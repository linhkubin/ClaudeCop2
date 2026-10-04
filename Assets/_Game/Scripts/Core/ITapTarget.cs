using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>
    /// C7. Muc tieu co the tap (Enemy, Hostage, WeaponPickup cai dat).
    /// TapShooter va UI vong target doc. Muc tieu tu dang ky vao <see cref="TargetRegistry"/>.
    /// </summary>
    public interface ITapTarget
    {
        /// <summary>Id on dinh trong suot doi song muc tieu.</summary>
        int Id { get; }
        TargetKind Kind { get; }
        bool IsTargetable { get; }
        /// <summary>Diem ngam (world) - noi dat vong target / tinh khoang cach tap.</summary>
        Vector3 AimPoint { get; }
        bool HasJusticePoint { get; }
        /// <summary>Diem Justice (world), chi co nghia khi HasJusticePoint.</summary>
        Vector3 JusticePoint { get; }
        bool ShowsReticle { get; }
        /// <summary>0 = vua lo ra, 1 = vong thu nho het (se ban).</summary>
        float ReticleProgress { get; }
        /// <summary>So giay da lo ra.</summary>
        float ExposedTime { get; }
        TapOutcome OnTapHit(ShotInfo shot, bool isJustice);
    }
}
