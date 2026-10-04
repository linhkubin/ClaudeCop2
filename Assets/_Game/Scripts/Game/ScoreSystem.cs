using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>Nghe ShotResolved (Kill/JusticeKill), cong diem, phat ScoreChanged/ScoreAwarded.</summary>
    public sealed class ScoreSystem : MonoBehaviour
    {
        [SerializeField] GameConfig config;

        public int Score { get; private set; }

        void OnEnable() => CombatEvents.ShotResolved += OnShotResolved;
        void OnDisable() => CombatEvents.ShotResolved -= OnShotResolved;

        public void ResetScore()
        {
            Score = 0;
            GameEvents.RaiseScoreChanged(0);
        }

        void OnShotResolved(ShotResult r)
        {
            if (GameEvents.Current.State != GameState.Playing) return;
            int pts = ScoreCalculator.Compute(config, r, out bool justice, out float mult);
            if (pts <= 0) return;
            Score += pts;
            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseScoreAwarded(pts, r.WorldPoint, justice, mult);
        }
    }
}
