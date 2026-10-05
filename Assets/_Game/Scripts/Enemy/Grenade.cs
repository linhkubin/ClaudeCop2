using System;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy
{
    /// <summary>
    /// Luu dan do Grenadier nem: bay cong toi diem truoc camera, co vong target rieng (ReticleProgress = tien do bay).
    /// Tap trung -> Kill, no giua khong trung, khong gay hai. Bay het gio -> PlayerDamageService.Damage(Explosion) dung 1 lan.
    /// Thoi gian bay dung khi CombatPauseSignal.IsPaused. Tu Register/Unregister TargetRegistry. Khong ai nhat/nem lai.
    /// </summary>
    public class Grenade : MonoBehaviour, ITapTarget
    {
        [Tooltip("Than luu dan (tat khi no). Tuy chon.")]
        [SerializeField] GameObject body;
        [Tooltip("Hieu ung no (bat khi no, phong to roi huy). Tuy chon.")]
        [SerializeField] Transform explosionFx;

        static int nextId = 200000;
        GrenadeFlight flight;
        EnemyConfig cfg;
        bool registered;
        int id;
        float fxTimer = -1f;

        /// <summary>Phat khi luu dan duoc giai quyet: (luu dan, bi ban roi?). false = no vao nguoi choi.</summary>
        public event Action<Grenade, bool> Resolved;

        public bool IsResolved => flight != null && flight.IsResolved;
        public bool IsFlying => flight != null && flight.IsFlying;
        public GrenadeState State => flight != null ? flight.State : GrenadeState.Flying;
        public float Progress => flight != null ? flight.Progress : 0f;

        public int Id => id;
        public TargetKind Kind => TargetKind.Grenade;
        public bool IsTargetable => IsFlying && !CombatPauseSignal.IsPaused;
        public Vector3 AimPoint => transform.position;
        public bool HasJusticePoint => false;
        public Vector3 JusticePoint => transform.position;
        public bool ShowsReticle => IsFlying;
        public float ReticleProgress => Progress;
        public float ExposedTime => flight != null ? flight.Elapsed : 0f;

        void Awake() { id = nextId++; if (explosionFx != null) explosionFx.gameObject.SetActive(false); }

        /// <summary>Nem tu start toi end. EnemyActor goi.</summary>
        public void Launch(Vector3 start, Vector3 end, EnemyConfig config)
        {
            cfg = config != null ? config : EnemyConfig.Fallback;
            flight = new GrenadeFlight(start, end, cfg.grenadeFlightTime, cfg.grenadeArcHeight);
            transform.position = start;
            if (body != null) body.SetActive(true);
            if (!registered) { registered = true; TargetRegistry.Register(this); }
        }

        void Update()
        {
            if (fxTimer >= 0f) { UpdateFx(Time.deltaTime); return; }
            Tick(Time.deltaTime);
        }

        /// <summary>Tien dt giay (Update goi; test goi truc tiep). Dung khi CombatPauseSignal.IsPaused.</summary>
        public void Tick(float dt)
        {
            if (flight == null || !flight.IsFlying || CombatPauseSignal.IsPaused) return;
            bool arrived = flight.Tick(dt);
            transform.position = flight.Position;
            if (arrived)
            {
                Vector3 pos = transform.position;
                Finish(false);
                PlayerDamageService.Damage(DamageSource.Explosion, pos);
            }
        }

        public TapOutcome OnTapHit(ShotInfo shot, bool isJustice)
        {
            if (!IsTargetable || !flight.Shoot()) return TapOutcome.Miss;
            Finish(true);
            return TapOutcome.Kill;
        }

        void Finish(bool shotDown)
        {
            Unreg();
            if (body != null) body.SetActive(false);
            if (explosionFx != null)
            {
                explosionFx.gameObject.SetActive(true);
                explosionFx.localScale = Vector3.zero;
            }
            fxTimer = 0f;
            Resolved?.Invoke(this, shotDown);
        }

        void UpdateFx(float dt)
        {
            fxTimer += dt;
            float dur = cfg != null ? cfg.grenadeExplosionTime : EnemyConfig.Fallback.grenadeExplosionTime;
            float size = cfg != null ? cfg.grenadeExplosionSize : EnemyConfig.Fallback.grenadeExplosionSize;
            float t = Mathf.Clamp01(fxTimer / dur);
            if (explosionFx != null) explosionFx.localScale = Vector3.one * (size * t);
            if (t >= 1f) Destroy(gameObject);
        }

        void Unreg()
        {
            if (registered) { registered = false; TargetRegistry.Unregister(this); }
        }

        void OnEnable()
        {
            if (flight != null && flight.IsFlying && !registered) { registered = true; TargetRegistry.Register(this); }
        }

        void OnDisable() { Abort(); }

        /// <summary>Huy luu dan dang bay (OnDisable goi; EditMode test goi truc tiep vi OnDisable khong chay o EditMode).</summary>
        public void Abort()
        {
            Unreg();
            // Bi tat/huy khi dang bay (wave Dismiss...): giai quyet sach, khong gay sat thuong, de wave khong ket.
            if (flight != null && flight.IsFlying && flight.Shoot())
                Resolved?.Invoke(this, false);
        }
    }
}
