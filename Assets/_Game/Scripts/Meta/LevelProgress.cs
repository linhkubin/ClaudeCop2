using System;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.RankScore;

namespace ClaudeCop.Meta
{
    /// <summary>Ket qua mot level vua xong (thang hoac thua): xu, huy hieu, rank.</summary>
    public struct LevelResult
    {
        public int Level;            // 0-based
        public bool Won;
        public int Score;            // diem trong level nay
        public int Rank;             // 0 = S ... 3 = C; -1 = khong co (thua / chua co rank)
        public int ScoreCoins, WinBonus, NoDamageBonus, JusticeBonus, ComboBonus, RadioBonus;
        public int Coins;            // tong xu da cong
        public int Badges;           // huy hieu nhan duoc (lan dau rank S)
        public bool Doubled;
    }

    /// <summary>Thong ke trong mot level de tinh thuong.</summary>
    public struct LevelRunStats
    {
        public int Score, DamageTaken, JusticeKills;
        public float MaxCombo;
    }

    /// <summary>Ham thuan: tinh thuong va ghi vao ho so.</summary>
    public static class LevelRewards
    {
        public static LevelResult Compute(int level, bool won, LevelRunStats s, MetaCatalog c, float coinBonus)
        {
            var r = new LevelResult { Level = level, Won = won, Score = Mathf.Max(0, s.Score), Rank = -1 };
            r.ScoreCoins = r.Score / Mathf.Max(1, c.ScorePerCoin);
            if (won)
            {
                r.WinBonus = c.WinBonus;
                r.NoDamageBonus = s.DamageTaken == 0 ? c.NoDamageBonus : 0;
                r.JusticeBonus = s.JusticeKills * c.PerJustice;
                r.ComboBonus = s.MaxCombo >= c.ComboBonusAt - 0.01f ? c.ComboBonus : 0;
            }
            int baseCoins = r.ScoreCoins + r.WinBonus + r.NoDamageBonus + r.JusticeBonus + r.ComboBonus;
            r.RadioBonus = Mathf.RoundToInt(baseCoins * Mathf.Max(0f, coinBonus));
            r.Coins = baseCoins + r.RadioBonus;
            return r;
        }

        /// <summary>Ghi ket qua vao ho so: xu, so lan choi/thang/thua, diem va rank tot nhat, huy hieu lan dau rank S.</summary>
        public static LevelResult Apply(ProfileData p, MetaCatalog c, LevelResult r)
        {
            var rec = p.Level(r.Level, true);
            if (r.Won) rec.wins++; else rec.losses++;
            if (r.Score > rec.bestScore) rec.bestScore = r.Score;
            if (r.Won && r.Rank >= 0 && (rec.bestRank < 0 || r.Rank < rec.bestRank)) rec.bestRank = r.Rank;
            if (r.Won && r.Rank == 0 && !rec.gotS)
            {
                rec.gotS = true;
                r.Badges = c.BadgesPerFirstS;
                p.badges += r.Badges;
            }
            p.coins += r.Coins;
            return r;
        }

        /// <summary>Rank den sau ket qua (thu tu su kien): cap nhat rank tot nhat + huy hieu cho level da ghi.</summary>
        public static LevelResult ApplyLateRank(ProfileData p, MetaCatalog c, LevelResult r, int rank)
        {
            if (!r.Won || rank < 0 || r.Rank >= 0) return r;
            r.Rank = rank;
            var rec = p.Level(r.Level, true);
            if (rec.bestRank < 0 || rank < rec.bestRank) rec.bestRank = rank;
            if (rank == 0 && !rec.gotS)
            {
                rec.gotS = true;
                r.Badges += c.BadgesPerFirstS;
                p.badges += c.BadgesPerFirstS;
            }
            return r;
        }

        /// <summary>Xem quang cao x2: cong them (AdMultiplier-1) lan xu cua level, chi mot lan.</summary>
        public static bool TryDouble(ProfileData p, MetaCatalog c, ref LevelResult r)
        {
            if (r.Doubled || r.Coins <= 0) return false;
            int extra = r.Coins * (Mathf.Max(1, c.AdMultiplier) - 1);
            p.coins += extra;
            r.Coins += extra;
            r.Doubled = true;
            return true;
        }
    }

