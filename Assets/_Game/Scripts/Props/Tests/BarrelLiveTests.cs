using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.Props.Tests
{
    public class BarrelLiveTests
    {
        GameObject go;

        [TearDown]
        public void TearDown() { if (go != null) Object.DestroyImmediate(go); }

        [Test]
        public void IsPriorityLive_FalseWhenInactiveInHierarchy_TrueAgainWhenReactivated()
        {
            var parent = new GameObject("zone");
            go = parent;
            var child = new GameObject("barrel");
            child.transform.SetParent(parent.transform);
            child.AddComponent<BoxCollider>();
            var barrel = child.AddComponent<ExplosiveBarrel>();

            Assert.IsTrue(barrel.IsPriorityLive);
            parent.SetActive(false);
            Assert.IsFalse(barrel.IsPriorityLive, "thung o khu bi tat khong duoc tap/no");
            parent.SetActive(true);
            Assert.IsTrue(barrel.IsPriorityLive);
        }

        [Test]
        public void IsPriorityLive_FalseAfterExploded()
        {
            go = new GameObject("barrel");
            go.AddComponent<BoxCollider>();
            var barrel = go.AddComponent<ExplosiveBarrel>();
            barrel.Explode();
            Assert.IsFalse(barrel.IsPriorityLive);
        }
    }
}
