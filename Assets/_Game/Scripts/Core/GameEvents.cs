using System;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>Anh chup trang thai game de UI doc khi OnEnable.</summary>
    public struct GameSnapshot
    {
        public GameState State;
        public int Lives;
        public int MaxLives;
        public int Score;
        public int RevivesRemaining;
    }

    /// <summary>
    /// C15. Su kien game. Chi Game (GameManager/PlayerHealth) duoc Raise; UI, Camera (shake), Combat (reset combo), RankScore nghe.
    /// Current luon cap nhat truoc khi phat event.
    /// </summary>
    public static class GameEvents
    {
        static GameSnapshot current = new GameSnapshot { State = GameState.Title };
        public static GameSnapshot Current => current;

        /// <summary>(cur, max)</summary>
        public static event Action<int, int> LivesChanged;
        /// <summary>(source, worldPos, livesLeft)</summary>
        public static event Action<DamageSource, Vector3, int> PlayerDamaged;
        public static event Action<int> ScoreChanged;
        /// <summary>(points, worldPos, isJustice, multiplier)</summary>
        public static event Action<int, Vector3, bool, float> ScoreAwarded;
        public static event Action<GameState> GameStateChanged;
        /// <summary>So lan revive con lai.</summary>
        public static event Action<int> ReviveAvailabilityChanged;

        public static void RaiseLivesChanged(int cur, int max)
        {
            current.Lives = cur; current.MaxLives = max;
            LivesChanged?.Invoke(cur, max);
        }
        public static void RaisePlayerDamaged(DamageSource source, Vector3 pos, int livesLeft) => PlayerDamaged?.Invoke(source, pos, livesLeft);
        public static void RaiseScoreChanged(int total)
        {
            current.Score = total;
            ScoreChanged?.Invoke(total);
        }
        public static void RaiseScoreAwarded(int points, Vector3 pos, bool isJustice, float multiplier) => ScoreAwarded?.Invoke(points, pos, isJustice, multiplier);
        public static void RaiseGameStateChanged(GameState state)
        {
            current.State = state;
            GameStateChanged?.Invoke(state);
        }
        public static void RaiseReviveAvailabilityChanged(int remaining)
        {
            current.RevivesRemaining = remaining;
            ReviveAvailabilityChanged?.Invoke(remaining);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            current = new GameSnapshot { State = GameState.Title };
            LivesChanged = null; PlayerDamaged = null; ScoreChanged = null; ScoreAwarded = null;
            GameStateChanged = null; ReviveAvailabilityChanged = null;
        }
    }
}
