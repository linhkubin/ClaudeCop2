using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game.Tests
{
    public class W7ScoreTests
    {
        GameConfig cfg;

        [SetUp] public void Setup() => cfg = ScriptableObject.CreateInstance<GameConfig>();
        [TearDown] public void Teardown() { Object.DestroyImmediate(cfg); CombatEvents.RaiseComboChanged(0, 1f); }

        [Test] public void Blast_Kills_TimesPointsTimesCombo()
        {
            int pts = ScoreCalculator.ComputeBlast(cfg, new BlastReport { EnemiesKilled = 3 }, 2f, out float m);
            Assert.AreEqual(600, pts); Assert.AreEqual(2f, m);
        }

        [Test] public void Blast_NoKills_Zero() =>
            Assert.AreEqual(0, ScoreCalculator.ComputeBlast(cfg, new BlastReport { EnemiesKilled = 0, HostagesHit = 1 }, 3f, out _));

        [Test] public void Grenade_Kill_Is50_NoEarlyBonus_ComboApplies()
        {
            var r = new ShotResult { Outcome = TapOutcome.Kill, TargetKind = TargetKind.Grenade, ReticleProgress = 0f, ComboMultiplier = 1f };
            Assert.AreEqual(50, ScoreCalculator.Compute(cfg, r, out bool j, out _));
            Assert.IsFalse(j);
            r.ComboMultiplier = 2f;
            Assert.AreEqual(100, ScoreCalculator.Compute(cfg, r, out _, out _));
        }

        [Test] public void ScoreSystem_Blast_AwardsOnce_AtCenter_NotJustice_ComboUntouched()
        {
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            CombatEvents.RaiseComboChanged(2, 2f);
            var go = new GameObject("score");
            var ss = go.AddComponent<ScoreSystem>();
            typeof(ScoreSystem).GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ss, cfg);
            int n = 0, pts = 0; Vector3 pos = default; bool just = true;
            System.Action<int, Vector3, bool, float> h = (p, v, j, m) => { n++; pts = p; pos = v; just = j; };
            GameEvents.ScoreAwarded += h;
            ss.OnBlasted(new BlastReport { Center = new Vector3(1, 2, 3), EnemiesKilled = 2 });
            GameEvents.ScoreAwarded -= h;
            Assert.AreEqual(1, n); Assert.AreEqual(400, pts); Assert.AreEqual(new Vector3(1, 2, 3), pos); Assert.IsFalse(just);
            Assert.AreEqual(400, ss.Score);
            Assert.AreEqual(2, CombatEvents.Current.ComboStreak);
            Assert.AreEqual(2f, CombatEvents.Current.ComboMultiplier);
            Object.DestroyImmediate(go);
        }

        [Test] public void Explosion_CostsLife_AndRespectsInvulnerability()
        {
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            var go = new GameObject("hp");
            var hp = go.AddComponent<PlayerHealth>();
            typeof(PlayerHealth).GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(hp, cfg);
            hp.ResetLives();
            hp.Damage(DamageSource.Explosion, Vector3.zero);
            Assert.AreEqual(cfg.MaxLives - 1, hp.Lives);
            hp.Damage(DamageSource.Explosion, Vector3.zero); // dang bat tu
            Assert.AreEqual(cfg.MaxLives - 1, hp.Lives);
            Object.DestroyImmediate(go);
        }
    }
}
