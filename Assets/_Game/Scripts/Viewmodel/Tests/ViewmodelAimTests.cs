using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.Viewmodel.Tests
{
    /// <summary>CAM-VC2: ngam theo vet dan + huong theo di chuyen (logic thuan).</summary>
    public class ViewmodelAimTests
    {
        [Test]
        public void Aim_HoldThenReturn()
        {
            var a = new ViewmodelAimState();
            a.Set(Quaternion.Euler(10f, 0f, 0f));
            Assert.AreEqual(10f, Quaternion.Angle(Quaternion.identity, a.Current(0.1f, 0.2f)), 1e-3f);
            a.Tick(0.1f);
            Assert.AreEqual(10f, Quaternion.Angle(Quaternion.identity, a.Current(0.1f, 0.2f)), 1e-3f);
            a.Tick(0.1f);
            Assert.AreEqual(5f, Quaternion.Angle(Quaternion.identity, a.Current(0.1f, 0.2f)), 0.05f);
            a.Tick(0.2f);
            Assert.AreEqual(0f, Quaternion.Angle(Quaternion.identity, a.Current(0.1f, 0.2f)), 1e-3f);
        }

        [Test]
        public void Aim_ClampAngle()
        {
            Assert.AreEqual(30f, Quaternion.Angle(Quaternion.identity, ViewmodelAimState.ClampAngle(Quaternion.Euler(50f, 20f, 0f), 30f)), 1e-3f);
            var small = Quaternion.Euler(5f, 0f, 0f);
            Assert.AreEqual(5f, Quaternion.Angle(Quaternion.identity, ViewmodelAimState.ClampAngle(small, 30f)), 1e-3f);
        }

        [Test]
        public void MoveTarget_LagsOppositeTurn_ClampedAndSpeedPitches()
        {
            var s = new ViewmodelMotionSettings();
            Vector3 right = ViewmodelMotion.MoveTarget(s, 30f, 0f);
            Assert.Less(right.y, 0f); Assert.Less(right.z, 0f);
            Vector3 big = ViewmodelMotion.MoveTarget(s, 5000f, 0f);
            Assert.LessOrEqual(Mathf.Abs(big.y), s.moveMaxYaw + 1e-4f); Assert.LessOrEqual(Mathf.Abs(big.z), s.moveMaxRoll + 1e-4f);
            Assert.AreEqual(s.moveSpeedPitch, ViewmodelMotion.MoveTarget(s, 0f, 40f).x, 1e-4f);
            Assert.AreEqual(Vector3.zero, ViewmodelMotion.MoveTarget(s, 0f, 0f));
        }

        [Test]
        public void TickMove_ConvergesAndResets()
        {
            var m = new ViewmodelMotion(new ViewmodelMotionSettings());
            for (int i = 0; i < 120; i++) m.TickMove(1f / 60f, 20f, 3f);
            Assert.Less(m.MoveEuler.y, -1f);
            m.Reset();
            Assert.AreEqual(Vector3.zero, m.MoveEuler);
        }
    }
}
