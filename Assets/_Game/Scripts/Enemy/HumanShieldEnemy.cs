using System;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy
{
    /// <summary>
    /// Enemy giu con tin lam khien. AimPoint = dau; Justice point o tay cam sung (lo ra canh con tin).
    /// Tap: Justice -> JusticeKill (dau hang, tha con tin); trong ban kinh dau (px man hinh) -> Kill; con lai -> HostageHit
    /// (con tin bi thuong, enemy van song, vong tiep tuc). Than con tin duoc dang ky them nhu muc tieu Kind=Hostage de tap
    /// ngoai ban kinh chon quanh dau van bi phat. Enemy chet/dau hang -> con tin chay khoi khung (khong cong diem).
    /// </summary>
    public class HumanShieldEnemy : EnemyActor
    {
        [Header("Human shield")]
        [Tooltip("Dau enemy: vong target bam vao day.")]
        [SerializeField] Transform head;
        [Tooltip("Than con tin (capsule xanh) gan truoc than enemy.")]
        [SerializeField] Transform hostageBody;
        [Tooltip("Diem Justice (tay cam sung) lo ra canh con tin. Trong thi dung EnemyConfig.justiceOffset.")]
        [SerializeField] Transform justiceHandle;
        [Tooltip("Renderer con tin de nhuom do khi bi thuong (tuy chon).")]
        [SerializeField] Renderer hostageRenderer;

        ShieldHostageTarget proxy;
        bool released, wounded;

        /// <summary>Test/Sandbox: thay cach chieu world -> man hinh (mac dinh dung Camera.main).</summary>
        public Func<Vector3, Vector2?> Projector;
        /// <summary>Test: chieu rong man hinh (mac dinh Screen.width).</summary>
        public float ScreenWidthOverride = -1f;

        /// <summary>Phat khi tap trung than con tin (con tin bi thuong).</summary>
        public event Action<HumanShieldEnemy> HostageWounded;

        public bool IsHostageWounded => wounded;
        public bool IsHostageReleased => released;
        public Transform HostageBody => hostageBody;

        public override Vector3 AimPoint => head != null ? head.position : transform.position + Vector3.up * Cfg.shieldHeadHeight;
        public override bool HasJusticePoint => !IsDead;
        public override Vector3 JusticePoint => justiceHandle != null ? justiceHandle.position : base.JusticePoint;
        /// <summary>Human shield luon co Justice point; wave khong tat duoc.</summary>
        public override void SetJustice(bool on) { base.SetJustice(true); }

        protected override void Awake()
        {
            base.Awake();
            base.SetJustice(true);
        }

        public override TapOutcome OnTapHit(ShotInfo shot, bool isJustice)
        {
            if (IsDead || !IsTargetable) return TapOutcome.Miss;
            // Vu no (ScreenPosition = NaN, khong phai tap man hinh): coi shield nhu enemy thuong -> ha enemy.
            if (float.IsNaN(shot.ScreenPosition.x)) return base.OnTapHit(shot, false);
            Vector2? headScreen = Project(AimPoint);
            float radius = HumanShieldRules.ScaleRadius(Cfg.shieldHeadRadiusPx,
                ScreenWidthOverride > 0f ? ScreenWidthOverride : Screen.width, Cfg.uiReferenceWidth);
            switch (HumanShieldRules.Classify(isJustice, shot.ScreenPosition, headScreen, radius))
            {
                case ShieldTap.Justice: return base.OnTapHit(shot, true);
                case ShieldTap.Head: return base.OnTapHit(shot, false);
                default: return HitHostage();
            }
        }

        /// <summary>Tap trung than con tin: con tin bi thuong, enemy van song.</summary>
        public TapOutcome HitHostage()
        {
            if (IsDead || released) return TapOutcome.Miss;
            wounded = true;
            if (hostageRenderer != null)
            {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_BaseColor", Color.red);
                mpb.SetColor("_Color", Color.red);
                hostageRenderer.SetPropertyBlock(mpb);
            }
            HostageWounded?.Invoke(this);
            return TapOutcome.HostageHit;
        }

        Vector2? Project(Vector3 world)
        {
            if (Projector != null) return Projector(world);
            var cam = UnityEngine.Camera.main;
            if (cam == null) return null;
            Vector3 p = cam.WorldToScreenPoint(world);
            if (p.z <= 0f) return null;
            return new Vector2(p.x, p.y);
        }

        protected override void OnTargetsRegistered()
        {
            if (hostageBody == null || released) return;
            if (proxy == null) proxy = new ShieldHostageTarget(this);
            TargetRegistry.Register(proxy);
        }

        protected override void OnTargetsUnregistered()
        {
            if (proxy != null) TargetRegistry.Unregister(proxy);
        }

        protected override void OnKilled(bool justice)
        {
            if (proxy != null) TargetRegistry.Unregister(proxy);
            ReleaseHostage();
        }

        void ReleaseHostage()
        {
            if (released || hostageBody == null) return;
            released = true;
            // Tach khoi enemy (enemy nga/xoay) de con tin chay doc lap.
            hostageBody.SetParent(transform.parent, true);
            var runner = hostageBody.gameObject.GetComponent<ShieldHostageRunner>();
            if (runner == null) runner = hostageBody.gameObject.AddComponent<ShieldHostageRunner>();
            runner.Run(transform.right, Cfg.hostageRunSpeed, Cfg.hostageRunTime);
        }

        /// <summary>Muc tieu phu: than con tin (Kind = Hostage, khong vong target).</summary>
        sealed class ShieldHostageTarget : ITapTarget
        {
            readonly HumanShieldEnemy owner;
            readonly int id;
            static int nextId = 300000;
            public ShieldHostageTarget(HumanShieldEnemy o) { owner = o; id = nextId++; }
            public int Id => id;
            public TargetKind Kind => TargetKind.Hostage;
            public bool IsTargetable => owner != null && !owner.IsDead && owner.IsTargetable;
            public Vector3 AimPoint => owner.hostageBody != null ? owner.hostageBody.position : owner.transform.position;
            public bool HasJusticePoint => false;
            public Vector3 JusticePoint => AimPoint;
            public bool ShowsReticle => false;
            public float ReticleProgress => 0f;
            public float ExposedTime => owner != null ? owner.ExposedTime : 0f;
            public TapOutcome OnTapHit(ShotInfo shot, bool isJustice)
            {
                // Vu no (ScreenPosition NaN) bo qua than con tin cua shield: khong phat.
                if (float.IsNaN(shot.ScreenPosition.x)) return TapOutcome.Miss;
                return owner.HitHostage();
            }
        }
    }

    /// <summary>Con tin duoc tha: chay doc lap khoi khung roi tu huy. Dung khi CombatPauseSignal.IsPaused.</summary>
    public class ShieldHostageRunner : MonoBehaviour
    {
        Vector3 dir; float speed, time, timer;
        public void Run(Vector3 direction, float runSpeed, float runTime)
        {
            dir = direction.normalized; speed = runSpeed; time = runTime; timer = 0f;
        }
        void Update()
        {
            if (CombatPauseSignal.IsPaused) return;
            Tick(Time.deltaTime);
        }
        public void Tick(float dt)
        {
            timer += dt;
            transform.position += dir * (speed * dt);
            if (timer >= time) Destroy(gameObject);
        }
    }
}
