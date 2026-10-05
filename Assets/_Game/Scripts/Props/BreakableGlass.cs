using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Props
{
    /// <summary>
    /// Tam kinh 1 phat vo. Kich thuoc o kinh = scale X/Y cua transform (lossy). Thay bang manh tam giac cat san (prefab shards,
    /// ve 1x1, tu pool, scale theo kich thuoc) + bui kinh; collider tat nen dan xuyen qua, khong vo lai.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreakableGlass : MonoBehaviour, IShootable
    {
        [SerializeField] PropConfig config;
        [Tooltip("Prefab manh (Prop_GlassShards), cat 1x1 trong mat phang XY.")]
        [SerializeField] GameObject shardsPrefab;
        [Tooltip("Prefab bui kinh (hat) - khong Rigidbody.")]
        [SerializeField] GameObject dustPrefab;
        [Tooltip("Phan hinh anh bi an khi vo.")]
        [SerializeField] GameObject visual;
        [SerializeField] Collider[] colliders;

        bool broken;
        public bool Broken => broken;
        PropConfig Cfg => config != null ? config : PropConfig.Fallback;

        void Awake()
        {
            if (colliders == null || colliders.Length == 0) colliders = GetComponentsInChildren<Collider>(true);
        }

        public void OnShot(ShotInfo shot)
        {
            if (broken) return;
            broken = true;
            var cfg = Cfg;
            if (colliders == null || colliders.Length == 0) colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = false;
            if (visual != null) visual.SetActive(false);

            var sys = PropSystem.Instance;
            if (sys == null) return;
            Vector3 ls = transform.lossyScale;
            var size = new Vector3(Mathf.Abs(ls.x), Mathf.Abs(ls.y), 1f);

            var inst = sys.Pool.Spawn(shardsPrefab, transform.position, transform.rotation, cfg.DebrisLifetime, Time.time, size);
            if (inst != null)
            {
                float f = cfg.GlassShardForce * shot.ImpulseScale;
                for (int i = 0; i < inst.Bodies.Length; i++)
                {
                    var rb = inst.Bodies[i];
                    Vector3 d = (shot.Direction + Random.insideUnitSphere * cfg.GlassShardSpread).normalized;
                    rb.AddForce(d * f, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * f * 0.2f, ForceMode.Impulse);
                }
            }
            if (dustPrefab != null)
            {
                Vector3 p = shot.HitPoint != Vector3.zero ? shot.HitPoint : transform.position;
                sys.Pool.Spawn(dustPrefab, p, Quaternion.LookRotation(shot.Direction.sqrMagnitude > 1e-6f ? shot.Direction : transform.forward),
                    cfg.GlassDustLifetime, Time.time);
            }
        }

        /// <summary>Dua kinh ve nguyen ven (sandbox/test).</summary>
        public void Restore()
        {
            broken = false;
            if (visual != null) visual.SetActive(true);
            for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = true;
        }
    }
}
