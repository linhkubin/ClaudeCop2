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

        public int Id => id;
        public TargetKind Kind => TargetKind.Enemy;
        public bool IsTargetable => brain != null && brain.IsTargetable && !CombatPauseSignal.IsPaused;
        public Vector3 AimPoint => transform.position + Vector3.up * (config != null ? config.aimHeight : 1.5f);
        public bool HasJusticePoint => justiceEnabled && !dead;
        public Vector3 JusticePoint => transform.TransformPoint(config != null ? config.justiceOffset : Vector3.zero);
        public bool ShowsReticle => brain != null && brain.ShowsReticle;
        public float ReticleProgress => brain != null ? brain.ReticleProgress : 0f;
        public float ExposedTime => brain != null ? brain.ExposedTime : 0f;

        public EnemyConfig Config => config;

        void Awake() { EnsureInit(); }

        void EnsureInit()
        {
            if (initialized) return;
            initialized = true;
            id = nextId++;
            if (hitCollider == null) hitCollider = GetComponentInChildren<Collider>();
            CachePositions();
            BuildBrain();
            SetCollider(false);
            RefreshMarkers();
        }

        /// <summary>Bat/tat Justice point cho enemy nay (EncounterWave goi).</summary>
        public void SetJustice(bool on)
        {
            justiceEnabled = on;
            RefreshMarkers();
        }

        void RefreshMarkers()
        {
            if (justiceMarker != null) justiceMarker.SetActive(justiceEnabled && !dead);
            if (handsUpMarker != null) handsUpMarker.SetActive(surrendered);
        }

        /// <summary>Doi thoi gian vong target (Jev reticle_time). Chi co hieu luc khi enemy chua kich hoat.</summary>
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
            positionsCached = true;
        }

        void BuildBrain()
        {
            var c = config;
            brain = new EnemyBrain(
                c != null ? c.peekDuration : 0.3f,
                reticleTimeOverride > 0f ? reticleTimeOverride : (c != null ? c.reticleTime : 2.5f),
                c != null ? c.retreatDuration : 0.3f,
                hideTimeOverride >= 0f ? hideTimeOverride : (c != null ? c.hideTime : 0.8f));
            brain.AimStarted = OnAimStarted;
            brain.AimEnded = OnAimEnded;
            brain.Fired = OnFired;
            dead = false;
        }

        /// <summary>Bat dau lo ra (EncounterWave goi, hoac goi tay).</summary>
        public void Activate()
        {
            EnsureInit();
            if (!dead) brain.Activate();
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
            if (!registered) { registered = true; TargetRegistry.Register(this); }
        }

        void OnAimEnded()
        {
            SetCollider(false);
            if (registered) { registered = false; TargetRegistry.Unregister(this); }
        }

        void OnFired()
        {
            PlayerDamageService.Damage(DamageSource.EnemyShot, AimPoint);
            Fired?.Invoke(this);
        }

        public TapOutcome OnTapHit(ShotInfo shot, bool isJustice)
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
                deadToRot = Quaternion.AngleAxis(90f, axis.normalized) * deadFromRot;
                if (justiceMarker != null) justiceMarker.SetActive(false);
            }
            Died?.Invoke(this, pos);
            return justice ? TapOutcome.JusticeKill : TapOutcome.Kill;
        }

        void UpdateDeath()
        {
            deadTimer += Time.deltaTime;
            if (surrendered)
            {
                float t = config != null ? config.surrenderTime : 1f;
                if (deadTimer >= t) gameObject.SetActive(false);
                return;
            }
            float linger = config != null ? config.deathLinger : 1f;
            transform.rotation = Quaternion.Slerp(deadFromRot, deadToRot, Mathf.Clamp01(deadTimer / 0.25f));
            if (deadTimer >= linger) gameObject.SetActive(false);
        }

        void SetCollider(bool on) { if (hitCollider != null) hitCollider.enabled = on; }

        void OnEnable()
        {
            // Bat lai giua luc Aiming: dang ky lai de van ban duoc.
            if (brain != null && !dead && brain.State == EnemyState.Aiming && !registered)
            {
                registered = true;
                SetCollider(true);
                TargetRegistry.Register(this);
            }
        }

        void OnDisable()
        {
            if (registered) { registered = false; TargetRegistry.Unregister(this); }
        }
    }
}
