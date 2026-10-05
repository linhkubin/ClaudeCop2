using System.Collections.Generic;
using ClaudeCop.Core;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.Props.Tests
{
    public class PropsTests
    {
        readonly List<Object> junk = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in junk) if (o != null) Object.DestroyImmediate(o);
            junk.Clear();
            var m = typeof(TargetRegistry).GetMethod("ResetStatics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            m.Invoke(null, null);
        }

        [SetUp]
        public void SetUp()
        {
            var m = typeof(TargetRegistry).GetMethod("ResetStatics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            m.Invoke(null, null);
        }

        GameObject Piece(int bodies, string name = "Piece")
        {
            var go = new GameObject(name);
            junk.Add(go);
            for (int i = 0; i < bodies; i++)
            {
                var c = new GameObject("b" + i);
                c.transform.SetParent(go.transform, false);
                c.transform.localPosition = new Vector3(i * 0.1f, 0, 0);
                c.AddComponent<BoxCollider>();
                c.AddComponent<Rigidbody>();
            }
            go.SetActive(false); // "prefab" mau
            return go;
        }

        [Test]
        public void Pool_NeverExceedsMaxBodies_RecyclesOldest()
        {
            var root = new GameObject("root"); junk.Add(root);
            var pool = new PropPool(root.transform, 40);
            var prefab = Piece(4);
            for (int i = 0; i < 30; i++)
            {
                pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 4f, 0.01f * (i + 1));
                Assert.LessOrEqual(pool.ActiveBodies, 40);
            }
            Assert.LessOrEqual(pool.PeakActiveBodies, 40);
            Assert.AreEqual(40, pool.ActiveBodies);
            Assert.AreEqual(10, pool.ActiveCount);
        }

        [Test]
        public void Pool_ExpiredAfterLifetime_ReturnsToPool()
        {
            var root = new GameObject("root"); junk.Add(root);
            var pool = new PropPool(root.transform, 40);
            var prefab = Piece(3);
            var inst = pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 4f, 10f);
            Assert.AreEqual(3, pool.ActiveBodies);
            pool.Tick(13.9f, null);
            Assert.AreEqual(3, pool.ActiveBodies);
            pool.Tick(14.0f, null);
            Assert.AreEqual(0, pool.ActiveBodies);
            Assert.IsFalse(inst.Active);
            int created = pool.TotalCreated;
            pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 4f, 15f);
            Assert.AreEqual(created, pool.TotalCreated, "phai tai dung instance, khong Instantiate them");
        }

        [Test]
        public void Pool_ScaleMultipliesBodyPositions()
        {
            var root = new GameObject("root"); junk.Add(root);
            var pool = new PropPool(root.transform, 40);
            var prefab = Piece(2);
            var inst = pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 4f, 0f, new Vector3(4f, 2f, 1f));
            Assert.AreEqual(0.4f, inst.Bodies[1].transform.localPosition.x, 1e-4f);
        }

        [Test]
        public void Blast_SelectsByKindRadiusAndTargetable()
        {
            var c = Vector3.zero;
            Assert.AreEqual(BlastEffect.Enemy, BlastResolver.Classify(TargetKind.Enemy, true, new Vector3(2.9f, 0, 0), c, 3f));
            Assert.AreEqual(BlastEffect.None, BlastResolver.Classify(TargetKind.Enemy, true, new Vector3(3.1f, 0, 0), c, 3f));
            Assert.AreEqual(BlastEffect.None, BlastResolver.Classify(TargetKind.Enemy, false, Vector3.one, c, 3f));
            Assert.AreEqual(BlastEffect.Hostage, BlastResolver.Classify(TargetKind.Hostage, true, Vector3.one, c, 3f));
            Assert.AreEqual(BlastEffect.Grenade, BlastResolver.Classify(TargetKind.Grenade, true, Vector3.one, c, 3f));
            Assert.AreEqual(BlastEffect.None, BlastResolver.Classify(TargetKind.Pickup, true, Vector3.one, c, 3f));
            Assert.IsFalse(BlastResolver.PenalizesPlayer(0));
            Assert.IsTrue(BlastResolver.PenalizesPlayer(1));
            Assert.IsTrue(BlastResolver.PenalizesPlayer(3));
        }

        sealed class FakeTarget : ITapTarget
        {
            static int next = 1000;
            readonly Vector3 pos;
            public int Hits, NonBlastHits;
            public FakeTarget(TargetKind k, Vector3 p) { Kind = k; pos = p; Id = next++; }
            public int Id { get; }
            public TargetKind Kind { get; }
            public bool IsTargetable => true;
            public Vector3 AimPoint => pos;
            public bool HasJusticePoint => false;
            public Vector3 JusticePoint => pos;
            public bool ShowsReticle => true;
            public float ReticleProgress => 0;
            public float ExposedTime => 0;
            public TapOutcome OnTapHit(ShotInfo s, bool j)
            {
                Hits++; TargetRegistry.Unregister(this);
                if (!float.IsNaN(s.ScreenPosition.x)) NonBlastHits++;
                return Kind == TargetKind.Hostage ? TapOutcome.HostageHit : TapOutcome.Kill;
            }
        }

        sealed class Receiver : IPlayerDamageReceiver
        {
            public int Calls; public DamageSource Last;
            public void Damage(DamageSource s, Vector3 p) { Calls++; Last = s; }
        }

        [Test]
        public void Barrel_ExplodesOnce_RaisesOneReport_PenalizesOnce()
        {
            var go = new GameObject("barrel"); junk.Add(go);
            var col = go.AddComponent<BoxCollider>();
            var barrel = go.AddComponent<ExplosiveBarrel>();
            var e1 = new FakeTarget(TargetKind.Enemy, new Vector3(1, 0.4f, 0));
            var e2 = new FakeTarget(TargetKind.Enemy, new Vector3(0, 0.4f, 2));
            var far = new FakeTarget(TargetKind.Enemy, new Vector3(10, 0, 0));
            var h1 = new FakeTarget(TargetKind.Hostage, new Vector3(-1, 0.4f, 0));
            var h2 = new FakeTarget(TargetKind.Hostage, new Vector3(0, 0.4f, -2));
            foreach (var t in new[] { e1, e2, far, h1, h2 }) TargetRegistry.Register(t);

            var rcv = new Receiver(); PlayerDamageService.Register(rcv);
            var reports = new List<BlastReport>();
            System.Action<BlastReport> h = r => reports.Add(r);
            BlastEvents.Blasted += h;
            try
            {
                barrel.OnShot(default);
                barrel.OnShot(default);
                barrel.Explode();
            }
            finally { BlastEvents.Blasted -= h; PlayerDamageService.Unregister(rcv); }

            Assert.IsTrue(barrel.Exploded);
            Assert.AreEqual(1, reports.Count);
            Assert.AreEqual(2, reports[0].EnemiesKilled);
            Assert.AreEqual(2, reports[0].HostagesHit);
            Assert.AreEqual(1, rcv.Calls, "phat dung 1 lan moi vu no");
            Assert.AreEqual(DamageSource.Explosion, rcv.Last);
            Assert.AreEqual(1, e1.Hits);
            Assert.AreEqual(0, e1.NonBlastHits, "vu no danh dau ScreenPosition = NaN");
            Assert.AreEqual(0, far.Hits);
            Assert.IsFalse(col.enabled);
        }
    }
}
