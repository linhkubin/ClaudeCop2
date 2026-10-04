using ClaudeCop.Core;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.FX.Tests
{
    public class FxTests
    {
        GameObject root;
        GameObject prefab;
        FxPool pool;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("PoolRoot");
            prefab = new GameObject("TestFx");
            var ps = prefab.AddComponent<ParticleSystem>();
            var m = ps.main; m.maxParticles = 10;
            pool = new FxPool(root.transform);
        }

        [TearDown]
        public void TearDown()
        {
            pool.Destroy();
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Pool_ReusesInstanceAfterExpiry()
        {
            var a = pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 1f, 4, 1f, 1000, 0f);
            Assert.IsTrue(a.activeSelf);
            pool.Tick(2f);
            Assert.IsFalse(a.activeSelf);
            var b = pool.Spawn(prefab, Vector3.one, Quaternion.identity, 1f, 4, 1f, 1000, 2f);
            Assert.AreSame(a, b);
            Assert.AreEqual(1, pool.TotalCreated);
        }

        [Test]
        public void Pool_RespectsMaxInstances_ByStealingOldest()
        {
            var first = pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 1f, 2, 1f, 1000, 0f);
            pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 5f, 2, 1f, 1000, 0.5f);
            var third = pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 5f, 2, 1f, 1000, 0.6f);
            Assert.AreEqual(2, pool.InstanceCount(prefab));
            Assert.AreEqual(2, pool.TotalCreated);
            Assert.AreSame(first, third);
            Assert.AreEqual(2, pool.ActiveCount);
        }

        [Test]
        public void Pool_RejectsOverParticleBudget()
        {
            Assert.IsNotNull(pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 1f, 8, 1f, 25, 0f));
            Assert.IsNotNull(pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 1f, 8, 1f, 25, 0f));
            Assert.IsNull(pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 1f, 8, 1f, 25, 0f));
            Assert.AreEqual(20, pool.ActiveCost);
        }

        [Test]
        public void Pool_ReduceMotionScaleLowersParticleCost()
        {
            var go = pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 1f, 4, 0.4f, 1000, 0f);
            Assert.AreEqual(4, pool.ActiveCost);
            Assert.AreEqual(4, go.GetComponent<ParticleSystem>().main.maxParticles);
        }

        [Test]
        public void Pool_ZeroMaxInstancesRejects()
        {
            Assert.IsNull(pool.Spawn(prefab, Vector3.zero, Quaternion.identity, 1f, 0, 1f, 1000, 0f));
        }

        [Test]
        public void Config_PicksPrefabBySurfaceMaterial()
        {
            var cfg = ScriptableObject.CreateInstance<FxConfig>();
            var fallback = new GameObject("c");
            var wood = new GameObject("w");
            cfg.surfaceImpacts = new FxEntry[6];
            cfg.surfaceImpacts[(int)SurfaceMaterial.Concrete] = new FxEntry { prefab = fallback };
            cfg.surfaceImpacts[(int)SurfaceMaterial.Wood] = new FxEntry { prefab = wood };
            Assert.AreSame(wood, cfg.GetSurface(SurfaceMaterial.Wood).prefab);
            Assert.AreSame(fallback, cfg.GetSurface(SurfaceMaterial.Metal).prefab, "thieu entry -> Concrete");
            Object.DestroyImmediate(cfg); Object.DestroyImmediate(fallback); Object.DestroyImmediate(wood);
        }

        [Test]
        public void Config_PicksFxByOutcome()
        {
            var cfg = ScriptableObject.CreateInstance<FxConfig>();
            var e = new GameObject("e"); var j = new GameObject("j");
            cfg.enemyHit = new FxEntry { prefab = e };
            cfg.justiceHit = new FxEntry { prefab = j };
            Assert.AreSame(e, cfg.GetOutcome(TapOutcome.Kill).prefab);
            Assert.AreSame(j, cfg.GetOutcome(TapOutcome.JusticeKill).prefab);
            Assert.AreSame(e, cfg.GetOutcome(TapOutcome.HostageHit).prefab, "hostage thieu prefab -> enemyHit");
            Assert.IsNull(cfg.GetOutcome(TapOutcome.Environment));
            Assert.IsNull(cfg.GetOutcome(TapOutcome.PickupCollected));
            Object.DestroyImmediate(cfg); Object.DestroyImmediate(e); Object.DestroyImmediate(j);
        }
    }
}
