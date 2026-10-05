using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.RankScore.Tests
{
    public class ScoreRankTests
    {
        RankScoreConfig cfg;

        [SetUp] public void Setup() { cfg = RankScoreConfig.CreateRuntimeDefault(); RankScoreDecisionLog.Clear(); ScoreRankBoard.Clear(); }
        [TearDown] public void Teardown() { Object.DestroyImmediate(cfg); }

        static PlayerStats Make(float acc, float react = 0.8f, int dmg = 0, int hostage = 0, int revive = 0, int shots = 20)
            => new PlayerStats
            {
                Shots = shots, Hits = Mathf.RoundToInt(acc * shots), Accuracy = acc,
                AvgReactionTime = react, ReactionSamples = 10, DamageTaken = dmg, HostageHits = hostage, RevivesUsed = revive
            };

        // ---------- Rank ----------

        [Test]
        public void Rank_Perfect_IsS_AndNoWeakness()
        {
            var r = ScoreRankEvaluator.Evaluate(Make(1f), cfg);
            Assert.AreEqual(ScoreRank.S, r.Rank);
            Assert.AreEqual(ScoreWeakness.None, r.Weakness);
            Assert.AreEqual("Hoàn hảo!", r.WeaknessText);
        }

        [Test]
        public void Rank_Boundaries_AreInclusive()
        {
            var s = Make(0.5f, 1.2f, 0);
            float skill = OfflineRankScoreClient.Skill(s, cfg);
            cfg.rankSMinSkill = 2f; // S khong dat
            cfg.rankAMinSkill = skill;
            Assert.AreEqual(ScoreRank.A, ScoreRankEvaluator.Evaluate(s, cfg).Rank);
            cfg.rankAMinSkill = skill + 0.001f; cfg.rankBMinSkill = skill;
            Assert.AreEqual(ScoreRank.B, ScoreRankEvaluator.Evaluate(s, cfg).Rank);
            cfg.rankBMinSkill = skill + 0.001f;
            Assert.AreEqual(ScoreRank.C, ScoreRankEvaluator.Evaluate(s, cfg).Rank);
        }

        [Test]
        public void Rank_S_NeedsSkillThreshold_NoDamage()
        {
            var s = Make(1f);
            float skill = OfflineRankScoreClient.Skill(s, cfg);
            cfg.rankSMinSkill = skill;
            Assert.AreEqual(ScoreRank.S, ScoreRankEvaluator.Evaluate(s, cfg).Rank);
            cfg.rankSMinSkill = skill + 0.001f;
            Assert.AreEqual(ScoreRank.A, ScoreRankEvaluator.Evaluate(s, cfg).Rank);
        }

        [Test]
        public void Rank_LostLife_NotS()
        {
            var r = ScoreRankEvaluator.Evaluate(Make(1f, 0.8f, 1), cfg);
            Assert.AreEqual(ScoreRank.A, r.Rank); // skill 0.45+0.3+0.125 = 0.875 nhung mat mang => khong S
        }

        [Test]
        public void Rank_Revive_CapsAtB()
        {
            Assert.AreEqual(ScoreRank.B, ScoreRankEvaluator.Evaluate(Make(1f, 0.8f, 0, 0, 1), cfg).Rank);
            // Da thap hon B thi giu nguyen C
            Assert.AreEqual(ScoreRank.C, ScoreRankEvaluator.Evaluate(Make(0.1f, 2f, 3, 0, 1), cfg).Rank);
        }

        [Test]
        public void Rank_Poor_IsC()
        {
            Assert.AreEqual(ScoreRank.C, ScoreRankEvaluator.Evaluate(Make(0.2f, 2f, 3, 2), cfg).Rank);
        }

        // ---------- Weakness ----------

        [Test]
        public void Weakness_PicksLowestComponent()
        {
            Assert.AreEqual(ScoreWeakness.Accuracy, ScoreRankEvaluator.PickWeakness(Make(0.3f), cfg));
            Assert.AreEqual(ScoreWeakness.Reaction, ScoreRankEvaluator.PickWeakness(Make(1f, 1.9f), cfg));
            Assert.AreEqual(ScoreWeakness.Damage, ScoreRankEvaluator.PickWeakness(Make(1f, 0.8f, 2), cfg));
            Assert.AreEqual(ScoreWeakness.Hostage, ScoreRankEvaluator.PickWeakness(Make(1f, 0.8f, 0, 3), cfg));
        }

        [Test]
        public void Weakness_Text_MatchesSpec()
        {
            Assert.AreEqual("Bắn trượt nhiều — ngắm kỹ hơn", ScoreRankEvaluator.TextFor(ScoreWeakness.Accuracy));
            Assert.AreEqual("Phản xạ chậm — bắn sớm khi vòng còn xanh", ScoreRankEvaluator.TextFor(ScoreWeakness.Reaction));
            Assert.AreEqual("Mất nhiều mạng — ưu tiên kẻ địch vòng đỏ", ScoreRankEvaluator.TextFor(ScoreWeakness.Damage));
            Assert.AreEqual("Bắn trúng con tin — quan sát trước khi bắn", ScoreRankEvaluator.TextFor(ScoreWeakness.Hostage));
        }

        [Test]
        public void Weakness_None_WhenAllGood()
        {
            Assert.AreEqual(ScoreWeakness.None, ScoreRankEvaluator.PickWeakness(Make(0.85f, 0.9f), cfg));
        }

        [Test]
        public void Result_CopiesStats()
        {
            var r = ScoreRankEvaluator.Evaluate(new PlayerStats { Shots = 4, Hits = 2, Accuracy = 0.5f, AvgReactionTime = 1f, ReactionSamples = 2, DamageTaken = 1, HostageHits = 2, RevivesUsed = 1, BlastKills = 3 }, cfg);
            Assert.AreEqual(0.5f, r.Accuracy, 1e-4f); Assert.AreEqual(1, r.DamageTaken);
            Assert.AreEqual(2, r.HostageHits); Assert.AreEqual(1, r.RevivesUsed); Assert.AreEqual(3, r.BlastKills);
        }

        // ---------- Board / Director ----------

        [Test]
        public void Board_PublishRaisesAndStoresLast_ClearResets()
        {
            int n = 0; System.Action<ScoreRankResult> h = _ => n++;
            ScoreRankBoard.RankEvaluated += h;
            Assert.IsFalse(ScoreRankBoard.HasResult);
            ScoreRankBoard.Publish(new ScoreRankResult { Rank = ScoreRank.B });
            Assert.IsTrue(ScoreRankBoard.HasResult); Assert.AreEqual(ScoreRank.B, ScoreRankBoard.Last.Rank); Assert.AreEqual(1, n);
            ScoreRankBoard.Clear();
            Assert.IsFalse(ScoreRankBoard.HasResult);
            ScoreRankBoard.RankEvaluated -= h;
        }

        [Test]
        public void Director_LevelCompleted_PublishesRankAndLogs()
        {
            var go = new GameObject("rankScore");
            var t = go.AddComponent<PlayerStatsTracker>(); t.Subscribe();
            var d = go.AddComponent<RankScoreDirector>(); d.Configure(cfg, t); d.Subscribe();
            Fire(new ShotResult { Outcome = TapOutcome.Kill, ReactionTime = 0.8f });
            RailEvents.RaiseLevelCompleted();
            Assert.IsTrue(ScoreRankBoard.HasResult);
            Assert.AreEqual(ScoreRank.S, ScoreRankBoard.Last.Rank);
            Assert.AreEqual("rank", RankScoreDecisionLog.Last.Question);
            d.Unsubscribe(); t.Unsubscribe();
            Object.DestroyImmediate(go);
        }

        [Test]
        public void NewRun_ClearsLevelStatsAndRank()
        {
            var go = new GameObject("t");
            var t = go.AddComponent<PlayerStatsTracker>(); t.Subscribe();
            Fire(new ShotResult { Outcome = TapOutcome.Kill, ReactionTime = 1f });
            ScoreRankBoard.Publish(new ScoreRankResult());
            GameEvents.RaiseGameStateChanged(GameState.Playing); // tu Title => luot moi
            Assert.AreEqual(0, t.SnapshotLevel().Shots);
            Assert.IsFalse(ScoreRankBoard.HasResult);
            t.Unsubscribe(); Object.DestroyImmediate(go);
        }

        // ---------- Tracker ----------

        static void Fire(ShotResult r)
        {
            CombatEvents.RaiseShotFired(WeaponKind.Pistol, Vector2.zero);
            CombatEvents.RaiseShotResolved(r);
        }

        [Test]
        public void Tracker_Environment_NotMiss_CountedSeparately_MissCounts()
        {
            var go = new GameObject("t");
            var t = go.AddComponent<PlayerStatsTracker>(); t.Subscribe();
            Fire(new ShotResult { Outcome = TapOutcome.Environment });
            Fire(new ShotResult { Outcome = TapOutcome.Environment });
            Fire(new ShotResult { Outcome = TapOutcome.Miss });
            var s = t.Snapshot();
            Assert.AreEqual(1, s.Shots); Assert.AreEqual(1, s.Misses); Assert.AreEqual(2, s.EnvironmentShots);
            Assert.AreEqual(2, t.SnapshotLevel().EnvironmentShots);
            t.Unsubscribe(); Object.DestroyImmediate(go);
        }

        [Test]
        public void Tracker_Blast_CountsSeparately_NotAccuracy_HostageAddsToHostageHits()
        {
            var go = new GameObject("t");
            var t = go.AddComponent<PlayerStatsTracker>(); t.Subscribe();
            Fire(new ShotResult { Outcome = TapOutcome.Kill, ReactionTime = 1f });
            float before = t.Snapshot().Accuracy;
            BlastEvents.Raise(new BlastReport { EnemiesKilled = 3, HostagesHit = 1, Radius = 3f });
            var s = t.Snapshot();
            Assert.AreEqual(3, s.BlastKills); Assert.AreEqual(1, s.BlastHostageHits); Assert.AreEqual(1, s.HostageHits);
            Assert.AreEqual(1, s.Shots); Assert.AreEqual(1, s.Hits); Assert.AreEqual(before, s.Accuracy, 1e-5f);
            t.Unsubscribe(); Object.DestroyImmediate(go);
        }

        [Test]
        public void Tracker_LevelAccumulatesAcrossPhases_PhaseResets()
        {
            var go = new GameObject("t");
            var t = go.AddComponent<PlayerStatsTracker>(); t.Subscribe();
            RailEvents.RaisePhaseStarted(0, "a");
            Fire(new ShotResult { Outcome = TapOutcome.Kill, ReactionTime = 1f });
            RailEvents.RaisePhaseStarted(1, "b");
            Fire(new ShotResult { Outcome = TapOutcome.Miss });
            Assert.AreEqual(1, t.Snapshot().Shots);
            var l = t.SnapshotLevel();
            Assert.AreEqual(2, l.Shots); Assert.AreEqual(1, l.PhaseIndex);
            t.Unsubscribe(); Object.DestroyImmediate(go);
        }

        [Test]
        public void Tracker_CountsRevive()
        {
            var go = new GameObject("t");
            var t = go.AddComponent<PlayerStatsTracker>(); t.Subscribe();
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            GameEvents.RaiseGameStateChanged(GameState.RevivePrompt);
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            Assert.AreEqual(1, t.SnapshotLevel().RevivesUsed);
            t.Unsubscribe(); Object.DestroyImmediate(go);
        }

        // ---------- wave_preset / weapon_drop ----------

        static PlayerStats Lv(float acc, float react, int dmg, int hostage, int phase = 1, int shots = 10)
        {
            var s = Make(acc, react, dmg, hostage, 0, shots); s.PhaseIndex = phase; return s;
        }

        [Test]
        public void WavePreset_Rules()
        {
            Assert.AreEqual("intense", OfflineRankScoreClient.AnswerWavePreset(Lv(1f, 0.8f, 0, 0), cfg).Choice);
            Assert.AreEqual("calm", OfflineRankScoreClient.AnswerWavePreset(Lv(0.1f, 2f, 3, 0), cfg).Choice);
            // skill ~0.64: 0.45*0.75+0.3*1+0.25*1 = 0.8875 -> dung acc thap hon
            var hh = Lv(0.5f, 1.2f, 0, 0, 1); // 0.225+0.2+0.25 = 0.675
            Assert.AreEqual("hostage_heavy", OfflineRankScoreClient.AnswerWavePreset(hh, cfg).Choice);
            hh.PhaseIndex = 0;
            Assert.AreEqual("standard", OfflineRankScoreClient.AnswerWavePreset(hh, cfg).Choice); // Phase 1: chua co hostage_heavy
            hh.PhaseIndex = 1; hh.HostageHits = 1; // skill 0.525 => standard
            Assert.AreEqual("standard", OfflineRankScoreClient.AnswerWavePreset(hh, cfg).Choice);
        }

        [Test]
        public void WeaponDrop_Rules()
        {
            Assert.AreEqual("machinegun", OfflineRankScoreClient.AnswerWeaponDrop(Lv(0.1f, 2f, 3, 0), cfg).Choice);
            Assert.AreEqual("shotgun", OfflineRankScoreClient.AnswerWeaponDrop(Lv(0.5f, 1.2f, 0, 0), cfg).Choice); // 0.675
            Assert.AreEqual("none", OfflineRankScoreClient.AnswerWeaponDrop(Lv(1f, 0.8f, 0, 0), cfg).Choice);
        }

        [Test]
        public void NewQuestions_Noul_WhenNotEnoughData()
        {
            var few = Lv(1f, 0.8f, 0, 0, 1, 2);
            Assert.IsTrue(OfflineRankScoreClient.AnswerWavePreset(few, cfg).IsNoul);
            Assert.IsTrue(OfflineRankScoreClient.AnswerWeaponDrop(few, cfg).IsNoul);
            Assert.AreEqual(RankScoreDecisionSource.Default, RankScorePolicy.DecideWavePreset(few, cfg).Source);
            Assert.AreEqual("default", RankScorePolicy.DecideWeaponDrop(few, cfg).Choice);
        }

        [Test]
        public void NewQuestions_LowConfidence_Default()
        {
            var resp = new RankScoreResponse { Success = true };
            resp.Answers[OfflineRankScoreClient.QuestionWavePreset] = new RankScoreChoiceAnswer { Choice = "calm", Confidence = 0.3f };
            resp.Answers[OfflineRankScoreClient.QuestionWeaponDrop] = new RankScoreChoiceAnswer { Choice = "bogus", Confidence = 0.9f };
            Assert.AreEqual(RankScoreDecisionSource.Default, RankScorePolicy.DecideWavePreset(resp, Lv(1f, 0.8f, 0, 0), cfg).Source);
            Assert.AreEqual(RankScoreDecisionSource.Default, RankScorePolicy.DecideWeaponDrop(resp, Lv(1f, 0.8f, 0, 0), cfg).Source);
        }

        [Test]
        public void NewQuestions_Disabled_Default()
        {
            cfg.enabled = false;
            Assert.AreEqual(RankScoreDecisionSource.Default, RankScorePolicy.DecideWavePreset(Lv(1f, 0.8f, 0, 0), cfg).Source);
            Assert.AreEqual(RankScoreDecisionSource.Default, RankScorePolicy.DecideWeaponDrop(Lv(1f, 0.8f, 0, 0), cfg).Source);
        }

        [Test]
        public void Client_AnswersAllThreeQuestions_Deterministic_ProbabilitiesSumToOne()
        {
            var c = new OfflineRankScoreClient(cfg, () => Lv(1f, 0.8f, 0, 0), () => Lv(1f, 0.8f, 0, 0));
            RankScoreResponse r = null;
            var req = OfflineRankScoreClient.BuildWaveRequest(Lv(1f, 0.8f, 0, 0), Lv(1f, 0.8f, 0, 0));
            c.Ask(req, 1.5f, x => r = x);
            Assert.AreEqual(3, r.Answers.Count);
            foreach (var key in new[] { "wave_preset", "weapon_drop" })
            {
                float sum = 0f; foreach (var p in r.Answers[key].Probabilities.Values) sum += p;
                Assert.AreEqual(1f, sum, 1e-4f);
            }
            Assert.AreEqual(4, req.Questions[1].Criteria.Count);
            Assert.AreEqual(3, req.Questions[2].Criteria.Count);
        }

        // ---------- RankScoreDirector ap dung ----------

        [Test]
        public void Director_AppliesPresetAndWeaponDrop_OnlyWhenPickupSpawnExists()
        {
            var go = new GameObject("rankScore");
            var d = go.AddComponent<RankScoreDirector>();
            var level = Lv(1f, 0.8f, 0, 0);
            d.Configure(cfg, null, new OfflineRankScoreClient(cfg, () => level, () => level));
            string preset = null, drop = null;
            d.PresetApplier = (w, c) => preset = c;
            d.WeaponDropApplier = (w, c) => drop = c;
            d.ReticleApplier = (w, t) => { };
            var wave = new GameObject("wave").AddComponent<ClaudeCop.Enemy.EncounterWave>();

            d.OnMoveSegmentStarted(wave); // W7: dot dau luon mac dinh
            Assert.IsNull(preset);
            d.HasPickupSpawn = _ => false;
            d.OnMoveSegmentStarted(wave);
            Assert.AreEqual("intense", preset); Assert.IsNull(drop);

            d.HasPickupSpawn = _ => true;
            d.OnMoveSegmentStarted(wave);
            Assert.AreEqual("none", drop);

            preset = drop = null;
            cfg.enabled = false;
            d.OnMoveSegmentStarted(wave);
            Assert.IsNull(preset); Assert.IsNull(drop);

            Object.DestroyImmediate(wave.gameObject); Object.DestroyImmediate(go);
        }
    }
}
