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

        /// <summary>Phat khi mang ve 0 (sau LivesChanged va PlayerDamaged).</summary>
        public event Action OutOfLives;

        public int Lives => Tracker.Lives;
        public bool IsInvulnerable => Tracker.IsInvulnerable(Time.unscaledTime);

        LifeTracker Tracker => tracker ??= new LifeTracker(config.MaxLives, config.InvulnerableSeconds);

        void OnEnable() => PlayerDamageService.Register(this);
        void OnDisable() => PlayerDamageService.Unregister(this);

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
            if (!Tracker.TryDamage(Time.unscaledTime)) return;
            GameEvents.RaiseLivesChanged(Tracker.Lives, Tracker.MaxLives);
            GameEvents.RaisePlayerDamaged(source, worldPosition, Tracker.Lives);
            if (Tracker.IsDead) OutOfLives?.Invoke();
        }
    }
}
