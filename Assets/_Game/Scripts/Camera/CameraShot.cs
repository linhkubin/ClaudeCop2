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

        [Header("Combat")]
        public EncounterBase encounter;
        public float fov = 60f;
        public List<SubAngle> subAngles = new List<SubAngle>();

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
    }
}
