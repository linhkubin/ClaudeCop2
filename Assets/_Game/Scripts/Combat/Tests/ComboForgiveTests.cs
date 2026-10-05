using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Combat;

namespace ClaudeCop.Combat.Tests
{
    public class ComboForgiveTests
    {
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
