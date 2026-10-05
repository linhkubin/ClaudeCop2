using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.FX
{
    /// <summary>
    /// Nghe CombatEvents (Core) va phat FX. Khong tham chieu Combat/Enemy.
    /// Diem trung + phap tuyen moi truong: tu raycast tu camera qua ShotFired.screenPos
    /// (Combat khong phai doi; ShotResult khong co phap tuyen).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FxSystem : MonoBehaviour
    {
        [SerializeField] FxConfig config;

        FxPool pool;
        TracerPool tracers;
        Camera cam;
        Vector2 lastScreenPos;
        bool hasShot;
        readonly RaycastHit[] hits = new RaycastHit[8];

        public FxPool Pool => pool;

        void Awake()
        {
            pool = new FxPool(transform);
            if (config != null && config.explosion != null && config.explosion.prefab != null)
                pool.Prewarm(config.explosion.prefab, config.explosionPrewarm);
        }

        void OnEnable()
        {
            CombatEvents.ShotFired += OnShotFired;
            CombatEvents.ShotResolved += OnShotResolved;
            BlastEvents.Blasted += OnBlasted;
        }

        void OnDisable()
        {
            tracers?.ReleaseAll();
            CombatEvents.ShotFired -= OnShotFired;
            CombatEvents.ShotResolved -= OnShotResolved;
            BlastEvents.Blasted -= OnBlasted;
            pool?.ReleaseAll();
        }

        void OnDestroy() { pool?.Destroy(); tracers?.Destroy(); }

        void Update() { pool.Tick(Time.time); tracers?.Tick(Time.time); }

        bool EnsureTracers()
        {
            if (tracers != null) return true;
            var mat = config.tracerMaterial;
            if (mat == null)
            {
                var sh = Shader.Find("Sprites/Default");
                if (sh == null) return false;
                mat = new Material(sh) { name = "TracerRuntime", hideFlags = HideFlags.DontSave };
            }
            tracers = new TracerPool(transform, mat, config.tracerPoolSize);
            return true;
        }

        void SpawnTracers(ShotResult r)
        {
            if (!config.tracerEnabled || r.Outcome == TapOutcome.Blocked || !hasShot) return;
            if (!EnsureCam() || !EnsureTracers()) return;
            var style = config.GetTracer(r.Weapon);
            Vector3 camPos = cam.transform.position;

            var ray = cam.ScreenPointToRay(lastScreenPos);
            bool hasHit = r.Outcome != TapOutcome.Miss;
            Vector3 end = TracerMath.ComputeEnd(ray.origin, ray.direction, hasHit, r.WorldPoint, config.tracerMissDistance);

            // CAM-VC2: viewmodel xoay sung ve diem trung ngay luc nay (khong phu thuoc thu tu su kien) roi tra diem nong
            // (da quy ve camera chinh de vet dan bat dau dung o dau nong tren man hinh). Khong co viewmodel: nong gan nhat / canh duoi man hinh.
            Vector3 origin;
            if (!MuzzleAnchor.TryAimAt(end, out origin) && !MuzzleAnchor.TryGet(out origin))
            {
                float w = Screen.width, h = Screen.height;
                origin = cam.ScreenToWorldPoint(new Vector3(w * 0.5f + (lastScreenPos.x - w * 0.5f) * 0.35f, h * 0.02f, 0.8f));
            }

            float rm = UserSettings.ReduceMotion ? config.tracerReduceMotionScale : 1f;
            Color col = style.color; col.a *= rm;
            float tail = style.tailLag * rm;
            int count = Mathf.Max(1, r.TargetsHit) + Mathf.Max(0, style.extraPellets);
            if (UserSettings.ReduceMotion) count = Mathf.Min(count, 2);
            for (int i = 0; i < count; i++)
            {
                // Vet dau la tia chinh; cac vet con lai tan nhe quanh no.
                Vector3 e = i == 0 ? end : TracerMath.SpreadEnd(origin, end, Mathf.Max(style.spreadDeg, 0.01f), Random.value, Random.value);
                float dist = (e - origin).magnitude;
                float camDist = ((origin + e) * 0.5f - camPos).magnitude;
                float width = TracerMath.WidthAtDistance(style.width, config.tracerWidthPerMeter, camDist, config.tracerMaxWidth);
                float travel = TracerMath.TravelTime(dist, style.speed, 0.03f);
                tracers.Spawn(origin, e, col, width * (i == 0 ? 1f : 0.7f), travel, tail, Time.time);
            }
        }

        float Scale => UserSettings.ReduceMotion ? config.reduceMotionScale : 1f;

        bool EnsureCam()
        {
            if (cam == null || !cam.isActiveAndEnabled) cam = Camera.main;
            return cam != null;
        }

        void Play(FxEntry e, Vector3 pos, Quaternion rot)
        {
            if (e == null || e.prefab == null) return;
            pool.Spawn(e.prefab, pos, rot, e.lifetime, e.maxInstances, Scale, config.maxActiveParticles, Time.time);
        }

        void OnBlasted(BlastReport r)
        {
            if (config == null) return;
            Play(config.explosion, r.Center, Quaternion.identity);
        }

        void OnShotFired(WeaponKind weapon, Vector2 screenPos)
        {
            lastScreenPos = screenPos; hasShot = true;
            if (config == null || !config.muzzleEnabled || !EnsureCam()) return;
            // Dau nong: gan canh duoi man hinh, huong ve diem tap.
            float w = Screen.width, h = Screen.height;
            var origin = new Vector3(w * 0.5f + (screenPos.x - w * 0.5f) * 0.35f, h * 0.02f, 0.8f);
            var target = new Vector3(screenPos.x, screenPos.y, 6f);
            Vector3 o = cam.ScreenToWorldPoint(origin), t = cam.ScreenToWorldPoint(target);
            var dir = t - o;
            Play(config.muzzle, o, dir.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(dir) : cam.transform.rotation);
        }

        void OnShotResolved(ShotResult r)
        {
            if (config == null || !EnsureCam()) return;
            SpawnTracers(r);
            var targetFx = config.GetOutcome(r.Outcome);
            if (targetFx != null)
            {
                var toCam = cam.transform.position - r.WorldPoint;
                Play(targetFx, r.WorldPoint, toCam.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(toCam) : Quaternion.identity);
                return;
            }
            if ((r.Outcome == TapOutcome.Environment || r.Outcome == TapOutcome.Miss) && hasShot)
                EnvironmentImpact();
        }

        void EnvironmentImpact()
        {
            var ray = cam.ScreenPointToRay(lastScreenPos);
            int n = Physics.RaycastNonAlloc(ray, hits, config.rayMaxDistance, config.rayMask, QueryTriggerInteraction.Ignore);
            if (n <= 0) return;
            int best = 0;
            for (int i = 1; i < n; i++) if (hits[i].distance < hits[best].distance) best = i;
            var hit = hits[best];
            var mat = SurfaceMaterialTag.Resolve(hit.collider);
            Play(config.GetSurface(mat), hit.point, Quaternion.LookRotation(hit.normal));
            if (mat != SurfaceMaterial.Flesh)
            {
                var markRot = Quaternion.LookRotation(-hit.normal) * Quaternion.AngleAxis(Random.value * 360f, Vector3.forward);
                Play(config.bulletMark, hit.point + hit.normal * config.markOffset, markRot);
            }
        }
    }
}
