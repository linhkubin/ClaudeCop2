using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.Core.Tests
{
    public class CoreTests
    {
        class FakeTarget : ITapTarget
        {
            public int Id => 1; public TargetKind Kind => TargetKind.Enemy; public bool IsTargetable => true;
            public Vector3 AimPoint => Vector3.zero; public bool HasJusticePoint => false; public Vector3 JusticePoint => Vector3.zero;
            public bool ShowsReticle => true; public float ReticleProgress => 0; public float ExposedTime => 0;
            public TapOutcome OnTapHit(ShotInfo s, bool j) => TapOutcome.Kill;
        }

        [Test]
        public void PauseSignal_CountsPushPop()
        {
            int changes = 0; bool last = false;
            System.Action<bool> h = v => { changes++; last = v; };
            CombatPauseSignal.Changed += h;
            CombatPauseSignal.Push("a"); CombatPauseSignal.Push("b");
            Assert.IsTrue(CombatPauseSignal.IsPaused);
            Assert.AreEqual(1, changes);
            CombatPauseSignal.Pop("a");
            Assert.IsTrue(CombatPauseSignal.IsPaused);
            CombatPauseSignal.Pop("b");
            Assert.IsFalse(CombatPauseSignal.IsPaused);
            Assert.AreEqual(2, changes); Assert.IsFalse(last);
            CombatPauseSignal.Changed -= h;
        }

        [Test]
        public void TargetRegistry_RegisterUnregister()
        {
            var t = new FakeTarget(); int reg = 0, unreg = 0; int before = TargetRegistry.Count;
            System.Action<ITapTarget> r = _ => reg++, u = _ => unreg++;
            TargetRegistry.Registered += r; TargetRegistry.Unregistered += u;
            TargetRegistry.Register(t); TargetRegistry.Register(t);
            Assert.AreEqual(before + 1, TargetRegistry.Count); Assert.AreEqual(1, reg);
            TargetRegistry.Unregister(t); TargetRegistry.Unregister(t);
            Assert.AreEqual(before, TargetRegistry.Count); Assert.AreEqual(1, unreg);
            TargetRegistry.Registered -= r; TargetRegistry.Unregistered -= u;
        }

        [Test]
        public void GameEvents_SnapshotUpdates()
        {
            GameEvents.RaiseScoreChanged(123);
            Assert.AreEqual(123, GameEvents.Current.Score);
            GameEvents.RaiseScoreChanged(0);
        }

        [Test]
        public void BlastEvents_RaiseDeliversReport_AndResetClearsSubscribers()
        {
            BlastReport got = default; int n = 0;
            System.Action<BlastReport> h = r => { got = r; n++; };
            BlastEvents.Blasted += h;
            BlastEvents.Raise(new BlastReport { Center = new Vector3(1, 2, 3), Radius = 3f, EnemiesKilled = 2, HostagesHit = 1, SourceId = 77 });
            Assert.AreEqual(1, n);
            Assert.AreEqual(new Vector3(1, 2, 3), got.Center);
            Assert.AreEqual(3f, got.Radius); Assert.AreEqual(2, got.EnemiesKilled);
            Assert.AreEqual(1, got.HostagesHit); Assert.AreEqual(77, got.SourceId);

            var reset = typeof(BlastEvents).GetMethod("ResetStatics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            reset.Invoke(null, null);
            BlastEvents.Raise(new BlastReport());
            Assert.AreEqual(1, n);
        }
    }
}
