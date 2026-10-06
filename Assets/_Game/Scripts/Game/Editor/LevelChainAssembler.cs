using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ClaudeCop.Camera;
using ClaudeCop.UI;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// Ghep Scenes/Gameplay/Level_Chain.unity: Level 1 + 2 + 3 + 4 trong MOT scene (khong load, khong cat). Ban sao Level_01.unity;
    /// noi that ngan hang cua Level 1 (sanh nho) duoc go va thay bang sanh giao dich cua Level 2 dat ngay sau cua chinh (xoay 90 do);
    /// Phase 4-6 = Level 2 noi tiep sau Phase 3 bang rail di tu cua vao sanh. Het Phase 3: bang ket qua (Continue / Home), Continue chay tiep.
    /// Chay lai an toan (tao lai tu Level_01.unity moi lan). Menu: ClaudeCop/Game/Assemble Level Chain (1-4).
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

        [MenuItem("ClaudeCop/Game/Assemble Level Chain (1-4)")]
        public static void Assemble()
        {
            Level02Assembler.EnsurePreset();
            Level03Assembler.EnsurePresets();
            Level04Assembler.EnsurePresets();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Level04Builder.PrefabPath) == null) Level04Builder.Build();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Level03Builder.PrefabPath) == null) Level03Builder.Build();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Level02Builder.PrefabPath) == null) Level02Builder.Build();
            Level01Assembler.ApplyMainDoorFix();   // L1-DOOR: enemy cuoi Level 1 buoc ra cua chinh (idempotent, truoc khi copy Level_01.unity)
            AssetDatabase.DeleteAsset(ChainScenePath);
            AssetDatabase.CopyAsset(SourceScene, ChainScenePath);
            var scene = EditorSceneManager.OpenScene(ChainScenePath, OpenSceneMode.Single);
            // Ban sao Level_01.unity co the mang StandaloneLevelRedirect (cua so vao chuoi) -> bo trong chuoi (tranh nap lai chinh no).
            foreach (var rd in Object.FindObjectsByType<StandaloneLevelRedirect>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(rd.gameObject);

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
            if (Object.FindFirstObjectByType<ReloadHintPresenter>() == null)
            {
                var hint = new GameObject("ReloadHintUI");
                hint.AddComponent<ReloadHintPresenter>();
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(hint, scene);
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

            // 5) Level 3 noi sau Level 2: cung goc/huong voi Level 2 (toa do local trung), ray P1_S1 bat dau dung goc cuoi Level 2.
            int removedL2 = AppendLevel3(scene, FindRoot(scene, "Level_02"));
            scene = EditorSceneManager.GetActiveScene();
            rig = FindRoot(scene, "CameraRig").GetComponent<PhaseDirector>();
            if (rig.phases.Count >= 7) rig.phases[5].showResultsAfter = true;   // het Level 2: bang ket qua; Level 3 la level cuoi chuoi (thang -> WinPanel)
            EditorUtility.SetDirty(rig);
            Level03Assembler.AddDoors(scene, "Level_03", "L3_");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            // 6) Level 4 (kho tien) noi sau Level 3: cung goc/huong (toa do local trung), ray P1_S1 bat dau dung goc cuoi Level 3, qua cua ham xuong cau thang bao mat.
            int removedL3 = AppendLevel4(scene, FindRoot(scene, "Level_03"));
            scene = EditorSceneManager.GetActiveScene();
            rig = FindRoot(scene, "CameraRig").GetComponent<PhaseDirector>();
            if (rig.phases.Count >= 7) rig.phases[5].showResultsAfter = true;    // het Level 2
            if (rig.phases.Count >= 10) rig.phases[8].showResultsAfter = true;   // het Level 3; Level 4 la level cuoi chuoi (thang -> WinPanel)
            EditorUtility.SetDirty(rig);
            DoorOpener vault = null;
            foreach (var g in scene.GetRootGameObjects())
                if (g.name == "L3_DoorOpener_Vault") vault = g.GetComponent<DoorOpener>();
            if (vault == null) vault = Level03Assembler.AddDoors(scene, "Level_03", "L3_");
            var shutter = Level04Assembler.AddDoors(scene, "Level_04", "L4_");
            var chainSetup = Object.FindFirstObjectByType<ChainStartSetup>();
            if (chainSetup != null)
            {
                var cso = new SerializedObject(chainSetup);
                var later = cso.FindProperty("laterDoors"); later.arraySize = 3;
                later.GetArrayElementAtIndex(0).objectReferenceValue = null;    // het Level 2: khong co cua (cau thang)
                later.GetArrayElementAtIndex(1).objectReferenceValue = vault;   // het Level 3: cua ham (noi Level 4)
                later.GetArrayElementAtIndex(2).objectReferenceValue = shutter; // het Level 4: cua cuon nap tien (noi Level 5 - chua co)
                cso.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AddToBuildSettings();
            EnsureStandaloneRedirects();
            EditorSceneManager.OpenScene(ChainScenePath, OpenSceneMode.Single);
            Debug.Log("[LevelChainAssembler] Done: go " + removed + " vat noi that Level 1, " + removedL2 + " vat Level 2, " + removedL3 + " vat Level 3, " + rig.phases.Count
                + " Phase (Level 1-4, moi level 3 Phase). Bang ket qua sau Phase 3, 6 va 9.");
        }

        // Vat Level 2 chong len / chan duong vao Level 3 (tuong sau sanh thay bang tuong co cua cua Level 3; lan can kinh cau thang Dong nam tren ray).
        static readonly string[] RemoveFromLevel2 = { "Wall_Back", "Mezzanine_Backwall", "Stairs_Rail_In_E" };

        /// <summary>Dat Level_03 (unpack) cung transform voi Level_02, go group Approach_Standalone va cac vat L2 chong len, ghep Phase 7-9.</summary>
        static int AppendLevel3(UnityEngine.SceneManagement.Scene scene, GameObject l2)
        {
            int removed = 0;
            foreach (var t in l2.GetComponentsInChildren<Transform>(true))
                if (t != null && System.Array.IndexOf(RemoveFromLevel2, t.name) >= 0) { Object.DestroyImmediate(t.gameObject); removed++; }
            var l3 = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Level03Builder.PrefabPath), scene);
            l3.name = "Level_03";
            l3.transform.SetPositionAndRotation(l2.transform.position, l2.transform.rotation);
            PrefabUtility.UnpackPrefabInstance(l3, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            foreach (var t in l3.GetComponentsInChildren<Transform>(true))
                if (t != null && t.name == "Approach_Standalone") { Object.DestroyImmediate(t.gameObject); break; }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            LevelAssembler.Run(new LevelAssembler.LevelSpec
            {
                scenePath = ChainScenePath, rootName = "Level_03", logName = "LevelChainAssembler",
                waves = Level03Assembler.Waves, titles = Level03Assembler.Titles, moveShots = new[] { 1, 4 }, keyedMoves = true,
                append = true, namePrefix = "L3_", baseYaw = AnchorYaw, linkSpeed = 7.6f, linkFirst = true,
                exitCamPoint = Level03Builder.ExitCamPoint,   // L3-L4: nhin cua ham mo truoc bang ket qua
            });
            return removed;
        }

        // Vat Level 3 chong len / chan duong vao Level 4 (chieu nghi + bac ngan + tuong den sau cua ham thay bang cau thang bao mat cua Level 4).
        static readonly string[] RemoveFromLevel3 =
        {
            "Vault_Landing", "Vault_Dark", "Vault_Side_N", "Vault_Side_S",
            "Vault_Stair_1", "Vault_Stair_2", "Vault_Stair_3", "Vault_Stair_4", "Vault_Stair_5", "Vault_Stair_6",
        };

        /// <summary>Dat Level_04 (unpack) cung transform voi Level_03, go group Approach_Standalone va cac vat L3 chong len, ghep Phase 10-12.</summary>
        static int AppendLevel4(UnityEngine.SceneManagement.Scene scene, GameObject l3)
        {
            int removed = 0;
            foreach (var t in l3.GetComponentsInChildren<Transform>(true))
                if (t != null && (System.Array.IndexOf(RemoveFromLevel3, t.name) >= 0 || t.name == "VaultStair_Stub")) { Object.DestroyImmediate(t.gameObject); removed++; }
            var l4 = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Level04Builder.PrefabPath), scene);
            l4.name = "Level_04";
            l4.transform.SetPositionAndRotation(l3.transform.position, l3.transform.rotation);
            PrefabUtility.UnpackPrefabInstance(l4, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            foreach (var t in l4.GetComponentsInChildren<Transform>(true))
                if (t != null && t.name == "Approach_Standalone") { Object.DestroyImmediate(t.gameObject); break; }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            LevelAssembler.Run(new LevelAssembler.LevelSpec
            {
                scenePath = ChainScenePath, rootName = "Level_04", logName = "LevelChainAssembler",
                waves = Level04Assembler.Waves, titles = Level04Assembler.Titles, moveShots = new[] { 1, 4 }, keyedMoves = true,
                append = true, namePrefix = "L4_", baseYaw = AnchorYaw, linkSpeed = 7.6f, linkFirst = true,
            });
            return removed;
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
            var lsp = go.AddComponent<LevelSelectPresenter>();
            var lso = new SerializedObject(lsp);
            var lc = lso.FindProperty("levelCount"); if (lc != null) lc.intValue = 4;
            // 4 nut vua chieu ngang 1000 (man doc): 4 x 220 + 3 x 40 = 1000 (nhan nut tu co chu, khong xuong dong - LevelSelectPresenter)
            var bs = lso.FindProperty("buttonSize"); if (bs != null) bs.vector2Value = new Vector2(220f, 130f);
            var sp = lso.FindProperty("spacing"); if (sp != null) sp.floatValue = 40f;
            lso.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[LevelChainAssembler] Da them LevelSelect vao Title (" + parent.name + ").");
        }

        /// <summary>Scene level choi rieng (thu tu = chi so level trong chuoi).</summary>
        public static readonly string[] StandaloneScenes =
        {
            "Assets/_Game/Scenes/Gameplay/Level_01.unity", "Assets/_Game/Scenes/Gameplay/Level_02.unity",
            "Assets/_Game/Scenes/Gameplay/Level_03.unity", "Assets/_Game/Scenes/Gameplay/Level_04.unity",
        };

        /// <summary>
        /// L-CHAIN-ENTRY: dat (idempotent) root "ChainEntryRedirect" + StandaloneLevelRedirect(levelIndex = k) vao moi scene Level_0k choi rieng:
        /// Play thang scene do trong Editor -> chay Level_Chain tu level k toi het. Menu: ClaudeCop/Game/Add Chain Redirect To Level Scenes.
        /// </summary>
        [MenuItem("ClaudeCop/Game/Add Chain Redirect To Level Scenes")]
        public static void EnsureStandaloneRedirects()
        {
            for (int k = 0; k < StandaloneScenes.Length; k++)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StandaloneScenes[k]) == null) continue;
                var s = EditorSceneManager.OpenScene(StandaloneScenes[k], OpenSceneMode.Single);
                StandaloneLevelRedirect rd = null;
                foreach (var g in s.GetRootGameObjects()) { var c = g.GetComponent<StandaloneLevelRedirect>(); if (c != null) rd = c; }
                if (rd == null) rd = new GameObject("ChainEntryRedirect").AddComponent<StandaloneLevelRedirect>();
                var so = new SerializedObject(rd);
                so.FindProperty("levelIndex").intValue = k;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(s);
                EditorSceneManager.SaveScene(s);
            }
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
