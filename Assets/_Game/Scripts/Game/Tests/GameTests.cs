using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game.Tests
{
    public class GameTests
    {
        GameConfig cfg;

        [SetUp] public void Setup() => cfg = ScriptableObject.CreateInstance<GameConfig>();
        [TearDown] public void Teardown() => Object.DestroyImmediate(cfg);

        static ShotResult Shot(TapOutcome o, float progress, float combo = 1f, int hits = 1) =>
            new ShotResult { Outcome = o, ReticleProgress = progress, ComboMultiplier = combo, TargetsHit = hits };

        [Test] public void Kill_AtProgress1_Is100() =>
            Assert.AreEqual(100, ScoreCalculator.Compute(cfg, Shot(TapOutcome.Kill, 1f), out _, out _));

        [Test] public void Kill_AtProgress0_Is200() =>
            Assert.AreEqual(200, ScoreCalculator.Compute(cfg, Shot(TapOutcome.Kill, 0f), out _, out _));

        [Test] public void Kill_AtProgressHalf_Is150() =>
            Assert.AreEqual(150, ScoreCalculator.Compute(cfg, Shot(TapOutcome.Kill, 0.5f), out _, out _));

        [Test] public void Justice_Green_Is450()
        {
            Assert.AreEqual(450, ScoreCalculator.Compute(cfg, Shot(TapOutcome.JusticeKill, 0.2f), out bool j, out _));
            Assert.IsTrue(j);
        }

        [Test] public void Justice_NotGreen_Is300() =>
            Assert.AreEqual(300, ScoreCalculator.Compute(cfg, Shot(TapOutcome.JusticeKill, 0.5f), out _, out _));

        [Test] public void Miss_And_HostageHit_Score0()
        {
            Assert.AreEqual(0, ScoreCalculator.Compute(cfg, Shot(TapOutcome.Miss, 0f), out _, out _));
            Assert.AreEqual(0, ScoreCalculator.Compute(cfg, Shot(TapOutcome.HostageHit, 0f), out _, out _));
        }

        [Test] public void Combo_Streak3_Multiplier3()
        {
            int pts = ScoreCalculator.Compute(cfg, Shot(TapOutcome.Kill, 1f, 3f), out _, out float m);
            Assert.AreEqual(300, pts);
            Assert.AreEqual(3f, m);
        }

        [Test] public void Shotgun_MultiHit_DoesNotMultiply() =>
            Assert.AreEqual(100, ScoreCalculator.Compute(cfg, Shot(TapOutcome.Kill, 1f, 1f, 3), out _, out _));

        [Test] public void Life_DamageThenInvulnerable()
        {
            var t = new LifeTracker(3, 0.5f);
            Assert.IsTrue(t.TryDamage(10f));
            Assert.AreEqual(2, t.Lives);
            Assert.IsFalse(t.TryDamage(10.3f));
            Assert.AreEqual(2, t.Lives);
            Assert.IsTrue(t.TryDamage(10.6f));
            Assert.AreEqual(1, t.Lives);
        }

        [Test] public void Life_DiesAtZero_NoMoreDamage()
        {
            var t = new LifeTracker(1, 0.5f);
            Assert.IsTrue(t.TryDamage(0f));
            Assert.IsTrue(t.IsDead);
            Assert.IsFalse(t.TryDamage(5f));
            Assert.AreEqual(0, t.Lives);
        }

        [Test] public void Flow_OutOfLives_GameOver_WhenReviveDisabled()
        {
            var f = new GameFlow(); f.Begin(1);
            Assert.AreEqual(GameState.GameOver, f.OnOutOfLives());
            Assert.IsTrue(f.IsEnded);
        }

        [Test] public void Flow_OutOfLives_RevivePrompt_WhenEnabled_ThenAccept()
        {
            var f = new GameFlow { ReviveEnabled = true }; f.Begin(1);
            Assert.AreEqual(GameState.RevivePrompt, f.OnOutOfLives());
            Assert.IsTrue(f.AcceptRevive());
            Assert.AreEqual(GameState.Playing, f.State);
            Assert.AreEqual(GameState.GameOver, f.OnOutOfLives());
        }

        [Test] public void Flow_LevelCompleted_Win_OnlyFromPlaying()
        {
            var f = new GameFlow();
            Assert.AreEqual(GameState.Title, f.OnLevelCompleted());
            f.Begin(0);
            Assert.AreEqual(GameState.Win, f.OnLevelCompleted());
            Assert.AreEqual(GameState.Win, f.OnOutOfLives());
        }

        [Test] public void Config_Defaults_T304()
        {
            Assert.AreEqual(1.5f, cfg.InvulnerableSeconds, 1e-4f);
            Assert.AreEqual(1.0f, cfg.ReviveGraceSeconds, 1e-4f);
            Assert.AreEqual(1, cfg.RevivesPerRun);
        }

        [Test] public void Life_Invulnerable1_5s_ThreeEnemiesNeedMoreThanOneSecond()
        {
            var t = new LifeTracker(3, 1.5f);
            Assert.IsTrue(t.TryDamage(0f));
            Assert.IsFalse(t.TryDamage(1.4f));   // 3 enemy ban cung luc: chi mat 1 tim
            Assert.IsTrue(t.TryDamage(1.6f));
            Assert.AreEqual(1, t.Lives);
        }

        [Test] public void Flow_Revive_OnlyOncePerRun()
        {
            var f = new GameFlow { ReviveEnabled = true }; f.Begin(1);
            Assert.AreEqual(GameState.RevivePrompt, f.OnOutOfLives());
            Assert.IsTrue(f.AcceptRevive());
            Assert.AreEqual(0, f.RevivesRemaining);
            Assert.AreEqual(GameState.GameOver, f.OnOutOfLives()); // het luot -> thang GameOver
            Assert.IsFalse(f.AcceptRevive());
        }

        [Test] public void Flow_Revive_Decline_GameOver()
        {
            var f = new GameFlow { ReviveEnabled = true }; f.Begin(1);
            f.OnOutOfLives();
            Assert.AreEqual(GameState.GameOver, f.DeclineRevive());
            Assert.AreEqual(1, f.RevivesRemaining); // chua dung luot
            Assert.IsFalse(f.AcceptRevive());
        }

        [Test] public void Flow_Revive_ZeroRevives_GoesStraightToGameOver()
        {
            var f = new GameFlow { ReviveEnabled = false }; f.Begin(0);
            Assert.AreEqual(GameState.GameOver, f.OnOutOfLives());
            f = new GameFlow { ReviveEnabled = true }; f.Begin(0);
            Assert.AreEqual(GameState.GameOver, f.OnOutOfLives());
        }

        [Test] public void Flow_Decline_IgnoredOutsidePrompt()
        {
            var f = new GameFlow { ReviveEnabled = true }; f.Begin(1);
            Assert.AreEqual(GameState.Playing, f.DeclineRevive());
            Assert.IsFalse(f.AcceptRevive());
        }

        [Test] public void Flow_Revive_GraceInvulnerability_ThenDamageAgain()
        {
            var f = new GameFlow { ReviveEnabled = true }; f.Begin(1);
            var t = new LifeTracker(3, 1.5f);
            t.TryDamage(0f); t.TryDamage(2f); t.TryDamage(4f);
            Assert.IsTrue(t.IsDead);
            Assert.AreEqual(GameState.RevivePrompt, f.OnOutOfLives());
            Assert.IsTrue(f.AcceptRevive());
            t.Reset(); t.GrantInvulnerability(10f, 1.0f);   // GameManager: ResetLives + GrantInvulnerability(grace)
            Assert.AreEqual(3, t.Lives);
            Assert.IsFalse(t.TryDamage(10.9f));
            Assert.IsTrue(t.TryDamage(11.1f));
            Assert.AreEqual(2, t.Lives);
        }

        [Test] public void Flow_ReturnToTitle_FromAnyState_ThenBeginAgain()
        {
            var f = new GameFlow { ReviveEnabled = true }; f.Begin(1);
            f.OnOutOfLives();
            Assert.AreEqual(GameState.Title, f.ReturnToTitle());
            Assert.AreEqual(0, f.RevivesRemaining);
            Assert.AreEqual(GameState.Title, f.OnOutOfLives());
            f.Begin(1);
            Assert.AreEqual(GameState.Playing, f.State);
            Assert.AreEqual(1, f.RevivesRemaining);
        }

        [Test] public void Session_PendingStart_ConsumedOnce()
        {
            GameSession.RequestStartOnLoad();
            Assert.IsTrue(GameSession.ConsumePendingStart());
            Assert.IsFalse(GameSession.ConsumePendingStart());
        }
    }
}
