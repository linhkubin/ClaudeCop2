using System;
using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Enemy;

namespace ClaudeCop.Jev.Tests
{
    public class JevDirectorTests
    {
        class FakeClient : IJevClient
        {
            public int Calls;
            public bool Immediate = true;
            public JevResponse Response;
            public Action<JevResponse> Stored;
            public JevCall Ask(JevRequest request, float timeoutSeconds, Action<JevResponse> onDone)
            {
                Calls++;
                var call = new JevCall();
                if (Immediate) { call.Complete(); onDone(Response); }
                else Stored = onDone;
                return call;
            }
        }

        JevConfig cfg;
        GameObject root;
        JevDirector director;
        EncounterWave wave;
        FakeClient fake;
        int applied; float appliedTime; EncounterWave appliedTo;

        static JevResponse Answer(string choice, float conf = 0.9f)
        {
            var r = new JevResponse { Success = true };
            r.Answers[OfflineJevClient.QuestionReticleTime] = new JevChoiceAnswer { Choice = choice, Confidence = conf };
            return r;
        }

        [SetUp]
        public void Setup()
        {
            JevDecisionLog.Clear();
            cfg = JevConfig.CreateRuntimeDefault();
            root = new GameObject("jev");
            director = root.AddComponent<JevDirector>();
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
            Assert.AreEqual(1, JevDecisionLog.Count);
        }

        [Test]
        public void MoveSegment_Null_DoesNothing()
        {
            director.OnMoveSegmentStarted(null);
            Assert.AreEqual(0, fake.Calls); Assert.AreEqual(0, applied);
        }

        [Test]
        public void JevDisabled_NoCallNoApply()
        {
            cfg.enabled = false;
            director.OnMoveSegmentStarted(wave);
            Assert.AreEqual(0, fake.Calls); Assert.AreEqual(0, applied);
        }

        [Test]
        public void Timeout_KeepsDefault_NotApplied()
        {
            fake.Response = JevResponse.Timeout();
            director.OnMoveSegmentStarted(wave);
            Assert.AreEqual(0, applied);
            Assert.AreEqual(JevDecisionSource.Default, JevDecisionLog.Last.Source);
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
    }
}