    /// <summary>
    /// Theo doi level dang choi trong scene gameplay: dem diem/mat mau/Justice/combo moi level; thang (StageCompleted/LevelCompleted)
    /// hoac thua (GameOver) thi ghi ho so (xu, thang/thua, rank tot nhat, huy hieu rank S) va phat <see cref="Recorded"/>.
    /// Level hien tai = GameCommands.SelectedLevel, +1 moi lan Continue. Game.MetaBootstrap tu gan vao scene gameplay.
    /// </summary>
    public sealed class LevelProgressTracker : MonoBehaviour
    {
        public static event Action<LevelResult> Recorded;
        public static bool HasLast { get; private set; }
        public static LevelResult Last { get; private set; }

        public float CoinBonus { get; set; }

        int level = -1;
        LevelRunStats stats;
        int scoreBase;
        bool playing, awaitingRank;
        int pendingRank = -1;
        LevelResult lastRecorded;

        public int CurrentLevel => level;

        void OnEnable()
        {
            GameEvents.GameStateChanged += OnState;
            GameEvents.PlayerDamaged += OnDamaged;
            CombatEvents.ShotResolved += OnShot;
            CombatEvents.ComboChanged += OnCombo;
            RailEvents.StageCompleted += OnStageCompleted;
            RailEvents.LevelCompleted += OnLevelCompleted;
            GameCommands.ContinueRequested += OnContinue;
            ScoreRankBoard.RankEvaluated += OnRank;
        }

        void OnDisable()
        {
            GameEvents.GameStateChanged -= OnState;
            GameEvents.PlayerDamaged -= OnDamaged;
            CombatEvents.ShotResolved -= OnShot;
            CombatEvents.ComboChanged -= OnCombo;
            RailEvents.StageCompleted -= OnStageCompleted;
            RailEvents.LevelCompleted -= OnLevelCompleted;
            GameCommands.ContinueRequested -= OnContinue;
            ScoreRankBoard.RankEvaluated -= OnRank;
        }

        void OnState(GameState s)
        {
            if (s == GameState.Playing && !playing)
            {
                playing = true;
                BeginLevel(GameCommands.SelectedLevel);
            }
            else if (s == GameState.GameOver && playing)
            {
                playing = false;
                Record(false);
            }
        }

        void BeginLevel(int index)
        {
            level = Mathf.Max(0, index);
            stats = default;
            scoreBase = GameEvents.Current.Score;
            pendingRank = -1;
            awaitingRank = false;
            var p = PlayerProfile.Data;
            p.Level(level, true).plays++;
            PlayerProfile.Save();
        }

        void OnDamaged(DamageSource src, Vector3 pos, int livesLeft) { if (playing) stats.DamageTaken++; }

        void OnShot(ShotResult r) { if (playing && r.Outcome == TapOutcome.JusticeKill) stats.JusticeKills++; }

        void OnCombo(int streak, float mult) { if (playing && mult > stats.MaxCombo) stats.MaxCombo = mult; }

        void OnStageCompleted(int phaseIndex, string title) { if (playing) Record(true); }

        void OnLevelCompleted() { if (playing) { Record(true); playing = false; } }

        void OnContinue()
        {
            if (!playing) return;
            BeginLevel(level + 1);
        }

        void OnRank(ScoreRankResult r)
        {
            int rank = (int)r.Rank;
            if (awaitingRank)
            {
                // Rank den sau khi da ghi ket qua.
                awaitingRank = false;
                lastRecorded = LevelRewards.ApplyLateRank(PlayerProfile.Data, PlayerProfile.Catalog, lastRecorded, rank);
                PlayerProfile.Save();
                Publish(lastRecorded);
            }
            else pendingRank = rank; // rank den truoc: dung khi ghi
        }

        void Record(bool won)
        {
            stats.Score = GameEvents.Current.Score - scoreBase;
            var c = PlayerProfile.Catalog;
            var r = LevelRewards.Compute(level, won, stats, c, CoinBonus);
            if (won && pendingRank >= 0) r.Rank = pendingRank;
            r = LevelRewards.Apply(PlayerProfile.Data, c, r);
            awaitingRank = won && r.Rank < 0;
            pendingRank = -1;
            PlayerProfile.Save();
            lastRecorded = r;
            Publish(r);
        }

        static void Publish(LevelResult r)
        {
            Last = r; HasLast = true;
            Recorded?.Invoke(r);
        }

        /// <summary>Xem quang cao x2 cho ket qua gan nhat (UI goi sau khi quang cao xong).</summary>
        public static bool TryDoubleLast()
        {
            if (!HasLast) return false;
            var r = Last;
            if (!LevelRewards.TryDouble(PlayerProfile.Data, PlayerProfile.Catalog, ref r)) return false;
            PlayerProfile.Save();
            Publish(r);
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Recorded = null; HasLast = false; Last = default; }
    }
}
