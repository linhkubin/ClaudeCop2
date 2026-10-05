using System;
using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>C13. Nhan sat thuong nguoi choi: tru mang, bat tu, phat GameEvents. Het mang -> OutOfLives (GameManager nghe).</summary>
    public sealed class PlayerHealth : MonoBehaviour, IPlayerDamageReceiver
    {
        [SerializeField] GameConfig config;

        LifeTracker tracker;
        int bonusLives, armorPerStage;

        /// <summary>Phat khi mang ve 0 (sau LivesChanged va PlayerDamaged).</summary>
        public event Action OutOfLives;
        /// <summary>Giap do mot phat (khong mat mang). Tham so: giap con lai.</summary>
        public event Action<int> ArmorAbsorbed;
        public int Armor => Tracker.Armor;

        public int Lives => Tracker.Lives;
        public bool IsInvulnerable => Tracker.IsInvulnerable(Time.unscaledTime);

        LifeTracker Tracker => tracker ??= new LifeTracker(config.MaxLives + bonusLives, config.InvulnerableSeconds);

        void OnEnable() { PlayerDamageService.Register(this); RailEvents.PhaseStarted += OnPhaseStarted; }
        void OnDisable() { PlayerDamageService.Unregister(this); RailEvents.PhaseStarted -= OnPhaseStarted; }

        /// <summary>
        /// Trang bi tu man Home (goi truoc khi bat dau luot): extraLives = tim them (mu), armorEachStage = giap nap lai moi Stage (ao),
        /// startArmor = giap them mot lan cho ca luot (xem quang cao truoc tran).
        /// </summary>
        public void ConfigureLoadout(int extraLives, int armorEachStage, int startArmor)
        {
            bonusLives = Mathf.Max(0, extraLives);
            armorPerStage = Mathf.Max(0, armorEachStage);
            tracker = new LifeTracker(config.MaxLives + bonusLives, config.InvulnerableSeconds);
            tracker.AddArmor(startArmor);
            tracker.TopUpArmor(armorPerStage);
            GameEvents.RaiseLivesChanged(tracker.Lives, tracker.MaxLives);
        }

        void OnPhaseStarted(int index, string title) { if (armorPerStage > 0) Tracker.TopUpArmor(armorPerStage); }

        /// <summary>GameManager goi khi bat dau/restart/revive. lives &lt; 0 = day mang.</summary>
        public void ResetLives(int lives = -1)
        {
            Tracker.Reset(lives);
            GameEvents.RaiseLivesChanged(Tracker.Lives, Tracker.MaxLives);
        }

        public void GrantInvulnerability(float seconds) => Tracker.GrantInvulnerability(Time.unscaledTime, seconds);

        public void Damage(DamageSource source, Vector3 worldPosition)
        {
            if (GameEvents.Current.State != GameState.Playing) return;
            if (!Tracker.TryDamage(Time.unscaledTime))
            {
                if (Tracker.LastAbsorbed) ArmorAbsorbed?.Invoke(Tracker.Armor);
                return;
            }
            GameEvents.RaiseLivesChanged(Tracker.Lives, Tracker.MaxLives);
            GameEvents.RaisePlayerDamaged(source, worldPosition, Tracker.Lives);
            if (Tracker.IsDead) OutOfLives?.Invoke();
        }
    }
}
