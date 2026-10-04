using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>C10. Ket qua mot phat ban. Combat phat qua CombatEvents.ShotResolved; Game, Jev, UI nghe.</summary>
    public struct ShotResult
    {
        public TapOutcome Outcome;
        public TargetKind TargetKind;
        /// <summary>Diem world cua phat ban (diem trung hoac diem tap chieu ra).</summary>
        public Vector3 WorldPoint;
        /// <summary>ReticleProgress cua muc tieu luc trung (0 neu khong co).</summary>
        public float ReticleProgress;
        /// <summary>Thoi gian tu luc muc tieu lo ra den luc trung (s).</summary>
        public float ReactionTime;
        public WeaponKind Weapon;
        /// <summary>He so combo sau phat nay.</summary>
        public float ComboMultiplier;
        /// <summary>So muc tieu trung boi phat nay (Shotgun co the &gt; 1).</summary>
        public int TargetsHit;
    }
}
