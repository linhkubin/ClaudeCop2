using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.RankScore.Tests
{
    public class RankScoreTests
    {
        RankScoreConfig cfg;

        [SetUp] public void Setup() { cfg = RankScoreConfig.CreateRuntimeDefault(); RankScoreDecisionLog.Clear(); }
        [TearDown] public void Teardown() { Object.DestroyImmediate(cfg); }

        static PlayerStats Good() => new PlayerStats { Shots = 10, Hits = 10, Accuracy = 1f, AvgReactionTime = 0.7f, ReactionSamples = 10, DamageTaken = 0 };
        static PlayerStats Bad() => new PlayerStats { Shots = 10, Hits = 2, Misses = 8, Accuracy = 0.2f, AvgReactionTime = 2.0f, ReactionSamples = 2, DamageTaken = 3 };
        static PlayerStats Mid() => new PlayerStats { Shots = 10, Hits = 6, Misses = 4, Accuracy = 0.6f, AvgReactionTime = 1.4f, ReactionSamples = 6, DamageTaken = 1 };

        [Test]
        public void Offline_GoodPlayer_GetsShortReticle()
        {
            var a = OfflineRankScoreClient.AnswerReticle(Good(), cfg);
            Assert.AreEqual("short", a.Choice);
            Assert.GreaterOrEqual(a.Confidence, 0.6f);
            Assert.AreEqual(1f, a.Probabilities["short"] + a.Probabilities["normal"] + a.Probabilities["long"], 1e-4f);
        }

        [Test]
        public void Offline_BadPlayer_GetsLongReticle()
        {
            Assert.AreEqual("long", OfflineRankScoreClient.AnswerReticle(Bad(), cfg).Choice);
        }

        [Test]
        public void Offline_Deterministic_ViaClient()
        {
            var c = new OfflineRankScoreClient(cfg, Mid);
            RankScoreResponse r1 = null, r2 = null;
            var req = OfflineRankScoreClient.BuildReticleRequest(Mid());
            c.Ask(req, 1.5f, r => r1 = r); c.Ask(req, 1.5f, r => r2 = r);
            Assert.IsTrue(r1.Success);
            var a1 = r1.Answers["reticle_time"]; var a2 = r2.Answers["reticle_time"];
            Assert.AreEqual(a1.Choice, a2.Choice);
            Assert.AreEqual(a1.Confidence, a2.Confidence);
            Assert.AreEqual(3, req.Questions[0].Criteria.Count);
        }

        [Test]
        public void Policy_Good_Offline2s()
        {
            var d = RankScorePolicy.Decide(Good(), cfg);
            Assert.AreEqual(RankScoreDecisionSource.Offline, d.Source);
            Assert.AreEqual(2.0f, d.ReticleTime, 1e-4f);
        }

        [Test]
        public void Policy_NoData_LowConfidence_FallsBackToDefault()
        {
            var d = RankScorePolicy.Decide(default(PlayerStats), cfg);
            Assert.AreEqual(RankScoreDecisionSource.Default, d.Source);
            Assert.AreEqual(2.5f, d.ReticleTime, 1e-4f);
        }

        [Test]
        public void Policy_Timeout_Null_Noul_Disabled_AllDefault()
        {
            Assert.AreEqual(RankScoreDecisionSource.Default, RankScorePolicy.Decide(RankScoreResponse.Timeout(), Good(), cfg).Source);
            Assert.AreEqual(RankScoreDecisionSource.Default, RankScorePolicy.Decide((RankScoreResponse)null, Good(), cfg).Source);
            var noul = new RankScoreResponse { Success = true };
            noul.Answers["reticle_time"] = new RankScoreChoiceAnswer { IsNoul = true };
            Assert.AreEqual(RankScoreDecisionSource.Default, RankScorePolicy.Decide(noul, Good(), cfg).Source);
            cfg.enabled = false;
            var d = RankScorePolicy.Decide(Good(), cfg);
            Assert.AreEqual(RankScoreDecisionSource.Default, d.Source);
            Assert.AreEqual(2.5f, d.ReticleTime, 1e-4f);
        }

        [Test]
        public void DecisionLog_RecordsAndRaises()
        {
            int n = 0; System.Action<RankScoreDecision> h = _ => n++;
            RankScoreDecisionLog.DecisionMade += h;
            Assert.IsFalse(RankScoreDecisionLog.HasLast);
            RankScoreDecisionLog.Record(RankScorePolicy.Decide(Good(), cfg, 5f));
            Assert.IsTrue(RankScoreDecisionLog.HasLast);
            Assert.AreEqual(1, n);
            Assert.AreEqual("short", RankScoreDecisionLog.Last.Choice);
            RankScoreDecisionLog.DecisionMade -= h;
        }

        [Test]
        public void Tracker_ComputesStatsFromCoreEvents_AndResetsOnPhase()
        {
            var go = new GameObject("t");
            var t = go.AddComponent<PlayerStatsTracker>();
            t.Subscribe();
            Fire(new ShotResult { Outcome = TapOutcome.Kill, ReactionTime = 1f });
            Fire(new ShotResult { Outcome = TapOutcome.JusticeKill, ReactionTime = 0.5f });
            Fire(new ShotResult { Outcome = TapOutcome.Miss });
            Fire(new ShotResult { Outcome = TapOutcome.HostageHit });
            Fire(new ShotResult { Outcome = TapOutcome.Blocked });
            Fire(new ShotResult { Outcome = TapOutcome.Environment });
            GameEvents.RaisePlayerDamaged(DamageSource.EnemyShot, Vector3.zero, 2);
            var s = t.Snapshot();
            Assert.AreEqual(4, s.Shots); Assert.AreEqual(2, s.Hits); Assert.AreEqual(2, s.Misses);
            Assert.AreEqual(1, s.HostageHits); Assert.AreEqual(1, s.DamageTaken);
            Assert.AreEqual(0.5f, s.Accuracy, 1e-4f);
            Assert.AreEqual(0.75f, s.AvgReactionTime, 1e-4f);
            RailEvents.RaisePhaseStarted(1, "x");
            Assert.AreEqual(0, t.Snapshot().Shots);
            t.Unsubscribe();
            Fire(new ShotResult { Outcome = TapOutcome.Kill });
            Assert.AreEqual(0, t.Snapshot().Shots);
            Object.DestroyImmediate(go);
        }

        static void Fire(ShotResult r)
        {
            CombatEvents.RaiseShotFired(WeaponKind.Pistol, Vector2.zero);
            CombatEvents.RaiseShotResolved(r);
        }

        [Test]
        public void Tracker_Shotgun_CountsPerShot_NotPerTarget()
        {
            var go = new GameObject("t");
            var t = go.AddComponent<PlayerStatsTracker>();
            t.Subscribe();
            CombatEvents.RaiseShotFired(WeaponKind.Shotgun, Vector2.zero);
            for (int i = 0; i < 3; i++) CombatEvents.RaiseShotResolved(new ShotResult { Outcome = TapOutcome.Kill, ReactionTime = 1f });
            Fire(new ShotResult { Outcome = TapOutcome.Miss });
            var s = t.Snapshot();
            Assert.AreEqual(2, s.Shots); Assert.AreEqual(1, s.Hits); Assert.AreEqual(1, s.Misses);
            Assert.AreEqual(0.5f, s.Accuracy, 1e-4f);
            Assert.AreEqual(3, s.ReactionSamples);
            t.Unsubscribe();
            Object.DestroyImmediate(go);
        }
    }
}
