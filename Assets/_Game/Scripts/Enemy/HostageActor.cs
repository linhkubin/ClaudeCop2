using System;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy
{
    /// <summary>
    /// Con tin (dan thuong, capsule xanh): lo ra tu cho nap nhu enemy nhung KHONG co vong target. Tap trung -> HostageHit
    /// (Combat tru mang qua PlayerDamageService) va con tin bien mat. Khong bi ban thi tu roi di sau EnemyConfig.hostageExposeTime.
    /// Pivot o chan. (Ten HostageActor de khong trung TargetKind.Hostage.)
    /// </summary>
    public class HostageActor : MonoBehaviour, ITapTarget
    {
        [SerializeField] EnemyConfig config;
        [Tooltip("Vi tri lo ra (con 'Peek'). Neu trong thi dung peekLocalOffset.")]
        [SerializeField] Transform peekPoint;
        [SerializeField] Vector3 peekLocalOffset = new Vector3(0f, 0f, 1.2f);

        static int nextId = 100000;
        EnemyConfig Cfg => config != null ? config : EnemyConfig.Fallback;
        HostageBrain brain;
        Vector3 hidePos, peekPos;
        Quaternion hideRot, peekRot;
        bool positionsCached, registered, initialized;
        int id;

        /// <summary>Phat khi con tin bi ban (vi tri world).</summary>
        public event Action<HostageActor, Vector3> Shot;
        /// <summary>Phat khi con tin tu roi di (khong bi ban) hoac bi huy bo.</summary>
        public event Action<HostageActor> Left;

        public HostageState State => brain != null ? brain.State : HostageState.Hidden;
        public bool IsFinished => brain != null && brain.IsFinished;
        public bool IsActivated => brain != null && brain.IsActivated;

        public int Id => id;
        public TargetKind Kind => TargetKind.Hostage;
        public bool IsTargetable => brain != null && brain.IsTargetable && !CombatPauseSignal.IsPaused;
        public Vector3 AimPoint => transform.position + Vector3.up * (Cfg.hostageAimHeight);
        public bool HasJusticePoint => false;
        public Vector3 JusticePoint => AimPoint;
        public bool ShowsReticle => false;
        public float ReticleProgress => 0f;
        public float ExposedTime => brain != null ? brain.ExposedTime : 0f;

        void Awake() { EnsureInit(); }

        void EnsureInit()
        {
            if (initialized) return;
            initialized = true;
            id = nextId++;
            CachePositions();
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

        /// <summary>Doi config cho con tin dat san (chi khi chua kich hoat).</summary>
        public void ApplyConfig(EnemyConfig cfg)
        {
            EnsureInit();
            if (cfg == null || IsActivated) return;
            config = cfg;
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
            var c = Cfg;
            brain = new HostageBrain(
                c.peekDuration, c.hostageExposeTime, c.retreatDuration);
            brain.ExposeStarted = OnExposeStarted;
            brain.ExposeEnded = OnExposeEnded;
        }

        /// <summary>Bat dau lo ra (EncounterWave goi).</summary>
        public void Activate()
        {
            EnsureInit();
            brain.Activate();
        }

        /// <summary>Huy bo con tin (dot da xong): bien mat ngay, huy dang ky.</summary>
        public void Dismiss()
        {
            if (brain != null && !brain.IsFinished)
            {
                brain.Dismiss();
                Left?.Invoke(this);
            }
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (CombatPauseSignal.IsPaused) return;
            Tick(Time.deltaTime);
        }

        /// <summary>Tien state machine dt giay va cap nhat vi tri (Update goi; test goi truc tiep).</summary>
        public void Tick(float dt)
        {
            if (brain == null || brain.IsFinished || !brain.IsActivated) return;
            brain.Tick(dt);
            float t = brain.PeekT;
            transform.SetPositionAndRotation(Vector3.Lerp(hidePos, peekPos, t), Quaternion.Slerp(hideRot, peekRot, t));
            if (brain.State == HostageState.Left)
            {
                Left?.Invoke(this);
                gameObject.SetActive(false);
            }
        }

        void OnExposeStarted()
        {
            if (!registered) { registered = true; TargetRegistry.Register(this); }
        }

        void OnExposeEnded()
        {
            if (registered) { registered = false; TargetRegistry.Unregister(this); }
        }

        public TapOutcome OnTapHit(ShotInfo shot, bool isJustice)
        {
            if (brain == null || !IsTargetable) return TapOutcome.Miss;
            Vector3 pos = transform.position;
            brain.Shoot();   // ExposeEnded -> huy dang ky
            Shot?.Invoke(this, pos);
            gameObject.SetActive(false);   // bi ban thi bien mat
            return TapOutcome.HostageHit;
        }

        void OnEnable()
        {
            // Bat lai giua luc Exposed: dang ky lai de van ban duoc (F-103).
            if (brain != null && brain.State == HostageState.Exposed && !registered)
            {
                registered = true;
                TargetRegistry.Register(this);
            }
        }

        void OnDisable()
        {
            if (registered) { registered = false; TargetRegistry.Unregister(this); }
        }
    }
}
