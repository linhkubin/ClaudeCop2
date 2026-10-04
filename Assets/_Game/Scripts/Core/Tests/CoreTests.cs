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
    }
}
