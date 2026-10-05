using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Cinemachine;
using ClaudeCop.Core;

namespace ClaudeCop.Camera
{
    public enum ShotKind { Move, Combat }
    public enum ShotEntry { Cut, Blend, Spline }

    /// <summary>Moc huong nhin (yaw, do) tai ti le quang duong cua ray Move (0..1). Dung de ray di lui/di ngang ma van xoay mem toi huong shot ke.</summary>
    [Serializable]
    public struct LookKey
    {
        [Range(0f, 1f)] public float progress;
        public float yaw;
        public LookKey(float progress, float yaw) { this.progress = progress; this.yaw = yaw; }
    }

    [Serializable]
    public class SubAngle
    {
        [Tooltip("Transform con cua Shot, dat vi tri/huong cua goc phu")] public Transform view;
        public float fov = 55f;
        [Tooltip("Bao nhieu giay sau khi giao tranh bat dau thi chuyen sang goc nay")] public float startAfter = 4f;
        [Tooltip("Giu goc nay bao lau (s) roi quay lai goc chinh (1-2 s)")] public float hold = 1.5f;
        [Tooltip("<0 = lay subAngleBlend trong profile (EaseInOut)")] public float blendTime = -1f;
        /// <summary>CinemachineCamera runtime cua goc phu (tao boi CameraShot.EnsureCameras, KHONG la con cua Shot vi vcam long nhau khong duoc brain chon).</summary>
        [NonSerialized] public CinemachineCamera cam;
    }

