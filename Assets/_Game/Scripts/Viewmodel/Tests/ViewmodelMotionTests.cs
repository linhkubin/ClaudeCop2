using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.Viewmodel.Tests
{
    public class ViewmodelMotionTests
    {
        static void Run(ViewmodelMotion m, float seconds, float dt = 1f / 60f)
        {
            for (float t = 0; t < seconds; t += dt) m.Tick(dt);
        }

        [Test]
        public void Recoil_SettlesBackToZero()
        {
            var s = new ViewmodelMotionSettings();
            var m = new ViewmodelMotion(s);
            m.OnShot(new Vector2(540, 960), 1080, 1920);
            Run(m, 0.1f);
            Assert.Greater(m.Recoil, 0.05f, "phai giat len sau phat ban");
            Run(m, 2f);
            Assert.AreEqual(0f, m.Recoil, 0.01f);
        }

        [Test]
        public void Recoil_StaysWithinLimit_UnderRapidFire()
        {
            var s = new ViewmodelMotionSettings();
            var m = new ViewmodelMotion(s);
            float max = 0f;
            for (int i = 0; i < 600; i++)
            {
                if (i % 6 == 0) m.OnShot(new Vector2(500, 900), 1080, 1920, 1f); // 10 phat/giay
                m.Tick(1f / 60f);
                max = Mathf.Max(max, m.Recoil);
                Assert.LessOrEqual(m.Recoil, s.recoilMax + 1e-4f);
                Assert.GreaterOrEqual(m.Recoil, -0.3f * s.recoilMax - 1e-4f);
            }
            Assert.Greater(max, 0.15f, "van phai giat thay duoc khi xa lien thanh");
        }

        [Test]
        public void Recoil_ClampedAtRecoilMax_AndReturnsToZero()
        {
            var s = new ViewmodelMotionSettings { recoilMax = 0.1f }; // tran nho hon dinh tu nhien (~0.23)
            var m = new ViewmodelMotion(s);
            float peak = 0f;
            for (int i = 0; i < 300; i++)
            {
                if (i % 6 == 0) m.OnShot(new Vector2(500, 900), 1080, 1920, 1f);
                m.Tick(1f / 60f);
                Assert.LessOrEqual(m.Recoil, s.recoilMax + 1e-5f, "khong duoc vuot tran");
                Assert.GreaterOrEqual(m.Recoil, -0.3f * s.recoilMax - 1e-5f);
                peak = Mathf.Max(peak, m.Recoil);
            }
            Assert.AreEqual(s.recoilMax, peak, 1e-5f, "phai cham dung tran");
            Run(m, 3f);
            Assert.AreEqual(0f, m.Recoil, 0.01f, "ngung ban thi ve 0");
        }

        [Test]
        public void Recoil_HitchDoesNotExplode()
        {
            var m = new ViewmodelMotion(new ViewmodelMotionSettings());
            m.OnShot(Vector2.zero, 1080, 1920);
            m.Tick(5f); // dt khong lo bi kep
            Assert.IsFalse(float.IsNaN(m.Recoil) || float.IsInfinity(m.Recoil));
            Assert.LessOrEqual(Mathf.Abs(m.Recoil), 2f);
        }

        [Test]
        public void NormalizeScreen_ClampsToUnitRange()
        {
            var v = ViewmodelMotion.NormalizeScreen(new Vector2(-500, 5000), 1080, 1920);
            Assert.AreEqual(-1f, v.x, 1e-5f);
            Assert.AreEqual(1f, v.y, 1e-5f);
            Assert.AreEqual(Vector2.zero, ViewmodelMotion.NormalizeScreen(new Vector2(540, 960), 1080, 1920));
            Assert.AreEqual(Vector2.zero, ViewmodelMotion.NormalizeScreen(new Vector2(10, 10), 0, 0));
        }

        [Test]
        public void Tilt_ClampedToMaxAngles()
        {
            var s = new ViewmodelMotionSettings { tiltMaxYaw = 7f, tiltMaxPitch = 5f, tiltMaxRoll = 3f };
            s.recoilEuler = Vector3.zero;
            var m = new ViewmodelMotion(s);
            m.OnShot(new Vector2(99999, 99999), 1080, 1920); // ngoai man hinh
            s.recoilImpulse = 0f;
            Run(m, 1f, 1f / 60f);
            m = new ViewmodelMotion(s);
            m.OnShot(new Vector2(99999, -99999), 1080, 1920);
            for (int i = 0; i < 30; i++)
            {
                m.Tick(1f / 60f);
                var e = m.EulerOffset();
                Assert.LessOrEqual(Mathf.Abs(e.y), s.tiltMaxYaw + 1e-3f);
                Assert.LessOrEqual(Mathf.Abs(e.z), s.tiltMaxRoll + 1e-3f);
            }
        }

        [Test]
        public void Tilt_FollowsTapSide_AndReturnsToCenter()
        {
            var s = new ViewmodelMotionSettings { tiltHoldTime = 0.3f };
            var m = new ViewmodelMotion(s);
            m.OnShot(new Vector2(1080, 960), 1080, 1920); // mep phai
            Run(m, 0.25f);
            Assert.Greater(m.TiltNormalized.x, 0.5f);
            Assert.Greater(m.EulerOffset().y, 0f);
            Run(m, 2f);
            Assert.AreEqual(0f, m.TiltNormalized.x, 0.02f);
        }

        [Test]
        public void ReduceMotion_ScalesAmplitude()
        {
            var s = new ViewmodelMotionSettings();
            var m = new ViewmodelMotion(s);
            m.OnShot(new Vector2(1000, 500), 1080, 1920);
            Run(m, 0.06f);
            Vector3 full = m.EulerOffset(1f);
            Vector3 red = m.EulerOffset(s.reduceMotionScale);
            Assert.Less(red.magnitude, full.magnitude);
            Assert.AreEqual(full.magnitude * s.reduceMotionScale, red.magnitude, 1e-3f);
            Assert.Less(m.PositionOffset(s.reduceMotionScale).magnitude, m.PositionOffset(1f).magnitude);
        }

        [Test]
        public void Reset_ClearsState()
        {
            var m = new ViewmodelMotion(new ViewmodelMotionSettings());
            m.OnShot(new Vector2(1000, 500), 1080, 1920);
            Run(m, 0.05f);
            m.Reset();
            Assert.AreEqual(0f, m.Recoil);
            Assert.AreEqual(Vector2.zero, m.TiltNormalized);
        }

        [Test]
        public void AnchorLocalPosition_BottomRight_IsBelowAndRightOfCenter()
        {
            var p = ViewmodelMotion.AnchorLocalPosition(new Vector2(0.7f, 0.1f), 0.5f, 45f, 0.5625f);
            Assert.Greater(p.x, 0f);
            Assert.Less(p.y, 0f);
            Assert.AreEqual(0.5f, p.z, 1e-5f);
            var c = ViewmodelMotion.AnchorLocalPosition(new Vector2(0.5f, 0.5f), 0.5f, 45f, 0.5625f);
            Assert.AreEqual(0f, c.x, 1e-5f);
            Assert.AreEqual(0f, c.y, 1e-5f);
        }

        [Test]
        public void AnchorLocalPosition_NarrowScreen_SameViewportAnchorStaysOnScreen()
        {
            // Voi cung neo viewport, x lech tam thu hep lai khi aspect nho (van trong khung nhin).
            var wide = ViewmodelMotion.AnchorLocalPosition(new Vector2(0.64f, 0.1f), 0.45f, 45f, 0.5625f);
            var narrow = ViewmodelMotion.AnchorLocalPosition(new Vector2(0.64f, 0.1f), 0.45f, 45f, 0.46f);
            Assert.Less(narrow.x, wide.x);
            Assert.AreEqual(wide.y, narrow.y, 1e-5f);
        }
    }
}
