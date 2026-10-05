using System;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Viewmodel
{
    [Serializable]
    public sealed class ViewmodelWeaponEntry
    {
        public WeaponKind kind;
        [Tooltip("Prefab Viewmodel_<Kind>: goc co Animator, Pivot > Body, Muzzle.")] public GameObject prefab;
        [Tooltip("Do dai clip Reload du phong (giay) neu khong doc duoc tu Animator.")] [Min(0.05f)] public float fallbackReloadClipLength = 1f;
        [Tooltip("Sung ban nhanh hon nguong nay (giay/phat) thi KHONG chay clip Fire, chi giat spring.")] [Min(0f)] public float fireClipMinInterval = 0.25f;
        [Tooltip("He so impulse giat rieng cua sung (1 = chuan).")] [Min(0f)] public float recoilImpulseScale = 1f;
        [Tooltip("Nhan do giat hien thi cua sung nay (vd. shotgun manh hon).")] [Min(0f)] public float recoilVisualScale = 1f;
    }

    /// <summary>Cau hinh viewmodel sung: prefab theo WeaponKind, camera overlay, vi tri theo aspect, muzzle flash, chuyen dong. Khong hard-code so trong script.</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Viewmodel Config", fileName = "ViewmodelConfig")]
    public sealed class ViewmodelConfig : ScriptableObject
    {
        public ViewmodelWeaponEntry[] weapons = new ViewmodelWeaponEntry[3];

        [Header("Camera overlay")]
        [Tooltip("FOV doc co dinh cua ViewmodelCamera (do) - sung khong bi phong/thu theo FOV camera chinh.")] [Range(20f, 90f)] public float overlayFov = 45f;
        [Min(0.001f)] public float overlayNear = 0.03f;
        [Min(0.1f)] public float overlayFar = 5f;

        [Header("Vi tri theo aspect (w/h). Neo theo viewport (0..1) o do sau depth.")]
        [Tooltip("Aspect hep (vd. 9:19.5 = 0.46).")] public float narrowAspect = 0.46f;
        [Tooltip("Aspect rong (vd. 9:16 = 0.5625). Ngoai khoang thi kep.")] public float wideAspect = 0.5625f;
        public Vector2 anchorNarrow = new Vector2(0.64f, 0.09f);
        public Vector2 anchorWide = new Vector2(0.64f, 0.1f);
        [Tooltip("Do sau tu camera overlay toi sung (m).")] [Min(0.05f)] public float depth = 0.45f;
        [Tooltip("Scale o aspect hep / rong.")] public float scaleNarrow = 1.0f;
        public float scaleWide = 1.1f;
        [Tooltip("Xoay co dinh (euler) cua sung de nhin nghieng 3/4 tu goc duoi phai.")] public Vector3 baseEuler = new Vector3(-4f, -8f, 0f);

        [Header("Muzzle flash (viewmodel tu phat)")]
        [Min(0.01f)] public float flashDuration = 0.05f;
        public Vector2 flashScaleRange = new Vector2(0.8f, 1.3f);

        [Header("Reload")]
        [Tooltip("Toc do Animator khi reload bi kep trong khoang nay de van doc duoc.")] public Vector2 reloadSpeedRange = new Vector2(0.4f, 4f);

        [Header("Ngam theo vet dan (CAM-VC2)")]
        [Tooltip("Bat: khi ban, nong sung xoay ve dung diem trung that (khop vet dan).")] public bool aimEnabled = true;
        [Tooltip("Goc xoay toi da cua sung khi ngam (do). Vuot thi giu sung trong khung; vet dan van di tu nong toi diem trung.")] [Range(0f, 60f)] public float aimMaxAngle = 30f;
        [Tooltip("Giu tu the ngam sau phat ban (giay).")] [Min(0f)] public float aimHoldTime = 0.10f;
        [Tooltip("Thoi gian ve tu the nghi (giay).")] [Min(0.01f)] public float aimReturnTime = 0.22f;
        [Tooltip("Do sau (m) cua diem ngam o camera overlay - diem xa de nong chi theo huong tap.")] [Min(1f)] public float aimDepth = 20f;
        [Tooltip("Tia du doan diem trung luc ShotFired (truoc khi FX co ket qua that).")] [Min(1f)] public float aimRayDistance = 60f;
        [Tooltip("Layer tia du doan (tu dong loai layer Viewmodel).")] public LayerMask aimRayMask = ~0;

        [Header("Chuyen dong")]
        public ViewmodelMotionSettings motion = new ViewmodelMotionSettings();

        public ViewmodelWeaponEntry Get(WeaponKind kind)
        {
            if (weapons == null) return null;
            for (int i = 0; i < weapons.Length; i++)
                if (weapons[i] != null && weapons[i].kind == kind) return weapons[i];
            return null;
        }

        /// <summary>Neo viewport + scale noi suy theo aspect.</summary>
        public void Layout(float aspect, out Vector2 anchor, out float scale)
        {
            float t = wideAspect > narrowAspect ? Mathf.InverseLerp(narrowAspect, wideAspect, aspect) : 1f;
            anchor = Vector2.Lerp(anchorNarrow, anchorWide, t);
            scale = Mathf.Lerp(scaleNarrow, scaleWide, t);
        }
    }
}
