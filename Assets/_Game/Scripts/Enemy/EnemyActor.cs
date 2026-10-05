using System;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy
{
    /// <summary>
    /// Enemy lo ra tu cho nap: Hidden -> Peeking -> Aiming (vong target) -> ban -> Retreat -> nap -> lo lai.
    /// Pivot o chan. Vi tri nap = vi tri luc Awake/Setup; vi tri Peek = peekPoint hoac offset local.
    /// (Ten class EnemyActor de khong trung namespace ClaudeCop.Enemy.)
    /// </summary>
    public class EnemyActor : MonoBehaviour, ITapTarget
    {
        [SerializeField] EnemyConfig config;
        [Tooltip("Vi tri lo ra (con 'Peek'). Neu trong thi dung peekLocalOffset.")]
        [SerializeField] Transform peekPoint;
        [SerializeField] Vector3 peekLocalOffset = new Vector3(0f, 0f, 1.2f);
        [Tooltip("Collider de tap; chi bat khi targetable. Neu trong tu lay trong con.")]
        [SerializeField] Collider hitCollider;
        [Tooltip("Enemy nay co Justice point (cham o tay cam sung). EncounterWave bat cho ~1/3 enemy theo EnemyConfig; bat tay o prefab variant.")]
        [SerializeField] bool justiceEnabled;
        [Tooltip("Cham vang hien Justice point (bat/tat theo justiceEnabled). Tuy chon.")]
        [SerializeField] GameObject justiceMarker;
        [Tooltip("Hien khi dau hang (gio tay). Tuy chon.")]
        [SerializeField] GameObject handsUpMarker;

        [Header("Dung san (SceneStanding)")]
        [Tooltip("Enemy dat san trong scene, dung lo san o vi tri hien tai (khong lo ra tu cho nap): khi kich hoat vao thang Ngam, van co vong target; het vong thi ban roi ngam tiep.")]
        [SerializeField] bool sceneStanding;

        [Header("Grenadier")]
        [Tooltip("Khi vong thu het thi NEM luu dan thay vi ban (can grenadePrefab).")]
        [SerializeField] bool throwsGrenade;
        [SerializeField] Grenade grenadePrefab;
        [Tooltip("Tay nem (tuy chon). Trong thi dung chan + EnemyConfig.grenadeThrowHeight.")]
        [SerializeField] Transform throwOrigin;

        const float RunInTime = 2.2f, RunInDistance = 10f;
        static int nextId = 1;
        EnemyBrain brain;
        Vector3 hidePos, peekPos;
        Quaternion hideRot, peekRot;
        bool positionsCached, registered, dead, initialized;
        float deadTimer;
        Quaternion deadFromRot, deadToRot;
        int id;
        bool surrendered;
        float reticleTimeOverride = -1f, hideTimeOverride = -1f;

        /// <summary>Phat khi enemy chet (enemy, vi tri world).</summary>
        public event Action<EnemyActor, Vector3> Died;
        /// <summary>Phat khi enemy ban nguoi choi.</summary>
        public event Action<EnemyActor> Fired;

        public EnemyState State => brain != null ? brain.State : EnemyState.Hidden;
        public bool IsDead => dead;
        public bool IsSurrendered => surrendered;
        public bool JusticeEnabled => justiceEnabled;
        /// <summary>Con o trang thai Hidden va chua duoc kich hoat (con doi duoc cau hinh lai).</summary>
        public bool IsActivated => brain != null && brain.IsActivated;
        /// <summary>Enemy dung san (khong lo ra tu cho nap). Doi truoc khi kich hoat.</summary>
        public bool SceneStanding => sceneStanding;

        /// <summary>Bat/tat che do dung san. Chi co hieu luc khi chua kich hoat.</summary>
        public void SetSceneStanding(bool on)
        {
            EnsureInit();
            sceneStanding = on;
            if (IsActivated || dead) return;
            if (on) { peekPos = hidePos; peekRot = hideRot; }
            SetRenderers(!on);
            BuildBrain();
        }

        public int Id => id;
        public TargetKind Kind => TargetKind.Enemy;
        public bool IsTargetable => brain != null && brain.IsTargetable && !CombatPauseSignal.IsPaused;
        public virtual Vector3 AimPoint => transform.position + Vector3.up * Cfg.aimHeight;
        public virtual bool HasJusticePoint => justiceEnabled && !dead;
        public virtual Vector3 JusticePoint => transform.TransformPoint(Cfg.justiceOffset);
        public bool ShowsReticle => brain != null && brain.ShowsReticle;
        public float ReticleProgress => brain != null ? brain.ReticleProgress : 0f;
        public float ExposedTime => brain != null ? brain.ExposedTime : 0f;

        public EnemyConfig Config => config;
        protected EnemyConfig Cfg => config != null ? config : EnemyConfig.Fallback;
        public bool ThrowsGrenade => throwsGrenade && grenadePrefab != null;

        /// <summary>Phat khi enemy nem luu dan (enemy, luu dan). EncounterWave nghe de cho luu dan duoc giai quyet.</summary>
        public event Action<EnemyActor, Grenade> GrenadeThrown;

        /// <summary>Bien enemy nay thanh Grenadier (hoac tat voi null).</summary>
        public void ConfigureGrenadier(Grenade prefab)
        {
            grenadePrefab = prefab;
            throwsGrenade = prefab != null;
        }

        protected virtual void Awake() { EnsureInit(); }

        void EnsureInit()
        {
            if (initialized) return;
            initialized = true;
            id = nextId++;
            if (hitCollider == null) hitCollider = GetComponentInChildren<Collider>();
            CachePositions();
            BuildBrain();
            SetCollider(false);
            if (sceneStanding) SetRenderers(false);
            RefreshMarkers();
        }

        /// <summary>Bat/tat Justice point cho enemy nay (EncounterWave goi).</summary>
        public virtual void SetJustice(bool on)
        {
            justiceEnabled = on;
            RefreshMarkers();
        }

        void RefreshMarkers()
        {
            if (justiceMarker != null) justiceMarker.SetActive(justiceEnabled && !dead);
            if (handsUpMarker != null) handsUpMarker.SetActive(surrendered);
        }

        /// <summary>Doi thoi gian vong target (RankScore reticle_time). Chi co hieu luc khi enemy chua kich hoat.</summary>
        public void SetReticleTime(float seconds)
        {
            EnsureInit();
            reticleTimeOverride = seconds;
            if (!IsActivated && !dead) BuildBrain();
        }

        /// <summary>Doi thoi gian nap truoc khi lo lai. Chi co hieu luc khi enemy chua kich hoat.</summary>
        public void SetHideTime(float seconds)
        {
            EnsureInit();
            hideTimeOverride = seconds;
            if (!IsActivated && !dead) BuildBrain();
        }

        /// <summary>Doi config (vd. wave cau hinh cho enemy dat san trong scene). Chi khi chua kich hoat.</summary>
        public void ApplyConfig(EnemyConfig cfg)
        {
            EnsureInit();
            if (cfg == null || IsActivated || dead) return;
            config = cfg;
            BuildBrain();
        }

        /// <summary>Dung khi spawn tu prefab: dat vi tri nap/Peek tuong minh.</summary>
        public void Setup(EnemyConfig cfg, Vector3 hidePosition, Quaternion rotation, Vector3 peekPosition, Quaternion peekRotation)
        {
            EnsureInit();
            if (cfg != null) config = cfg;
            hidePos = hidePosition; hideRot = rotation; peekPos = peekPosition; peekRot = peekRotation;
            if (sceneStanding) { peekPos = hidePos; peekRot = hideRot; }
            positionsCached = true;
            transform.SetPositionAndRotation(hidePos, hideRot);
            BuildBrain();
        }

        void CachePositions()
        {
            if (positionsCached) return;
            hidePos = transform.position; hideRot = transform.rotation;
            if (peekPoint != null) { peekPos = peekPoint.position; peekRot = peekPoint.rotation; }
            else { peekPos = transform.TransformPoint(peekLocalOffset); peekRot = hideRot; }
            if (sceneStanding) { peekPos = hidePos; peekRot = hideRot; }
            positionsCached = true;
        }

        void BuildBrain()
        {
            var c = Cfg;
            brain = new EnemyBrain(
                sceneStanding ? Mathf.Max(c.peekDuration, RunInTime) : c.peekDuration,
                reticleTimeOverride > 0f ? reticleTimeOverride : c.reticleTime,
                c.retreatDuration,
                sceneStanding ? 0f : (hideTimeOverride >= 0f ? hideTimeOverride : c.hideTime),
                c.targetableThreshold);
            brain.StandsGround = sceneStanding;
            brain.AimStarted = OnAimStarted;
            brain.AimEnded = OnAimEnded;
            brain.Fired = OnFired;
            dead = false;
        }

        /// <summary>Bat dau lo ra (EncounterWave goi, hoac goi tay).</summary>
        public void Activate()
        {
            EnsureInit();
            if (dead) return;
            if (sceneStanding && !brain.IsActivated) StartRunIn();
            brain.Activate();
        }

        /// <summary>Enemy dung san: xuat phat ngoai man hinh (lech ngang theo phia cua camera) roi chay vao vi tri dung khi wave bat dau (player da dung).</summary>
        void StartRunIn()
        {
            var cam = UnityEngine.Camera.main;
            if (cam != null)
            {
                Vector3 right = cam.transform.right; right.y = 0f;
                right = right.sqrMagnitude < 1e-4f ? Vector3.right : right.normalized;
                float side = Vector3.Dot(peekPos - cam.transform.position, right) >= 0f ? 1f : -1f;
                hidePos = peekPos + right * side * RunInDistance;
                hideRot = Quaternion.LookRotation(-right * side);
            }
            transform.SetPositionAndRotation(hidePos, hideRot);
            SetRenderers(true);
        }

        void SetRenderers(bool on)
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = on;
        }

        void Update()
        {
            if (dead) { UpdateDeath(); return; }
            if (CombatPauseSignal.IsPaused) return;
            Tick(Time.deltaTime);
        }

        /// <summary>Tien state machine dt giay va cap nhat vi tri (Update goi; test goi truc tiep).</summary>
        public void Tick(float dt)
        {
            if (dead || brain == null) return;
            brain.Tick(dt);
            float t = brain.PeekT;
            transform.SetPositionAndRotation(Vector3.Lerp(hidePos, peekPos, t), Quaternion.Slerp(hideRot, peekRot, t));
        }

        void OnAimStarted()
        {
            SetCollider(true);
            if (!registered) { registered = true; TargetRegistry.Register(this); OnTargetsRegistered(); }
        }

        void OnAimEnded()
        {
            SetCollider(false);
            if (registered) { registered = false; TargetRegistry.Unregister(this); OnTargetsUnregistered(); }
        }

        /// <summary>Hook cho lop con: vua dang ky vao TargetRegistry (dang ky them muc tieu phu).</summary>
        protected virtual void OnTargetsRegistered() { }
        /// <summary>Hook cho lop con: vua huy dang ky.</summary>
        protected virtual void OnTargetsUnregistered() { }
        /// <summary>Hook cho lop con: enemy vua chet/dau hang (sau khi da huy dang ky).</summary>
        protected virtual void OnKilled(bool justice) { }

        void OnFired()
        {
            if (ThrowsGrenade) { ThrowGrenade(); return; }
            PlayerDamageService.Damage(DamageSource.EnemyShot, AimPoint);
            Fired?.Invoke(this);
        }

        public virtual TapOutcome OnTapHit(ShotInfo shot, bool isJustice)
        {
            if (dead || !IsTargetable) return TapOutcome.Miss;
            bool justice = isJustice && HasJusticePoint;
            Vector3 pos = transform.position;
            brain.Kill();           // goi AimEnded -> huy dang ky
            dead = true;
            deadTimer = 0f;
            surrendered = justice;  // Justice Shot: tuoc vu khi, enemy dau hang (gio tay) roi bien mat
            if (justice) RefreshMarkers();
            else
            {
                // Nga quanh chan (pivot o chan): xoay ca root.
                deadFromRot = transform.rotation;
                Vector3 dir = shot.Direction.sqrMagnitude > 0.001f ? shot.Direction : transform.forward;
                Vector3 axis = Vector3.Cross(Vector3.up, dir);
                if (axis.sqrMagnitude < 0.001f) axis = transform.right;
                deadToRot = Quaternion.AngleAxis(Cfg.fallAngle, axis.normalized) * deadFromRot;
                if (justiceMarker != null) justiceMarker.SetActive(false);
            }
            OnKilled(justice);
            Died?.Invoke(this, pos);
            return justice ? TapOutcome.JusticeKill : TapOutcome.Kill;
        }

        void UpdateDeath()
        {
            deadTimer += Time.deltaTime;
            if (surrendered)
            {
                float t = Cfg.surrenderTime;
                if (deadTimer >= t) gameObject.SetActive(false);
                return;
            }
            float linger = Cfg.deathLinger;
            transform.rotation = Quaternion.Slerp(deadFromRot, deadToRot, Mathf.Clamp01(deadTimer / Cfg.fallDuration));
            if (deadTimer >= linger) gameObject.SetActive(false);
        }

        void SetCollider(bool on) { if (hitCollider != null) hitCollider.enabled = on; }

        void OnEnable()
        {
            // Bat lai giua luc Aiming: dang ky lai de van ban duoc.
            if (brain != null && !dead && brain.IsTargetable && !registered)
            {
                registered = true;
                SetCollider(true);
                TargetRegistry.Register(this);
                OnTargetsRegistered();
            }
        }

        void OnDisable()
        {
            if (registered) { registered = false; TargetRegistry.Unregister(this); OnTargetsUnregistered(); }
        }

        // ---------- Luu dan ----------

        /// <summary>Diem dap cua luu dan: truoc camera camDistance m. Khong co camera: tu tay nem di thang ve phia forward.</summary>
        public static Vector3 ComputeGrenadeTarget(Vector3 start, Vector3 forward, bool hasCamera, Vector3 camPos, Vector3 camForward, EnemyConfig c)
        {
            if (hasCamera) return camPos + camForward * c.grenadeLandDistance + Vector3.up * c.grenadeLandHeightOffset;
            return start + forward * c.grenadeFallbackDistance + Vector3.up * c.grenadeLandHeightOffset;
        }

        void ThrowGrenade()
        {
            var c = Cfg;
            Vector3 start = throwOrigin != null ? throwOrigin.position : transform.position + Vector3.up * c.grenadeThrowHeight;
            var cam = UnityEngine.Camera.main;
            Vector3 target = ComputeGrenadeTarget(start, transform.forward, cam != null,
                cam != null ? cam.transform.position : Vector3.zero, cam != null ? cam.transform.forward : Vector3.forward, c);
            var g = Instantiate(grenadePrefab, start, Quaternion.identity);
            g.Launch(start, target, c);
            GrenadeThrown?.Invoke(this, g);
        }
    }
}
