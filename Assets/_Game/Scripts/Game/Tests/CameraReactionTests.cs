using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Camera;

namespace ClaudeCop.Game.Tests
{
    /// <summary>CAM-LIVELY: logic thuan cua reaction "giat minh quay sang".</summary>
    public class CameraReactionTests
    {
        CameraFeelProfile P;
        CameraReaction R;

        [SetUp] public void Setup() { P = ScriptableObject.CreateInstance<CameraFeelProfile>(); R = new CameraReaction(P); }
        [TearDown] public void Teardown() { Object.DestroyImmediate(P); }

        [Test]
        public void Envelope_FastInHoldSlowOut()
        {
            Assert.AreEqual(0f, CameraReaction.Envelope(0f, 0.15f, 0.1f, 0.8f), 1e-5f);
            Assert.Greater(CameraReaction.Envelope(0.075f, 0.15f, 0.1f, 0.8f), 0.8f); // ease-out: da gan dinh o nua thoi gian vao
            Assert.AreEqual(1f, CameraReaction.Envelope(0.2f, 0.15f, 0.1f, 0.8f), 1e-5f);
            Assert.AreEqual(0.5f, CameraReaction.Envelope(0.25f + 0.4f, 0.15f, 0.1f, 0.8f), 1e-4f);
            Assert.AreEqual(0f, CameraReaction.Envelope(5f, 0.15f, 0.1f, 0.8f), 1e-5f);
        }

        [Test]
        public void SingleTarget_ReactsTowardIt_WithinLimits()
        {
            R.Notify(0f, 20f, 5f);
            R.Tick(P.reactGatherWindow + 0.01f, true, 1f);
            Assert.AreEqual(1, R.Count);
            Assert.Greater(R.LastYaw, 0f);
            Assert.LessOrEqual(Mathf.Abs(R.LastYaw), P.reactMaxYaw + 1e-4f);
            Assert.LessOrEqual(Mathf.Abs(R.LastPitch), P.reactMaxPitch + 1e-4f);
            R.Tick(P.reactGatherWindow + 0.01f + P.reactIn + 0.01f, true, 1f);
            Assert.Greater(R.Target.x, 0.9f * R.LastYaw);
            Assert.Greater(R.Target.z, 0f);
        }

        [Test]
        public void NegativeOffset_ReactsLeft()
        {
            R.Notify(0f, -15f, 0f);
            R.Tick(1f, true, 1f);
            Assert.Less(R.LastYaw, 0f);
        }

        [Test]
        public void SmallOffset_Ignored()
        {
            R.Notify(0f, 1f, 0.5f);
            R.Tick(1f, true, 1f);
            Assert.AreEqual(0, R.Count);
        }

        [Test]
        public void SeveralTargetsInWindow_MergeIntoOneTowardCentroid()
        {
            R.Notify(0f, 20f, 0f); R.Notify(0.05f, -4f, 0f); R.Notify(0.1f, 12f, 0f);
            R.Tick(0.05f, true, 1f);
            Assert.AreEqual(0, R.Count); // van trong cua so gop
            R.Tick(0.2f, true, 1f);
            Assert.AreEqual(1, R.Count);
            Assert.Greater(R.LastYaw, 0f);
        }

        [Test]
        public void Cooldown_BlocksSecondReaction()
        {
            R.Notify(0f, 20f, 0f); R.Tick(0.2f, true, 1f);
            R.Notify(0.5f, 20f, 0f); R.Tick(0.7f, true, 1f);
            Assert.AreEqual(1, R.Count);
            R.Notify(P.reactCooldown + 1f, 20f, 0f); R.Tick(P.reactCooldown + 1.3f, true, 1f);
            Assert.AreEqual(2, R.Count);
        }

        [Test]
        public void NotAllowed_DropsPending_NoReaction()
        {
            R.Notify(0f, 20f, 0f); R.Tick(0.05f, false, 1f);
            R.Tick(0.5f, true, 1f);
            Assert.AreEqual(0, R.Count);
        }

        [Test]
        public void ReduceMotionScaleZero_Disables()
        {
            R.Notify(0f, 20f, 0f); R.Tick(0.5f, true, 0f);
            Assert.AreEqual(0, R.Count);
            Assert.AreEqual(Vector3.zero, R.Target);
        }

        [Test]
        public void DisabledInProfile_NoReaction()
        {
            P.reactEnabled = false;
            R.Notify(0f, 20f, 0f); R.Tick(0.5f, true, 1f);
            Assert.AreEqual(0, R.Count);
        }

        [Test]
        public void ReturnsToZeroAfterFullDuration()
        {
            R.Notify(0f, 20f, 3f); R.Tick(0.2f, true, 1f);
            R.Tick(0.2f + P.reactIn + P.reactHold + P.reactOut + 0.1f, true, 1f);
            Assert.AreEqual(Vector3.zero, R.Target);
        }
    }
}
