using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Props
{
    public enum BlastEffect { None = 0, Enemy, Hostage, Grenade }

    /// <summary>Logic thuan chon muc tieu bi vu no (de test EditMode).</summary>
    public static class BlastResolver
    {
        public static BlastEffect Classify(TargetKind kind, bool targetable, Vector3 aimPoint, Vector3 center, float radius)
        {
            if (!targetable) return BlastEffect.None;
            if ((aimPoint - center).sqrMagnitude > radius * radius) return BlastEffect.None;
            switch (kind)
            {
                case TargetKind.Enemy: return BlastEffect.Enemy;
                case TargetKind.Hostage: return BlastEffect.Hostage;
                case TargetKind.Grenade: return BlastEffect.Grenade;
                default: return BlastEffect.None; // Pickup khong bi no
            }
        }

        /// <summary>Con tin trung &gt;= 1 thi phat nguoi choi dung 1 lan moi vu no.</summary>
        public static bool PenalizesPlayer(int hostagesHit) => hostagesHit > 0;
    }
}
