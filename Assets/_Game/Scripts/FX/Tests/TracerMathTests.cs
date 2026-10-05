using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.FX.Tests
{
    public class TracerMathTests
    {
        [Test] public void End_Hit_UsesHitPoint()
        {
            var e = TracerMath.ComputeEnd(Vector3.zero, Vector3.forward, true, new Vector3(1, 2, 3), 40f);
            Assert.AreEqual(new Vector3(1, 2, 3), e);
        }

        [Test] public void End_Miss_FarAlongRay()
        {
            var e = TracerMath.ComputeEnd(Vector3.up, new Vector3(0, 0, 2), false, Vector3.zero, 40f);
            Assert.That((e - new Vector3(0, 1, 40)).magnitude, Is.LessThan(1e-4f));
        }

        [Test] public void Spread_WithinCone()
        {
            for (int i = 0; i < 20; i++)
            {
                var d = TracerMath.SpreadDirection(Vector3.forward, 5f, i / 20f, (i * 7 % 20) / 20f);
                Assert.LessOrEqual(Vector3.Angle(Vector3.forward, d), 5.001f);
                Assert.AreEqual(1f, d.magnitude, 1e-4f);
            }
            Assert.AreEqual(Vector3.forward, TracerMath.SpreadDirection(Vector3.forward, 0f, 0.5f, 0.5f));
        }

        [Test] public void SpreadEnd_KeepsDistance()
        {
            var o = new Vector3(1, 1, 0); var end = new Vector3(1, 1, 20);
            var e = TracerMath.SpreadEnd(o, end, 4f, 0.7f, 0.3f);
            Assert.AreEqual(20f, (e - o).magnitude, 1e-3f);
        }

        [Test] public void Width_GrowsWithDistance_AndClamps()
        {
            float near = TracerMath.WidthAtDistance(0.02f, 0.004f, 2f, 0.12f);
            float far = TracerMath.WidthAtDistance(0.02f, 0.004f, 20f, 0.12f);
            float huge = TracerMath.WidthAtDistance(0.02f, 0.004f, 1000f, 0.12f);
            Assert.Greater(far, near);
            Assert.AreEqual(0.12f, huge, 1e-5f);
        }

        [Test] public void HeadTail_Progression()
        {
            TracerMath.HeadTail(0f, 0.1f, 0.1f, out float h, out float t);
            Assert.AreEqual(0f, h); Assert.AreEqual(0f, t);
            TracerMath.HeadTail(0.1f, 0.1f, 0.1f, out h, out t);
            Assert.AreEqual(1f, h); Assert.AreEqual(0f, t);
            TracerMath.HeadTail(0.2f, 0.1f, 0.1f, out h, out t);
            Assert.AreEqual(1f, h); Assert.AreEqual(1f, t, 1e-5f);
        }

        [Test] public void TravelTime_MinClamp()
        {
            Assert.AreEqual(0.03f, TracerMath.TravelTime(1f, 200f, 0.03f), 1e-6f);
            Assert.AreEqual(0.2f, TracerMath.TravelTime(40f, 200f, 0.03f), 1e-6f);
        }
    }
}
