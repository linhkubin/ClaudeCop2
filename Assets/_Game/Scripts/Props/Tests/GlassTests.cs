using ClaudeCop.Core;
using ClaudeCop.Props.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.Props.Tests
{
    public class GlassTests
    {
        [Test]
        public void Slice_CountInRange_AndCoversUnitQuad()
        {
            for (int n = 6; n <= 10; n++)
            {
                var tris = GlassShardBuilder.Slice(n);
                Assert.AreEqual(n, tris.Count);
                float area = 0f;
                foreach (var t in tris) area += Mathf.Abs((t[1].x - t[0].x) * (t[2].y - t[0].y) - (t[1].y - t[0].y) * (t[2].x - t[0].x)) * 0.5f;
                Assert.AreEqual(1f, area, 1e-3f, "cac manh phai phu kin o 1x1");
            }
            Assert.AreEqual(10, GlassShardBuilder.Slice(99).Count);
            Assert.AreEqual(6, GlassShardBuilder.Slice(1).Count);
        }

        [Test]
        public void BuiltPrefab_HasShardsInRange()
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Props/Prop_GlassShards.prefab");
            Assert.IsNotNull(p, "chay menu ClaudeCop/Props/Build Glass Shards truoc");
            int n = p.transform.childCount;
            Assert.GreaterOrEqual(n, 6);
            Assert.LessOrEqual(n, 10);
        }

        [Test]
        public void Glass_BreaksOnce_NoMoreShards_ColliderOff()
        {
            var sysGo = new GameObject("sys");
            var sys = sysGo.AddComponent<PropSystem>();
            var shards = new GameObject("shards");
            shards.SetActive(false);
            for (int i = 0; i < 8; i++)
            {
                var c = new GameObject("s" + i);
                c.transform.SetParent(shards.transform, false);
                c.AddComponent<BoxCollider>();
                c.AddComponent<Rigidbody>();
            }
            var glass = new GameObject("glass");
            glass.transform.localScale = new Vector3(2f, 1.5f, 1f);
            var col = glass.AddComponent<BoxCollider>();
            var bg = glass.AddComponent<BreakableGlass>();
            var so = new SerializedObject(bg);
            so.FindProperty("shardsPrefab").objectReferenceValue = shards;
            so.ApplyModifiedPropertiesWithoutUndo();
            PropSystem.SetInstanceForTests(sys);
            try
            {
                var shot = new ShotInfo { Direction = Vector3.forward, HitPoint = Vector3.zero, ImpulseScale = 1f };
                bg.OnShot(shot);
                Assert.IsTrue(bg.Broken);
                Assert.IsFalse(col.enabled);
                Assert.AreEqual(8, sys.Pool.ActiveBodies);
                bg.OnShot(shot);
                Assert.AreEqual(8, sys.Pool.ActiveBodies, "khong vo lai");
                Assert.AreEqual(1, sys.Pool.ActiveCount);
            }
            finally
            {
                PropSystem.SetInstanceForTests(null);
                Object.DestroyImmediate(sysGo);
                Object.DestroyImmediate(shards);
                Object.DestroyImmediate(glass);
            }
        }
    }
}
