using NUnit.Framework;
using ClaudeCop.Core;

namespace ClaudeCop.Core.Tests
{
    public class PhaseBannerGateTests
    {
        [Test]
        public void Banner_Shows_Once_Per_Phase_Only()
        {
            var g = new PhaseBannerGate();
            Assert.IsTrue(g.TryShow(0));
            Assert.IsFalse(g.TryShow(0), "khong hien lai o doan Move khac / hoi sinh");
            Assert.IsTrue(g.TryShow(1));
            Assert.IsFalse(g.TryShow(1));
            Assert.IsTrue(g.TryShow(2));
            Assert.AreEqual(3, g.ShownCount);
        }

        [Test]
        public void Banner_Gate_Reset_Allows_New_Run()
        {
            var g = new PhaseBannerGate();
            g.TryShow(0); g.Reset();
            Assert.IsTrue(g.TryShow(0));
        }
    }
}
