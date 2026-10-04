using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Combat
{
    /// <summary>
    /// Giu combo, phat CombatEvents.ComboChanged. TapShooter goi <see cref="RegisterShot"/> TRUOC khi phat ShotResolved
    /// de ShotResult.ComboMultiplier phan anh he so sau phat. Bi ban (GameEvents.PlayerDamaged) thi reset.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ComboSystem : MonoBehaviour
    {
        [SerializeField] CombatConfig config;
        ComboTracker tracker;

        public int Streak => Tracker.Streak;
        public float Multiplier => Tracker.Multiplier;

        ComboTracker Tracker => tracker ?? (tracker = new ComboTracker(
            config != null ? config.ComboMaxMultiplier : 5, config != null ? config.ComboHitsPerStep : 1));

        void OnEnable()
        {
            GameEvents.PlayerDamaged += OnPlayerDamaged;
            ResetCombo();
        }

        void OnDisable() { GameEvents.PlayerDamaged -= OnPlayerDamaged; }

        void OnPlayerDamaged(DamageSource s, Vector3 p, int livesLeft) { ResetCombo(); }

        /// <summary>Cap nhat combo theo ket qua mot phat; tra ve he so SAU phat.</summary>
        public float RegisterShot(TapOutcome outcome, bool envKeepsCombo = false)
        {
            var t = Tracker;
            if (t.Apply(outcome, envKeepsCombo)) CombatEvents.RaiseComboChanged(t.Streak, t.Multiplier);
            return t.Multiplier;
        }

        public void ResetCombo()
        {
            var t = Tracker;
            bool changed = t.Reset();
            if (changed || CombatEvents.Current.ComboStreak != 0 || CombatEvents.Current.ComboMultiplier != 1f)
                CombatEvents.RaiseComboChanged(0, 1f);
        }
    }
}
