using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// Level 2 (Sanh giao dich) - dung blockout bang primitive + cac diem marker, luu Assets/_Game/Level/Level_02.prefab.
    /// Marker theo quy uoc Level_01: CamPoint_P*_S*, RailHint_P*_S*_NN, EnemySpawn_P*_W*_NN (+Peek), HostageSpawn_P*_W*_NN (+Peek),
    /// EnemyStand_P*_W*_NN (enemy dung san - SceneStanding). W = so shot (2,3,5,6). Goc/yaw cua shot Combat tinh tu tam cum muc tieu.
    /// Chay lai an toan (ghi de prefab). Sau do chay Level02Assembler.
    /// </summary>
    public static class Level02Builder
    {
        public const string PrefabPath = "Assets/_Game/Level/Level_02.prefab";
        const string MatDir = "Assets/_Game/Level/Materials/";

        // ---- du lieu ----
        struct Sp { public int p, w; public char kind; public bool stand; public float x, y, z; public string cover; }
        static Sp E(int p, int w, float x, float z, string cover = null, float y = 0f) => new Sp { p = p, w = w, kind = 'E', x = x, y = y, z = z, cover = cover };
        static Sp S(int p, int w, float x, float z, float y = 0f) => new Sp { p = p, w = w, kind = 'E', stand = true, x = x, y = y, z = z };
        static Sp H(int p, int w, float x, float z, string cover = null) => new Sp { p = p, w = w, kind = 'H', x = x, z = z, cover = cover };

        // E = enemy lo ra (co cho nap), S = enemy dung san, H = con tin. Tong 23 enemy (7 dung san) + 3 con tin.
        static readonly Sp[] Spawns =
        {
            // P1 - cua xoay + tiep tan (reception z=27, quay thong tin z=34, ATM x~9)
            S(1,2, 2.3f,24.2f), E(1,2,-1.0f,27.9f),
            H(1,3, 5.2f,27.9f,"Planter"), E(1,3, 8.5f,27.9f,"Kiosk"), E(1,3, 9.9f,28.7f,"Kiosk"),
            S(1,5,-7.4f,32.4f),
            E(1,6,-5.8f,31.5f,"Planter"), E(1,6,-2.2f,27.9f),
            // P2 - day quay giao dich (6 quay tai z=60, tam x -10.5..10.5 buoc 4.2)
            E(2,2,-10.5f,61.3f), S(2,2,-8.9f,57.0f),
            E(2,3,-2.9f,61.3f),
            E(2,5, 1.3f,61.3f), H(2,5, 3.4f,61.3f), S(2,5,-0.2f,57.2f),
            E(2,6,-2.7f,61.3f), S(2,6,-0.4f,57.6f),
            // P3 - khu cho + cau thang (lan can tang lung z=108 cao 4.4, ghe cho z~100)
            E(3,2,-1.0f,108.6f,null,4.4f), S(3,2, 2.2f,96.2f),
            E(3,3, 6.0f,108.6f,null,4.4f), E(3,3, 7.0f,99.5f,"Bench"),
            E(3,5,-2.4f,100.2f,"Bench"), H(3,5,-1.0f,98.4f,"Bench"), S(3,5, 0.6f,99.6f),
            E(3,6, 4.5f,108.6f,null,4.4f), E(3,6, 2.4f,101.0f,"Bench"), S(3,6, 4.6f,98.5f),
        };

        // Vi tri camera Combat (x,z) theo [phase, shot]; y = 1.65. Pitch am = ngang len.
        static Vector3 CamPos(int p, int s)
        {
            switch (p * 10 + s)
            {
                case 12: return new Vector3(0f, 1.65f, 5f);
                case 13: return new Vector3(0.8f, 1.65f, 5.1f);
                case 15: return new Vector3(-1f, 1.65f, 11.5f);
                case 16: return new Vector3(-0.7f, 1.65f, 11.7f);
                case 22: return new Vector3(-12f, 1.65f, 44f);
                case 23: return new Vector3(-11.7f, 1.65f, 44.2f);
                case 25: return new Vector3(-3.2f, 1.65f, 43.6f);
                case 26: return new Vector3(-3.5f, 1.65f, 43.9f);
                case 32: return new Vector3(0f, 1.65f, 78f);
                case 33: return new Vector3(0.8f, 1.65f, 78.2f);
                case 35: return new Vector3(1.4f, 1.65f, 82.2f);
                case 36: return new Vector3(1.5f, 1.65f, 82.4f);
            }
            return Vector3.zero;
        }

        static float CamPitch(int p, int s) => (p == 3 && (s == 2 || s == 3 || s == 6)) ? -3f : 0f;

        // Rail Move: S1 (vao khu) va S4 (giua khu). Diem dau S4 = vi tri S3, diem cuoi = vi tri S5; diem cuoi S1 = vi tri S2.
        static Vector3[] Rail(int p, int s)
        {
            Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
            if (s == 1)
            {
                switch (p)
                {
                    case 1: return new[] { V(0, 1.6f, -6f), V(0, 1.6f, -1.5f), V(0, 1.62f, 2f), CamPos(1, 2) };
                    case 2: return new[] { V(-12.6f, 1.6f, 30f), V(-12.4f, 1.6f, 35f), V(-12.2f, 1.62f, 40f), CamPos(2, 2) };
                    default: return new[] { V(0, 1.6f, 60f), V(0, 1.6f, 66f), V(0, 1.62f, 72f), CamPos(3, 2) };
                }
            }
            switch (p)
            {
                case 1: return new[] { CamPos(1, 3), V(0.2f, 1.65f, 7.4f), V(-0.5f, 1.65f, 9.5f), CamPos(1, 5) };
                case 2: return new[] { CamPos(2, 3), V(-8.7f, 1.65f, 43.5f), V(-5.9f, 1.65f, 43.2f), CamPos(2, 5) };
                default: return new[] { CamPos(3, 3), V(1.0f, 1.65f, 79.5f), V(1.2f, 1.65f, 80.9f), CamPos(3, 5) };
            }
        }

        // ---- materials ----
        static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();
        static Material M(string n)
        {
            if (mats.TryGetValue(n, out var m) && m != null) return m;
            m = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Blockout_" + n + ".mat");
            if (m == null && n == "Wood") m = MakeWood();
            if (m == null) m = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Blockout_Gray.mat");
            return mats[n] = m;
        }

        static Material MakeWood()
        {
            string path = MatDir + "M_Blockout_Wood.mat";
            var src = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Blockout_Cardboard.mat");
            if (src == null) return null;
            AssetDatabase.CopyAsset(MatDir + "M_Blockout_Cardboard.mat", path);
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            var c = new Color(0.36f, 0.22f, 0.12f, 1f);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            EditorUtility.SetDirty(m);
            return m;
        }

        // ---- helpers ----
        static Transform Group(Transform parent, string name)
        {
            var g = new GameObject(name).transform; g.SetParent(parent, false); return g;
        }

        static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, string mat, float yaw = 0f, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = center; go.transform.rotation = Quaternion.Euler(0f, yaw, 0f); go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = M(mat);
            if (!collider) Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
            return go;
        }

        static GameObject Cyl(Transform parent, string name, Vector3 center, float radius, float height, string mat, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = center; go.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            go.GetComponent<MeshRenderer>().sharedMaterial = M(mat);
            if (!collider) Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
            return go;
        }

        static Transform Marker(Transform parent, string name, Vector3 pos, Quaternion rot)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); t.SetPositionAndRotation(pos, rot); return t;
        }

        static float YawToward(Vector3 from, Vector3 to) => Mathf.Atan2(to.x - from.x, to.z - from.z) * Mathf.Rad2Deg;

        [MenuItem("ClaudeCop/Game/Build Level_02 Prefab")]
        public static void Build()
        {
            mats.Clear();
            var root = new GameObject("Level_02");
            var shared = Group(root.transform, "Shared_Hall");
            var a1 = Group(root.transform, "Area_P1_Lobby");
            var a2 = Group(root.transform, "Area_P2_Counters");
            var a3 = Group(root.transform, "Area_P3_Waiting");
            var areas = new[] { a1, a2, a3 };

            BuildShared(shared);
            BuildP1(Group(a1, "Geometry"), Group(a1, "Decor"));
            BuildP2(Group(a2, "Geometry"), Group(a2, "Decor"));
            BuildP3(Group(a3, "Geometry"), Group(a3, "Decor"));

            // Marker camera + rail
            var camYaw = new Dictionary<int, float>();
            for (int p = 1; p <= 3; p++)
            {
                var camPts = Group(areas[p - 1], "CamPoints");
                var hints = Group(areas[p - 1], "RailHints");
                foreach (int s in new[] { 2, 3, 5, 6 })
                {
                    var c = CamPos(p, s);
                    float mn = 999f, mx = -999f;
                    foreach (var sp in Spawns)
                    {
                        if (sp.p != p || sp.w != s) continue;
                        float b = YawToward(c, new Vector3(sp.x, 0, sp.z)); mn = Mathf.Min(mn, b); mx = Mathf.Max(mx, b);
                    }
                    float yaw = Mathf.Round((mn + mx) * 0.5f);
                    camYaw[p * 10 + s] = yaw;
                    Marker(camPts, "CamPoint_P" + p + "_S" + s, c, Quaternion.Euler(CamPitch(p, s), yaw, 0f));
                }
                foreach (int s in new[] { 1, 4 })
                {
                    var pts = Rail(p, s);
                    Marker(camPts, "CamPoint_P" + p + "_S" + s, pts[0], Quaternion.identity);
                    for (int i = 0; i < pts.Length; i++)
                        Marker(hints, "RailHint_P" + p + "_S" + s + "_" + (i + 1).ToString("00"), pts[i], Quaternion.identity);
                }
            }

            // Spawn / stand / hostage
            var spawnGroups = new[] { Group(a1, "Spawns"), Group(a2, "Spawns"), Group(a3, "Spawns") };
            var covers = new[] { Group(a1, "Cover"), Group(a2, "Cover"), Group(a3, "Cover") };
            var counters = new Dictionary<string, int>();
            foreach (var sp in Spawns)
            {
                string key = "P" + sp.p + "_W" + sp.w + "_";
                string tag = sp.kind == 'H' ? "HostageSpawn_" : (sp.stand ? "EnemyStand_" : "EnemySpawn_");
                counters.TryGetValue(tag + key, out int n); counters[tag + key] = ++n;
                string name = tag + key + n.ToString("00");
                var cam = CamPos(sp.p, sp.w);
                var ground = new Vector3(sp.x, sp.y, sp.z);
                float yaw = YawToward(ground, cam);
                var rot = Quaternion.Euler(0f, yaw, 0f);
                var par = spawnGroups[sp.p - 1];
                if (sp.stand)
                {
                    Marker(par, name, ground, rot);
                    continue;
                }
                var t = Marker(par, name, ground + Vector3.down * 1.15f, rot);
                Marker(t, "Peek", ground + Vector3.up * 0.05f, rot);
                if (!string.IsNullOrEmpty(sp.cover)) BuildCover(covers[sp.p - 1], name.Replace(tag, "Cover_" + sp.cover + "_"), sp.cover, ground, yaw);
            }

            System.IO.Directory.CreateDirectory("Assets/_Game/Level");
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log("[Level02Builder] Da luu " + PrefabPath + " (" + Spawns.Length + " diem enemy/con tin).");
            LogFraming(camYaw);
        }

        static void LogFraming(Dictionary<int, float> camYaw)
        {
            var sb = new System.Text.StringBuilder("[Level02Builder] yaw cam: ");
            foreach (var kv in camYaw) sb.Append("P" + kv.Key / 10 + "S" + kv.Key % 10 + "=" + kv.Value + " ");
            Debug.Log(sb.ToString());
        }

        static void BuildCover(Transform parent, string name, string type, Vector3 enemyGround, float yawToCam)
        {
            Vector3 toCam = Quaternion.Euler(0, yawToCam, 0) * Vector3.forward;
            // Khoang cach = nua be day vat nap + 0.5 m: enemy dung sau vat nap, khong chui vao vat the.
            float depth = type == "Planter" ? 1.5f : (type == "Kiosk" ? 0.8f : 0.7f);
            Vector3 c = enemyGround + toCam * (depth * 0.5f + 0.5f);
            switch (type)
            {
                case "Planter":
                    Box(parent, name, c + Vector3.up * 0.55f, new Vector3(1.9f, 1.1f, 1.5f), "GrayDark", yawToCam);
                    Box(parent, name + "_Plants", c + Vector3.up * 1.35f, new Vector3(1.5f, 0.6f, 1.1f), "Foliage", yawToCam, false);
                    break;
                case "Kiosk":
                    Box(parent, name, c + Vector3.up * 0.65f, new Vector3(1.3f, 1.3f, 0.8f), "Metal", yawToCam);
                    Box(parent, name + "_Screen", c + Vector3.up * 1.0f + toCam * 0.41f, new Vector3(0.6f, 0.4f, 0.04f), "Blue", yawToCam, false);
                    break;
                case "Bench":
                    Box(parent, name, c + Vector3.up * 0.4f, new Vector3(2.2f, 0.8f, 0.7f), "Wood", yawToCam);
                    break;
            }
        }

        // ---- sanh chung ----
        static void BuildShared(Transform g)
        {
            Box(g, "Plaza_Floor", new Vector3(0, -0.25f, -8f), new Vector3(40f, 0.5f, 16f), "Ground");
            Box(g, "Hall_Floor", new Vector3(0, -0.25f, 56.5f), new Vector3(37f, 0.5f, 113f), "Stone");
            Box(g, "Wall_West", new Vector3(-18.5f, 4.5f, 56.5f), new Vector3(1f, 9f, 113f), "White");
            Box(g, "Wall_East", new Vector3(18.5f, 4.5f, 56.5f), new Vector3(1f, 9f, 113f), "White");
            Box(g, "Wall_Back", new Vector3(0, 4.5f, 113.5f), new Vector3(38f, 9f, 1f), "White");
            // Mat tien: kinh + cua xoay (hanh lang kinh giua x=-1.2..1.2 de camera di qua)
            Box(g, "Facade_L", new Vector3(-10.1f, 3f, -0.3f), new Vector3(16.2f, 6f, 0.4f), "Glass", 0f, false);
            Box(g, "Facade_R", new Vector3(10.1f, 3f, -0.3f), new Vector3(16.2f, 6f, 0.4f), "Glass", 0f, false);
            Box(g, "Facade_Lintel", new Vector3(0, 6.75f, -0.3f), new Vector3(36f, 4.5f, 0.6f), "Stone");
            Box(g, "Facade_Frame_L", new Vector3(-1.9f, 2.4f, -0.3f), new Vector3(0.5f, 4.8f, 0.7f), "Metal");
            Box(g, "Facade_Frame_R", new Vector3(1.9f, 2.4f, -0.3f), new Vector3(0.5f, 4.8f, 0.7f), "Metal");
            Box(g, "Vestibule_Glass_L", new Vector3(-1.3f, 1.3f, 1.2f), new Vector3(0.06f, 2.6f, 3.2f), "Glass", 0f, false);
            Box(g, "Vestibule_Glass_R", new Vector3(1.3f, 1.3f, 1.2f), new Vector3(0.06f, 2.6f, 3.2f), "Glass", 0f, false);
            foreach (var z in new[] { 10f, 38f, 74f, 90f })
                foreach (var x in new[] { -10f, 10f })
                    Box(g, "Column_" + (x < 0 ? "W_" : "E_") + z, new Vector3(x, 4.5f, z), new Vector3(1.2f, 9f, 1.2f), "Stone");
        }

        static void BuildP1(Transform geo, Transform decor)
        {
            // Ban tiep tan dai ~9 m (z=27), quay thong tin (z=34), tam chan sau ban, cot H02
            Box(geo, "Reception_Desk", new Vector3(0, 0.55f, 27f), new Vector3(8.8f, 1.1f, 1f), "Wood");
            Box(geo, "Reception_Desk_Top", new Vector3(0, 1.13f, 27f), new Vector3(9f, 0.07f, 1.2f), "Stone", 0f, false);
            Box(geo, "Reception_Backwall", new Vector3(0, 2.4f, 31.5f), new Vector3(10f, 4.8f, 0.4f), "GrayDark");
            Box(decor, "Reception_Sign", new Vector3(0, 3.9f, 31.25f), new Vector3(4.5f, 0.9f, 0.1f), "Red", 0f, false);
            Box(geo, "Info_Desk", new Vector3(-7.4f, 0.55f, 34f), new Vector3(3.4f, 1.1f, 1f), "Wood");
            Box(geo, "Info_Desk_Top", new Vector3(-7.4f, 1.13f, 34f), new Vector3(3.6f, 0.07f, 1.2f), "Stone", 0f, false);
            Box(geo, "ATM_Backpanel", new Vector3(9.2f, 1.3f, 30f), new Vector3(5f, 2.6f, 0.3f), "Metal");
            Box(geo, "Column_Hall_H02", new Vector3(-8.5f, 4.5f, 17f), new Vector3(1.4f, 9f, 1.4f), "Stone");
            // Chau cay lon, ghe cho nho ben canh
            foreach (var x in new[] { -13f, 13f })
            {
                Box(decor, "Planter_Big_" + (x < 0 ? "W" : "E"), new Vector3(x, 0.55f, 14f), new Vector3(2f, 1.1f, 2f), "GrayDark");
                Box(decor, "Planter_Big_Plants_" + (x < 0 ? "W" : "E"), new Vector3(x, 1.7f, 14f), new Vector3(1.6f, 1.2f, 1.6f), "Foliage", 0f, false);
            }
            Box(decor, "Floor_Strip_P1", new Vector3(0, 0.005f, 15f), new Vector3(6f, 0.01f, 30f), "White", 0f, false);
        }

        static void BuildP2(Transform geo, Transform decor)
        {
            // 6 quay giao dich lien nhau (z=60), cot vuong giua cac quay, kinh (khong collider), bien so quay
            for (int k = 0; k < 6; k++)
            {
                float x = -10.5f + 4.2f * k + (k < 3 ? -0.8f : 0.8f); // chua khe 2.0 m giua quay 3 va 4 cho rail P3 di qua
                Box(geo, "Counter_" + (k + 1), new Vector3(x, 0.55f, 60f), new Vector3(3.8f, 1.1f, 1.2f), "Wood");
                Box(geo, "Counter_Top_" + (k + 1), new Vector3(x, 1.13f, 60f), new Vector3(3.9f, 0.07f, 1.35f), "Stone", 0f, false);
                // Kinh quay: PropSlot_Glass_* -> Prop_Glass (BreakableGlass, ban vo duoc) khi ghep scene.
                Marker(decor, "PropSlot_Glass_P2_W0_" + (k + 1).ToString("00"), new Vector3(x, 1.95f, 60.35f), Quaternion.identity).localScale = new Vector3(3.5f, 1.7f, 1f);
                Box(decor, "Counter_Sign_" + (k + 1), new Vector3(x, 3.5f, 60f), new Vector3(1.4f, 0.55f, 0.1f), k % 2 == 0 ? "Red" : "Yellow", 0f, false);
            }
            for (int j = 0; j <= 6; j++)
            {
                if (j == 3) continue; // khe giua quay 3 va 4: khong co cot
                Box(geo, "Counter_Pier_" + j, new Vector3(-12.6f + 4.2f * j + (j < 3 ? -0.8f : 0.8f), 4.5f, 60.2f), new Vector3(0.7f, 9f, 0.9f), "Stone");
            }
            // Cua nhan vien o tuong sau ngay sau khe quay (x -1.75..1.75).
            Box(geo, "Counter_Backwall_L", new Vector3(-9.875f, 4.5f, 64.5f), new Vector3(16.25f, 9f, 0.6f), "GrayDark");
            Box(geo, "Counter_Backwall_R", new Vector3(9.875f, 4.5f, 64.5f), new Vector3(16.25f, 9f, 0.6f), "GrayDark");
            Box(geo, "Counter_Backwall_Lintel", new Vector3(0f, 6.5f, 64.5f), new Vector3(3.5f, 5f, 0.6f), "GrayDark");
            for (int i = 0; i < 4; i++)
                Box(decor, "Alarm_Light_" + i, new Vector3(-9f + 6f * i, 7.5f, 64.1f), new Vector3(0.6f, 0.35f, 0.3f), "LightRed", 0f, false);
            // Hang cho dung (xep hang) truoc quay
            Box(decor, "Queue_Rope_A", new Vector3(-14f, 0.45f, 52f), new Vector3(0.08f, 0.9f, 6f), "Metal");
            Box(decor, "Queue_Rope_B", new Vector3(14f, 0.45f, 52f), new Vector3(0.08f, 0.9f, 6f), "Metal");
        }

        static void BuildP3(Transform geo, Transform decor)
        {
            // Tang lung: san (z 108..112.5, cao 4.4), phan ben duoi day de che enemy rut xuong; lan can kinh gan cot
            Box(geo, "Mezzanine_Slab", new Vector3(0, 3.6f, 110.25f), new Vector3(28f, 1.6f, 4.5f), "Stone");
            for (int i = -2; i <= 2; i++)
                Box(geo, "Mezzanine_Post_" + (i + 2), new Vector3(i * 4f, 4.9f, 108.2f), new Vector3(0.12f, 1f, 0.12f), "Metal");
            Box(decor, "Mezzanine_Glass", new Vector3(0, 4.95f, 108.2f), new Vector3(16f, 0.9f, 0.04f), "Glass", 0f, false);
            Box(decor, "Mezzanine_Handrail", new Vector3(0, 5.42f, 108.2f), new Vector3(16f, 0.06f, 0.1f), "Metal", 0f, false);
            Box(geo, "Mezzanine_Backwall", new Vector3(0, 6.5f, 113f), new Vector3(28f, 4.2f, 0.3f), "GrayDark");
            // Hai canh cau thang da (x 9.5..13.5 va -13.5..-9.5), 22 bac tu z=92 den z=108, cao 4.4
            foreach (var side in new[] { -1f, 1f })
            {
                var st = Group(geo, side < 0 ? "Stairs_W" : "Stairs_E");
                for (int i = 0; i < 22; i++)
                {
                    float h = 0.2f * (i + 1);
                    Box(st, "Step_" + (i + 1), new Vector3(side * 11.5f, h * 0.5f, 92f + 0.727f * (i + 0.5f)), new Vector3(4f, h, 0.727f), "Stone");
                }
                Box(decor, "Stairs_Rail_In_" + (side < 0 ? "W" : "E"), new Vector3(side * 9.5f, 2.6f, 100f), new Vector3(0.1f, 0.9f, 16f), "Glass", 0f, false);
            }
            // Ghe cho phu + may rut so
            foreach (var x in new[] { -6.5f, -4f })
                Box(decor, "Bench_Side_" + x, new Vector3(x, 0.4f, 97f), new Vector3(2.2f, 0.8f, 0.7f), "Wood");
            Box(decor, "Ticket_Machine", new Vector3(-8f, 0.9f, 94f), new Vector3(0.7f, 1.8f, 0.6f), "PoliceBlue");
        }
    }
}
