using ClaudeCop.Enemy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// T-403: ghep Scenes/Gameplay/Level_01.unity tu Level_01.prefab (3 Phase x 6 Shot). Loi dung chung: <see cref="LevelAssembler"/>.
    /// </summary>
    public static class Level01Assembler
    {
        const string ScenePath = "Assets/_Game/Scenes/Gameplay/Level_01.unity";

        // Do kho: P1 Calm/Standard -> P2 Standard -> P3 Intense. Pickup: P1/P2 W2 Shotgun, P2 W5 MG, P3 luan phien.
        static readonly LevelAssembler.WaveDef[] Waves =
        {
            new LevelAssembler.WaveDef{phase=1,shot=2,preset="Calm",     pickup="Shotgun"},
            new LevelAssembler.WaveDef{phase=1,shot=3,preset="Calm",     pickup=null, blend=1.0f, fov=58f},
            new LevelAssembler.WaveDef{phase=1,shot=5,preset="Standard", pickup=null},
            new LevelAssembler.WaveDef{phase=1,shot=6,preset="Standard", pickup=null, blend=0.7f, fov=58f},
            new LevelAssembler.WaveDef{phase=2,shot=2,preset="Standard", pickup="Shotgun"},
            new LevelAssembler.WaveDef{phase=2,shot=3,preset="Standard", pickup=null, blend=0.7f, fov=58f},
            new LevelAssembler.WaveDef{phase=2,shot=5,preset="Standard", pickup="MachineGun"},
            new LevelAssembler.WaveDef{phase=2,shot=6,preset="Standard", pickup=null, blend=0.7f, fov=58f},
            new LevelAssembler.WaveDef{phase=3,shot=2,preset="Standard", pickup="Shotgun"},
            new LevelAssembler.WaveDef{phase=3,shot=3,preset="Intense",  pickup="MachineGun", blend=1.0f, fov=58f},
            new LevelAssembler.WaveDef{phase=3,shot=5,preset="Intense",  pickup="Shotgun"},
            new LevelAssembler.WaveDef{phase=3,shot=6,preset="Intense",  pickup="MachineGun", blend=1.0f, fov=58f},
        };

        static readonly string[] Titles = { "STAGE 1-1", "STAGE 1-2", "STAGE 1-3" };

        [MenuItem("ClaudeCop/Game/Assemble Level_01 (T-403)")]
        public static void Assemble()
        {
            FixMainDoorEntries();
            LevelAssembler.Run(new LevelAssembler.LevelSpec
            {
                scenePath = ScenePath, rootName = "Level_01", logName = "Level01Assembler",
                waves = Waves, titles = Titles, moveShots = new[] { 1, 4 },
            });
            WireMainDoor();
        }

        /// <summary>
        /// L1-DOOR: ap dung sua "enemy buoc ra cua chinh" len Level_01.prefab + Level_01.unity HIEN TAI (Level_01.unity da duoc chinh tay sau T-403,
        /// khong dung lai duoc bang Assemble). Idempotent; LevelChainAssembler goi truoc khi copy Level_01.unity.
        /// </summary>
        [MenuItem("ClaudeCop/Game/Fix Level_01 Main Door Entries")]
        public static void ApplyMainDoorFix()
        {
            FixMainDoorEntries();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            WireMainDoor();
        }

        const string PrefabPath = "Assets/_Game/Level/Level_01.prefab";
        // L1-DOOR: hai enemy dung tren bac tam cap truoc cua chinh (P3 W3 #03, P3 W4 #01 = ten cuoi) truoc day chay vao kieu Auto
        // (lech ngang theo camera) -> diem xuat phat nam TRONG khoi nha ngan hang, xuyen Facade_S. Nay di ra tu CUA CHINH (EntryStyle.Door).
        static readonly string[] MainDoorEnemies = { "EnemySpawn_P3_W3_03", "EnemySpawn_P3_W4_01" };
        const string MainDoorMarker = "Door_Enemy_P3_W4_01";
        static readonly Vector3 MainDoorMarkerPos = new Vector3(45.8f, 1.2f, 35.0f);   // trong o cua chinh (sau duong la cua x 45.3), giua 2 la

        /// <summary>
        /// Sua Level_01.prefab (idempotent): bo cua gia "Door_Enemy_P3_W4_01" dan tren Facade_S (chong len khung cua chinh, khong dung),
        /// dat marker rong cung ten trong o cua chinh, gan SpawnPointEntry(Door) cho cac enemy o bac tam cap.
        /// </summary>
        internal static void FixMainDoorEntries()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            Transform marker = null, decor = null;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t == null) continue;
                if (t.name == "Door_Frame_Top") decor = t.parent;
                if (t.name == MainDoorMarker)
                {
                    if (t.childCount > 0) Object.DestroyImmediate(t.gameObject);   // cua gia cu (khung + o den) tren mat tien
                    else marker = t;
                }
            }
            if (marker == null)
            {
                marker = new GameObject(MainDoorMarker).transform;
                marker.SetParent(decor != null ? decor : root.transform, false);
            }
            marker.SetPositionAndRotation(MainDoorMarkerPos, Quaternion.Euler(0f, 270f, 0f));
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (System.Array.IndexOf(MainDoorEnemies, t.name) < 0) continue;
                var spe = t.GetComponent<SpawnPointEntry>();
                if (spe == null) spe = t.gameObject.AddComponent<SpawnPointEntry>();
                var so = new SerializedObject(spe);
                so.FindProperty("style").enumValueIndex = (int)EnemyActor.EntryStyle.Door;
                so.FindProperty("dropHeight").floatValue = 0f;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        /// <summary>
        /// Cua chinh (DoorOpener_Main, truot) mo khi Wave_P3_W2 Cleared (truoc la Wave_P3_W4): cua bat mo luc camera sang goc S3, enemy W3 #03 va ten cuoi W4
        /// buoc ra tu cua; het W4 camera di thang qua cua da mo vao Level 2.
        /// </summary>
        static void WireMainDoor()
        {
            var scene = EditorSceneManager.GetActiveScene();
            DoorOpener door = null; EncounterWave w2 = null;
            foreach (var g in scene.GetRootGameObjects())
            {
                if (g.name == "DoorOpener_Main") door = g.GetComponent<DoorOpener>();
                foreach (var w in g.GetComponentsInChildren<EncounterWave>(true)) if (w.name == "Wave_P3_W2") w2 = w;
            }
            if (door == null || w2 == null) { Debug.LogWarning("[Level01Assembler] Thieu DoorOpener_Main hoac Wave_P3_W2"); return; }
            var so = new SerializedObject(door);
            so.FindProperty("trigger").objectReferenceValue = w2;
            so.FindProperty("delay").floatValue = 0.5f;
            so.FindProperty("duration").floatValue = 1.0f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
