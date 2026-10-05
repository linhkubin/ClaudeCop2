using UnityEngine;

namespace ClaudeCop.Camera
{
    /// <summary>
    /// CAM-LIVELY: logic thuan cua "giat minh quay sang" khi enemy moi lo ra. Gop cac Notify trong reactGatherWindow thanh MOT phan ung
    /// huong ve trong tam, cooldown, duong cong vao nhanh (ease-out) - giu ngan - ease ve (smoothstep). Khong biet gi ve Unity scene (de test).
    /// Target = (yaw do [phai +], pitch do [len +], giam FOV do).
    /// </summary>
    public sealed class CameraReaction
    {
        readonly CameraFeelProfile p;
        int pendCount; float pendStart, pendYaw, pendPitch;
        float startTime = -100f, cooldownEnd = -100f;
        float ampYaw, ampPitch, ampFov;
        bool running;

        /// <summary>So lan phan ung da kich hoat + bien do gan nhat/lon nhat (do) - de do kiem.</summary>
        public int Count { get; private set; }
        public float LastYaw { get; private set; }
        public float LastPitch { get; private set; }
        public float MaxAbsYaw { get; private set; }
        public Vector3 Target { get; private set; }

        public CameraReaction(CameraFeelProfile profile) { p = profile; }

        public void Notify(float now, float yawOffsetDeg, float pitchOffsetDeg)
        {
            if (pendCount == 0) pendStart = now;
            pendCount++; pendYaw += yawOffsetDeg; pendPitch += pitchOffsetDeg;
        }

        public void ClearPending() { pendCount = 0; pendYaw = pendPitch = 0f; }

        /// <summary>allowed = false (dang Move/blend/pause/kill zoom): bo cac Notify dang cho. scale: 1 binh thuong, reactReduceMotionScale khi Giam chuyen dong.</summary>
        public void Tick(float now, bool allowed, float scale)
        {
            if (pendCount > 0)
            {
                if (!allowed || !p.reactEnabled || scale <= 0f) ClearPending();
                else if (now - pendStart >= p.reactGatherWindow)
                {
                    float yaw = pendYaw / pendCount, pitch = pendPitch / pendCount;
                    ClearPending();
                    float off = Mathf.Sqrt(yaw * yaw + pitch * pitch);
                    if (now >= cooldownEnd && off >= p.reactMinOffset && off > 1e-4f)
                    {
                        float amp = Mathf.Clamp(off * p.reactGain, p.reactMinAngle, Mathf.Max(p.reactMaxYaw, p.reactMinAngle));
                        float k = amp / off;
                        ampYaw = Mathf.Clamp(yaw * k, -p.reactMaxYaw, p.reactMaxYaw) * scale;
                        ampPitch = Mathf.Clamp(pitch * k, -p.reactMaxPitch, p.reactMaxPitch) * scale;
                        ampFov = p.reactFov * scale;
                        startTime = now; cooldownEnd = now + p.reactCooldown; running = true;
                        Count++; LastYaw = ampYaw; LastPitch = ampPitch; MaxAbsYaw = Mathf.Max(MaxAbsYaw, Mathf.Abs(ampYaw));
                    }
                }
            }
            if (running)
            {
                float e = Envelope(now - startTime, p.reactIn, p.reactHold, p.reactOut);
                if (now - startTime > p.reactIn + p.reactHold + p.reactOut) { running = false; Target = Vector3.zero; }
                else Target = new Vector3(ampYaw * e, ampPitch * e, ampFov * e);
            }
            else Target = Vector3.zero;
        }

        public void Cancel() { running = false; ClearPending(); Target = Vector3.zero; }

        /// <summary>0..1: vao nhanh (ease-out bac 3), giu, ve smoothstep.</summary>
        public static float Envelope(float e, float tin, float thold, float tout)
        {
            if (e <= 0f) return 0f;
            if (e < tin) { float k = 1f - e / tin; return 1f - k * k * k; }
            if (e < tin + thold) return 1f;
            float r = 1f - (e - tin - thold) / Mathf.Max(0.01f, tout);
            if (r <= 0f) return 0f;
            return r * r * (3f - 2f * r);
        }
    }
}
