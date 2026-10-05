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
