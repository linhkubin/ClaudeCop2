using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using ClaudeCop.Camera;
using ClaudeCop.Camera.Editor;
using ClaudeCop.Enemy;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// T-403: dung lai phan "Rails / Encounters / Shots" va danh sach Phase cua Scenes/Gameplay/Level_01.unity tu Level_01.prefab
    /// (3 Phase x 6 Shot). Chay lai nhieu lan an toan (xoa va dung lai 3 nhom do). Dat/noi: FxSystems, JevSystems.
    /// </summary>
    public static class Level01Assembler
    {
        const string ScenePath = "Assets/_Game/Scenes/Gameplay/Level_01.unity";
        const string Pre = "Assets/_Game/Prefabs/";

        class WaveDef
        {
            public int phase, shot; public string preset; public string pickup; // pickup: "Shotgun" | "MachineGun" | null
            public float blend = -1f; public float fov = 60f;
        }

        // Do kho: P1 Calm/Standard -> P2 Standard -> P3 Intense. Pickup: P1/P2 W2 Shotgun, P2 W5 MG, P3 luan phien.
        static readonly WaveDef[] Waves =
        {
            new WaveDef{phase=1,shot=2,preset="Calm",     pickup="Shotgun"},
            new WaveDef{phase=1,shot=3,preset="Calm",     pickup=null, blend=1.0f, fov=58f},
            new WaveDef{phase=1,shot=5,preset="Standard", pickup=null},
            new WaveDef{phase=1,shot=6,preset="Standard", pickup=null, blend=0.7f, fov=58f},
            new WaveDef{phase=2,shot=2,preset="Standard", pickup="Shotgun"},
            new WaveDef{phase=2,shot=3,preset="Standard", pickup=null, blend=0.7f, fov=58f},
            new WaveDef{phase=2,shot=5,preset="Standard", pickup="MachineGun"},
            new WaveDef{phase=2,shot=6,preset="Standard", pickup=null, blend=0.7f, fov=58f},
            new WaveDef{phase=3,shot=2,preset="Standard", pickup="Shotgun"},
            new WaveDef{phase=3,shot=3,preset="Intense",  pickup="MachineGun", blend=1.0f, fov=58f},
            new WaveDef{phase=3,shot=5,preset="Intense",  pickup="Shotgun"},
            new WaveDef{phase=3,shot=6,preset="Intense",  pickup="MachineGun", blend=1.0f, fov=58f},
        };

        static readonly int[] MoveShots = { 1, 4 };
        static readonly string[] Titles = { "STAGE 1-1", "STAGE 1-2", "STAGE 1-3" };

        [MenuItem("ClaudeCop/Game/Assemble Level_01 (T-403)")]
        public static void Assemble()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var roots = new List<GameObject>(scene.GetRootGameObjects());
            GameObject Root(string n) => roots.Find(g => g != null && g.name == n);

            var level = Root("Level_01");
            var points = new Dictionary<string, Transform>();
            foreach (var t in level.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("Area_")) t.gameObject.SetActive(true);
                points[t.name] = t;
            }
            // Bat tat ca nhom cha cua Area (neu Level_01 hoac con bi tat)
            level.SetActive(true);

            foreach (var n in new[] { "Rails", "Encounters", "Shots" })
            {
                var g = Root(n); if (g != null) Object.DestroyImmediate(g);
            }
            var rails = new GameObject("Rails").transform;
            var encounters = new GameObject("Encounters").transform;
            var shots = new GameObject("Shots").transform;

            var rig = Root("CameraRig").GetComponent<PhaseDirector>();
            var profile = rig.profile;
            var phases = new List<RailPhase>();

            var enemyPrefab = AssetDatabase.LoadAssetAtPath<EnemyActor>(Pre + "Enemies/Enemy.prefab");
            var hostagePrefab = AssetDatabase.LoadAssetAtPath<HostageActor>(Pre + "Enemies/Hostage.prefab");
            var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(Pre + "Enemies/Data/EnemyConfig_Default.asset");

            for (int p = 1; p <= 3; p++)
            {
                var phase = new RailPhase { title = Titles[p - 1] };
                CameraShot prevShot = null;
                for (int s = 1; s <= 6; s++)
                {
                    var cp = points["CamPoint_P" + p + "_S" + s];
                    bool isMove = System.Array.IndexOf(MoveShots, s) >= 0;
                    var go = new GameObject("Shot_P" + p + "_S" + s + (isMove ? "_Move" : "_Combat"));
                    go.transform.SetParent(shots, false);
                    go.transform.SetPositionAndRotation(cp.position, cp.rotation);
                    var shot = go.AddComponent<CameraShot>();
                    if (isMove)
                    {
                        shot.kind = ShotKind.Move;
                        shot.entry = s == 1 ? ShotEntry.Cut : ShotEntry.Blend;
                        shot.blendTime = -1f; // Combat->Move: blend dai hon de giam dinh toc do xoay (rail cam cung dang xoay)
                        var hints = CameraRigBuilder.FindHints(level.transform, "RailHint_P" + p + "_S" + s + "_");
                        var sc = CameraRigBuilder.BuildSplineFromPoints("Rail_P" + p + "_S" + s, hints, rails);
                        shot.spline = sc;
                    }
                    else
                    {
                        var def = System.Array.Find(Waves, w => w.phase == p && w.shot == s);
                        shot.kind = ShotKind.Combat;
                        shot.entry = ShotEntry.Blend;
                        shot.fov = def.fov;
                        // Blend giua 2 goc giao tranh: dinh toc do xoay (EaseInOut ~1.5x trung binh) <= 60 do/s => trung binh <= 40 do/s.
                        Vector3 prevFwd;
                        if (prevShot.kind == ShotKind.Move && prevShot.spline != null)
                        {
                            var tn = SplineUtility.EvaluateTangent(prevShot.spline.Spline, 1f);
                            prevFwd = prevShot.spline.transform.TransformDirection(new Vector3(tn.x, tn.y, tn.z));
                        }
                        else prevFwd = prevShot.transform.forward;
                        float ang = Vector3.Angle(Flat(prevFwd), Flat(go.transform.forward));
                        shot.blendTime = Mathf.Max(def.blend >= 0f ? def.blend : profile.defaultBlend, Mathf.Ceil(ang / 40f / 0.05f) * 0.05f);
                        var wg = new GameObject("Wave_P" + p + "_W" + s);
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
                phases.Add(phase);
            }
            rig.phases = phases;
            EditorUtility.SetDirty(rig);

            // Cac he thong co san
            EnsurePrefab(roots, "FxSystems", Pre + "FX/FxSystems.prefab");
            EnsurePrefab(roots, "JevSystems", Pre + "Game/JevSystems.prefab");

            // F-210: luu lai CameraFeelProfile de lo cac field M2
            
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.ForceReserializeAssets(new[] { "Assets/_Game/Settings/CameraFeelProfile.asset" });

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Level01Assembler] Done: 3 Phase, " + Waves.Length + " wave.");
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
            var picks = Find(pts, "PickupSpawn_" + key);
            if (def.pickup != null && picks.Count > 0)
            {
                so.FindProperty("pickupPrefab").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<GameObject>(Pre + "Combat/WeaponPickup_" + def.pickup + ".prefab");
                SetList(so.FindProperty("pickupSpawnPoints"), picks);
            }
            so.FindProperty("description").stringValue = "P" + p + " W" + w + " (" + def.preset + ")";
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Diem can thay cua goc giao tranh (man doc - CameraShot.AutoFrame): diem ngam cua enemy/con tin tai Peek, tam thung vu khi.</summary>
        static List<Vector3> FrameTargets(int p, int w, Dictionary<string, Transform> pts, EnemyConfig config)
        {
            string key = "P" + p + "_W" + w + "_";
            float aim = config != null ? config.aimHeight : 1.5f;
            var list = new List<Vector3>();
            foreach (var prefix in new[] { "EnemySpawn_", "HostageSpawn_" })
                foreach (var sp in Find(pts, prefix + key))
                {
                    var peek = sp.Find("Peek");
                    list.Add((peek != null ? peek.position : sp.position) + Vector3.up * aim);
                }
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
