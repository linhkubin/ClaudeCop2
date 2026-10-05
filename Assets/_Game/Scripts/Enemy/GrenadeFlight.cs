using UnityEngine;

namespace ClaudeCop.Enemy
{
    public enum GrenadeState { Flying, ShotDown, Exploded }

    /// <summary>Logic thuan cua luu dan: duong bay cong + tien do 0..1. Khong pause/Unity object o day (Grenade lo viec do).</summary>
    public class GrenadeFlight
    {
        readonly Vector3 start, end;
        readonly float duration, arcHeight;

        public GrenadeState State { get; private set; } = GrenadeState.Flying;
        public float Elapsed { get; private set; }
        /// <summary>0 = vua nem, 1 = toi diem dap.</summary>
        public float Progress { get; private set; }
        public bool IsFlying => State == GrenadeState.Flying;
        public bool IsResolved => State != GrenadeState.Flying;

        public GrenadeFlight(Vector3 start, Vector3 end, float duration, float arcHeight)
        {
            this.start = start; this.end = end;
            this.duration = duration > 0.001f ? duration : 0.001f;
            this.arcHeight = arcHeight;
        }

        public Vector3 PositionAt(float t)
        {
            t = Mathf.Clamp01(t);
            return Vector3.Lerp(start, end, t) + Vector3.up * (4f * arcHeight * t * (1f - t));
        }

        public Vector3 Position => PositionAt(Progress);

        /// <summary>Tien dt giay. Tra true dung luc luu dan toi noi (State = Exploded).</summary>
        public bool Tick(float dt)
        {
            if (State != GrenadeState.Flying) return false;
            Elapsed += dt;
            Progress = Elapsed >= duration ? 1f : Elapsed / duration;
            if (Progress >= 1f) { State = GrenadeState.Exploded; return true; }
            return false;
        }

        /// <summary>Bi ban khi dang bay. Tra true neu thanh cong.</summary>
        public bool Shoot()
        {
            if (State != GrenadeState.Flying) return false;
            State = GrenadeState.ShotDown;
            return true;
        }
    }
}
