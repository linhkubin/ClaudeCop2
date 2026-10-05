using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.Tests.EditMode
{
    /// <summary>Kiem tra asset/prefab/Build Settings cho W6 (T-604, T-621 va cac prefab moi).</summary>
    [Category("Wave6")]
    public class W6AssetIntegrityTests
    {
        const string LevelPrefab = "Assets/_Game/Level/Level_01.prefab";

        [Test, Category("T604")]
        public void BuildSettings_OnlyTitleThenLevel01()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            Assert.AreEqual(2, scenes.Length, "Build Settings phai co dung 2 scene bat; thuc te: " + string.Join(", ", scenes));
            StringAssert.EndsWith("Title.unity", scenes[0], "Scene 0 phai la Title");
            StringAssert.EndsWith("Level_01.unity", scenes[1], "Scene 1 phai la Level_01");
        }

        [Test, Category("T604")]
        public void SampleScene_IsDeleted()
        {
            Assert.IsEmpty(AssetDatabase.FindAssets("SampleScene t:Scene"), "SampleScene.unity phai bi xoa (T-604)");
        }

        [Test]
        public void Prefabs_HaveNoMissingScripts()
        {
            var bad = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game/Prefabs", "Assets/_Game/Level" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    int n = 0;
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                        n += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                    if (n > 0) bad.Add(path + " (" + n + ")");
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            Assert.IsEmpty(bad, "Prefab co Missing Script: " + string.Join("; ", bad));
        }

        [Test, Category("T621")]
        public void Level01_M3Markers_NamesCountsAndPeek()
        {
            var root = PrefabUtility.LoadPrefabContents(LevelPrefab);
            try
            {
                var all = root.GetComponentsInChildren<Transform>(true);
                var m3 = all.Where(t => Regex.IsMatch(t.name, "^(PropSlot_|GrenadierSpawn_|ShieldSpawn_)")).ToArray();
                if (m3.Length == 0) Assert.Ignore("Level_01.prefab chua co marker M3 (T-621 chua xong)");

                var bad = m3.Where(t => !Regex.IsMatch(t.name, @"^(PropSlot_(Barrel|Box|Glass)|GrenadierSpawn|ShieldSpawn)_P[1-3]_W\d+_\d{2}$")).Select(t => t.name).ToArray();
                Assert.IsEmpty(bad, "Ten marker sai quy uoc: " + string.Join(", ", bad));

                foreach (var t in m3.Where(t => t.name.StartsWith("GrenadierSpawn_") || t.name.StartsWith("ShieldSpawn_")))
                    Assert.IsNotNull(t.Find("Peek"), t.name + " thieu con 'Peek'");

                int Count(string prefix, int p) => m3.Count(t => t.name.StartsWith(prefix + "_P" + p + "_"));
                Assert.AreEqual(0, Count("GrenadierSpawn", 1), "P1 khong co Grenadier");
                Assert.AreEqual(2, Count("GrenadierSpawn", 2), "P2 can 2 Grenadier");
                Assert.AreEqual(3, Count("GrenadierSpawn", 3), "P3 can 3 Grenadier");
                Assert.AreEqual(0, Count("ShieldSpawn", 1), "P1 khong co Human Shield");
                Assert.AreEqual(1, Count("ShieldSpawn", 2), "P2 can 1 Human Shield");
                Assert.That(Count("ShieldSpawn", 3), Is.InRange(1, 2), "P3 can 1-2 Human Shield");
                for (int p = 1; p <= 3; p++)
                {
                    Assert.AreEqual(2, Count("PropSlot_Barrel", p), "P" + p + " can 2 thung no");
                    Assert.That(Count("PropSlot_Glass", p), Is.InRange(2, 4), "P" + p + " can 2-4 kinh");
                    Assert.That(Count("PropSlot_Box", p), Is.InRange(4, 6), "P" + p + " can 4-6 hop");
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [Test, Category("T621")]
        public void Level01_BarrelsKeepAwayFromHostages_ExceptOneTrapInP3()
        {
            var root = PrefabUtility.LoadPrefabContents(LevelPrefab);
            try
            {
                var all = root.GetComponentsInChildren<Transform>(true);
                var barrels = all.Where(t => t.name.StartsWith("PropSlot_Barrel_")).ToArray();
                if (barrels.Length == 0) Assert.Ignore("Chua co thung no (T-621 chua xong)");
                var hostagePts = all.Where(t => t.name.StartsWith("HostageSpawn_") || (t.name == "Peek" && t.parent != null && t.parent.name.StartsWith("HostageSpawn_"))).ToArray();
                var near = barrels.Where(b => hostagePts.Any(h => Vector3.Distance(b.position, h.position) <= 3.5f)).ToArray();
                var nearOutsideP3 = near.Where(b => !b.name.Contains("_P3_")).Select(b => b.name).ToArray();
                Assert.IsEmpty(nearOutsideP3, "Thung gan con tin (<=3.5 m) chi duoc o P3: " + string.Join(", ", nearOutsideP3));
                Assert.LessOrEqual(near.Length, 1, "Chi 1 thung bay duoc gan con tin; thuc te " + near.Length);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
