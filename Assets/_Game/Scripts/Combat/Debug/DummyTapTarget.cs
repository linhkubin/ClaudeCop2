#if UNITY_EDITOR || DEVELOPMENT_BUILD
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Combat
{
    /// <summary>Muc tieu gia de thu TapShooter trong Sandbox. Khong dung trong gameplay that.</summary>
    public sealed class DummyTapTarget : MonoBehaviour, ITapTarget
    {
        [SerializeField] TargetKind kind = TargetKind.Enemy;
        [SerializeField] Transform justiceTransform;
        [SerializeField] float reticleDuration = 2.5f;
        [SerializeField] bool respawnAfterHit = true;

        static int nextId = 1;
        int id;
        float exposedAt;
        bool alive;
        MeshRenderer rend;

        public int Id => id;
        public TargetKind Kind => kind;
        public bool IsTargetable => alive;
        public Vector3 AimPoint => transform.position + Vector3.up;
        public bool HasJusticePoint => justiceTransform != null && kind == TargetKind.Enemy;
        public Vector3 JusticePoint => justiceTransform != null ? justiceTransform.position : AimPoint;
        public bool ShowsReticle => kind == TargetKind.Enemy;
        public float ExposedTime => Time.time - exposedAt;
        public float ReticleProgress => Mathf.Clamp01(ExposedTime / reticleDuration);

        void Awake() { id = nextId++; rend = GetComponentInChildren<MeshRenderer>(); }
        void OnEnable() { Expose(); }
        void OnDisable() { TargetRegistry.Unregister(this); }

        void Expose()
        {
            alive = true;
            exposedAt = Time.time;
            if (rend != null) rend.enabled = true;
            TargetRegistry.Register(this);
        }

        public TapOutcome OnTapHit(ShotInfo shot, bool isJustice)
        {
            alive = false;
            TargetRegistry.Unregister(this);
            if (rend != null) rend.enabled = false;
            if (respawnAfterHit) Invoke(nameof(Expose), 1.0f);
            Debug.Log("[Dummy] " + name + " hit, justice=" + isJustice + ", weapon=" + shot.Weapon);
            if (kind == TargetKind.Hostage) return TapOutcome.HostageHit;
            if (kind == TargetKind.Pickup) return TapOutcome.PickupCollected;
            return isJustice ? TapOutcome.JusticeKill : TapOutcome.Kill;
        }
    }
}
#endif
