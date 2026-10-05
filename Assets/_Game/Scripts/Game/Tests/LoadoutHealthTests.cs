using NUnit.Framework;
using ClaudeCop.Game;

namespace ClaudeCop.Game.Tests
{
    public class LoadoutHealthTests
    {
        [Test]
        public void Armor_AbsorbsHit_NoLifeLost_ThenInvulnerable()
        {
            var t = new LifeTracker(3, 1.5f);
            t.AddArmor(1);
            Assert.IsFalse(t.TryDamage(0f));
            Assert.IsTrue(t.LastAbsorbed);
            Assert.AreEqual(3, t.Lives);
            Assert.AreEqual(0, t.Armor);
            Assert.IsFalse(t.TryDamage(1f));          // con bat tu sau phat bi do
            Assert.IsFalse(t.LastAbsorbed);
            Assert.IsTrue(t.TryDamage(2f));
            Assert.AreEqual(2, t.Lives);
        }

        [Test]
        public void TopUpArmor_DoesNotStackAboveTarget()
        {
            var t = new LifeTracker(3, 1f);
            t.AddArmor(1);
            t.TopUpArmor(1);
            Assert.AreEqual(1, t.Armor);
            t.TopUpArmor(2);
            Assert.AreEqual(2, t.Armor);
        }

        [Test]
        public void Reset_KeepsArmor()
        {
            var t = new LifeTracker(4, 1f);
            t.AddArmor(1);
            t.Reset();
            Assert.AreEqual(4, t.Lives);
            Assert.AreEqual(1, t.Armor);
        }
    }
}
