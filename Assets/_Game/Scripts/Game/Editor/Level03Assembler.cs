using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ClaudeCop.Enemy;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// Ghep Scenes/Gameplay/Level_03.unity (Tang lung va van phong) tu Level_03.prefab (3 Phase x 6 Shot) bang loi chung <see cref="LevelAssembler"/>.
    /// Lan dau: nhan doi Level_01.unity (giu Camera/UI/He thong), bo Level_01 + DoorOpener_Main, dat prefab Level_03. Chay lai an toan.
    /// Luat moi Level 3: Justice shot (preset L3 bat Justice cho moi enemy cua dot), canh hai H01 (GagFall), cua ham mo khi ha ten cuoi.
    /// </summary>
    public static class Level03Assembler
    {
        public const string ScenePath = "Assets/_Game/Scenes/Gameplay/Level_03.unity";
        const string SourceScene = "Assets/_Game/Scenes/Gameplay/Level_01.unity";
        const string PresetDir = "Assets/_Game/Prefabs/Enemies/Data/";

        static LevelAssembler.WaveDef W(int p, int s, string preset, float fov, float blend = -1f, int conc = 2) =>
            new LevelAssembler.WaveDef { phase = p, shot = s, preset = preset, pickup = null, fov = fov, blend = blend, maxConcurrent = conc };

        // FOV doc toi thieu nhu Level 2; S6 = kill-zoom (32).
        internal static readonly LevelAssembler.WaveDef[] Waves =
        {
            W(1,2,"L3_Plain",40f), W(1,3,"L3",36f,0.7f), W(1,5,"L3",38f), W(1,6,"L3",32f,0.7f),
            W(2,2,"L3_Plain",40f), W(2,3,"L3_Plain",36f,0.7f), W(2,5,"L3_Plain",38f), W(2,6,"L3_Plain",32f,0.7f),
            W(3,2,"L3",40f), W(3,3,"L3",36f,0.7f), W(3,5,"L3_Rush",38f,-1f,2), W(3,6,"L3_Plain",32f,0.7f),
        };

        internal static readonly string[] Titles = { "STAGE 3-1", "STAGE 3-2", "STAGE 3-3" };

        [MenuItem("ClaudeCop/Game/Assemble Level_03")]
        public static void Assemble()
        {
            EnsurePresets();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Level03Builder.PrefabPath) == null) Level03Builder.Build();
            if (!System.IO.File.Exists(ScenePath)) AssetDatabase.CopyAsset(SourceScene, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var roots = new List<GameObject>(scene.GetRootGameObjects());
            if (!roots.Exists(g => g.name == "Level_03"))
            {
                foreach (var g in roots)
                    if (g.name == "Level_01" || g.name == "DoorOpener_Main") Object.DestroyImmediate(g);
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Level03Builder.PrefabPath), scene);
                inst.name = "Level_03";
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            LevelAssembler.Run(new LevelAssembler.LevelSpec
            {
                scenePath = ScenePath, rootName = "Level_03", logName = "Level03Assembler",
                waves = Waves, titles = Titles, moveShots = new[] { 1, 4 }, keyedMoves = true, linkSpeed = 7.6f, linkFirst = true,
                exitCamPoint = Level03Builder.ExitCamPoint,
            });
            scene = SceneManager.GetActiveScene();
            AddDoors(scene, "Level_03", "");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>
        /// Dat (hoac dat lai) DoorOpener cua Level 3 trong scene: cua ham (mo khi Wave_P3_W6 Cleared) va thang may (mo khi canh hai H01 xong).
        /// Tra ve DoorOpener cua ham (cua cuoi level, ChainStartSetup mo san khi bat dau o level sau).
        /// </summary>
        internal static DoorOpener AddDoors(Scene scene, string levelRoot, string prefix)
        {
            GameObject level = null; var waves = new Dictionary<string, EncounterWave>();
            foreach (var g in scene.GetRootGameObjects())
            {
                if (g.name == levelRoot) level = g;
                if (g.name == prefix + "DoorOpener_Vault" || g.name == prefix + "DoorOpener_Elevator") Object.DestroyImmediate(g);
            }
            foreach (var g in scene.GetRootGameObjects())
                foreach (var w in g.GetComponentsInChildren<EncounterWave>(true)) waves[w.name] = w;
            if (level == null) { Debug.LogWarning("[Level03Assembler] Khong thay " + levelRoot); return null; }
            Transform Find(string n) { foreach (var t in level.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t; return null; }

            DoorOpener Make(string name, string wave, float duration, float delay, params (Transform pivot, float yaw, Vector3 slide)[] leaves)
            {
                var go = new GameObject(prefix + name);
                SceneManager.MoveGameObjectToScene(go, scene);
                var d = go.AddComponent<DoorOpener>();
                var so = new SerializedObject(d);
                waves.TryGetValue(prefix + wave, out var w);
                if (w == null) Debug.LogWarning("[Level03Assembler] Khong thay wave " + prefix + wave);
                so.FindProperty("trigger").objectReferenceValue = w;
                so.FindProperty("duration").floatValue = duration;
                so.FindProperty("delay").floatValue = delay;
                var arr = so.FindProperty("leaves"); arr.arraySize = leaves.Length;
                for (int i = 0; i < leaves.Length; i++)
                {
                    var e = arr.GetArrayElementAtIndex(i);
                    e.FindPropertyRelative("pivot").objectReferenceValue = leaves[i].pivot;
                    e.FindPropertyRelative("yawDelta").floatValue = leaves[i].yaw;
                    e.FindPropertyRelative("slide").vector3Value = leaves[i].slide;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                return d;
            }

            // Thang may: canh truot (local z cua group Decor = z cua level), mo ngay khi H01 xong -> chuong thang may luc camera di (giam tai).
            Make("DoorOpener_Elevator", "Wave_P2_W3", 1.0f, 0.3f,
                (Find("Elevator_Door_L"), 0f, new Vector3(0f, 0f, -0.95f)), (Find("Elevator_Door_R"), 0f, new Vector3(0f, 0f, 0.95f)));
            // Cua ham: xoay ra ngoai quanh ban le. L3-L4: mo cham (cua nang) 0.3 s sau khi ten cuoi bi ha, xong ~1.9 s - trong luc camera
            // quay sang goc dwell CamPoint_P3_S7 (blend 1.0 s sau kill-zoom 0.6 + nghi 0.3) -> nguoi choi thay ca qua trinh mo truoc bang ket qua.
            // Goc mo -90 (truoc -100): la nam sat tuong Bac cau thang (z 163.8..164.2), khong xuyen tuong z 164.5 / tran.
            return Make("DoorOpener_Vault", "Wave_P3_W6", 1.6f, 0.3f, (Find("VaultDoor_Pivot"), -90f, Vector3.zero));
        }

        /// <summary>Preset Level 3: L3 (Justice cho moi enemy cua dot), L3_Plain (khong Justice), L3_Rush (don dap: hide 0, stagger 0-0.25).</summary>
        internal static void EnsurePresets()
        {
            Make("L3", 2.5f, 0.8f, 0.4f, 1f, true, 1f);
            Make("L3_Plain", 2.5f, 0.8f, 0.4f, 1f, false, 0f);
            Make("L3_Rush", 2.5f, 0f, 0f, 0.25f, false, 0f);
        }

        static void Make(string id, float reticle, float hide, float smin, float smax, bool justice, float frac)
        {
            string path = PresetDir + "EnemyPreset_" + id + ".asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyPreset>(path) != null) return;
            var p = ScriptableObject.CreateInstance<EnemyPreset>();
            var so = new SerializedObject(p);
            so.FindProperty("reticleTime").floatValue = reticle;
            so.FindProperty("hideTime").floatValue = hide;
            so.FindProperty("staggerMin").floatValue = smin;
            so.FindProperty("staggerMax").floatValue = smax;
            so.FindProperty("justiceEnabled").boolValue = justice;
            so.FindProperty("justiceFraction").floatValue = frac;
            so.FindProperty("useHostages").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(p, path);
            AssetDatabase.SaveAssets();
        }
    }
}
