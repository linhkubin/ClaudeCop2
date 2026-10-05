using ClaudeCop.Core;
using NUnit.Framework;

namespace ClaudeCop.Combat.Tests
{
    public class ShotClassifierTests
    {
        [Test]
        public void ShootableIsEnvironment_AndKeepsCombo()
        {
            var o = ShotClassifier.ClassifyEnvironment(true, true);
            Assert.AreEqual(TapOutcome.Environment, o);
            var c = new ComboTracker(); c.Apply(TapOutcome.Kill); c.Apply(TapOutcome.Kill);
            Assert.IsFalse(c.Apply(o, ShotClassifier.KeepsCombo(o)));
            Assert.AreEqual(2, c.Streak);
        }

        [Test]
        public void InertWallOrNothingIsMiss_AndResetsCombo()
        {
            Assert.AreEqual(TapOutcome.Miss, ShotClassifier.ClassifyEnvironment(true, false));
            Assert.AreEqual(TapOutcome.Miss, ShotClassifier.ClassifyEnvironment(false, false));
            var c = new ComboTracker(); c.Apply(TapOutcome.Kill);
            var o = ShotClassifier.ClassifyEnvironment(true, false);
            Assert.IsTrue(c.Apply(o, ShotClassifier.KeepsCombo(o)));
            Assert.AreEqual(0, c.Streak);
        }
    }
}
