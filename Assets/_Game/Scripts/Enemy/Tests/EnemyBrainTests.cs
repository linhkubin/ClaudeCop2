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
            Assert.AreEqual(0.205f + 1.25f, b.ExposedTime, 0.01f);
        }

        [Test]
        public void FiresAtEnd_ThenHidesAndReturnsWithNewReticle()
        {
            var b = Make(); int fired = 0, ended = 0; b.Fired = () => fired++; b.AimEnded = () => ended++;
            b.Activate(); b.Tick(0.31f); b.Tick(2.6f);
            Assert.AreEqual(1, fired); Assert.AreEqual(0, ended); // van ban duoc khi dang rut vao, chua huy
            Assert.AreEqual(EnemyState.Retreating, b.State);
            b.Tick(0.31f);
            Assert.AreEqual(1, ended);
            Assert.AreEqual(EnemyState.Hidden, b.State);
            b.Tick(0.81f);
            Assert.AreEqual(EnemyState.Peeking, b.State);
            b.Tick(0.31f);
            Assert.AreEqual(EnemyState.Aiming, b.State);
            Assert.AreEqual(0f, b.ReticleProgress, 0.001f);
        }

        [Test]
        public void KillBeforeTargetable_Fails_NoFire()
        {
            var b = Make(); int fired = 0; b.Fired = () => fired++;
            Assert.IsFalse(b.Kill());          // Hidden
            b.Activate(); b.Tick(0.05f);       // PeekT ~0.17 < 0.35
            Assert.IsFalse(b.IsTargetable); Assert.IsFalse(b.Kill());
            b.Tick(0.31f);
            Assert.IsTrue(b.Kill());
            b.Tick(10f);
            Assert.AreEqual(EnemyState.Dead, b.State); Assert.AreEqual(0, fired);
        }

        [Test]
        public void TargetableDuringPeekAboveThreshold_ReticleNotStarted()
        {
            var b = Make(); int start = 0, end = 0; b.AimStarted = () => start++; b.AimEnded = () => end++;
            b.Activate(); b.Tick(0.15f);       // PeekT 0.5
            Assert.AreEqual(EnemyState.Peeking, b.State);
            Assert.IsTrue(b.IsTargetable); Assert.IsFalse(b.ShowsReticle);
            Assert.AreEqual(0f, b.ReticleProgress); Assert.GreaterOrEqual(b.ExposedTime, 0f);
            Assert.AreEqual(1, start);
            b.Tick(0.2f);                       // sang Aiming, khong dang ky doi
            Assert.AreEqual(EnemyState.Aiming, b.State); Assert.AreEqual(1, start); Assert.AreEqual(0, end);
            Assert.IsTrue(b.ShowsReticle);
        }

        [Test]
        public void KillWhilePeeking_UnregistersOnce_ReticleZero()
        {
            var b = Make(); int start = 0, end = 0; b.AimStarted = () => start++; b.AimEnded = () => end++;
            b.Activate(); b.Tick(0.15f);
            Assert.IsTrue(b.Kill());
            Assert.AreEqual(EnemyState.Dead, b.State); Assert.IsFalse(b.IsTargetable);
            Assert.AreEqual(0f, b.ReticleProgress);
            b.Tick(5f);
            Assert.AreEqual(1, start); Assert.AreEqual(1, end);
            Assert.IsFalse(b.Kill());
        }

        [Test]
        public void RetreatingTargetableUntilBelowThreshold()
        {
            var b = Make(); int start = 0, end = 0; b.AimStarted = () => start++; b.AimEnded = () => end++;
            b.Activate(); b.Tick(0.31f); b.Tick(2.6f);   // ban, Retreating
            Assert.AreEqual(EnemyState.Retreating, b.State);
            Assert.IsTrue(b.IsTargetable); Assert.IsFalse(b.ShowsReticle); Assert.AreEqual(0, end);
            b.Tick(0.1f);                                  // PeekT ~0.67
            Assert.IsTrue(b.IsTargetable);
            b.Tick(0.15f);                                 // PeekT ~0.17
            Assert.IsFalse(b.IsTargetable); Assert.AreEqual(1, end);
            Assert.IsFalse(b.Kill());
            b.Tick(1f);
            Assert.AreEqual(1, start); Assert.AreEqual(1, end);
        }

        [Test]
        public void KillWhileRetreating_UnregistersOnce()
        {
            var b = Make(); int end = 0; b.AimEnded = () => end++;
            b.Activate(); b.Tick(0.31f); b.Tick(2.6f); b.Tick(0.05f);
            Assert.IsTrue(b.Kill());
            Assert.AreEqual(EnemyState.Dead, b.State);
            b.Tick(5f);
            Assert.AreEqual(1, end);
        }

        [Test]
        public void ReappearsAndIsTargetableAgainEachCycle()
        {
            var b = Make(); int start = 0, end = 0; b.AimStarted = () => start++; b.AimEnded = () => end++;
            b.Activate(); b.Tick(0.31f); b.Tick(2.6f); b.Tick(0.31f); b.Tick(0.81f); b.Tick(0.15f);
            Assert.IsTrue(b.IsTargetable); Assert.AreEqual(2, start); Assert.AreEqual(1, end);
        }
    }
}
