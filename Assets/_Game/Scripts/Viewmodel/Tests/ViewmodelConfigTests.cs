using ClaudeCop.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.Viewmodel.Tests
{
    public class ViewmodelConfigTests
    {
        const string ConfigPath = "Assets/_Game/Prefabs/Viewmodel/ViewmodelConfig.asset";

        static ViewmodelConfig Load()
        {
            var c = AssetDatabase.LoadAssetAtPath<ViewmodelConfig>(ConfigPath);
            Assert.IsNotNull(c, "Thieu " + ConfigPath + " (chay ClaudeCop/Viewmodel/Build Viewmodel)");
            return c;
        }

        [Test]
        public void Config_HasAllThreeWeaponKinds_WithPrefab()
        {
            var c = Load();
            foreach (WeaponKind k in System.Enum.GetValues(typeof(WeaponKind)))
            {
                var e = c.Get(k);
                Assert.IsNotNull(e, "Thieu entry " + k);
                Assert.IsNotNull(e.prefab, "Thieu prefab " + k);
            }
        }

        [Test]
        public void Prefabs_HonorNodeContract()
        {
            var c = Load();
            foreach (WeaponKind k in System.Enum.GetValues(typeof(WeaponKind)))
            {
                var t = c.Get(k).prefab.transform;
                Assert.AreEqual("Viewmodel_" + k, c.Get(k).prefab.name);
                Assert.IsNotNull(t.GetComponent<Animator>(), k + ": thieu Animator");
                Assert.IsNotNull(t.Find("Pivot/Body"), k + ": thieu Pivot/Body");
                Assert.IsNotNull(t.Find("Pivot/Body/Muzzle"), k + ": thieu Muzzle");
                Assert.IsNotNull(t.Find("Pivot/Body/Muzzle/Flash"), k + ": thieu Muzzle/Flash");
                string mover = k == WeaponKind.Pistol ? "Slide" : (k == WeaponKind.Shotgun ? "Pump" : "Bolt");
                string store = k == WeaponKind.Shotgun ? "ShellTube" : "Magazine";
                Assert.IsNotNull(t.Find("Pivot/Body/" + mover), k + ": thieu " + mover);
                Assert.IsNotNull(t.Find("Pivot/Body/" + store), k + ": thieu " + store);
                Assert.IsEmpty(t.GetComponentsInChildren<Collider>(true), k + ": viewmodel khong duoc co collider");
            }
        }

        [Test]
        public void Prefabs_AreOnViewmodelLayer_AndHaveAllStates()
        {
            var c = Load();
            int layer = LayerMask.NameToLayer("Viewmodel");
            Assert.GreaterOrEqual(layer, 0, "Chua co layer Viewmodel");
            foreach (WeaponKind k in System.Enum.GetValues(typeof(WeaponKind)))
            {
                var go = c.Get(k).prefab;
                foreach (var tr in go.GetComponentsInChildren<Transform>(true))
                    Assert.AreEqual(layer, tr.gameObject.layer, k + "/" + tr.name + " sai layer");
                var rc = go.GetComponent<Animator>().runtimeAnimatorController;
                Assert.IsNotNull(rc);
                var clips = rc.animationClips;
                foreach (var n in new[] { "Idle", "Fire", "Reload", "Equip", "DryFire" })
                {
                    bool found = false;
                    foreach (var cl in clips) if (cl != null && cl.name.EndsWith("_" + n)) found = true;
                    Assert.IsTrue(found, k + ": thieu clip " + n);
                }
            }
        }

        [Test]
        public void Layout_InterpolatesByAspect_AndClamps()
        {
            var c = ScriptableObject.CreateInstance<ViewmodelConfig>();
            c.narrowAspect = 0.4f; c.wideAspect = 0.6f;
            c.anchorNarrow = new Vector2(0.6f, 0.1f); c.anchorWide = new Vector2(0.7f, 0.2f);
            c.scaleNarrow = 1f; c.scaleWide = 2f;
            c.Layout(0.5f, out var a, out var s);
            Assert.AreEqual(0.65f, a.x, 1e-4f);
            Assert.AreEqual(1.5f, s, 1e-4f);
            c.Layout(0.1f, out a, out s);
            Assert.AreEqual(0.6f, a.x, 1e-4f);
            c.Layout(2f, out a, out s);
            Assert.AreEqual(2f, s, 1e-4f);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void MuzzleFlashOfFxConfig_IsDisabled()
        {
            var fx = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/_Game/Prefabs/FX/FxConfig.asset");
            Assert.IsNotNull(fx);
            var so = new SerializedObject(fx);
            Assert.IsFalse(so.FindProperty("muzzleEnabled").boolValue, "Viewmodel tu phat muzzle: FxConfig.muzzleEnabled phai = false");
        }
    }
}
