using System.Collections.Generic;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Props
{
    /// <summary>
    /// Thung no: 1 phat la no (chi 1 lan). Ha enemy lo ra / con tin / luu dan trong ban kinh, day vat ly, no day chuyen,
    /// roi phat <see cref="BlastEvents"/>. Khong tu cong diem, khong cham GameEvents/CombatEvents.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ExplosiveBarrel : MonoBehaviour, IShootable
    {
        [SerializeField] PropConfig config;
        [Tooltip("Phan hinh anh bi an khi no. Rong thi khong an.")]
        [SerializeField] GameObject visual;
        [SerializeField] Collider[] colliders;
        [SerializeField] GameObject debrisPrefab;

        static readonly Collider[] overlap = new Collider[64];
        static readonly List<ITapTarget> snapshot = new List<ITapTarget>(32);
        static readonly List<Rigidbody> pushed = new List<Rigidbody>(32);

        bool exploded, armed;
        float explodeAt;

        public bool Exploded => exploded;
        public int BlastId => GetInstanceID();
        PropConfig Cfg => config != null ? config : PropConfig.Fallback;

        void Awake()
        {
            enabled = false;
            if (colliders == null || colliders.Length == 0) colliders = GetComponentsInChildren<Collider>(true);
        }

        public void OnShot(ShotInfo shot) { Explode(); }

        /// <summary>No day chuyen sau ChainDelay (bo qua neu da no/da hen).</summary>
        public void ArmChain()
        {
            if (exploded || armed) return;
            armed = true; explodeAt = Time.time + Cfg.ChainDelay; enabled = true;
        }

        void Update() { if (armed && Time.time >= explodeAt) Explode(); }

        /// <summary>Dua thung ve nguyen ven (sandbox/test).</summary>
        public void Restore()
        {
            exploded = false; armed = false; enabled = false;
            if (visual != null) visual.SetActive(true);
            if (colliders != null)
                for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = true;
        }

        public void Explode()
        {
            if (exploded) return;
            exploded = true; armed = false; enabled = false;
            var cfg = Cfg;
            Vector3 center = transform.position + Vector3.up * 0.4f;
            float radius = cfg.BlastRadius;

            if (colliders == null || colliders.Length == 0) colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = false;
            if (visual != null) visual.SetActive(false);

            // 1) Muc tieu tap (snapshot vi OnTapHit se Unregister khoi TargetRegistry).
            snapshot.Clear();
            var list = TargetRegistry.Targets;
            for (int i = 0; i < list.Count; i++) snapshot.Add(list[i]);
            int enemies = 0, hostages = 0;
            for (int i = 0; i < snapshot.Count; i++)
            {
                var t = snapshot[i];
                if (t is Object uo && uo == null) continue; // muc tieu da bi huy ma chua Unregister
                var effect = BlastResolver.Classify(t.Kind, t.IsTargetable, t.AimPoint, center, radius);
                if (effect == BlastEffect.None) continue;
                Vector3 aim = t.AimPoint;
                Vector3 dir = aim - center;
                dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.up;
                var info = new ShotInfo
                {
                    ScreenPosition = new Vector2(float.NaN, float.NaN), // NaN = nguon no, khong phai tap man hinh (HumanShield nhan biet)
                    HitPoint = aim, HitNormal = -dir, Direction = dir,
                    Weapon = WeaponKind.Pistol, ImpulseScale = cfg.BlastImpulseScale
                };
                var outcome = t.OnTapHit(info, false);
                if (effect == BlastEffect.Enemy && (outcome == TapOutcome.Kill || outcome == TapOutcome.JusticeKill)) enemies++;
                else if (effect == BlastEffect.Hostage && outcome == TapOutcome.HostageHit) hostages++;
            }
            snapshot.Clear();
            if (BlastResolver.PenalizesPlayer(hostages)) PlayerDamageService.Damage(DamageSource.Explosion, center);

            // 2) Vat ly + no day chuyen.
            int n = Physics.OverlapSphereNonAlloc(center, radius, overlap, cfg.BlastMask, QueryTriggerInteraction.Ignore);
            pushed.Clear();
            for (int i = 0; i < n; i++)
            {
                var col = overlap[i]; overlap[i] = null;
                if (col == null) continue;
                var other = col.GetComponentInParent<ExplosiveBarrel>();
                if (other != null) { if (other != this) other.ArmChain(); continue; }
                var rb = col.attachedRigidbody;
                if (rb == null || rb.isKinematic || pushed.Contains(rb)) continue;
                pushed.Add(rb);
                rb.AddExplosionForce(cfg.ExplosionForce, center, radius, cfg.ExplosionUpwardsModifier);
            }
            pushed.Clear();

            // 3) Manh vo tu pool.
            var sys = PropSystem.Instance;
            if (sys != null && debrisPrefab != null)
            {
                var inst = sys.SpawnDebris(debrisPrefab, transform.position, transform.rotation);
                if (inst != null)
                    for (int i = 0; i < inst.Bodies.Length; i++)
                        inst.Bodies[i].AddExplosionForce(cfg.ExplosionForce, center, radius, cfg.ExplosionUpwardsModifier);
            }

            BlastEvents.Raise(new BlastReport { Center = center, Radius = radius, EnemiesKilled = enemies, HostagesHit = hostages, SourceId = BlastId });
        }
    }
}
