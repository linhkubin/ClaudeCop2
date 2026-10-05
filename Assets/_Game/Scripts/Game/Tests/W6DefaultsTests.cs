using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Camera;

namespace ClaudeCop.Game.Tests
{
    public class W6DefaultsTests
    {
        [Test] public void GameConfig_ExplosionDefaults()
        {
            var c = ScriptableObject.CreateInstance<GameConfig>();
            Assert.AreEqual(100, c.ExplosionKillPoints);
            Assert.AreEqual(50, c.GrenadeShotPoints);
            Object.DestroyImmediate(c);
        }

        [Test] public void CameraFeelProfile_NewDefaults()
        {
            var p = ScriptableObject.CreateInstance<CameraFeelProfile>();
            Assert.AreEqual(0.6f, p.explosionShakeScale, 1e-5f);
            Assert.AreEqual(0.25f, p.explosionShakeDuration, 1e-5f);
            Assert.IsTrue(p.explosionShakeOffWhenReduceMotion);
            Assert.AreEqual(40f, p.hitShakeFrequency, 1e-5f);
            Assert.AreEqual(0.3f, p.rollSmoothTime, 1e-5f);
            Assert.AreEqual(0.5f, p.blendWaitMargin, 1e-5f);
            Object.DestroyImmediate(p);
        }
    }
}
