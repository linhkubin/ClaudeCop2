using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ClaudeCop.Enemy;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// Ghep Scenes/Gameplay/Level_02.unity (Sanh giao dich) tu Level_02.prefab (3 Phase x 6 Shot) bang loi chung <see cref="LevelAssembler"/>.
    /// Lan dau: nhan doi Level_01.unity (giu Camera/UI/He thong), bo Level_01 + DoorOpener_Main, dat prefab Level_02. Chay lai an toan.
    /// Level 2: nhieu enemy hon (23, toi da 2-3 cung luc), 3 con tin, enemy dung san (EnemyStand_*), FOV Combat hep (zoom).
    /// </summary>
    public static class Level02Assembler
    {
        const string ScenePath = "Assets/_Game/Scenes/Gameplay/Level_02.unity";
        const string SourceScene = "Assets/_Game/Scenes/Gameplay/Level_01.unity";
        const string PresetPath = "Assets/_Game/Prefabs/Enemies/Data/EnemyPreset_L2.asset";

        // fov = FOV doc TOI THIEU (do) cua shot Combat; FOV that = max(fov, FOV can de vua cac muc tieu), tran baseFov (45) o man doc.
        // Level 1 dung 58-60 (luon bi chan o 45). Level 2: 40 (S2), 36 (S3), 38 (S5), 32 (S6 = kill-zoom o wave cuoi moi phase).
        static LevelAssembler.WaveDef W(int p, int s, float fov, float blend = -1f, int conc = 2) =>
            new LevelAssembler.WaveDef { phase = p, shot = s, preset = "L2", pickup = null, fov = fov, blend = blend, maxConcurrent = conc };

        static readonly LevelAssembler.WaveDef[] Waves =
        {
            W(1,2,40f), W(1,3,36f,0.7f), W(1,5,38f), W(1,6,32f,0.7f),
            W(2,2,40f), W(2,3,36f,0.7f), W(2,5,38f), W(2,6,32f,0.7f),
            W(3,2,40f), W(3,3,36f,0.7f), W(3,5,38f), W(3,6,32f,0.7f,3),
        };

        static readonly string[] Titles = { "STAGE 2-1", "STAGE 2-2", "STAGE 2-3" };

        [MenuItem("ClaudeCop/Game/Assemble Level_02")]
        public static void Assemble()
        {
            EnsurePreset();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Level02Builder.PrefabPath) == null) Level02Builder.Build();
            if (!System.IO.File.Exists(ScenePath)) AssetDatabase.CopyAsset(SourceScene, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var roots = new List<GameObject>(scene.GetRootGameObjects());
            if (!roots.Exists(g => g.name == "Level_02"))
            {
                foreach (var g in roots)
                    if (g.name == "Level_01" || g.name == "DoorOpener_Main") Object.DestroyImmediate(g);
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Level02Builder.PrefabPath), scene);
                inst.name = "Level_02";
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            LevelAssembler.Run(new LevelAssembler.LevelSpec
            {
                scenePath = ScenePath, rootName = "Level_02", logName = "Level02Assembler",
                waves = Waves, titles = Titles, moveShots = new[] { 1, 4 }, keyedMoves = true,
            });
        }

        // Preset Level 2: reticle 2.5 s, khong Justice (luat moi cua level 3), co con tin.
        static void EnsurePreset()
        {
            if (AssetDatabase.LoadAssetAtPath<EnemyPreset>(PresetPath) != null) return;
            var p = ScriptableObject.CreateInstance<EnemyPreset>();
            var so = new SerializedObject(p);
            so.FindProperty("reticleTime").floatValue = 2.5f;
            so.FindProperty("hideTime").floatValue = 0.8f;
            so.FindProperty("staggerMin").floatValue = 0.4f;
            so.FindProperty("staggerMax").floatValue = 1f;
            so.FindProperty("justiceEnabled").boolValue = false;
            so.FindProperty("justiceFraction").floatValue = 0f;
            so.FindProperty("useHostages").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(p, PresetPath);
            AssetDatabase.SaveAssets();
        }
    }
}
