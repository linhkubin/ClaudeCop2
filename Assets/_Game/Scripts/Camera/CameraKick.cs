using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Camera
{
    /// <summary>
    /// CAM-VC2: giat nhe camera khi ban (logic thuan, test EditMode). Moi phat them mot "xung" pitch len + punch FOV, bao hinh
    /// smoothstep vao kickRise (s) roi smoothstep ve 0 trong kickFall (s) (~0.1 s tong). Cac xung cong don, KEP tran kickMaxPitch / kickMaxFov
    /// (Machine Gun cong don co tran). He so theo vu khi trong profile. Ket qua: pitch (do, len +), fov (do, giam FOV +).
    /// </summary>
    public sealed class CameraKick
    {
        const int Cap = 24;
        readonly CameraFeelProfile p;
        readonly float[] t0 = new float[Cap];
        readonly float[] ap = new float[Cap];
        readonly float[] af = new float[Cap];
        int n;

        /// <summary>So phat da kich hoat va dinh pitch/fov lon nhat da tra ve (do kiem).</summary>
        public int Count { get; private set; }
        public float MaxPitch { get; private set; }
        public float MaxFov { get; private set; }

        public CameraKick(CameraFeelProfile profile) { p = profile; }

        public static float WeaponScale(CameraFeelProfile p, WeaponKind k)
        {
            switch (k)
            {
                case WeaponKind.Shotgun: return p.kickScaleShotgun;
                case WeaponKind.MachineGun:
                case WeaponKind.SMG: return p.kickScaleMachineGun;
                default: return p.kickScalePistol;
            }
        }

        /// <summary>Mot phat ban luc now. scale: 1 binh thuong, kickReduceMotionScale khi Giam chuyen dong (0 = tat).</summary>
        public void Fire(float now, WeaponKind weapon, float scale)
        {
            if (!p.kickEnabled || scale <= 0f) return;
            float w = WeaponScale(p, weapon) * scale;
            if (w <= 0f) return;
            if (n == Cap) { for (int i = 1; i < n; i++) { t0[i - 1] = t0[i]; ap[i - 1] = ap[i]; af[i - 1] = af[i]; } n--; }
            t0[n] = now; ap[n] = p.kickPitch * w; af[n] = p.kickFov * w; n++;
            Count++;
        }

        public static float Envelope(float e, float rise, float fall)
        {
            if (e <= 0f) return 0f;
            rise = Mathf.Max(0.005f, rise); fall = Mathf.Max(0.005f, fall);
            if (e < rise) { float k = e / rise; return k * k * (3f - 2f * k); }
            float r = 1f - (e - rise) / fall;
            if (r <= 0f) return 0f;
            return r * r * (3f - 2f * r);
        }

        /// <summary>damp: 0..1 nhan them (vd. giam khi CameraReaction/kill-zoom dang chay).</summary>
        public Vector2 Evaluate(float now, float damp = 1f)
        {
            float pitch = 0f, fov = 0f;
            int w = 0;
            for (int i = 0; i < n; i++)
            {
                float e = now - t0[i];
                if (e > p.kickRise + p.kickFall) continue; // het han: bo
                float env = Envelope(e, p.kickRise, p.kickFall);
                pitch += ap[i] * env; fov += af[i] * env;
                t0[w] = t0[i]; ap[w] = ap[i]; af[w] = af[i]; w++;
            }
            n = w;
            pitch = Mathf.Min(pitch, p.kickMaxPitch) * Mathf.Clamp01(damp);
            fov = Mathf.Min(fov, p.kickMaxFov) * Mathf.Clamp01(damp);
            MaxPitch = Mathf.Max(MaxPitch, pitch); MaxFov = Mathf.Max(MaxFov, fov);
            return new Vector2(pitch, fov);
        }

        public void Cancel() { n = 0; }
    }
}
