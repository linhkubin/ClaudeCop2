using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using ClaudeCop.Camera;
using ClaudeCop.Camera.Editor;
using ClaudeCop.Enemy;
using ClaudeCop.Props;
using ClaudeCop.RankScore;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// Loi ghep chung cho cac level (T-403, tach ra tu Level01Assembler): dung lai phan "Rails / Encounters / Shots" va danh sach Phase
    /// cua scene Gameplay tu prefab level (3 Phase x 6 Shot). Chay lai nhieu lan an toan (xoa va dung lai 3 nhom do). Dat/noi: FxSystems, RankScoreSystems.
    /// Moi level chi can mot LevelSpec (Level01Assembler, Level02Assembler).
    /// </summary>
    public static class LevelAssembler
    {
        const string Pre = "Assets/_Game/Prefabs/";

        public class WaveDef
        {
            public int phase, shot; public string preset; public string pickup; // pickup: "Shotgun" | "MachineGun" | null
            public float blend = -1f; public float fov = 60f;
            /// <summary>&gt; 0: ghi maxConcurrent cua wave (0 = giu mac dinh cua wave).</summary>
            public int maxConcurrent;
        }

        public class LevelSpec
        {
            public string scenePath, rootName, logName;
            public WaveDef[] waves;
            public string[] titles;
            public int[] moveShots = { 1, 4 };
            /// <summary>true: shot Move co lookKeys (giu huong nhin cua shot Combat truoc, den cuoi rail xoay dan ve huong shot Combat ke) -
            /// rail khong can tiep tuyen khop yaw shot (truot ngang). Mac dinh false (Level_01).</summary>
            public bool keyedMoves;
            /// <summary>true: noi them vao scene da co (giu Rails/Encounters/Shots/Props va cac Phase cu, them Phase moi o cuoi) - chuoi level lien mach.</summary>
            public bool append;
            /// <summary>Tien to ten Shot/Rail/Wave/Prop khi append (tranh trung ten voi level truoc).</summary>
            public string namePrefix = "";
            /// <summary>Huong nhin (yaw, do) cua +Z cua level khi level bi xoay (mac dinh 0).</summary>
            public float baseYaw;
            /// <summary>&gt; 0: toc do (m/s) cua shot Move dau Phase 2+ (ray noi Phase, dai hon ray thuong) va camera nhin theo tiep tuyen ray.</summary>
            public float linkSpeed;
            /// <summary>true: rail Move dau Phase 1 cung la ray noi (nhin theo tiep tuyen, linkSpeed) - level noi tiep trong chuoi bat dau tai goc cuoi level truoc.</summary>
            public bool linkFirst;
            /// <summary>So Phase cua level (mac dinh 3; Level 5 = 5). Moi Phase 6 shot.</summary>
            public int phaseCount = 3;
            /// <summary>L3-L4: ten marker (vd. "CamPoint_P3_S7") cua goc dwell cuoi level - them shot Combat KHONG encounter (dwell) vao cuoi Phase cuoi:
            /// sau wave cuoi camera quay nhin cua noi level ke mo (truoc bang ket qua / truoc ray level ke). null = khong co.</summary>
            public string exitCamPoint;
            /// <summary>Thoi gian giu goc exitCamPoint (s) va thoi gian blend vao goc do (s).</summary>
            public float exitDwell = 0.8f, exitBlend = 1.0f;
        }

        public static void Run(LevelSpec spec)
        {
            var Waves = spec.waves; var Titles = spec.titles; var MoveShots = spec.moveShots;
            var scene = EditorSceneManager.OpenScene(spec.scenePath, OpenSceneMode.Single);
            var roots = new List<GameObject>(scene.GetRootGameObjects());
            GameObject Root(string n) => roots.Find(g => g != null && g.name == n);

            var level = Root(spec.rootName);
            var points = new Dictionary<string, Transform>();
            foreach (var t in level.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("Area_")) t.gameObject.SetActive(true);
                points[t.name] = t;
            }
            // Bat tat ca nhom cha cua Area (neu Level_01 hoac con bi tat)
            level.SetActive(true);

            string pre = spec.namePrefix ?? "";
            Transform rails, encounters, shots;
            if (spec.append)
            {
                rails = Root("Rails").transform; encounters = Root("Encounters").transform; shots = Root("Shots").transform;
            }
            else
            {
                foreach (var n in new[] { "Rails", "Encounters", "Shots" })
                {
                    var g = Root(n); if (g != null) Object.DestroyImmediate(g);
                }
                rails = new GameObject("Rails").transform;
                encounters = new GameObject("Encounters").transform;
                shots = new GameObject("Shots").transform;
            }

            var rig = Root("CameraRig").GetComponent<PhaseDirector>();
            var profile = rig.profile;
            var phases = spec.append ? new List<RailPhase>(rig.phases) : new List<RailPhase>();

            var enemyPrefab = AssetDatabase.LoadAssetAtPath<EnemyActor>(Pre + "Enemies/Enemy.prefab");
            var hostagePrefab = AssetDatabase.LoadAssetAtPath<HostageActor>(Pre + "Enemies/Hostage.prefab");
            var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(Pre + "Enemies/Data/EnemyConfig_Default.asset");

            for (int p = 1; p <= spec.phaseCount; p++)
            {
                var phase = new RailPhase { title = Titles[p - 1] };
                CameraShot prevShot = null;
                for (int s = 1; s <= 6; s++)
                {
                    var cp = points["CamPoint_P" + p + "_S" + s];
                    bool isMove = System.Array.IndexOf(MoveShots, s) >= 0;
                    var go = new GameObject(pre + "Shot_P" + p + "_S" + s + (isMove ? "_Move" : "_Combat"));
                    go.transform.SetParent(shots, false);
                    go.transform.SetPositionAndRotation(cp.position, cp.rotation);
                    var shot = go.AddComponent<CameraShot>();
                    if (isMove)
                    {
                        shot.kind = ShotKind.Move;
                        shot.entry = s == 1 ? ShotEntry.Cut : ShotEntry.Blend;
                        shot.blendTime = -1f; // Combat->Move: blend dai hon de giam dinh toc do xoay (rail cam cung dang xoay)
                        var hints = CameraRigBuilder.FindHints(level.transform, "RailHint_P" + p + "_S" + s + "_");
                        var sc = CameraRigBuilder.BuildSplineFromPoints(pre + "Rail_P" + p + "_S" + s, hints, rails);
                        shot.spline = sc;
                        if (spec.keyedMoves)
                        {
                            // Giu huong cua shot Combat truoc (hoac +Z khi vao Phase) den 40% rail, roi xoay dan ve huong shot Combat ke (SetNextLook).
                            float holdYaw = prevShot != null && prevShot.kind == ShotKind.Combat ? prevShot.transform.eulerAngles.y : spec.baseYaw;
                            shot.lookKeys = new List<LookKey> { new LookKey(0.4f, holdYaw) };
                            if (s == 1 && (p > 1 || spec.linkFirst) && spec.linkSpeed > 0f)
                            {
                                // Ray noi Phase: camera nhin THEO HUONG DI (tiep tuyen ray, khong lookKeys) - khong truot ngang; dau/cuoi ray khop huong shot truoc/sau.
                                shot.lookKeys = new List<LookKey>();
                                shot.speedOverride = spec.linkSpeed;
                                shot.lookDampingOverride = 0.3f; shot.maxYawRateOverride = 70f; shot.lookAheadOverride = 10f; // bam tiep tuyen sat: camera luon nhin ve phia truoc luc di
                            }
                        }
                    }
                    else
                    {
                        var def = System.Array.Find(Waves, w => w.phase == p && w.shot == s);
                        shot.kind = ShotKind.Combat;
                        shot.entry = ShotEntry.Blend;
                        shot.fov = def.fov;
                        // Blend giua 2 goc giao tranh: dinh toc do xoay (EaseInOut ~1.5x trung binh) <= 60 do/s => trung binh <= 40 do/s.
                        Vector3 prevFwd;
                        if (prevShot.kind == ShotKind.Move && spec.keyedMoves) prevFwd = go.transform.forward; // rail co moc: ket thuc dung huong shot nay
                        else if (prevShot.kind == ShotKind.Move && prevShot.spline != null)
                        {
                            var tn = SplineUtility.EvaluateTangent(prevShot.spline.Spline, 1f);
                            prevFwd = prevShot.spline.transform.TransformDirection(new Vector3(tn.x, tn.y, tn.z));
                        }
                        else prevFwd = prevShot.transform.forward;
                        float ang = Vector3.Angle(Flat(prevFwd), Flat(go.transform.forward));
                        shot.blendTime = Mathf.Max(def.blend >= 0f ? def.blend : profile.defaultBlend, Mathf.Ceil(ang / 40f / 0.05f) * 0.05f);
                        var wg = new GameObject(pre + "Wave_P" + p + "_W" + s);
                        wg.transform.SetParent(encounters, false);
                        var wave = wg.AddComponent<EncounterWave>();
                        WireWave(wave, p, s, def, points, enemyPrefab, hostagePrefab, config);
                        shot.encounter = wave;
                        shot.frameTargets = FrameTargets(p, s, points, config);
                        shot.EnsureCameras(profile);
                    }
                    phase.shots.Add(shot);
                    prevShot = shot;
                    EditorUtility.SetDirty(shot);
                }
                if (p == spec.phaseCount && !string.IsNullOrEmpty(spec.exitCamPoint) && points.TryGetValue(spec.exitCamPoint, out var ecp))
                {
                    // Goc dwell cuoi level (khong encounter): nhin cua noi level ke mo, FOV = FOV ray (vao ray level ke khong doi FOV).
                    var go = new GameObject(pre + "Shot_P" + p + "_S7_Combat"); // ten theo quy uoc Shot_P*_S*_Combat (shot dwell khong encounter)
                    go.transform.SetParent(shots, false);
                    go.transform.SetPositionAndRotation(ecp.position, ecp.rotation);
                    var shot = go.AddComponent<CameraShot>();
                    shot.kind = ShotKind.Combat;
                    shot.entry = ShotEntry.Blend;
                    shot.fov = profile.baseFov;
                    shot.blendTime = spec.exitBlend;
                    shot.dwell = spec.exitDwell;
                    shot.EnsureCameras(profile);
                    phase.shots.Add(shot);
                    EditorUtility.SetDirty(shot);
                }
                phases.Add(phase);
            }
            rig.phases = phases;
            EditorUtility.SetDirty(rig);

            // Cac he thong co san
            EnsurePrefab(roots, "FxSystems", Pre + "FX/FxSystems.prefab");
            EnsurePrefab(roots, "RankScoreSystems", Pre + "Game/RankScoreSystems.prefab");

            AssembleM3(scene, roots, level, points, encounters, spec.append, pre);

            // F-210: luu lai CameraFeelProfile de lo cac field M2
            
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.ForceReserializeAssets(new[] { "Assets/_Game/Settings/CameraFeelProfile.asset" });

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[" + spec.logName + "] Done: " + spec.phaseCount + " Phase, " + Waves.Length + " wave.");
        }

        // ---- M3 (T-702): Props, Grenadier, HumanShield, PropSystems, RankScoreDirector ----
        static void AssembleM3(Scene scene, List<GameObject> roots, GameObject level, Dictionary<string, Transform> points, Transform encounters, bool append = false, string pre = "")
        {
            // Don dep ban cu (Props do menu tao); append: giu Props cua level truoc, them vao
            var oldProps = roots.Find(g => g != null && g.name == "Props");
            Transform propsRoot;
            if (append && oldProps != null) propsRoot = oldProps.transform;
            else
            {
                if (oldProps != null) Object.DestroyImmediate(oldProps);
                propsRoot = new GameObject("Props").transform;
                SceneManager.MoveGameObjectToScene(propsRoot.gameObject, scene);
            }

            var barrel = AssetDatabase.LoadAssetAtPath<GameObject>(Pre + "Props/Prop_Barrel.prefab");
            var box = AssetDatabase.LoadAssetAtPath<GameObject>(Pre + "Props/Prop_Box.prefab");
            var glass = AssetDatabase.LoadAssetAtPath<GameObject>(Pre + "Props/Prop_Glass.prefab");
            int nb = 0, nx = 0, ng = 0;
            foreach (var kv in new List<KeyValuePair<string, Transform>>(points))
            {
                string n = kv.Key; var m = kv.Value;
                GameObject pf = null;
                if (n.StartsWith("PropSlot_Barrel_")) { pf = barrel; nb++; }
                else if (n.StartsWith("PropSlot_Box_")) { pf = box; nx++; }
                else if (n.StartsWith("PropSlot_Glass_")) { pf = glass; ng++; }
                if (pf == null) continue;
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pf, scene);
                inst.name = pre + n.Replace("PropSlot_", "Prop_");
                inst.transform.SetParent(propsRoot, true);
                inst.transform.SetPositionAndRotation(m.position, m.rotation);
                if (pf == glass)
                {
                    var ls = m.lossyScale; var ps = inst.transform.localScale;
                    inst.transform.localScale = new Vector3(ls.x, ls.y, ps.z);
                }
            }

            // Grenadier / HumanShield: noi trong WireWave (moi Phase, ca khi append - Level 5 co khien nguoi tu P1).

            EnsurePrefab(roots, "PropSystems", Pre + "Props/PropSystems.prefab");

            if (append) { Debug.Log("[LevelAssembler] M3 (append): props barrel=" + nb + " box=" + nx + " glass=" + ng); return; }

            // RankScoreDirector
            var rsRoot = roots.Find(g => g != null && g.name == "RankScoreSystems");
            var dir = rsRoot != null ? rsRoot.GetComponentInChildren<RankScoreDirector>(true) : Object.FindFirstObjectByType<RankScoreDirector>();
            if (dir != null)
            {
                var so = new SerializedObject(dir);
                so.FindProperty("presetCalm").objectReferenceValue = AssetDatabase.LoadAssetAtPath<EnemyPreset>(Pre + "Enemies/Data/EnemyPreset_Calm.asset");
                so.FindProperty("presetStandard").objectReferenceValue = AssetDatabase.LoadAssetAtPath<EnemyPreset>(Pre + "Enemies/Data/EnemyPreset_Standard.asset");
                so.FindProperty("presetIntense").objectReferenceValue = AssetDatabase.LoadAssetAtPath<EnemyPreset>(Pre + "Enemies/Data/EnemyPreset_Intense.asset");
                so.FindProperty("presetHostageHeavy").objectReferenceValue = AssetDatabase.LoadAssetAtPath<EnemyPreset>(Pre + "Enemies/Data/EnemyPreset_HostageHeavy.asset");
                so.FindProperty("shotgunPickupPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(Pre + "Combat/WeaponPickup_Shotgun.prefab");
                so.FindProperty("machineGunPickupPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(Pre + "Combat/WeaponPickup_MachineGun.prefab");
                if (so.FindProperty("tracker").objectReferenceValue == null)
                    so.FindProperty("tracker").objectReferenceValue = dir.GetComponentInParent<PlayerStatsTracker>() ?? Object.FindFirstObjectByType<PlayerStatsTracker>();
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else Debug.LogWarning("[LevelAssembler] Khong thay RankScoreDirector");
            Debug.Log("[LevelAssembler] M3: props barrel=" + nb + " box=" + nx + " glass=" + ng);
        }

        static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude < 1e-6f ? Vector3.forward : v.normalized; }

        static void EnsurePrefab(List<GameObject> roots, string name, string path)
        {
            if (roots.Exists(g => g != null && g.name == name)) return;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
            inst.name = name;
        }

        static List<Transform> Find(Dictionary<string, Transform> pts, string prefix)
        {
            var list = new List<Transform>();
            foreach (var kv in pts) if (kv.Key.StartsWith(prefix)) list.Add(kv.Value);
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list;
        }

        static void WireWave(EncounterWave wave, int p, int w, WaveDef def, Dictionary<string, Transform> pts,
            EnemyActor enemyPrefab, HostageActor hostagePrefab, EnemyConfig config)
        {
            string key = "P" + p + "_W" + w + "_";
            var so = new SerializedObject(wave);
            so.FindProperty("config").objectReferenceValue = config;
            so.FindProperty("preset").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<EnemyPreset>(Pre + "Enemies/Data/EnemyPreset_" + def.preset + ".asset");
            so.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
            so.FindProperty("hostagePrefab").objectReferenceValue = hostagePrefab;
            SetList(so.FindProperty("spawnPoints"), Find(pts, "EnemySpawn_" + key));
            SetList(so.FindProperty("hostageSpawnPoints"), Find(pts, "HostageSpawn_" + key));
            // Grenadier / khien nguoi (marker GrenadierSpawn_ / ShieldSpawn_ trong prefab level)
            var gr = Find(pts, "GrenadierSpawn_" + key);
            if (gr.Count > 0)
            {
                so.FindProperty("grenadierPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<EnemyActor>(Pre + "Enemies/Enemy_Grenadier.prefab");
                SetList(so.FindProperty("grenadierSpawnPoints"), gr);
                so.FindProperty("maxGrenadiersPerWave").intValue = gr.Count;
            }
            var sh = Find(pts, "ShieldSpawn_" + key);
            if (sh.Count > 0)
            {
                so.FindProperty("humanShieldPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<HumanShieldEnemy>(Pre + "Enemies/Enemy_HumanShield.prefab");
                SetList(so.FindProperty("humanShieldSpawnPoints"), sh);
                so.FindProperty("maxHumanShieldsPerWave").intValue = sh.Count;
            }
            var picks = Find(pts, "PickupSpawn_" + key);
            if (def.pickup != null && picks.Count > 0)
            {
                so.FindProperty("pickupPrefab").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Pre + "Combat/WeaponPickup_" + def.pickup + ".prefab");
                SetList(so.FindProperty("pickupSpawnPoints"), picks);
            }
            // Canh hai (GagFall) dat san trong prefab level: Gag_P*_W*_NN
            var gagProp = so.FindProperty("gags");
            if (gagProp != null)
            {
                var gagList = new List<GagFall>();
                foreach (var t in Find(pts, "Gag_" + key)) { var g = t.GetComponent<GagFall>(); if (g != null) gagList.Add(g); }
                gagProp.arraySize = gagList.Count;
                for (int i = 0; i < gagList.Count; i++) gagProp.GetArrayElementAtIndex(i).objectReferenceValue = gagList[i];
            }
            so.FindProperty("description").stringValue = "P" + p + " W" + w + " (" + def.preset + ")";
            if (def.maxConcurrent > 0) so.FindProperty("maxConcurrent").intValue = def.maxConcurrent;
            // Enemy dung san (SceneStanding): diem EnemyStand_P*_W*_NN -> instance prefab Enemy lam con cua wave, vao sceneEnemies.
            var stands = Find(pts, "EnemyStand_" + key);
            if (stands.Count > 0 && enemyPrefab != null)
            {
                var list = so.FindProperty("sceneEnemies");
                list.arraySize = stands.Count;
                for (int i = 0; i < stands.Count; i++)
                {
                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab.gameObject, wave.gameObject.scene);
                    inst.name = "Enemy_Stand_" + key + (i + 1).ToString("00");
                    inst.transform.SetParent(wave.transform, true);
                    inst.transform.SetPositionAndRotation(stands[i].position, stands[i].rotation);
                    var actor = inst.GetComponent<EnemyActor>();
                    var aso = new SerializedObject(actor);
                    aso.FindProperty("sceneStanding").boolValue = true;
                    aso.ApplyModifiedPropertiesWithoutUndo();
                    list.GetArrayElementAtIndex(i).objectReferenceValue = actor;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Diem can thay cua goc giao tranh (man doc - CameraShot.AutoFrame): diem ngam cua enemy/con tin tai Peek, tam thung vu khi.</summary>
        static List<Vector3> FrameTargets(int p, int w, Dictionary<string, Transform> pts, EnemyConfig config)
        {
            string key = "P" + p + "_W" + w + "_";
            float aim = config != null ? config.aimHeight : 1.5f;
            var list = new List<Vector3>();
            foreach (var prefix in new[] { "EnemySpawn_", "HostageSpawn_", "ShieldSpawn_", "GrenadierSpawn_" })
                foreach (var sp in Find(pts, prefix + key))
                {
                    var peek = sp.Find("Peek");
                    list.Add((peek != null ? peek.position : sp.position) + Vector3.up * aim);
                }
            foreach (var sp in Find(pts, "EnemyStand_" + key)) list.Add(sp.position + Vector3.up * aim);
            foreach (var sp in Find(pts, "PickupSpawn_" + key)) list.Add(sp.position + Vector3.up * 0.5f);
            return list;
        }

        static void SetList(SerializedProperty prop, List<Transform> items)
        {
            prop.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }
}
