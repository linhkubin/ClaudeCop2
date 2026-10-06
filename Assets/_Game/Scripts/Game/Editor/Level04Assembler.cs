using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ClaudeCop.Enemy;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// Ghep Scenes/Gameplay/Level_04.unity (Kho tien, GDD vong 10-11) tu Level_04.prefab (3 Phase x 6 Shot) bang loi chung <see cref="LevelAssembler"/>.
    /// Lan dau: nhan doi Level_01.unity (giu Camera/UI/He thong), bo Level_01 + DoorOpener_Main, dat prefab Level_04. Chay lai an toan.
    /// Luat moi Level 4: khien nguoi (ShieldSpawn_* -> Enemy_HumanShield). Enemy moi: X xung phong (tam: Door sau lung), S xa thu (preset L4_Far).
    /// Nhip don dap: stagger ngan (L4: 0.3-0.6 s), maxConcurrent 2-3 moi wave, "Don dap loi vang" (L4_Rush) toi da 4.
    /// Cua vault tron quay mo khi Wave_P3_W3 Cleared (lay hoi truoc dot don dap); cua cuon nap tien (cuoi level, noi Level 5) truot len khi Wave_P3_W6 Cleared.
    /// </summary>
    public static class Level04Assembler
    {
        public const string ScenePath = "Assets/_Game/Scenes/Gameplay/Level_04.unity";
        const string SourceScene = "Assets/_Game/Scenes/Gameplay/Level_01.unity";
        const string PresetDir = "Assets/_Game/Prefabs/Enemies/Data/";

        static LevelAssembler.WaveDef W(int p, int s, string preset, float fov, float blend, int conc) =>
            new LevelAssembler.WaveDef { phase = p, shot = s, preset = preset, pickup = null, fov = fov, blend = blend, maxConcurrent = conc };

        // S3/S6: goc moi trong cung Phase (xoay ~26 do, blend 0.7 s) -> wave moi lo ngay. P3_S6 = kill-zoom (32).
        internal static readonly LevelAssembler.WaveDef[] Waves =
        {
            W(1,2,"L4",40f,-1f,2), W(1,3,"L4",36f,0.7f,2), W(1,5,"L4",38f,-1f,2), W(1,6,"L4",36f,0.7f,3),
            W(2,2,"L4",40f,-1f,2), W(2,3,"L4",36f,0.7f,3), W(2,5,"L4_Far",36f,-1f,2), W(2,6,"L4",36f,0.7f,2),
            W(3,2,"L4",40f,-1f,2), W(3,3,"L4_Far",36f,0.7f,3), W(3,5,"L4_Rush",38f,-1f,4), W(3,6,"L4",32f,0.7f,1),
        };

        internal static readonly string[] Titles = { "STAGE 4-1", "STAGE 4-2", "STAGE 4-3" };

        [MenuItem("ClaudeCop/Game/Assemble Level_04")]
        public static void Assemble()
        {
            EnsurePresets();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Level04Builder.PrefabPath) == null) Level04Builder.Build();
            if (!System.IO.File.Exists(ScenePath)) AssetDatabase.CopyAsset(SourceScene, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var roots = new List<GameObject>(scene.GetRootGameObjects());
            if (!roots.Exists(g => g.name == "Level_04"))
            {
                foreach (var g in roots)
                    if (g.name == "Level_01" || g.name == "DoorOpener_Main") Object.DestroyImmediate(g);
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Level04Builder.PrefabPath), scene);
                inst.name = "Level_04";
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            LevelAssembler.Run(new LevelAssembler.LevelSpec
            {
                scenePath = ScenePath, rootName = "Level_04", logName = "Level04Assembler",
                waves = Waves, titles = Titles, moveShots = new[] { 1, 4 }, keyedMoves = true, linkSpeed = 7.6f, linkFirst = true,
            });
            scene = SceneManager.GetActiveScene();
            AddDoors(scene, "Level_04", "");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>
        /// Dat (hoac dat lai) DoorOpener cua Level 4: cua vault tron (quay mo khi Wave_P3_W3 Cleared) va cua cuon nap tien (truot len khi Wave_P3_W6 Cleared).
        /// Tra ve DoorOpener cua cuon (cua cuoi level, ChainStartSetup mo san khi bat dau o level sau).
        /// </summary>
        internal static DoorOpener AddDoors(Scene scene, string levelRoot, string prefix)
        {
            GameObject level = null; var waves = new Dictionary<string, EncounterWave>();
            foreach (var g in scene.GetRootGameObjects())
            {
                if (g.name == levelRoot) level = g;
                if (g.name == prefix + "DoorOpener_VaultCore" || g.name == prefix + "DoorOpener_LoadingShutter") Object.DestroyImmediate(g);
            }
            foreach (var g in scene.GetRootGameObjects())
                foreach (var w in g.GetComponentsInChildren<EncounterWave>(true)) waves[w.name] = w;
            if (level == null) { Debug.LogWarning("[Level04Assembler] Khong thay " + levelRoot); return null; }
            // GDD vong 11: con tin khong bao gio dung im -> moi wave Level 4 tat hostagesStandStill (lo ~3 s roi tu cui).
            foreach (var kv in waves)
            {
                if (!kv.Key.StartsWith(prefix + "Wave_P")) continue;
                var wso = new SerializedObject(kv.Value);
                wso.FindProperty("hostagesStandStill").boolValue = false;
                wso.ApplyModifiedPropertiesWithoutUndo();
            }
            Transform Find(string n) { foreach (var t in level.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t; return null; }

            DoorOpener Make(string name, string wave, float duration, float delay, Transform pivot, float yaw, Vector3 slide)
            {
                var go = new GameObject(prefix + name);
                SceneManager.MoveGameObjectToScene(go, scene);
                var d = go.AddComponent<DoorOpener>();
                var so = new SerializedObject(d);
                waves.TryGetValue(prefix + wave, out var w);
                if (w == null) Debug.LogWarning("[Level04Assembler] Khong thay wave " + prefix + wave);
                so.FindProperty("trigger").objectReferenceValue = w;
                so.FindProperty("duration").floatValue = duration;
                so.FindProperty("delay").floatValue = delay;
                var arr = so.FindProperty("leaves"); arr.arraySize = 1;
                var e = arr.GetArrayElementAtIndex(0);
                e.FindPropertyRelative("pivot").objectReferenceValue = pivot;
                e.FindPropertyRelative("yawDelta").floatValue = yaw;
                e.FindPropertyRelative("slide").vector3Value = slide;
                so.ApplyModifiedPropertiesWithoutUndo();
                return d;
            }

            // Cua vault tron: ban le o mep Tay, quay ra phia camera (lay hoi 1.5 s truoc dot don dap).
            Make("DoorOpener_VaultCore", "Wave_P3_W3", 1.2f, 0.1f, Find("VaultDoor_Pivot"), -95f, Vector3.zero);
            // Cua cuon nap tien: truot len (local cua group Geometry = truc level), mo sau kill-zoom ten cuoi.
            return Make("DoorOpener_LoadingShutter", "Wave_P3_W6", 1.6f, 0.4f, Find("LoadingShutter_Pivot"), 0f, new Vector3(0f, 4.0f, 0f));
        }

        /// <summary>Preset Level 4: L4 (thuong, don dap nhe: hide 0.6, stagger 0.3-0.6), L4_Far (wave co xa thu: vong +0.5 s),
        /// L4_Rush ("Don dap loi vang": hide 0, stagger 0.5-0.8). Khong Justice (khien nguoi tu bat Justice o tay sung).</summary>
        internal static void EnsurePresets()
        {
            Make("L4", 2.5f, 0.6f, 0.3f, 0.6f);
            Make("L4_Far", 3.0f, 0.6f, 0.3f, 0.6f);
            Make("L4_Rush", 2.5f, 0f, 0.5f, 0.8f);
        }

        static void Make(string id, float reticle, float hide, float smin, float smax)
        {
            string path = PresetDir + "EnemyPreset_" + id + ".asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyPreset>(path) != null) return;
            var p = ScriptableObject.CreateInstance<EnemyPreset>();
            var so = new SerializedObject(p);
            so.FindProperty("reticleTime").floatValue = reticle;
            so.FindProperty("hideTime").floatValue = hide;
            so.FindProperty("staggerMin").floatValue = smin;
            so.FindProperty("staggerMax").floatValue = smax;
            so.FindProperty("justiceEnabled").boolValue = false;
            so.FindProperty("justiceFraction").floatValue = 0f;
            so.FindProperty("useHostages").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(p, path);
            AssetDatabase.SaveAssets();
        }
    }
}
