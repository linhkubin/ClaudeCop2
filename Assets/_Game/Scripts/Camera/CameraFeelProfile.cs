using UnityEngine;
using Unity.Cinemachine;

namespace ClaudeCop.Camera
{
    /// <summary>Moi thong so cam giac camera (plan: "Cam giac camera"). Gia tri mac dinh = diem khoi dau trong plan.</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Camera Feel Profile", fileName = "CameraFeelProfile")]
    public class CameraFeelProfile : ScriptableObject
    {
        [Header("Ray (Move)")]
        [Min(0.1f)] public float railSpeed = 3.5f;
        [Tooltip("Thoi gian tang toc luc xuat phat (s)")] [Min(0.01f)] public float easeInTime = 1.0f;
        [Tooltip("Thoi gian giam toc luc dung (s)")] [Min(0.01f)] public float easeOutTime = 1.0f;
        [Tooltip("Nhin truoc doc ray (m)")] [Range(3f, 5f)] public float lookAhead = 4f;
        [Tooltip("Thoi gian lam muot huong nhin (s)")] [Min(0.01f)] public float lookDamping = 0.35f;
        public float baseFov = 60f;

        [Header("Gioi han xoay (chong chong mat)")]
        [Tooltip("Do/giay")] public float maxYawRate = 60f;
        [Tooltip("Do")] public float maxRoll = 2f;
        [Tooltip("Do")] public float maxPitch = 10f;
        [Tooltip("Doi huong lon hon goc nay giua 2 Shot thi dung Cut")] public float cutAngle = 90f;
        [Tooltip("Khi Giam chuyen dong: doi huong lon hon goc nay thi Cut")] public float reduceMotionCutAngle = 30f;

        [Header("Blend")]
        public float defaultBlend = 0.5f;
        public CinemachineBlendDefinition.Styles blendStyle = CinemachineBlendDefinition.Styles.EaseInOut;
        public float subAngleBlend = 0.6f;
        public float restAfterClear = 0.3f;

        [Header("Khong bao gio dung im")]
        [Tooltip("Do rung cam tay (do), nhan voi Perlin")] public float handheldAmplitude = 0.3f;
        [Tooltip("Tan so Perlin")] public float handheldFrequency = 0.4f;
        [Tooltip("He so doi tan so Perlin sang mau/giay")] public float handheldTimeScale = 5f;
        [Tooltip("Nhun theo buoc chan (m)")] [Range(0f, 0.03f)] public float bobAmplitude = 0.025f;
        [Tooltip("Buoc/giay khi di o toc do toi da")] public float bobStepsPerSecond = 1.9f;
        [Tooltip("Dolly-in FOV tong cong khi giao tranh (do, <= 10)")] [Range(0f, 10f)] public float dollyInFov = 8f;
        [Tooltip("Thoi gian dolly-in (s)")] public float dollyInDuration = 12f;
        [Tooltip("Thoi gian chuyen Move <-> Combat cua noise/nhun (s)")] public float modeFade = 0.4f;

        [Header("Zoom (M2)")]
        [Tooltip("Zoom punch khi ha enemy: giam FOV (do)")] public float punchFov = 5f;
        [Tooltip("Thoi gian vao punch (s)")] public float punchIn = 0.15f;
        [Tooltip("Thoi gian tra punch (s)")] public float punchOut = 0.4f;
        [Tooltip("Zoom vao enemy cuoi: giam FOV (do)")] public float killZoomFov = 6f;
        [Tooltip("Thoi gian zoom vao enemy cuoi (s) truoc khi chuyen goc")] public float killZoomDuration = 0.5f;
        [Tooltip("Ti le goc xoay ve phia vi tri kill cuoi (0..1)")] [Range(0f, 1f)] public float killZoomAimBlend = 0.35f;
        [Tooltip("Goc xoay toi da ve phia vi tri kill (do)")] public float killZoomMaxTurn = 6f;

        [Header("Chuyen Phase")]
        public float phaseFadeOut = 0.4f;
        public float phaseHold = 1.0f;
        public float phaseFadeIn = 0.4f;
        [Tooltip("Giam chuyen dong: fade vao/ra ngan hon (s)")] public float reduceMotionFade = 0.2f;

        [Header("Nhip ray")]
        [Tooltip("Doan Move dai hon thi tang toc do (toi da maxRailSpeed) de khong qua nay (s). Plan: <= 6 s")] public float maxMoveSeconds = 5.8f;
        [Tooltip("Toc do ray toi da khi bi tang de giu maxMoveSeconds (m/s). Plan: 3-4 m/s")] public float maxRailSpeed = 4f;

        [Header("Shake khi trung dan")]
        public float hitShakeDuration = 0.2f;
        [Tooltip("Do lech vi tri (m)")] public float hitShakePosition = 0.04f;
        [Tooltip("Do lech goc (do)")] public float hitShakeAngle = 1.2f;
        [Tooltip("He so khi Giam chuyen dong")] [Range(0f, 1f)] public float hitShakeReduceScale = 0.35f;
    }
}
