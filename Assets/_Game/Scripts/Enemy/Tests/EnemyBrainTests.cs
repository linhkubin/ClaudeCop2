using NUnit.Framework;

namespace ClaudeCop.Enemy.Tests
{
    public class EnemyBrainTests
    {
        static EnemyBrain Make() => new EnemyBrain(0.3f, 2.5f, 0.3f, 0.8f);

        [Test]
        public void StaysHiddenUntilActivated()
        {
            var b = Make(); b.Tick(5f);
            Assert.AreEqual(EnemyState.Hidden, b.State);
        }

        [Test]
        public void PeekThenAim_ReticleRuns()
        {
            var b = Make(); int aim = 0; b.AimStarted = () => aim++;
            b.Activate(); b.Tick(0.31f);
            Assert.AreEqual(EnemyState.Aiming, b.State); Assert.AreEqual(1, aim);
            Assert.IsTrue(b.IsTargetable);
            b.Tick(1.25f);
            Assert.AreEqual(0.5f, b.ReticleProgress, 0.01f);
            Assert.AreEqual(1.25f, b.ExposedTime, 0.01f);
        }

        [Test]
        public void FiresAtEnd_ThenHidesAndReturnsWithNewReticle()
        {
            var b = Make(); int fired = 0, ended = 0; b.Fired = () => fired++; b.AimEnded = () => ended++;
            b.Activate(); b.Tick(0.31f); b.Tick(2.6f);
            Assert.AreEqual(1, fired); Assert.AreEqual(1, ended);
            Assert.AreEqual(EnemyState.Retreating, b.State);
            b.Tick(0.31f);
            Assert.AreEqual(EnemyState.Hidden, b.State);
            b.Tick(0.81f);
            Assert.AreEqual(EnemyState.Peeking, b.State);
            b.Tick(0.31f);
            Assert.AreEqual(EnemyState.Aiming, b.State);
            Assert.AreEqual(0f, b.ReticleProgress, 0.001f);
        }

        [Test]
        public void KillOnlyWhileAiming_NoFire()
        {
            var b = Make(); int fired = 0; b.Fired = () => fired++;
            b.Activate();
            Assert.IsFalse(b.Kill());
            b.Tick(0.31f);
            Assert.IsTrue(b.Kill());
            b.Tick(10f);
            Assert.AreEqual(EnemyState.Dead, b.State); Assert.AreEqual(0, fired);
        }
    }
}
