using System;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Combat
{
    /// <summary>
    /// Thung vu khi tap de nhat (Kind = Pickup). Tu dang ky TargetRegistry khi duoc kich hoat (OnEnable) hoac goi <see cref="Show"/>,
    /// bien mat sau khi nhat hoac het <c>visibleSeconds</c> (0 = o mai den khi bi tat). TapShooter doc <see cref="Weapon"/>,
    /// hoan lai vien dan cua phat nhat va Equip. EncounterWave (Enemy) chi can SetActive(true/false) GameObject nay.
    /// </summary>
    public sealed class WeaponPickup : MonoBehaviour, ITapTarget
    {
        [SerializeField] WeaponData weapon;
        [SerializeField] float aimHeight = 0.5f;
        [Tooltip("Tu bien mat sau bao lau neu khong nhat (s). 0 = khong tu bien mat.")]
        [SerializeField, Min(0f)] float visibleSeconds = 0f;
        [SerializeField] GameObject visual;
        [SerializeField] float spinDegPerSec = 90f;

        static int nextId = 1;
        int id;
        bool shown, registered, idAssigned;
        float shownAt;

        public WeaponData Weapon => weapon;
        /// <summary>Phat khi duoc nhat.</summary>
        public event Action<WeaponPickup> Collected;

        public int Id { get { EnsureId(); return id; } }
        public TargetKind Kind => TargetKind.Pickup;
        public bool IsTargetable => shown && !CombatPauseSignal.IsPaused;
        public Vector3 AimPoint => transform.position + Vector3.up * aimHeight;
        public bool HasJusticePoint => false;
        public Vector3 JusticePoint => AimPoint;
        public bool ShowsReticle => false;
        public float ReticleProgress => 0f;
        public float ExposedTime => shown ? Time.time - shownAt : 0f;

        void EnsureId() { if (!idAssigned) { idAssigned = true; id = nextId++; } }

        public void Setup(WeaponData data) { weapon = data; }

        public void Show()
        {
            EnsureId();
            shown = true;
            shownAt = Time.time;
            if (visual != null) visual.SetActive(true);
            if (!registered) { registered = true; TargetRegistry.Register(this); }
        }

        public void Hide()
        {
            shown = false;
            if (registered) { registered = false; TargetRegistry.Unregister(this); }
            if (visual != null) visual.SetActive(false);
        }

        public TapOutcome OnTapHit(ShotInfo shot, bool isJustice)
        {
            if (!IsTargetable) return TapOutcome.Miss;
            Hide();
            Collected?.Invoke(this);
            gameObject.SetActive(false);
            return TapOutcome.PickupCollected;
        }

        void OnEnable() { Show(); }
        void OnDisable() { Hide(); }

        void Update()
        {
            if (!shown) return;
            if (visual != null && spinDegPerSec != 0f) visual.transform.Rotate(0f, spinDegPerSec * Time.deltaTime, 0f, Space.World);
            if (visibleSeconds > 0f && !CombatPauseSignal.IsPaused && Time.time - shownAt >= visibleSeconds)
                gameObject.SetActive(false);
        }
    }
}
