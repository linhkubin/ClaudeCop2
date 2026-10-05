using System.Collections.Generic;
using ClaudeCop.Core;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.Combat.Tests
{
    public class BarrelPriorityTests
    {
        sealed class FakeBarrel : IPriorityShootable
        {
            public bool IsPriorityLive { get; set; } = true;
            public Bounds PriorityBounds => default;
            public Rect Rect;
            public bool Occluded;
            public float Depth;
            public void OnShot(ShotInfo shot) { }
        }

        sealed class FakeEnemy : ITapTarget
        {
            public int Id { get; set; }
            public TargetKind Kind => TargetKind.Enemy;
            public bool IsTargetable => true;
            public Vector3 AimPoint { get; set; }
            public bool HasJusticePoint { get; set; }
            public Vector3 JusticePoint { get; set; }
            public bool ShowsReticle => true;
            public float ReticleProgress => 0f;
            public float ExposedTime => 0f;
            public TapOutcome OnTapHit(ShotInfo s, bool j) => TapOutcome.Kill;
        }

        static System.Func<Vector3, Vector2?> Proj => w => new Vector2(w.x, w.y);
        static readonly Rect EnemyBody = new Rect(-50, -300, 100, 400);

        // Mo phong ResolveShot: Select enemy -> neu BarrelPriority.Applies thi chon thung; tra ve "barrel", enemy id hoac 0.
        static string Resolve(List<FakeBarrel> barrels, FakeEnemy enemy, Vector2 tap, int maxHits = 1, bool enabled = true)
        {
            var res = new List<TargetHit>();
            int count = TargetSelector.Select(new List<ITapTarget> { enemy }, tap, 40f, 35f, maxHits, Proj, res,
                t => EnemyBody);
            if (BarrelPriority.Applies(enabled, maxHits, count, count > 0 && res[0].Justice))
            {
                var list = new List<IPriorityShootable>(barrels);
                var b = BarrelPriority.Pick(list, tap, x => ((FakeBarrel)x).Rect, x => ((FakeBarrel)x).Occluded, x => ((FakeBarrel)x).Depth);
                if (b != null) return "barrel";
            }
            return count > 0 ? "enemy" + res[0].Target.Id : "none";
        }

        [Test]
        public void TapOnBarrelNextToEnemy_PicksBarrel()
        {
            // Than enemy x -50..50; thung chong len mep phai (x 40..90).
            var barrel = new FakeBarrel { Rect = new Rect(40, -300, 50, 100) };
            var enemy = new FakeEnemy { Id = 1, AimPoint = new Vector3(0, 0, 1) };
            Assert.AreEqual("barrel", Resolve(new List<FakeBarrel> { barrel }, enemy, new Vector2(45, -250)));
        }

        [Test]
        public void TapOnEnemyBodyAwayFromBarrel_PicksEnemy()
        {
            var barrel = new FakeBarrel { Rect = new Rect(40, -300, 50, 100) };
            var enemy = new FakeEnemy { Id = 1, AimPoint = new Vector3(0, 0, 1) };
            Assert.AreEqual("enemy1", Resolve(new List<FakeBarrel> { barrel }, enemy, new Vector2(-20, -100)));
        }

        [Test]
        public void JusticeBeatsBarrel()
        {
            var barrel = new FakeBarrel { Rect = new Rect(-10, -10, 100, 100) };
            var enemy = new FakeEnemy { Id = 1, AimPoint = new Vector3(0, 0, 1), HasJusticePoint = true, JusticePoint = new Vector3(20, 20, 1) };
            Assert.AreEqual("enemy1", Resolve(new List<FakeBarrel> { barrel }, enemy, new Vector2(20, 20)));
        }

        [Test]
        public void OccludedExplodedDisabledAndShotgunSkipBarrel()
        {
            var enemy = new FakeEnemy { Id = 1, AimPoint = new Vector3(0, 0, 1) };
            var tap = new Vector2(45, -250);
            var barrel = new FakeBarrel { Rect = new Rect(40, -300, 50, 100), Occluded = true };
            Assert.AreEqual("enemy1", Resolve(new List<FakeBarrel> { barrel }, enemy, tap));
            barrel.Occluded = false; barrel.IsPriorityLive = false;
            Assert.AreEqual("enemy1", Resolve(new List<FakeBarrel> { barrel }, enemy, tap));
            barrel.IsPriorityLive = true;
            Assert.AreEqual("enemy1", Resolve(new List<FakeBarrel> { barrel }, enemy, tap, 3));
            Assert.AreEqual("enemy1", Resolve(new List<FakeBarrel> { barrel }, enemy, tap, 1, false));
        }

        [Test]
        public void TapFarFromBarrelDoesNotPullTap_AndNearestBarrelWins()
        {
            var near = new FakeBarrel { Rect = new Rect(0, 0, 50, 50), Depth = 1f };
            var far = new FakeBarrel { Rect = new Rect(0, 0, 50, 50), Depth = 9f };
            var list = new List<IPriorityShootable> { far, near };
            Assert.IsNull(BarrelPriority.Pick(list, new Vector2(200, 200), x => ((FakeBarrel)x).Rect, null, null));
            Assert.AreSame(near, BarrelPriority.Pick(list, new Vector2(10, 10), x => ((FakeBarrel)x).Rect, null, x => ((FakeBarrel)x).Depth));
        }
    }
}
