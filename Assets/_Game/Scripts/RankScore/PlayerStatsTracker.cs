using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.RankScore
{
    /// <summary>
    /// Thong ke theo Phase (reset khi PhaseStarted) va CA MAN (cong don qua cac Phase, reset khi bat dau luot moi) tu event Core.
    /// Ngu nghia: Miss (ke ca trung tuong) = ban truot; Environment (trung vat IShootable) = KHONG truot, dem rieng;
    /// no (BlastEvents) dem rieng, khong cong Hits/Accuracy, con tin trung no cong vao HostageHits.
    /// </summary>
    public class PlayerStatsTracker : MonoBehaviour
    {
        struct Counters
        {
            public int Shots, Hits, Misses, HostageHits, Damage, ReactionSamples, Env, BlastKills, BlastHostage, Revives;
            public float ReactionSum;

            public PlayerStats ToStats(int phaseIndex) => new PlayerStats
            {
                Shots = Shots, Hits = Hits, Misses = Misses, HostageHits = HostageHits,
                Accuracy = Shots > 0 ? (float)Hits / Shots : 0f,
                AvgReactionTime = ReactionSamples > 0 ? ReactionSum / ReactionSamples : 0f,
                ReactionSamples = ReactionSamples,
                DamageTaken = Damage,
                EnvironmentShots = Env,
                BlastKills = BlastKills,
                BlastHostageHits = BlastHostage,
                RevivesUsed = Revives,
                PhaseIndex = phaseIndex
            };
        }

        Counters phase, level;
        int phaseIndex;
        GameState prevState = GameState.Title;
        bool subscribed;
        bool shotOpen; // true tu ShotFired den ket qua dau tien cua phat do (Shotgun phat nhieu ShotResolved/phat)

        /// <summary>Thong ke Phase hien tai.</summary>
        public PlayerStats Snapshot() => phase.ToStats(phaseIndex);

        /// <summary>Thong ke CA MAN (cong don qua cac Phase).</summary>
        public PlayerStats SnapshotLevel() => level.ToStats(phaseIndex);

        public void ResetPhase() { phase = default; }

        /// <summary>Bat dau luot moi: xoa thong ke ca man, Phase va ket qua rank cu.</summary>
        public void ResetLevel()
        {
            level = default; phase = default; phaseIndex = 0; shotOpen = false;
            ScoreRankBoard.Clear();
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
            GameEvents.GameStateChanged += OnGameState;
            BlastEvents.Blasted += OnBlast;
            RailEvents.PhaseStarted += OnPhaseStarted;
            GameCommands.ContinueRequested += ResetLevel; // Continue sang level ke: moi level co rank rieng
        }

        public void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            CombatEvents.ShotFired -= OnFired;
            CombatEvents.ShotResolved -= OnShot;
            GameEvents.PlayerDamaged -= OnDamaged;
            GameEvents.GameStateChanged -= OnGameState;
            BlastEvents.Blasted -= OnBlast;
            RailEvents.PhaseStarted -= OnPhaseStarted;
            GameCommands.ContinueRequested -= ResetLevel;
        }

        void OnFired(WeaponKind weapon, Vector2 screenPos) { shotOpen = true; }

        void OnShot(ShotResult r)
        {
            // Dem theo phat ban (F-204): ket qua dau tien cua mot phat tinh 1 shot; ket qua them (Shotgun) chi cong hit/phan ung.
            bool first = shotOpen;
            shotOpen = false;
            Apply(ref phase, r, first);
            Apply(ref level, r, first);
        }

        static void Apply(ref Counters c, ShotResult r, bool first)
        {
            switch (r.Outcome)
            {
                case TapOutcome.Kill:
                case TapOutcome.JusticeKill:
                    if (first) { c.Shots++; c.Hits++; }
                    if (r.ReactionTime > 0f) { c.ReactionSum += r.ReactionTime; c.ReactionSamples++; }
                    break;
                case TapOutcome.HostageHit:
                    c.HostageHits++;
                    if (first) { c.Shots++; c.Misses++; }
                    break;
                case TapOutcome.Miss: // ke ca trung tuong/khong trung gi
                    if (first) { c.Shots++; c.Misses++; }
                    break;
                case TapOutcome.Environment: // trung vat IShootable: khong phai ban truot
                    if (first) c.Env++;
                    break;
                // PickupCollected, Blocked: khong tinh
            }
        }

        void OnBlast(BlastReport b)
        {
            phase.BlastKills += b.EnemiesKilled; level.BlastKills += b.EnemiesKilled;
            phase.BlastHostage += b.HostagesHit; level.BlastHostage += b.HostagesHit;
            phase.HostageHits += b.HostagesHit; level.HostageHits += b.HostagesHit;
        }

        void OnDamaged(DamageSource source, Vector3 pos, int livesLeft) { phase.Damage++; level.Damage++; }

        void OnGameState(GameState state)
        {
            if (state == GameState.Playing)
            {
                if (prevState == GameState.RevivePrompt) { phase.Revives++; level.Revives++; }
                else ResetLevel(); // vao Playing tu Title/GameOver/Win = luot moi
            }
            prevState = state;
        }

        void OnPhaseStarted(int index, string title)
        {
            if (index == 0) ResetLevel(); // Phase dau = luot moi (du khong co GameManager)
            ResetPhase();
            phaseIndex = index;
        }
    }
}
