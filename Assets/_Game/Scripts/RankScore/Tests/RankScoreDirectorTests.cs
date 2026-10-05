using System;
using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Enemy;

namespace ClaudeCop.RankScore.Tests
{
    public class RankScoreDirectorTests
    {
        class FakeClient : IRankScoreClient
        {
            public int Calls;
            public bool Immediate = true;
            public RankScoreResponse Response;
            public Action<RankScoreResponse> Stored;
            public RankScoreCall Ask(RankScoreRequest request, float timeoutSeconds, Action<RankScoreResponse> onDone)
            {
                Calls++;
                var call = new RankScoreCall();
                if (Immediate) { call.Complete(); onDone(Response); }
                else Stored = onDone;
                return call;
            }
        }

        RankScoreConfig cfg;
        GameObject root;
        RankScoreDirector director;
        EncounterWave wave;
        FakeClient fake;
        int applied; float appliedTime; EncounterWave appliedTo;

        static RankScoreResponse Answer(string choice, float conf = 0.9f)
        {
            var r = new RankScoreResponse { Success = true };
            r.Answers[OfflineRankScoreClient.QuestionReticleTime] = new RankScoreChoiceAnswer { Choice = choice, Confidence = conf };
            return r;
        }

        [SetUp]
        public void Setup()
        {
            RankScoreDecisionLog.Clear();
            cfg = RankScoreConfig.CreateRuntimeDefault();
            root = new GameObject("rankScore");
            director = root.AddComponent<RankScoreDirector>();
            wave = new GameObject("wave").AddComponent<EncounterWave>();
            fake = new FakeClient { Response = Answer("long") };
            director.Configure(cfg, null, fake);
            applied = 0; appliedTo = null;
            director.ReticleApplier = (w, t) => { applied++; appliedTo = w; appliedTime = t; };
        }

        [TearDown]
        public void Teardown()
        {
            director.Unsubscribe();
            UnityEngine.Object.DestroyImmediate(wave.gameObject);
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(cfg);
        }

        [Test]
        public void MoveSegment_ToWave_AppliesReticleBeforeBegin()
        {
            director.OnMoveSegmentStarted(wave);
            Assert.AreEqual(1, applied);
            Assert.AreSame(wave, appliedTo);
            Assert.AreEqual(cfg.TimeFor("long"), appliedTime, 1e-4f);
            Assert.IsFalse(wave.IsActive);
            Assert.AreEqual(1, RankScoreDecisionLog.Count);
        }

        [Test]
        public void MoveSegment_Null_DoesNothing()
        {
            director.OnMoveSegmentStarted(null);
            Assert.AreEqual(0, fake.Calls); Assert.AreEqual(0, applied);
        }

        [Test]
        public void RankScoreDisabled_NoCallNoApply()
        {
            cfg.enabled = false;
            director.OnMoveSegmentStarted(wave);
            Assert.AreEqual(0, fake.Calls); Assert.AreEqual(0, applied);
        }

        [Test]
        public void Timeout_KeepsDefault_NotApplied()
        {
            fake.Response = RankScoreResponse.Timeout();
            director.OnMoveSegmentStarted(wave);
            Assert.AreEqual(0, applied);
            Assert.AreEqual(RankScoreDecisionSource.Default, RankScoreDecisionLog.Last.Source);
        }

        [Test]
        public void LowConfidence_KeepsDefault()
        {
            fake.Response = Answer("short", 0.1f);
            director.OnMoveSegmentStarted(wave);
            Assert.AreEqual(0, applied);
        }

        [Test]
        public void LateResponse_AfterWaveBegan_NotApplied()
        {
            fake.Immediate = false;
            director.OnMoveSegmentStarted(wave);
            wave.Begin();
            fake.Stored(Answer("long"));
            Assert.AreEqual(0, applied);
        }

        [Test]
        public void RailEvent_TriggersDirector()
        {
            director.Subscribe();
            RailEvents.RaiseMoveSegmentStarted(wave);
            Assert.AreEqual(1, fake.Calls);
            Assert.AreEqual(1, applied);
        }

        [Test]
        public void NewMoveSegment_CancelsPendingCall()
        {
            fake.Immediate = false;
            director.OnMoveSegmentStarted(wave);
            var first = fake.Stored;
            director.OnMoveSegmentStarted(wave);
            first(Answer("long")); // phan hoi cu cua cuoc goi bi huy
            Assert.AreEqual(0, applied);
        }
    
        // ---------- W7: wave_preset / weapon_drop ----------

        RankScoreResponse Full(string preset, string drop, bool noul = false)
        {
            var r = Answer("long");
            r.Answers[OfflineRankScoreClient.QuestionWavePreset] = new RankScoreChoiceAnswer { Choice = preset, Confidence = 0.9f, IsNoul = noul };
            r.Answers[OfflineRankScoreClient.QuestionWeaponDrop] = new RankScoreChoiceAnswer { Choice = drop, Confidence = 0.9f, IsNoul = noul };
            return r;
        }

        [Test]
        public void W7_PresetAndDrop_AppliedFromSecondWave()
        {
            string preset = null, drop = null;
            director.PresetApplier = (w, c) => preset = c;
            director.WeaponDropApplier = (w, c) => drop = c;
            director.HasPickupSpawn = w => true;
            fake.Response = Full(RankScoreConfig.PresetIntense, RankScoreConfig.DropShotgun);
            director.ResetRun();
            director.OnMoveSegmentStarted(wave); // dot dau: mac dinh
            Assert.IsNull(preset); Assert.IsNull(drop);
            director.OnMoveSegmentStarted(wave);
            Assert.AreEqual(RankScoreConfig.PresetIntense, preset);
            Assert.AreEqual(RankScoreConfig.DropShotgun, drop);
        }

        [Test]
        public void W7_Noul_DoesNotApplyPresetOrDrop()
        {
            string preset = null, drop = null;
            director.PresetApplier = (w, c) => preset = c;
            director.WeaponDropApplier = (w, c) => drop = c;
            director.HasPickupSpawn = w => true;
            fake.Response = Full(RankScoreConfig.PresetIntense, RankScoreConfig.DropShotgun, true);
            director.ResetRun();
            director.OnMoveSegmentStarted(wave);
            director.OnMoveSegmentStarted(wave);
            Assert.IsNull(preset); Assert.IsNull(drop);
        }

        [Test]
        public void W7_Drop_NotAppliedWhenNoPickupSpawn()
        {
            string drop = null;
            director.PresetApplier = (w, c) => { };
            director.WeaponDropApplier = (w, c) => drop = c;
            director.HasPickupSpawn = w => false;
            fake.Response = Full(RankScoreConfig.PresetCalm, RankScoreConfig.DropNone);
            director.ResetRun();
            director.OnMoveSegmentStarted(wave);
            director.OnMoveSegmentStarted(wave);
            Assert.IsNull(drop);
        }

        [Test]
        public void W7_DefaultApply_PresetAndNoneDrop_UseEncounterWaveApi()
        {
            var preset = ScriptableObject.CreateInstance<EnemyPreset>();
            preset.reticleTime = 1.7f;
            director.GetType().GetField("presetIntense", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(director, preset);
            director.HasPickupSpawn = w => true;
            fake.Response = Full(RankScoreConfig.PresetIntense, RankScoreConfig.DropNone);
            director.ResetRun();
            director.OnMoveSegmentStarted(wave);
            Assert.DoesNotThrow(() => director.OnMoveSegmentStarted(wave));
            UnityEngine.Object.DestroyImmediate(preset);
        }
}
}
