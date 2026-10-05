using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Camera;
using ClaudeCop.Core;

namespace ClaudeCop.Game.Tests
{
    /// <summary>CAM-VC2: giat khi ban + duong cong toc do ray.</summary>
    public class CameraVc2Tests
    {
        CameraFeelProfile P;
        [SetUp] public void Setup() { P = ScriptableObject.CreateInstance<CameraFeelProfile>(); }
        [TearDown] public void Teardown() { Object.DestroyImmediate(P); }

        [Test]
        public void Kick_PistolPeakInRange_AndReturnsToZero()
        {
            var k = new CameraKick(P);
            k.Fire(0f, WeaponKind.Pistol, 1f);
            float peak = 0f;
            for (float t = 0f; t < 0.3f; t += 0.002f) peak = Mathf.Max(peak, k.Evaluate(t).x);
            Assert.GreaterOrEqual(peak, 0.3f); Assert.LessOrEqual(peak, 0.6f);
            Assert.AreEqual(0f, k.Evaluate(0.2f).x, 1e-5f);
            Assert.LessOrEqual(P.kickFov * P.kickScaleShotgun, 0.5f);
        }

        [Test]
        public void Kick_ShotgunStrongerThanPistol_MachineGunCappedAndFovBounded()
        {
            var a = new CameraKick(P); a.Fire(0f, WeaponKind.Pistol, 1f);
            var b = new CameraKick(P); b.Fire(0f, WeaponKind.Shotgun, 1f);
            Assert.Greater(b.Evaluate(P.kickRise).x, a.Evaluate(P.kickRise).x);
            var m = new CameraKick(P);
            float maxP = 0f, maxF = 0f;
            for (int i = 0; i < 40; i++) { m.Fire(i * 0.02f, WeaponKind.MachineGun, 1f); var v = m.Evaluate(i * 0.02f); maxP = Mathf.Max(maxP, v.x); maxF = Mathf.Max(maxF, v.y); }
            Assert.LessOrEqual(maxP, P.kickMaxPitch + 1e-4f); Assert.LessOrEqual(maxF, P.kickMaxFov + 1e-4f);
            Assert.Greater(maxP, a.MaxPitch);
        }

        [Test]
        public void Kick_ReduceMotionScaleZero_NoKick_AndDampApplies()
        {
            var k = new CameraKick(P);
            k.Fire(0f, WeaponKind.Pistol, 0f);
            Assert.AreEqual(0, k.Count);
            k.Fire(0f, WeaponKind.Pistol, 1f);
            float full = k.Evaluate(P.kickRise, 1f).x, damped = k.Evaluate(P.kickRise, 0.3f).x;
            Assert.AreEqual(full * 0.3f, damped, 1e-5f);
        }

        [Test]
        public void RailCurve_SameTotalAsTrapezoid_SoftEnds_Monotonic()
        {
            float L = 18f, v = 3.5f;
            float trap = RailSpeedCurve.TrapezoidTotal(L, v, 1f, 1f);
            var c = new RailSpeedCurve(L, trap, 0.8f, 0.25f, 5.5f);
            Assert.AreEqual(trap, c.Total, 0.05f);
            c.Evaluate(0f, out float d0, out float s0); Assert.AreEqual(0f, s0, 1e-5f);
            c.Evaluate(c.Total, out float dE, out float sE); Assert.AreEqual(L, dE, 1e-4f); Assert.AreEqual(0f, sE, 1e-5f);
            float prev = 0f, prevSpeed = 0f, maxAcc = 0f; float dt = 0.01f;
            for (float t = 0f; t <= c.Total; t += dt)
            {
                c.Evaluate(t, out float d, out float s);
                Assert.GreaterOrEqual(d, prev - 1e-5f); prev = d;
                maxAcc = Mathf.Max(maxAcc, Mathf.Abs(s - prevSpeed) / dt); prevSpeed = s;
            }
            Assert.Less(maxAcc, 10f); // khong nhay toc do
            // 25% cuoi: toc do giam dan
            c.Evaluate(c.Total - c.DecelTime, out float dd, out float sd);
            Assert.AreEqual(L * 0.75f, dd, 0.05f);
            c.Evaluate(c.Total - c.DecelTime * 0.2f, out _, out float sl); Assert.Less(sl, sd * 0.2f);
            // ra dau nhanh hon hinh thang: sau 0.5 s toc do da cao hon
            c.Evaluate(0.5f, out _, out float sFast);
            Assert.Greater(sFast, v * 0.5f / 1f * 0.9f);
        }

        [Test]
        public void RailCurve_ShortRail_StillFinishes()
        {
            var c = new RailSpeedCurve(2f, 3f, 0.6f, 0.25f, 5.5f);
            c.Evaluate(c.Total * 0.999f, out float d, out _);
            Assert.AreEqual(2f, d, 0.05f);
            Assert.Greater(c.Total, 0.5f);
        }
    }
}
