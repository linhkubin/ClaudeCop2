using System;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.FX
{
    [Serializable]
    public sealed class FxEntry
    {
        public GameObject prefab;
        [Min(0.05f)] public float lifetime = 1f;
        [Min(0)] public int maxInstances = 8;
    }

    /// <summary>Kieu vet dan cho mot loai sung.</summary>
    [Serializable]
    public struct TracerStyle
    {
        public Color color;
        [Tooltip("Do day co so (m).")] public float width;
        [Tooltip("Toc do dau dan (m/s).")] public float speed;
        [Tooltip("Do tre cua duoi so voi dau (s) = do dai vet = thoi gian mo dan.")] public float tailLag;
        [Tooltip("Do tan (do) cua cac vet phu / vet them.")] public float spreadDeg;
        [Tooltip("So vet phu tan nhe them (ngoai moi muc tieu trung).")] public int extraPellets;

        public static TracerStyle Pistol => new TracerStyle { color = new Color(1f, 0.92f, 0.55f, 1f), width = 0.02f, speed = 220f, tailLag = 0.12f, spreadDeg = 0f, extraPellets = 0 };
        public static TracerStyle Shotgun => new TracerStyle { color = new Color(1f, 0.8f, 0.4f, 1f), width = 0.016f, speed = 200f, tailLag = 0.1f, spreadDeg = 3f, extraPellets = 3 };
        public static TracerStyle MachineGun => new TracerStyle { color = new Color(1f, 0.6f, 0.2f, 0.85f), width = 0.012f, speed = 260f, tailLag = 0.08f, spreadDeg = 0.5f, extraPellets = 0 };
    }

    /// <summary>Cau hinh FX: prefab theo chat lieu / ket qua ban, gioi han pool va hat.</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/FX Config", fileName = "FxConfig")]
    public sealed class FxConfig : ScriptableObject
    {
        [Tooltip("Theo thu tu enum SurfaceMaterial: Concrete, Wood, Metal, Glass, Foliage, Flesh")]
        public FxEntry[] surfaceImpacts = new FxEntry[6];
        public FxEntry muzzle = new FxEntry();
        public FxEntry enemyHit = new FxEntry();
        public FxEntry justiceHit = new FxEntry();
        public FxEntry hostageHit = new FxEntry();
        [Tooltip("No thung (BlastEvents). Song <= 1 s, prewarm theo explosionPrewarm.")]
        public FxEntry explosion = new FxEntry { lifetime = 1f, maxInstances = 3 };
        [Min(0)] public int explosionPrewarm = 3;
        public FxEntry bulletMark = new FxEntry { lifetime = 8f, maxInstances = 24 };

        [Header("Gioi han")]
        [Min(1)] public int maxActiveParticles = 300;
        [Range(0.05f, 1f)] public float reduceMotionScale = 0.4f;
        public bool muzzleEnabled = true;

        [Header("Tracer (vet dan)")]
        public bool tracerEnabled = true;
        [Tooltip("Tuy chon. Trong = Sprites/Default (ho tro mau dinh diem).")]
        public Material tracerMaterial;
        [Min(4)] public int tracerPoolSize = 24;
        public TracerStyle tracerPistol = TracerStyle.Pistol;
        public TracerStyle tracerShotgun = TracerStyle.Shotgun;
        public TracerStyle tracerMachineGun = TracerStyle.MachineGun;
        [Tooltip("Do day them moi met khoang cach toi camera de xa van thay.")]
        public float tracerWidthPerMeter = 0.004f;
        public float tracerMaxWidth = 0.12f;
        [Tooltip("Khoang cach vet khi ban truot (m).")]
        public float tracerMissDistance = 40f;
        [Range(0.05f, 1f)] public float tracerReduceMotionScale = 0.5f;

        public TracerStyle GetTracer(WeaponKind k)
        {
            switch (k)
            {
                case WeaponKind.Shotgun: return tracerShotgun;
                case WeaponKind.MachineGun:
                case WeaponKind.SMG: return tracerMachineGun;
                default: return tracerPistol;
            }
        }

        [Header("Raycast moi truong")]
        public float rayMaxDistance = 80f;
        public LayerMask rayMask = ~0;
        public float markOffset = 0.012f;

        public FxEntry GetSurface(SurfaceMaterial m)
        {
            int i = (int)m;
            if (surfaceImpacts == null || surfaceImpacts.Length == 0) return null;
            if (i < 0 || i >= surfaceImpacts.Length || surfaceImpacts[i] == null || surfaceImpacts[i].prefab == null)
                return surfaceImpacts[0];
            return surfaceImpacts[i];
        }

        /// <summary>FX khi trung muc tieu; null neu ket qua khong can FX tai muc tieu.</summary>
        public FxEntry GetOutcome(TapOutcome o)
        {
            switch (o)
            {
                case TapOutcome.Kill: return enemyHit;
                case TapOutcome.JusticeKill: return justiceHit;
                case TapOutcome.HostageHit: return hostageHit != null && hostageHit.prefab != null ? hostageHit : enemyHit;
                default: return null;
            }
        }

        void OnValidate()
        {
            if (surfaceImpacts == null || surfaceImpacts.Length != 6)
                Array.Resize(ref surfaceImpacts, 6);
        }
    }
}
