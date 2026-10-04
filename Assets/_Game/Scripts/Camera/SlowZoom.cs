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
        static float punchStart = -10f, punchIn = 0.15f, punchOut = 0.4f, punchFov = 5f;
        static float killStart = -10f, killDuration = 0.5f, killFov = 6f, killAim = 0.35f, killMaxTurn = 6f;
        static bool killActive;
        static Vector3 killPos;
        static CinemachineVirtualCameraBase killCam;

        public static bool KillZoomActive => killActive;
        public static float KillAimBlend => killAim;
        public static float KillMaxTurn => killMaxTurn;
        public static Vector3 KillPosition => killPos;
        public static CinemachineVirtualCameraBase KillCamera => killCam;

        public static void Punch(CameraFeelProfile p, float now)
        {
            punchStart = now; punchIn = Mathf.Max(0.01f, p.punchIn); punchOut = Mathf.Max(0.01f, p.punchOut); punchFov = p.punchFov;
        }

        /// <summary>Do giam FOV (do) tai thoi diem now (0 khi het).</summary>
        public static float PunchOffset(float now) => PunchEnvelope(now - punchStart, punchIn, punchOut) * punchFov;

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
        static void ResetStatics() { punchStart = -10f; killStart = -10f; killActive = false; killCam = null; }
    }
}
