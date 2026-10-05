using UnityEngine;
using Unity.Cinemachine;

namespace ClaudeCop.Camera
{
    /// <summary>
    /// Trang thai zoom tam thoi (tinh, doc boi CameraFeelApplier):
    /// - Punch: giam FOV punchFov do trong punchIn giay roi tra lai trong punchOut giay (khi ha enemy).
    /// - KillZoom: khi enemy cuoi chet, camera dang active giam FOV + xoay nhe ve vi tri kill cuoi truoc khi chuyen goc.
    /// Khong ap khi Giam chuyen dong (Applier kiem tra). Tu reset khi vao Play mode.
    /// </summary>
    public static class SlowZoom
    {
        // Hai o punch: "dang chay" (cur) va "duoi" (tail). Do lech = max(cur, tail): max cua hai ham lien tuc la ham lien tuc,
        // nen khi co punch moi chong len punch dang chay thi FOV KHONG bi nhay (truoc day punch moi lam do lech ve 0 trong 1 frame).
        struct Slot { public float start, fov, tin, tout; }
        static Slot cur = new Slot { start = -10f, tin = 0.15f, tout = 0.4f }, tail = new Slot { start = -10f, tin = 0.15f, tout = 0.4f };
        static float killStart = -10f, killDuration = 0.5f, killFov = 6f, killAim = 0.35f, killMaxTurn = 6f;
        static bool killActive;
        static Vector3 killPos;
        static CinemachineVirtualCameraBase killCam;

        public static bool KillZoomActive => killActive;
        public static float KillAimBlend => killAim;
        public static float KillMaxTurn => killMaxTurn;
        public static Vector3 KillPosition => killPos;
        public static CinemachineVirtualCameraBase KillCamera => killCam;

        static float Eval(in Slot s, float now) => PunchEnvelope(now - s.start, s.tin, s.tout) * s.fov;

        public static void Punch(CameraFeelProfile p, float now, bool justice = false)
        {
            float f = justice ? p.justicePunchFov : p.punchFov;
            // Giu lai o co do lech lon hon o "duoi"; o moi bat dau tu 0 -> max() van lien tuc.
            if (Eval(cur, now) >= Eval(tail, now)) tail = cur;
            cur = new Slot { start = now, fov = f, tin = Mathf.Max(0.01f, justice ? p.justicePunchIn : p.punchIn), tout = Mathf.Max(0.01f, justice ? p.justicePunchOut : p.punchOut) };
        }

        /// <summary>Do giam FOV (do) tai thoi diem now (0 khi het).</summary>
        public static float PunchOffset(float now) => Mathf.Max(Eval(cur, now), Eval(tail, now));

        public static float PunchEnvelope(float elapsed, float tin, float tout)
        {
            if (elapsed < 0f) return 0f;
            if (elapsed < tin) { float k = elapsed / tin; return k * k * (3f - 2f * k); }
            float r = 1f - (elapsed - tin) / tout;
            if (r <= 0f) return 0f;
            return r * r * (3f - 2f * r);
        }

        public static void BeginKill(CinemachineVirtualCameraBase cam, Vector3 worldPos, CameraFeelProfile p, float now)
        {
            killCam = cam; killPos = worldPos; killStart = now; killActive = cam != null;
            killDuration = Mathf.Max(0.01f, p.killZoomDuration); killFov = p.killZoomFov; killAim = p.killZoomAimBlend; killMaxTurn = p.killZoomMaxTurn;
        }

        public static void EndKill() { killActive = false; killCam = null; }

        /// <summary>0..1 (smoothstep) cho camera nay; 0 neu khong phai camera dang zoom.</summary>
        public static float KillWeight(CinemachineVirtualCameraBase cam, float now)
        {
            if (!killActive || cam != killCam) return 0f;
            float k = Mathf.Clamp01((now - killStart) / killDuration);
            return k * k * (3f - 2f * k);
        }

        public static float KillFovOffset => killFov;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { cur = new Slot { start = -10f, tin = 0.15f, tout = 0.4f }; tail = cur; killStart = -10f; killActive = false; killCam = null; }
    }
}