    /// <summary>
    /// Mot Shot: Move (di theo SplineContainer) hoac Combat (camera tinh tai vi tri/huong cua Transform nay + FOV).
    /// Entry: Cut / Blend / Spline (Spline = vao bang cach chay doc ray; voi Combat tuong duong Blend).
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraShot : MonoBehaviour
    {
        public ShotKind kind = ShotKind.Combat;
        public ShotEntry entry = ShotEntry.Blend;
        [Tooltip("<0 = lay defaultBlend trong profile")] public float blendTime = -1f;

        [Header("Move")]
        public SplineContainer spline;
        [Tooltip("<=0 = lay railSpeed trong profile")] public float speedOverride = 0f;
        [Tooltip("Rong = ray nhin theo tiep tuyen (mac dinh). Co moc = huong nhin do cac moc quyet dinh (smoothstep giua cac moc, bat dau tu huong camera luc vao ray, ket thuc o huong Shot Combat ke) - dung cho doan noi 2 Phase de khong phai quay dau gap.")]
        public List<LookKey> lookKeys = new List<LookKey>();
        [Tooltip("> 0: damping (s) nhin theo tiep tuyen cua doan Move nay thay cho profile.lookDamping (nho = bam huong di sat hon).")] public float lookDampingOverride = 0f;
        [Tooltip("> 0: nhin truoc doc ray bao nhieu m (thay profile.lookAhead): lon hon = camera nhin xa hon ve phia di chuyen.")] public float lookAheadOverride = 0f;
        [Tooltip("> 0: toc do xoay toi da (do/giay) cua doan Move nay thay cho profile.maxYawRate.")] public float maxYawRateOverride = 0f;

        [Header("Combat")]
        public EncounterBase encounter;
        [Tooltip("Shot khong co encounter: giu camera bay nhieu giay (nhip giam tai) roi di tiep. 0 = bo qua ngay (co canh bao).")]
        public float dwell = 0f;
        public float fov = 60f;
        public List<SubAngle> subAngles = new List<SubAngle>();
        [Tooltip("Diem world phai nam trong khung (cho lo ra cua enemy/con tin/thung). Level01Assembler dien; dung cho AutoFrame.")]
        public List<Vector3> frameTargets = new List<Vector3>();

        /// <summary>Da can khung theo man that: CameraFeelApplier khong quy doi FOV nua.</summary>
        public bool AutoFramed { get; private set; }

        bool baseCaptured; Vector3 basePos; Quaternion baseRot; float baseFovValue;
        CinemachineCamera vcam;
        public CinemachineCamera VCam => vcam != null ? vcam : (vcam = GetComponent<CinemachineCamera>());

        /// <summary>Tao CinemachineCamera (Combat) cho Shot va cac goc phu neu chua co. Priority 0 = chua active.</summary>
        public void EnsureCameras(CameraFeelProfile profile)
        {
            if (kind != ShotKind.Combat) return;
            Setup(gameObject, fov, profile);
            for (int i = 0; i < subAngles.Count; i++)
            {
                var a = subAngles[i];
                if (a.view == null || a.cam != null) continue;
                var go = new GameObject(name + "_AngleCam" + i);
                go.transform.SetParent(transform.parent, false);
                go.transform.SetPositionAndRotation(a.view.position, a.view.rotation);
                Setup(go, a.fov, profile);
                a.cam = go.GetComponent<CinemachineCamera>();
            }
        }

        static void Setup(GameObject go, float fov, CameraFeelProfile profile)
        {
            var c = go.GetComponent<CinemachineCamera>();
            if (c == null) c = go.AddComponent<CinemachineCamera>();
            var l = c.Lens; l.FieldOfView = fov; c.Lens = l;
            c.Priority.Enabled = true; c.Priority.Value = 0;
            var ap = go.GetComponent<CameraFeelApplier>();
            if (ap == null) ap = go.AddComponent<CameraFeelApplier>();
            ap.profile = profile;
        }

        public float ResolveBlend(CameraFeelProfile p) => blendTime >= 0f ? blendTime : p.defaultBlend;

        /// <summary>
        /// Can khung goc Combat cho ti le man aspect (rong/cao): xoay ngang de frameTargets vao giua (<= frameMaxYaw),
        /// noi FOV doc vua du (<= maxVerticalFov, chua cho dolly-in), con thieu thi lui ra sau (<= frameMaxPullBack, dung truoc vat can).
        /// Goi mot lan sau EnsureCameras.
        /// </summary>
        float warnedAspect;

        public void AutoFrame(CameraFeelProfile p, float aspect)
        {
            if (!baseCaptured) { basePos = transform.position; baseRot = transform.rotation; baseFovValue = fov; baseCaptured = true; }
            else { transform.SetPositionAndRotation(basePos, baseRot); }
            if (kind != ShotKind.Combat || p == null || !p.autoFrame || aspect <= 0f || frameTargets.Count == 0) return;
            var cam = VCam;
            if (cam == null) return;

            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            Vector3 flat = Vector3.ProjectOnPlane(rot * Vector3.forward, Vector3.up);
            if (flat.sqrMagnitude < 1e-4f) return;
            flat.Normalize();

            // 1) Xoay ngang cho cum diem vao giua.
            float minH = float.MaxValue, maxH = float.MinValue;
            Vector3 right = Vector3.Cross(Vector3.up, flat);
            foreach (var t in frameTargets)
            {
                Vector3 d = t - pos;
                float h = Mathf.Atan2(Vector3.Dot(d, right), Vector3.Dot(d, flat)) * Mathf.Rad2Deg;
                minH = Mathf.Min(minH, h); maxH = Mathf.Max(maxH, h);
            }
            float yaw = Mathf.Clamp((minH + maxH) * 0.5f, -p.frameMaxYaw, p.frameMaxYaw);
            rot = Quaternion.AngleAxis(yaw, Vector3.up) * rot;
            Vector3 back = -Vector3.ProjectOnPlane(rot * Vector3.forward, Vector3.up).normalized;

            // 2) Lui ra sau den khi vua FOV toi da (khong xuyen vat can).
            float maxBack = p.frameMaxPullBack;
            if (maxBack > 0f && Physics.SphereCast(pos, 0.3f, back, out RaycastHit hit, maxBack, p.autoFrameBlockMask, QueryTriggerInteraction.Ignore))
                maxBack = Mathf.Max(0f, hit.distance - 0.2f);
            // FOV khoi dau Combat <= FOV camera ray (baseFov) de blend Move->Combat khong zoom-out; push-in sau do chi lam hep them.
            float limit = Mathf.Min(p.maxVerticalFov, p.baseFov > 0f ? p.baseFov : p.maxVerticalFov);
            float dist = 0f, need = RequiredFov(pos, rot, aspect, p.frameMargin);
            while (need > limit && dist < maxBack)
            {
                dist = Mathf.Min(maxBack, dist + 0.25f);
                need = RequiredFov(pos + back * dist, rot, aspect, p.frameMargin);
            }

            // May rat dai (aspect rat hep): neu van thieu sau khi lui, cho noi FOV them toi da frameExtraFov.
            float hardLimit = limit + Mathf.Max(0f, p.frameExtraFov);
            transform.SetPositionAndRotation(pos + back * dist, rot);
            var l = cam.Lens; l.FieldOfView = Mathf.Clamp(Mathf.Max(fov, need), 1f, need > limit ? hardLimit : limit); cam.Lens = l;
            if (need > hardLimit && !Mathf.Approximately(warnedAspect, aspect) && (warnedAspect = aspect) > 0f) Debug.LogWarning($"[CameraShot] {name}: aspect {aspect:0.00} qua dai, can FOV {need:0.0} > {hardLimit:0.0}; mot so diem co the tran khung.", this);
            AutoFramed = true;
        }

        /// <summary>
        /// Can khung lai shot DANG LIVE mot cach mem: tinh pose dich (AutoFrame) roi di chuyen/xoay/noi FOV dan tu pose hien tai.
        /// Tra ve true khi da toi dich (goi moi frame cho den khi true).
        /// </summary>
        public bool AutoFrameSmooth(CameraFeelProfile p, float aspect, float dt)
        {
            var cam = VCam;
            if (cam == null) { AutoFrame(p, aspect); return true; }
            Vector3 curPos = transform.position; Quaternion curRot = transform.rotation; float curFov = cam.Lens.FieldOfView;
            AutoFrame(p, aspect);
            Vector3 tPos = transform.position; Quaternion tRot = transform.rotation; float tFov = cam.Lens.FieldOfView;
            float k = 1f - Mathf.Exp(-Mathf.Max(0.1f, p.liveReframeRate) * dt);
            bool done = (tPos - curPos).sqrMagnitude < 1e-4f && Quaternion.Angle(tRot, curRot) < 0.1f && Mathf.Abs(tFov - curFov) < 0.1f;
            if (done) return true;
            transform.SetPositionAndRotation(Vector3.Lerp(curPos, tPos, k), Quaternion.Slerp(curRot, tRot, k));
            var l = cam.Lens; l.FieldOfView = Mathf.Lerp(curFov, tFov, k); cam.Lens = l;
            return false;
        }

        /// <summary>FOV doc nho nhat de moi frameTargets (cong le) nam trong khung tu pose nay voi ti le aspect.</summary>
        float RequiredFov(Vector3 pos, Quaternion rot, float aspect, float margin)
        {
            var inv = Quaternion.Inverse(rot);
            float hMax = 0f, vMax = 0f;
            foreach (var t in frameTargets)
            {
                Vector3 d = inv * (t - pos);
                if (d.z < 0.1f) return 180f; // sau camera: khong the vua
                hMax = Mathf.Max(hMax, Mathf.Abs(Mathf.Atan2(d.x, d.z)) * Mathf.Rad2Deg);
                vMax = Mathf.Max(vMax, Mathf.Abs(Mathf.Atan2(d.y, d.z)) * Mathf.Rad2Deg);
            }
            float halfH = Mathf.Min(89f, hMax + margin) * Mathf.Deg2Rad;
            float fromH = 2f * Mathf.Atan(Mathf.Tan(halfH) / aspect) * Mathf.Rad2Deg;
            float fromV = 2f * Mathf.Min(89f, vMax + margin);
            return Mathf.Max(fromH, fromV);
        }
    }
}
