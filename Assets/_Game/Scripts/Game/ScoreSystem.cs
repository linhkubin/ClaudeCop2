using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>Nghe ShotResolved (Kill/JusticeKill), cong diem, phat ScoreChanged/ScoreAwarded.</summary>
    public sealed class ScoreSystem : MonoBehaviour
    {
        [SerializeField] GameConfig config;

        public int Score { get; private set; }

        void OnEnable() { CombatEvents.ShotResolved += OnShotResolved; BlastEvents.Blasted += OnBlasted; }
        void OnDisable() { CombatEvents.ShotResolved -= OnShotResolved; BlastEvents.Blasted -= OnBlasted; }

        /// <summary>Public de test. Cong diem no 1 lan tai tam no; khong dung toi combo; khong phat con tin lan hai (Props da lam).</summary>
        public void OnBlasted(BlastReport b)
        {
            if (GameEvents.Current.State != GameState.Playing) return;
            int pts = ScoreCalculator.ComputeBlast(config, b, CombatEvents.Current.ComboMultiplier, out float mult);
            if (pts <= 0) return;
            Score += pts;
            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseScoreAwarded(pts, b.Center, false, mult);
        }

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
