using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Combat;

namespace ClaudeCop.Combat.Tests
{
    public class SwipeAndForgiveTests
    {
        [Test]
        public void SwipeDown_NeedsDistanceAndVerticalDirection()
        {
            float side = 1080f, frac = 0.12f; // >= 129.6 px
            Assert.IsTrue(TapShooter.IsSwipeDown(new Vector2(500, 1200), new Vector2(510, 1000), side, frac));
            Assert.IsFalse(TapShooter.IsSwipeDown(new Vector2(500, 1200), new Vector2(500, 1100), side, frac)); // qua ngan
            Assert.IsFalse(TapShooter.IsSwipeDown(new Vector2(500, 1000), new Vector2(500, 1300), side, frac)); // vuot len
            Assert.IsFalse(TapShooter.IsSwipeDown(new Vector2(200, 1200), new Vector2(500, 1000), side, frac)); // cheo ngang
        }

        [Test]
        public void Gloves_ForgiveMiss_ButNotHostage()
        {
            var t = new ComboTracker(5, 1);
            t.ForgiveCharges = 1;
            t.Apply(TapOutcome.Kill); t.Apply(TapOutcome.Kill);
            t.Apply(TapOutcome.Miss);
            Assert.AreEqual(2, t.Streak);
            Assert.AreEqual(0, t.ForgiveCharges);
            t.Apply(TapOutcome.Miss);
            Assert.AreEqual(0, t.Streak);

            t.ForgiveCharges = 3;
            t.Apply(TapOutcome.Kill);
            t.Apply(TapOutcome.HostageHit);
            Assert.AreEqual(0, t.Streak);
            Assert.AreEqual(3, t.ForgiveCharges);
        }

        [Test]
        public void Forgive_NotSpentWithoutStreak()
        {
            var t = new ComboTracker(5, 1);
            t.ForgiveCharges = 1;
            t.Apply(TapOutcome.Miss);
            Assert.AreEqual(1, t.ForgiveCharges);
        }
    }
}
