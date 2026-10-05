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

        [Header("Sinh dong (CAM-LIVELY)")]
        [Tooltip("Rung cam tay: he so roll (0 = khong lac nghieng). Handheld chi chay khi Combat")] [Range(0f, 1f)] public float handheldRollScale = 0f;
        [Tooltip("'Tho' FOV khi Combat: bien do +- (do). 0 = tat")] [Range(0f, 3f)] public float breathFov = 1.2f;
        [Tooltip("Chu ky tho FOV (s)")] [Min(0.5f)] public float breathPeriod = 4.5f;
        [Tooltip("Dolly-in them theo combo: toi da giam FOV (do). 0 = tat")] [Range(0f, 5f)] public float comboDollyFov = 2f;
        [Tooltip("Moi muc combo giam them FOV (do)")] [Min(0f)] public float comboDollyPerCombo = 0.25f;
        [Tooltip("Thoi gian lam muot dolly-in theo combo / ease ra khi het dot (s)")] [Min(0.05f)] public float comboDollySmooth = 0.9f;
        [Tooltip("GIAT MINH QUAY SANG: bat/tat")] public bool reactEnabled = true;
        [Tooltip("Enemy moi lo ra lech truc camera it nhat goc nay (do) moi phan ung")] [Min(0f)] public float reactMinOffset = 2.5f;
        [Tooltip("Ti le goc lech duoc quay ve (0..1)")] [Range(0f, 1f)] public float reactGain = 0.85f;
        [Tooltip("Bien do yaw nho nhat (do) khi da phan ung")] [Min(0f)] public float reactMinAngle = 4.5f;
        [Tooltip("Bien do yaw toi da (do)")] public float reactMaxYaw = 8f;
        [Tooltip("Bien do pitch toi da (do)")] public float reactMaxPitch = 4f;
        [Tooltip("Thoi gian vao (giat) (s)")] [Min(0.02f)] public float reactIn = 0.15f;
        [Tooltip("Giu ngan (s)")] [Min(0f)] public float reactHold = 0.12f;
        [Tooltip("Thoi gian ease ve (s)")] [Min(0.05f)] public float reactOut = 0.8f;
        [Tooltip("Punch FOV kem theo (do giam)")] [Range(0f, 4f)] public float reactFov = 1.5f;
        [Tooltip("Cooldown giua 2 phan ung (s, tinh tu luc bat dau)")] [Min(0f)] public float reactCooldown = 1.6f;
        [Tooltip("Gop cac enemy lo ra trong cua so nay (s) thanh MOT phan ung huong ve trong tam")] [Min(0f)] public float reactGatherWindow = 0.12f;
        [Tooltip("Gioi han rieng cua kenh reaction trong CameraPoseSmoother: van toc xoay (do/s)")] public float reactMaxAngVel = 170f;
        [Tooltip("... gia toc xoay (do/s^2)")] public float reactMaxAngAcc = 2200f;
        [Tooltip("... toc do doi FOV (do/s)")] public float reactMaxFovRate = 40f;
        [Tooltip("... gia toc FOV (do/s^2)")] public float reactMaxFovAcc = 500f;
        [Tooltip("Giam chuyen dong: he so bien do reaction (0 = tat)")] [Range(0f, 1f)] public float reactReduceMotionScale = 0f;

        [Header("Giat khi ban (CAM-VC2)")]
        [Tooltip("Bat/tat giat nhe camera moi phat ban")] public bool kickEnabled = true;
        [Tooltip("Ngang len (pitch) moi phat o he so vu khi = 1 (do). Plan: 0.3-0.6")] [Range(0f, 1.5f)] public float kickPitch = 0.4f;
        [Tooltip("Punch FOV (giam FOV) moi phat o he so vu khi = 1 (do). <= 0.5")] [Range(0f, 1f)] public float kickFov = 0.3f;
        [Tooltip("Thoi gian vao (s)")] [Min(0.005f)] public float kickRise = 0.03f;
        [Tooltip("Thoi gian hoi ve (s); rise + fall ~ 0.1 s")] [Min(0.01f)] public float kickFall = 0.09f;
        [Tooltip("He so Pistol / Shotgun / MachineGun (nhan kickPitch, kickFov)")] [Min(0f)] public float kickScalePistol = 1f;
        [Min(0f)] public float kickScaleShotgun = 1.5f;
        [Min(0f)] public float kickScaleMachineGun = 0.45f;
        [Tooltip("Tran cong don pitch (do) khi ban lien thanh")] [Range(0f, 2f)] public float kickMaxPitch = 0.8f;
        [Tooltip("Tran cong don punch FOV (do)")] [Range(0f, 1f)] public float kickMaxFov = 0.5f;
        [Tooltip("Giam chuyen dong: he so bien do giat (0 = tat)")] [Range(0f, 1f)] public float kickReduceMotionScale = 0f;
        [Tooltip("Khi CameraReaction dang chay (target > nay, do): giam giat toi kickReactDamp de khong chong len")] [Min(0.1f)] public float kickReactRef = 2f;
        [Tooltip("He so con lai cua giat khi reaction/kill-zoom dang o muc toi da (0..1)")] [Range(0f, 1f)] public float kickReactDamp = 0.3f;
        [Tooltip("Kenh giat rieng trong CameraPoseSmoother: van toc pitch toi da (do/s)")] public float kickMaxAngVel = 120f;
        [Tooltip("... gia toc pitch (do/s^2)")] public float kickMaxAngAcc = 6000f;
        [Tooltip("... toc do doi FOV (do/s)")] public float kickMaxFovRate = 20f;
        [Tooltip("... gia toc FOV (do/s^2)")] public float kickMaxFovAcc = 1500f;

        [Header("Nhip ray VC2 (CAM-VC2)")]
        [Tooltip("Bat: duong cong toc do ray (tang toc nhanh o dau, giam toc mem o cuoi) thay hinh thang cu. Tong thoi gian giu bang hinh thang cu")] public bool railCurveEnabled = true;
        [Tooltip("Thoi gian tang toc ra dau ray (s, smoothstep)")] [Min(0.05f)] public float railAccelTime = 0.8f;
        [Tooltip("Ti le quang duong cuoi dung de giam toc mem (0.25 = 25% cuoi)")] [Range(0.05f, 0.6f)] public float railDecelFraction = 0.25f;
        [Tooltip("Tran toc do ray khi duong cong tang toc de giu tong thoi gian (m/s)")] public float railCurveMaxSpeed = 5.5f;
        [Tooltip("Nhin truoc encounter ke: bat dau xoay huong nhin khi ray da di toi ti le nay (0.7 = 30% cuoi)")] [Range(0.3f, 0.95f)] public float lookNextStart = 0.7f;
        [Tooltip("Muc do huong ve huong shot ke o cuoi ray (0 = tat, 1 = khop dung huong shot)")] [Range(0f, 1f)] public float lookNextWeight = 1f;
        [Tooltip("Lam muot huong nhin trong doan nhin truoc (s) - nho hon lookDamping de kip khop huong shot")] [Min(0.05f)] public float lookNextDamping = 0.55f;
        [Tooltip("Van toc xoay yaw toi da trong doan nhin truoc (do/s), <= maxYawRate CAM-SMOOTH ~44")] public float lookNextMaxYawRate = 40f;

        [Header("Khac")]
        [Tooltip("Lam muot roll theo toc do xoay (s)")] [Min(0.01f)] public float rollSmoothTime = 0.3f;
        [Tooltip("Cho them (s) sau blend truoc khi nha CombatPause")] [Min(0f)] public float blendWaitMargin = 0.5f;
    }
}
