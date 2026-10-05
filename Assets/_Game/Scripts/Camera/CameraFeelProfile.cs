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
        [Tooltip("FOV doc cua camera ray = FOV toi da luc bat dau Combat (khong zoom out khi toi diem). Uu tien 35-45")] public float baseFov = 45f;

        [Header("Gioi han xoay (chong chong mat)")]
        [Tooltip("Do/giay")] public float maxYawRate = 60f;
        [Tooltip("Do")] public float maxRoll = 2f;
        [Tooltip("Do")] public float maxPitch = 10f;
        [Tooltip("DA BO (CAM-SMOOTH): khong con Cut theo goc; chi Cut o lan vao level dau tien va luc man den giua 2 Phase")] public float cutAngle = 360f;
        [Tooltip("DA BO (CAM-SMOOTH): Giam chuyen dong khong con Cut, chi giam bien do")] public float reduceMotionCutAngle = 360f;

        [Header("Blend")]
        public float defaultBlend = 0.5f;
        public CinemachineBlendDefinition.Styles blendStyle = CinemachineBlendDefinition.Styles.EaseInOut;
        public float subAngleBlend = 0.6f;
        public float restAfterClear = 0.3f;
        [Tooltip("Blend dai ra de toc do doi vi tri khong vuot muc nay (m/s, trung binh; dinh ~1.5x). 0 = tat")] public float maxBlendSpeed = 7f;
        [Tooltip("Blend dai ra de toc do xoay khong vuot muc nay (do/giay, trung binh). 0 = tat")] public float maxBlendAngularSpeed = 40f;
        [Tooltip("Tran thoi gian blend sau khi keo dai (s)")] public float maxBlendTime = 2.5f;

        [Header("Khong bao gio dung im")]
        [Tooltip("Do rung cam tay (do), nhan voi Perlin")] public float handheldAmplitude = 0.3f;
        [Tooltip("Tan so Perlin")] public float handheldFrequency = 0.4f;
        [Tooltip("He so doi tan so Perlin sang mau/giay")] public float handheldTimeScale = 5f;
        [Tooltip("Nhun theo buoc chan (m)")] [Range(0f, 0.03f)] public float bobAmplitude = 0.025f;
        [Tooltip("Buoc/giay khi di o toc do toi da")] public float bobStepsPerSecond = 1.9f;
        [Tooltip("Push-in cham trong khi giu khung (do giam FOV, chi zoom-in, sau settle)")] [Range(0f, 10f)] public float dollyInFov = 2.5f;
        [Tooltip("Thoi gian push-in cham (s)")] public float dollyInDuration = 8f;
        [Tooltip("Settle khi toi diem: giam FOV nhe (do), ease-out")] [Range(0f, 5f)] public float settleFov = 1.5f;
        [Tooltip("Thoi gian settle (s)")] [Min(0.05f)] public float settleTime = 0.55f;
        [Tooltip("Fade-in cam tay sau khi toi diem (s)")] [Min(0.01f)] public float handheldFadeIn = 0.6f;
        [Tooltip("Thoi gian chuyen Move <-> Combat cua noise/nhun (s)")] public float modeFade = 0.4f;

        [Header("Zoom (M2)")]
        [Tooltip("Zoom punch khi ha enemy: giam FOV (do)")] public float punchFov = 2f;
        [Tooltip("Thoi gian vao punch (s)")] public float punchIn = 0.15f;
        [Tooltip("Thoi gian tra punch (s)")] public float punchOut = 0.4f;
        [Tooltip("Push-in khi Justice shot: giam FOV (do)")] public float justicePunchFov = 3f;
        [Tooltip("Thoi gian vao push-in Justice (s)")] public float justicePunchIn = 0.25f;
        [Tooltip("Thoi gian tra push-in Justice (s)")] public float justicePunchOut = 0.8f;
        [Tooltip("Zoom vao enemy cuoi: giam FOV (do)")] public float killZoomFov = 3f;
        [Tooltip("Thoi gian zoom vao enemy cuoi (s) truoc khi chuyen goc")] public float killZoomDuration = 0.5f;
        [Tooltip("Ti le goc xoay ve phia vi tri kill cuoi (0..1)")] [Range(0f, 1f)] public float killZoomAimBlend = 0.25f;
        [Tooltip("Goc xoay toi da ve phia vi tri kill (do)")] public float killZoomMaxTurn = 4f;

        [Header("Chuyen Phase")]
        public float phaseFadeOut = 0.4f;
        public float phaseHold = 1.0f;
        public float phaseFadeIn = 0.4f;
        [Tooltip("Giam chuyen dong: fade vao/ra ngan hon (s)")] public float reduceMotionFade = 0.2f;

        [Header("Nhip ray")]
        [Tooltip("Doan Move dai hon thi tang toc do (toi da maxRailSpeed) de khong qua nay (s). Plan: <= 6 s")] public float maxMoveSeconds = 5.8f;
        [Tooltip("Toc do ray toi da khi bi tang de giu maxMoveSeconds (m/s). Plan: 3-4 m/s")] public float maxRailSpeed = 4.5f;

        [Header("Man hinh doc")]
        [Tooltip("Ti le khung hinh luc dat FOV cua cac Shot (rong/cao). FOV trong Shot/baseFov la FOV doc o ti le nay.")]
        [Min(0.1f)] public float designAspect = 16f / 9f;
        [Tooltip("Khi man hep hon designAspect: 0 = giu FOV doc (mat hai ben), 1 = giu tron goc nhin ngang")]
        [Range(0f, 1f)] public float keepHorizontalFov = 0f;
        [Tooltip("Tran FOV doc sau khi quy doi (do), tranh meo hinh qua muc")]
        [Range(30f, 120f)] public float maxVerticalFov = 45f;
        [Tooltip("Goc Combat tu can khung theo ti le man that: xoay cho enemy vao giua, noi FOV, lui ra sau neu can")]
        public bool autoFrame = true;
        [Tooltip("Le (do) quanh moi diem can thay: than enemy + vong target")] public float frameMargin = 5f;
        [Tooltip("Layer vat can khi lui camera trong AutoFrame")] public LayerMask autoFrameBlockMask = ~0;
        [Tooltip("Xoay ngang toi da khi can giua (do)")] public float frameMaxYaw = 12f;
        [Tooltip("Lui ra sau toi da (m); dung truoc vat can")] public float frameMaxPullBack = 2.5f;
        [Tooltip("May rat dai: cho noi FOV doc them toi da (do) vuot maxVerticalFov/baseFov khi lui het van thieu khung")] [Min(0f)] public float frameExtraFov = 5f;
        [Tooltip("Toc do hoi tu (1/s) khi can khung lai shot dang live sau khi doi aspect")] [Min(0.1f)] public float liveReframeRate = 3f;
        [Tooltip("Khi giao tranh: muc tieu dang lo ra ngoai khung thi camera lia ngang cho no vao khung")]
        public bool trackTargets = true;
        [Tooltip("Goc lia toi da so voi goc dat (do)")] public float trackMaxYaw = 12f;
        [Tooltip("Toc do lia (do/giay), < maxYawRate")] public float trackSpeed = 20f;
        [Tooltip("Thoi gian lam muot luc lia theo muc tieu (s) - tranh bat/dung dot ngot")] [Min(0.01f)] public float trackSmoothTime = 0.8f;
        [Tooltip("Le (do) trong khung truoc khi camera lia theo muc tieu (dead-zone)")] public float trackMargin = 1.5f;
        [Tooltip("Muc tieu gan hon khoang nay (m) so voi camera bi bo qua khi lia")] public float trackMinDepth = 3f;

        [Header("Shake khi trung dan")]
        public float hitShakeDuration = 0.2f;
        [Tooltip("Do lech vi tri (m)")] public float hitShakePosition = 0.04f;
        [Tooltip("Do lech goc (do)")] public float hitShakeAngle = 1.2f;
        [Tooltip("He so khi Giam chuyen dong")] [Range(0f, 1f)] public float hitShakeReduceScale = 0.35f;
        [Tooltip("Tan so Perlin cua shake (lan/giay)")] public float hitShakeFrequency = 40f;

        [Header("Rung khi no (W7 dung)")]
        [Tooltip("Bien do rung no = he so nay * bien do shake trung dan")] [Min(0f)] public float explosionShakeScale = 0.6f;
        [Tooltip("Thoi luong rung no (s)")] [Min(0.01f)] public float explosionShakeDuration = 0.25f;
        [Tooltip("true = tat rung no khi Giam chuyen dong")] public bool explosionShakeOffWhenReduceMotion = true;

        [Header("Muot (CAM-SMOOTH)")]
        [Tooltip("Bo gioi han van toc/gia toc cuoi pipeline (luoi an toan): camera that khong bao gio vuot cac gioi han duoi day, ke ca khi blend bi ngat / Cut / hieu ung chong nhau")]
        public bool smootherEnabled = true;
        [Tooltip("Van toc xoay toi da (do/giay)")] public float smoothMaxAngVel = 70f;
        [Tooltip("Gia toc xoay toi da (do/giay^2)")] public float smoothMaxAngAcc = 260f;
        [Tooltip("Van toc doi vi tri toi da (m/s)")] public float smoothMaxVel = 12f;
        [Tooltip("Gia toc doi vi tri toi da (m/s^2)")] public float smoothMaxAcc = 30f;
        [Tooltip("Toc do doi FOV toi da (do/giay)")] public float smoothMaxFovRate = 14f;
        [Tooltip("Gia toc doi FOV toi da (do/giay^2)")] public float smoothMaxFovAcc = 50f;
        [Tooltip("Giam chuyen dong: nhan cac gioi han tren voi he so nay (<1 = cham hon). KHONG Cut")] [Range(0.2f, 1f)] public float smoothReduceMotionScale = 0.6f;
        [Tooltip("Blend giua 2 shot: gia toc xoay toi da (do/giay^2) -> thoi gian blend toi thieu = sqrt(6*goc/gia toc)")] public float blendMaxAngAccel = 150f;
        [Tooltip("Blend giua 2 shot: gia toc vi tri toi da (m/s^2)")] public float blendMaxAccel = 14f;

        [Header("Khac")]
        [Tooltip("Lam muot roll theo toc do xoay (s)")] [Min(0.01f)] public float rollSmoothTime = 0.3f;
        [Tooltip("Cho them (s) sau blend truoc khi nha CombatPause")] [Min(0f)] public float blendWaitMargin = 0.5f;
    }
}
