using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ClaudeCop.Camera;
using ClaudeCop.UI;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// Ghep Scenes/Gameplay/Level_Chain.unity: Level 1 + Level 2 trong MOT scene (khong load, khong cat). Ban sao Level_01.unity;
    /// noi that ngan hang cua Level 1 (sanh nho) duoc go va thay bang sanh giao dich cua Level 2 dat ngay sau cua chinh (xoay 90 do);
    /// Phase 4-6 = Level 2 noi tiep sau Phase 3 bang rail di tu cua vao sanh. Het Phase 3: bang ket qua (Continue / Home), Continue chay tiep.
    /// Chay lai an toan (tao lai tu Level_01.unity moi lan). Menu: ClaudeCop/Game/Assemble Level Chain (1+2).
    /// </summary>
    public static class LevelChainAssembler
    {
        public const string ChainScenePath = "Assets/_Game/Scenes/Gameplay/Level_Chain.unity";
        const string SourceScene = "Assets/_Game/Scenes/Gameplay/Level_01.unity";
        // Cua chinh Level 1 o x=45.3, z=35, nhin +X. Mat tien kinh Level 2 (z local -0.3) dat tai x=46.0 (mat trong tuong mat tien Level 1).
        static readonly Vector3 AnchorPos = new Vector3(46.0f + 0.3f, 1f, 35f);
        const float AnchorYaw = 90f;

        // Noi that Level 1 chong len sanh Level 2.
        static readonly string[] RemoveFromLevel1 =
        {
            "Bank_Main_South", "Bank_Main_North", "Bank_Main_Back", "Lobby_Floor", "Lobby_Upper",
            "Lobby_RevolvingDoor", "Lobby_RevolvingDoor_Frame", "Lobby_Counter", "Lobby_Counter_N", "Backdrop_East", "Backdrop_North",
        };

        [MenuItem("ClaudeCop/Game/Assemble Level Chain (1+2)")]
        public static void Assemble()
        {
            Level02Assembler.EnsurePreset();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Level02Builder.PrefabPath) == null) Level02Builder.Build();
            AssetDatabase.DeleteAsset(ChainScenePath);
            AssetDatabase.CopyAsset(SourceScene, ChainScenePath);
            var scene = EditorSceneManager.OpenScene(ChainScenePath, OpenSceneMode.Single);

            // 1) go noi that Level 1
            var l1 = FindRoot(scene, "Level_01");
            if (PrefabUtility.IsPartOfPrefabInstance(l1)) PrefabUtility.UnpackPrefabInstance(l1, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            int removed = 0;
            foreach (var t in l1.GetComponentsInChildren<Transform>(true))
                if (t != null && System.Array.IndexOf(RemoveFromLevel1, t.name) >= 0) { Object.DestroyImmediate(t.gameObject); removed++; }

            // Diem camera cuoi cua Level 1 (cua chinh) de noi rail vao sanh
            Vector3 l1End = Vector3.zero;
            foreach (var t in l1.GetComponentsInChildren<Transform>(true)) if (t.name == "CamPoint_P3_S6") l1End = t.position;

            // 2) dat Level 2 sau cua chinh
            var l2 = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Level02Builder.PrefabPath), scene);
            l2.name = "Level_02";
            l2.transform.SetPositionAndRotation(AnchorPos, Quaternion.Euler(0f, AnchorYaw, 0f));
            PrefabUtility.UnpackPrefabInstance(l2, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            foreach (var t in l2.GetComponentsInChildren<Transform>(true))
                if (t != null && t.name == "Plaza_Floor") { Object.DestroyImmediate(t.gameObject); break; }

            // Rail P1_S1 cua Level 2: bat dau tu goc camera cuoi Level 1, di qua cua vao sanh (bo diem ngoai cua)
            Transform h1 = null, h2 = null;
            foreach (var t in l2.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "RailHint_P1_S1_01") h1 = t;
                else if (t.name == "RailHint_P1_S1_02") h2 = t;
            }
            if (h1 != null && l1End != Vector3.zero) h1.position = l1End;
            if (h2 != null) Object.DestroyImmediate(h2.gameObject);

            // Nguong cua: san noi tu them truoc cua (Lobby_Floor da go)
            var floorMat = FindMaterial(l2, "Hall_Floor");
            var threshold = GameObject.CreatePrimitive(PrimitiveType.Cube);
            threshold.name = "Threshold_Floor";
            threshold.transform.SetParent(l1.transform, true);
            threshold.transform.position = new Vector3(46.0f, 0.5f, 35f);
            threshold.transform.localScale = new Vector3(2.4f, 1f, 14f);
            if (floorMat != null) threshold.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            // 3) ghep Phase 4-6 vao cung scene
            LevelAssembler.Run(new LevelAssembler.LevelSpec
            {
                scenePath = ChainScenePath, rootName = "Level_02", logName = "LevelChainAssembler",
                waves = Level02Assembler.Waves, titles = Level02Assembler.Titles, moveShots = new[] { 1, 4 }, keyedMoves = true,
                append = true, namePrefix = "L2_", baseYaw = AnchorYaw, linkSpeed = 7.6f,
            });

            // 4) bang ket qua het Level 1 + presenter
            scene = EditorSceneManager.GetActiveScene();
            var rig = FindRoot(scene, "CameraRig").GetComponent<PhaseDirector>();
            if (rig.phases.Count >= 4) rig.phases[2].showResultsAfter = true;
            EditorUtility.SetDirty(rig);
            if (Object.FindFirstObjectByType<StageResultPresenter>() == null)
            {
                var ui = new GameObject("StageResultUI");
                var presenter = ui.AddComponent<StageResultPresenter>();
                var pso = new SerializedObject(presenter);
                pso.FindProperty("winPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WinView>("Assets/_Game/Prefabs/UI/WinPanel.prefab");
                pso.ApplyModifiedPropertiesWithoutUndo();
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(ui, scene);
            }
            if (Object.FindFirstObjectByType<ChainStartSetup>() == null)
            {
                var cs = new GameObject("ChainStartSetup").AddComponent<ChainStartSetup>();
                var cso = new SerializedObject(cs);
                cso.FindProperty("doorToOpen").objectReferenceValue = Object.FindFirstObjectByType<DoorOpener>();
                cso.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AddToBuildSettings();
            Debug.Log("[LevelChainAssembler] Done: go " + removed + " vat noi that Level 1, " + rig.phases.Count + " Phase (Level 1: 3, Level 2: 3). Bang ket qua sau Phase 3.");
        }

        /// <summary>Them hang nut chon level vao Title.unity (Content cua TitlePanel). Idempotent. Menu: ClaudeCop/Game/Add Level Select To Title.</summary>
        [MenuItem("ClaudeCop/Game/Add Level Select To Title")]
        public static void AddLevelSelectToTitle()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/Title.unity", OpenSceneMode.Single);
            var view = Object.FindFirstObjectByType<TitleView>();
            if (view == null) { Debug.LogWarning("[LevelChainAssembler] Khong thay TitleView"); return; }
            var content = view.transform.Find("Content");
            Transform parent = view.transform;
            if (content != null) { var safe = content.Find("SafeArea"); parent = safe != null ? safe : content; }
            var old = parent.Find("LevelSelect"); if (old != null) Object.DestroyImmediate(old.gameObject);
            var go = new GameObject("LevelSelect", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = new Vector2(0f, -560f);
            go.AddComponent<LevelSelectPresenter>();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[LevelChainAssembler] Da them LevelSelect vao Title (" + parent.name + ").");
        }

        static GameObject FindRoot(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var g in scene.GetRootGameObjects()) if (g.name == name) return g;
            return null;
        }

        static Material FindMaterial(GameObject root, string objName)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == objName) { var r = t.GetComponent<MeshRenderer>(); if (r != null) return r.sharedMaterial; }
            return null;
        }

        static void AddToBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!list.Exists(s => s.path == ChainScenePath)) { list.Add(new EditorBuildSettingsScene(ChainScenePath, true)); EditorBuildSettings.scenes = list.ToArray(); }
        }
    }
}
