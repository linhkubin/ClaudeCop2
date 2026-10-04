using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Jev
{
    /// <summary>Tinh accuracy / phan ung / mang mat trong Phase hien tai tu event Core. Reset khi PhaseStarted.</summary>
    public class PlayerStatsTracker : MonoBehaviour
    {
        int shots, hits, misses, hostageHits, damage, reactionSamples;
        float reactionSum;
        bool subscribed;
        bool shotOpen; // true tu ShotFired den ket qua dau tien cua phat do (Shotgun phat nhieu ShotResolved/phat)

        public PlayerStats Snapshot()
        {
            return new PlayerStats
            {
                Shots = shots, Hits = hits, Misses = misses, HostageHits = hostageHits,
                Accuracy = shots > 0 ? (float)hits / shots : 0f,
                AvgReactionTime = reactionSamples > 0 ? reactionSum / reactionSamples : 0f,
                ReactionSamples = reactionSamples,
                DamageTaken = damage
            };
        }

        public void ResetPhase()
        {
            shots = hits = misses = hostageHits = damage = reactionSamples = 0;
            reactionSum = 0f;
        }

        void OnEnable() { Subscribe(); }
        void OnDisable() { Unsubscribe(); }

        /// <summary>Public de test EditMode (OnEnable khong chay o edit mode).</summary>
        public void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;
            CombatEvents.ShotFired += OnFired;
            CombatEvents.ShotResolved += OnShot;
            GameEvents.PlayerDamaged += OnDamaged;
            RailEvents.PhaseStarted += OnPhaseStarted;
        }

        public void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            CombatEvents.ShotFired -= OnFired;
            CombatEvents.ShotResolved -= OnShot;
            GameEvents.PlayerDamaged -= OnDamaged;
            RailEvents.PhaseStarted -= OnPhaseStarted;
        }

        void OnFired(WeaponKind weapon, Vector2 screenPos) { shotOpen = true; }

        void OnShot(ShotResult r)
        {
            // Dem theo phat ban (F-204): ket qua dau tien cua mot phat tinh 1 shot; ket qua them (Shotgun) chi cong hit/phan ung.
            bool first = shotOpen;
            shotOpen = false;
            switch (r.Outcome)
            {
                case TapOutcome.Kill:
                case TapOutcome.JusticeKill:
                    if (first) { shots++; hits++; }
                    if (r.ReactionTime > 0f) { reactionSum += r.ReactionTime; reactionSamples++; }
                    break;
                case TapOutcome.HostageHit:
                    hostageHits++;
                    if (first) { shots++; misses++; }
                    break;
                case TapOutcome.Miss:
                    if (first) { shots++; misses++; }
                    break;
                // Environment (ban vat the/tuong), PickupCollected, Blocked: khong tinh la ban truot (plan muc 14)
            }
        }

        void OnDamaged(DamageSource source, Vector3 pos, int livesLeft) { damage++; }

        void OnPhaseStarted(int index, string title) { ResetPhase(); }
    }
}
