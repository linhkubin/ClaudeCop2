using System.Collections.Generic;
using ClaudeCop.Core;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.Combat.Tests
{
    public class TargetSelectorTests
    {
        sealed class Fake : ITapTarget
        {
            public int Id { get; set; }
            public TargetKind Kind { get; set; }
            public bool IsTargetable { get; set; } = true;
            public Vector3 AimPoint { get; set; }
            public bool HasJusticePoint { get; set; }
            public Vector3 JusticePoint { get; set; }
            public bool ShowsReticle => true;
            public float ReticleProgress => 0f;
            public float ExposedTime => 0f;
            public TapOutcome OnTapHit(ShotInfo s, bool j) => TapOutcome.Kill;
        }

        // Chieu "world" thanh man hinh bang (x, y); z < 0 la sau camera.
        static System.Func<Vector3, Vector2?> Proj => w => w.z < 0 ? (Vector2?)null : new Vector2(w.x, w.y);

        static Fake E(float x, float y, int id = 1) => new Fake { Id = id, Kind = TargetKind.Enemy, AimPoint = new Vector3(x, y, 1) };
        static Fake H(float x, float y, int id = 9) => new Fake { Id = id, Kind = TargetKind.Hostage, AimPoint = new Vector3(x, y, 1) };

        static int Run(List<ITapTarget> list, Vector2 tap, float r, int max, List<TargetHit> res)
            => TargetSelector.Select(list, tap, r, 35f, max, Proj, res);

        [Test]
        public void PistolPicksNearestEnemyInRadius()
        {
            var res = new List<TargetHit>();
            var list = new List<ITapTarget> { E(100, 0, 1), E(60, 0, 2), E(500, 0, 3) };
            Assert.AreEqual(1, Run(list, Vector2.zero, 90f, 1, res));
            Assert.AreEqual(2, res[0].Target.Id);
        }

        [Test]
        public void OutsideRadiusIsMiss()
        {
            var res = new List<TargetHit>();
            Assert.AreEqual(0, Run(new List<ITapTarget> { E(200, 0) }, Vector2.zero, 90f, 1, res));
        }

        [Test]
        public void EnemyBeatsHostageEvenIfHostageCloser()
        {
            var res = new List<TargetHit>();
            var list = new List<ITapTarget> { H(5, 0), E(80, 0) };
            Assert.AreEqual(1, Run(list, Vector2.zero, 90f, 1, res));
            Assert.AreEqual(TargetKind.Enemy, res[0].Target.Kind);
        }

        [Test]
        public void HostageHitWhenNoEnemyInRange()
        {
            var res = new List<TargetHit>();
            var list = new List<ITapTarget> { H(10, 0), E(500, 0) };
            Assert.AreEqual(1, Run(list, Vector2.zero, 90f, 1, res));
            Assert.AreEqual(TargetKind.Hostage, res[0].Target.Kind);
        }

        [Test]
        public void JusticePointWins()
        {
            var res = new List<TargetHit>();
            var a = E(0, 0, 1);
            var b = E(50, 0, 2);
            b.HasJusticePoint = true; b.JusticePoint = new Vector3(30, 0, 1);
            Assert.AreEqual(1, Run(new List<ITapTarget> { a, b }, Vector2.zero, 90f, 1, res));
            Assert.AreEqual(2, res[0].Target.Id);
            Assert.IsTrue(res[0].Justice);
        }

        [Test]
        public void JusticePointOutsideJusticeRadiusIsNormalKill()
        {
            var res = new List<TargetHit>();
            var b = E(50, 0, 2);
            b.HasJusticePoint = true; b.JusticePoint = new Vector3(60, 0, 1);
            Assert.AreEqual(1, Run(new List<ITapTarget> { b }, Vector2.zero, 90f, 1, res));
            Assert.IsFalse(res[0].Justice);
        }

        [Test]
        public void ShotgunHitsMultipleSortedAndCapped()
        {
            var res = new List<TargetHit>();
            var list = new List<ITapTarget> { E(150, 0, 1), E(20, 0, 2), E(100, 0, 3), E(300, 0, 4) };
            Assert.AreEqual(2, Run(list, Vector2.zero, 180f, 2, res));
            Assert.AreEqual(2, res[0].Target.Id);
            Assert.AreEqual(3, res[1].Target.Id);
        }

        [Test]
        public void IgnoresNonTargetableAndBehindCamera()
        {
            var res = new List<TargetHit>();
            var dead = E(0, 0, 1); dead.IsTargetable = false;
            var behind = new Fake { Id = 2, Kind = TargetKind.Enemy, AimPoint = new Vector3(0, 0, -5) };
            Assert.AreEqual(0, Run(new List<ITapTarget> { dead, behind }, Vector2.zero, 90f, 1, res));
        }

        static System.Func<ITapTarget, Rect?> Body(Rect r) => t => t.Kind == TargetKind.Enemy ? (Rect?)r : null;

        [Test]
        public void BodyRectHitsOutsideAimRadius()
        {
            var res = new List<TargetHit>();
            var e = E(0, 0, 1);
            var body = Body(new Rect(-50, -300, 100, 400)); // chan o y=-250 xa AimPoint
            var tap = new Vector2(10, -250);
            Assert.AreEqual(0, TargetSelector.Select(new List<ITapTarget> { e }, tap, 90f, 35f, 1, Proj, res));
            Assert.AreEqual(1, TargetSelector.Select(new List<ITapTarget> { e }, tap, 90f, 35f, 1, Proj, res, body));
            Assert.AreEqual(1, res[0].Target.Id);
            Assert.IsFalse(res[0].Justice);
        }

        [Test]
        public void BodyRectOutsideIsMissAndNonTargetableIgnored()
        {
            var res = new List<TargetHit>();
            var e = E(0, 0, 1);
            Assert.AreEqual(0, TargetSelector.Select(new List<ITapTarget> { e }, new Vector2(300, 0), 90f, 35f, 1, Proj, res, Body(new Rect(-50, -300, 100, 400))));
            e.IsTargetable = false;
            Assert.AreEqual(0, TargetSelector.Select(new List<ITapTarget> { e }, new Vector2(10, -250), 90f, 35f, 1, Proj, res, Body(new Rect(-50, -300, 100, 400))));
        }

        [Test]
        public void BodyHitBeatsHostageUnderTap()
        {
            var res = new List<TargetHit>();
            var list = new List<ITapTarget> { H(10, -250), E(0, 0, 1) };
            Assert.AreEqual(1, TargetSelector.Select(list, new Vector2(10, -250), 90f, 35f, 1, Proj, res, Body(new Rect(-50, -300, 100, 400))));
            Assert.AreEqual(TargetKind.Enemy, res[0].Target.Kind);
        }

        static Fake EZ(float x, float y, float z, int id) => new Fake { Id = id, Kind = TargetKind.Enemy, AimPoint = new Vector3(x, y, z) };
        static System.Func<ITapTarget, float> Depth => t => t.AimPoint.z;

        [Test]
        public void OverlappingEnemies_FrontOneWinsEvenIfBackAimPointCloserToTap()
        {
            var res = new List<TargetHit>();
            var front = EZ(0, 0, 5, 1);   // gan camera, tam xa tap hon
            var back = EZ(40, 0, 20, 2);  // xa camera, tam gan tap hon
            var list = new List<ITapTarget> { back, front };
            var tap = new Vector2(35, 0);
            var body = Body(new Rect(-50, -300, 100, 400)); // than ca hai chong nhau
            Assert.AreEqual(1, TargetSelector.Select(list, tap, 90f, 35f, 1, Proj, res, body, Depth));
            Assert.AreEqual(1, res[0].Target.Id);
            // Khong co depth: hanh vi cu (tam gan tap nhat).
            Assert.AreEqual(1, TargetSelector.Select(list, tap, 90f, 35f, 1, Proj, res, body));
            Assert.AreEqual(2, res[0].Target.Id);
        }

        [Test]
        public void Depth_JusticeStillWinsAndMultiTargetUnchanged()
        {
            var res = new List<TargetHit>();
            var front = EZ(0, 0, 5, 1);
            var back = EZ(40, 0, 20, 2);
            back.HasJusticePoint = true; back.JusticePoint = new Vector3(35, 0, 20);
            var list = new List<ITapTarget> { front, back };
            Assert.AreEqual(1, TargetSelector.Select(list, new Vector2(35, 0), 90f, 35f, 1, Proj, res, null, Depth));
            Assert.AreEqual(2, res[0].Target.Id);
            Assert.IsTrue(res[0].Justice);
            // Shotgun (max 3): giu thu tu cu, khong doi cho theo do sau.
            Assert.AreEqual(2, TargetSelector.Select(new List<ITapTarget> { EZ(0, 0, 5, 1), EZ(40, 0, 20, 2) }, new Vector2(30, 0), 90f, 35f, 3, Proj, res, null, Depth));
            Assert.AreEqual(2, res[0].Target.Id);
        }
    }
}
