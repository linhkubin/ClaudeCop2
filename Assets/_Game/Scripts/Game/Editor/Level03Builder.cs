using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ClaudeCop.Enemy;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// Level 3 (Tang lung va van phong) - blockout primitive + marker, luu Assets/_Game/Level/Level_03.prefab.
    /// Toa do LOCAL TRUNG voi Level_02 (cung goc, cung huong): trong chuoi Level_03 dat dung transform cua Level_02.
    /// Ray P1_S1 bat dau DUNG goc camera cuoi Level 2 (CamPoint_P3_S6 cua Level_02.prefab), len cau thang Dong cua sanh L2,
    /// qua tang lung, vao hanh lang van phong (san cao F = 4.4). P1 hanh lang, P2 phong hop kinh (gac thap, H01), P3 phong an ninh (cua ham).
    /// Group "Approach_Standalone" (san sanh, cau thang, san tang lung) chi de choi rieng Level_03.unity; chuoi go bo (dung cua L2).
    /// Marker: CamPoint_P*_S*, RailHint_P*_S*_NN, EnemySpawn_P*_W*_NN (+Peek, + SpawnPointEntry), HostageSpawn_*, Door_Enemy_P*_W*_NN,
    /// PropSlot_Glass_*, Gag_P2_W3_01 (GagFall). Chay lai an toan (ghi de prefab). Sau do chay Level03Assembler.
    /// </summary>
    public static class Level03Builder
    {
        public const string PrefabPath = "Assets/_Game/Level/Level_03.prefab";
        const string MatDir = "Assets/_Game/Level/Materials/";
        public const float F = 4.4f;          // cao do san van phong (= mat san tang lung Level 2)
        const float Eye = 1.65f;
        /// <summary>Goc dwell cuoi Level 3 (nhin cua ham mo truoc bang ket qua / truoc ray Level 4).</summary>
        public const string ExitCamPoint = "CamPoint_P3_S7";
        /// <summary>Ban dieu khien phong an ninh (chan ngang lan z 160..165, nam tren ray chuyen canh L3 -> L4): tam x, nua be day, mat tren (ca console).</summary>
        public const float DeskX = 40.30f, DeskHalf = 0.4f, DeskTop = F + 0.96f;

        // ---- du lieu ----
        struct Sp { public int p, w; public char kind; public float x, z; public string cover; public EnemyActor.EntryStyle style; public float drop; }
        static Sp E(int p, int w, float x, float z, EnemyActor.EntryStyle st = EnemyActor.EntryStyle.Auto, string cover = null, float drop = 0f)
            => new Sp { p = p, w = w, kind = 'E', x = x, z = z, cover = cover, style = st, drop = drop };
        static Sp H(int p, int w, float x, float z) => new Sp { p = p, w = w, kind = 'H', x = x, z = z };

        const EnemyActor.EntryStyle Door = EnemyActor.EntryStyle.Door, Drop = EnemyActor.EntryStyle.Drop,
            Vault = EnemyActor.EntryStyle.Vault, Slide = EnemyActor.EntryStyle.Slide;

        // 19 enemy (+1 ten tu nga H01 khong tinh) + 3 con tin. W = so shot (2,3,5,6) - wave GDD: W1->S2, W2->S3, W3->S5+S6 (P1, P2); P3: W1->S2+S3, W2->S5, W3->S6.
        // L3-FIX (2026-10-06): them 4 enemy (P1_W2, P1_W5, P2_W5, P3_W3) - tong 19 <= 20, toi da 3/wave.
        // L3-HOST (2026-10-06): +1 enemy xa P1_W3 (tong 20 = gioi han), +2 con tin chan mot phan (P2_W2 phai, P3_W6 phai) -> 5 con tin.
        static readonly Sp[] Spawns =
        {
            // P1 hanh lang van phong (x 8.5..14.5, z 113.5..148)
            E(1,2, 11.5f,133.0f, Door),                       // W1: lo ra tu cua van phong
            E(1,2, 13.0f,134.6f, Door),                       // W1b: ra tu cua Dong (Door_Enemy_P1_W3_01)
            E(1,3, 12.6f,135.8f, Door),                       // W2: cam sung lo ro - Justice
            E(1,3, 10.8f,138.6f, Door),                       // W2b (L3-HOST): ten o XA (~22.5 m, xa nhat P1) truoc tu ho so CabinetLow, ra tu cua Dong
            E(1,5,  9.5f,140.5f, Vault, "CabinetLow"),        // W3a: nhay qua tu ho so
            E(1,5, 12.0f,141.0f, Door),                       // W3a2: ra tu cua Dong (Door_Enemy_P1_W6_01)
            E(1,6, 12.8f,142.0f, Door),                       // W3b
            // P2 phong hop kinh (x 2..19, z 148..176), gac thap z 170..176 cao F+3
            // L3-HOST: con tin cu 9.4 che kin enemy W1 (44/45 tia) -> doi sang 8.2 (ke ben trai enemy); them con tin BEN PHAI dung chan
            // ~1/2 ben phai enemy (nua trai + dau van ban duoc). H01 dich +1.5 m de con tin phai (dong bang sau W1) khong che ten nhay.
            E(2,2,  8.4f,171.2f, Door), H(2,2, 7.4f,165.7f), H(2,2, 8.2f,165.7f), H(2,2, 9.5f,167.6f),   // W1: sau cua kinh + 3 con tin
            E(2,3, 11.0f,169.4f, Drop, null, 3.0f), E(2,3, 14.0f,169.4f, Drop, null, 3.0f), // W2 H01: nhay tu gac thap (ten giua = GagFall, x 12.5)
            E(2,5, 16.0f,168.0f, EnemyActor.EntryStyle.Auto, "CabinetTall"),        // W3a: chui len sau tu ho so cao
            E(2,5, 14.0f,169.4f, Drop, null, 3.0f),           // W3a2: nhay tu gac thap
            E(2,6, 17.6f,167.0f, Vault, "CabinetLow"),        // W3b
            // P3 phong an ninh (x 19..45, z 153..172)
            // L3-HOST: con tin W1a cu (41.3,161.6) dong bang sau dot nen che kin W2_01 va 3/4 ten cuoi -> doi ra sau, sat tuong Dong-Bac.
            E(3,2, 42.5f,163.6f), H(3,2, 44.3f,165.2f),       // W1a: Justice + con tin bi troi ghe
            E(3,3, 42.5f,164.6f, Slide),                      // W1b: Justice
            E(3,3, 43.4f,166.0f, Slide),                      // W1b2
            E(3,5, 43.8f,161.4f), E(3,5, 44.2f,163.6f), E(3,5, 44.0f,162.5f),     // W2: don dap (Rush, toi da 2)
            E(3,6, 43.8f,160.9f), H(3,6, 42.3f,160.65f),      // W3: ten cuoi truoc man hinh, kill-zoom + con tin BEN PHAI chan ~1/2 phai (L3-HOST)
        };

        static Vector3 CamPos(int p, int s)
        {
            float y = F + Eye;
            switch (p * 10 + s)
            {
                case 12: return new Vector3(11.5f, y, 115.9f);   // L3-FIX: lui 0.6 m bu do dai ray P1_S1 cong rong hon (Move <= 6 s)
                case 13: return new Vector3(11.7f, y, 116.1f);
                case 15: return new Vector3(11.5f, y, 126.0f);
                case 16: return new Vector3(11.7f, y, 126.2f);
                case 22: return new Vector3(11.5f, y, 152.0f);
                case 23: return new Vector3(11.7f, y, 152.2f);
                case 25: return new Vector3(12.5f, y, 155.5f);
                case 26: return new Vector3(12.7f, y, 155.7f);
                case 32: return new Vector3(29.0f, y, 162.5f);
                case 33: return new Vector3(29.2f, y, 162.7f);
                // L3-HOST: lui 1.5 m de con tin chan ten cuoi (dung truoc enemy) van cach camera >= 12 m
                case 35: return new Vector3(30.0f, y, 162.5f);
                case 36: return new Vector3(30.2f, y, 162.7f);
            }
            return Vector3.zero;
        }

        /// <summary>Do cao camera tren cau thang Dong cua Level 2 (bac 0.2 m / 0.727 m tu z=92 toi z=108, cao 4.4).</summary>
        static float StairEye(float z) => Eye + Mathf.Clamp((z - 92f) * 0.275f, 0f, F);

        // ---- materials ----
        static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();
        static Material M(string n)
        {
            if (mats.TryGetValue(n, out var m) && m != null) return m;
            m = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Blockout_" + n + ".mat");
            if (m == null) m = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Blockout_Gray.mat");
            return mats[n] = m;
        }

        // ---- helpers ----
        static Transform Group(Transform parent, string name) { var g = new GameObject(name).transform; g.SetParent(parent, false); return g; }

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

        /// <summary>Vat the chuyen dong luc choi (cua): bo co static (static batching lam no dung im).</summary>
        static void Movable(GameObject go) { GameObjectUtility.SetStaticEditorFlags(go, 0); }

        static Transform Marker(Transform parent, string name, Vector3 pos, Quaternion rot)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); t.SetPositionAndRotation(pos, rot); return t;
        }

        static float YawToward(Vector3 from, Vector3 to) => Mathf.Atan2(to.x - from.x, to.z - from.z) * Mathf.Rad2Deg;
        static Vector3 Fwd(float yawDeg) => Quaternion.Euler(0f, yawDeg, 0f) * Vector3.forward;

        /// <summary>Cua tren tuong: khung + o (den = cua mo cho enemy; go = cua dong). Group dat sat mat tuong, nhin theo normal (vao phong).</summary>
        static Transform DoorAt(Transform parent, string name, Vector3 wallFloorPos, Vector3 normal, bool open)
        {
            var g = new GameObject(name).transform;
            g.SetParent(parent, false);
            g.SetPositionAndRotation(wallFloorPos + normal * 0.02f, Quaternion.LookRotation(normal));
            void Part(string n, Vector3 local, Vector3 size, string mat)
            {
                var b = Box(g, n, Vector3.zero, size, mat, 0f, false);
                b.transform.localPosition = local; b.transform.localRotation = Quaternion.identity;
            }
            Part("DoorJamb_L", new Vector3(-0.62f, 1.12f, 0.05f), new Vector3(0.12f, 2.24f, 0.1f), "White");
            Part("DoorJamb_R", new Vector3(0.62f, 1.12f, 0.05f), new Vector3(0.12f, 2.24f, 0.1f), "White");
            Part("DoorHead", new Vector3(0f, 2.3f, 0.05f), new Vector3(1.36f, 0.14f, 0.12f), "White");
            Part("DoorPanel", new Vector3(0f, 1.06f, 0.01f), new Vector3(1.12f, 2.12f, 0.03f), open ? "Black" : "Wood");
            return g;
        }

        [MenuItem("ClaudeCop/Game/Build Level_03 Prefab")]
        public static void Build()
        {
            mats.Clear();
            var root = new GameObject("Level_03");
            var approach = Group(root.transform, "Approach_Standalone");
            var shell = Group(root.transform, "Shared_Office");
            var a1 = Group(root.transform, "Area_P1_Corridor");
            var a2 = Group(root.transform, "Area_P2_Meeting");
            var a3 = Group(root.transform, "Area_P3_Security");
            var areas = new[] { a1, a2, a3 };

            BuildApproach(approach);
            BuildShell(shell);
            BuildP1(Group(a1, "Geometry"), Group(a1, "Decor"));
            BuildP2(Group(a2, "Geometry"), Group(a2, "Decor"));
            BuildP3(Group(a3, "Geometry"), Group(a3, "Decor"));

            // Goc camera cuoi Level 2 (diem bat dau ray P1_S1)
            Vector3 l2End = new Vector3(1.5f, Eye, 82.4f); float l2Yaw = 7f;
            var l2 = AssetDatabase.LoadAssetAtPath<GameObject>(Level02Builder.PrefabPath);
            if (l2 != null)
                foreach (var t in l2.GetComponentsInChildren<Transform>(true))
                    if (t.name == "CamPoint_P3_S6") { l2End = t.position; l2Yaw = t.eulerAngles.y; }

            var camYaw = new Dictionary<int, float>();
            var report = new System.Text.StringBuilder("[Level03Builder] ");
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
                    Marker(camPts, "CamPoint_P" + p + "_S" + s, c, Quaternion.Euler(0f, yaw, 0f));
                }
                foreach (int s in new[] { 1, 4 })
                {
                    List<Vector3> pts;
                    if (s == 1) pts = LinkRail(p, camYaw, l2End, l2Yaw);
                    else pts = MidRail(p);
                    Marker(camPts, "CamPoint_P" + p + "_S" + s, pts[0], Quaternion.identity);
                    for (int i = 0; i < pts.Count; i++)
                        Marker(hints, "RailHint_P" + p + "_S" + s + "_" + (i + 1).ToString("00"), pts[i], Quaternion.identity);
                    if (s == 1) report.Append("P" + p + "S1 max " + MaxTurnPerMeter(pts).ToString("0.0") + " do/m, dai " + Length(pts).ToString("0.0") + " m; ");
                }
            }
            // L3-L4: goc "nhin cua ham mo" sau wave cuoi (shot dwell, khong encounter): cung vi tri S6, nhin thang vao cua ham.
            // Level04Builder bat dau ray P1_S1 tai day theo huong nay (thang qua cua, khong cong chu S).
            {
                var c6 = CamPos(3, 6);
                Marker(areas[2].Find("CamPoints"), ExitCamPoint, c6, Quaternion.Euler(0f, YawToward(c6, new Vector3(45.6f, F + Eye, 162.5f)), 0f));
            }

            // Spawn / hostage + kieu xuat hien + vat nap
            var spawnGroups = new[] { Group(a1, "Spawns"), Group(a2, "Spawns"), Group(a3, "Spawns") };
            var covers = new[] { Group(a1, "Cover"), Group(a2, "Cover"), Group(a3, "Cover") };
            var counters = new Dictionary<string, int>();
            foreach (var sp in Spawns)
            {
                string key = "P" + sp.p + "_W" + sp.w + "_";
                string tag = sp.kind == 'H' ? "HostageSpawn_" : "EnemySpawn_";
                counters.TryGetValue(tag + key, out int n); counters[tag + key] = ++n;
                string name = tag + key + n.ToString("00");
                var cam = CamPos(sp.p, sp.w);
                var ground = new Vector3(sp.x, F, sp.z);
                float yaw = YawToward(ground, cam);
                var rot = Quaternion.Euler(0f, yaw, 0f);
                var t = Marker(spawnGroups[sp.p - 1], name, ground + Vector3.down * 1.15f, rot);
                Marker(t, "Peek", ground + Vector3.up * 0.05f, rot);
                if (sp.kind == 'E' && (sp.style != EnemyActor.EntryStyle.Auto || sp.drop > 0f))
                {
                    var spe = t.gameObject.AddComponent<SpawnPointEntry>();
                    var so = new SerializedObject(spe);
                    so.FindProperty("style").enumValueIndex = (int)sp.style;
                    so.FindProperty("dropHeight").floatValue = sp.drop;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                if (!string.IsNullOrEmpty(sp.cover)) BuildCover(covers[sp.p - 1], name.Replace(tag, "Cover_" + sp.cover + "_"), sp.cover, ground, yaw);
            }

            BuildGag(Group(a2, "Gags"));

            System.IO.Directory.CreateDirectory("Assets/_Game/Level");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            var sb = new System.Text.StringBuilder();
            foreach (var kv in camYaw) sb.Append("P" + kv.Key / 10 + "S" + kv.Key % 10 + "=" + kv.Value + " ");
            Debug.Log(report + "yaw cam: " + sb + "- da luu " + PrefabPath + " (" + Spawns.Length + " diem).");
        }

        // ---- ray ----

        /// <summary>Ray noi Phase (camera nhin theo tiep tuyen): Hermite mem tu huong goc truoc toi diem cach shot ke 6 m, 6 m cuoi thang hang yaw/pitch shot ke.</summary>
        static List<Vector3> LinkRail(int p, Dictionary<int, float> camYaw, Vector3 l2End, float l2Yaw)
        {
            var end = CamPos(p, 2); float ny = camYaw[p * 10 + 2];
            var list = new List<Vector3>();
            if (p == 1)
            {
                // Tu goc cuoi Level 2 cong mem sang cau thang Dong (x=11.5), len cau thang, qua tang lung, vao hanh lang.
                list.Add(l2End);
                var lead = l2End + Fwd(l2Yaw) * 1.5f;   // tiep tuyen dau ray = huong goc cuoi Level 2 (AutoSmooth huong toi diem ke)
                list.Add(lead);
                // L3-FIX: cong rong hon vao long cau thang - bat truc thang som hon (z 100 thay 102, tiep tuyen cuoi x1.3):
                // camera vao trong thang (x >= 10) tu z ~95 thay vi ~97.5, khong cham cot Column_E_90 (cach >= 2 m).
                var stair = new Vector3(11.5f, 0f, 100f);
                AddHermite(list, lead, l2Yaw, stair, 0f, 7, z => StairEye(z), 1f, 1.3f);
                list.Add(new Vector3(11.5f, StairEye(104f), 104f));
                // Dinh thang: dat tren duong thang toi shot ke (yaw ny) de khong co khuc lac ngang o dau tang lung.
                var top = end - Fwd(ny) * (end.z - 108.3f); top.y = F + Eye;
                list.Add(top);
            }
            else
            {
                var start = CamPos(p - 1, 6); float py = camYaw[(p - 1) * 10 + 6];
                var lead = start + Fwd(py) * 1.5f;
                list.Add(start); list.Add(lead);
                AddHermite(list, lead, py, end - Fwd(ny) * 6f, ny, 4, z => F + Eye);
                list.RemoveAt(list.Count - 1);
            }
            list.Add(end - Fwd(ny) * 6f);
            list.Add(end - Fwd(ny) * 3f);
            list.Add(end);
            return list;
        }

        static List<Vector3> MidRail(int p)
        {
            Vector3 a = CamPos(p, 3), b = CamPos(p, 5);
            return new List<Vector3> { a, Vector3.Lerp(a, b, 0.33f), Vector3.Lerp(a, b, 0.66f), b };
        }

        /// <summary>Them n diem Hermite giua (tiep tuyen theo yaw dau/cuoi, he so 1.0) roi diem b. Do cao theo yAt(z).</summary>
        static void AddHermite(List<Vector3> list, Vector3 a, float yawA, Vector3 b, float yawB, int n, System.Func<float, float> yAt, float k0 = 1f, float k1 = 1f)
        {
            float len = Vector3.Distance(new Vector3(a.x, 0, a.z), new Vector3(b.x, 0, b.z));
            Vector3 m0 = Fwd(yawA) * len * k0, m1 = Fwd(yawB) * len * k1;
            for (int i = 1; i <= n + 1; i++)
            {
                float t = i / (float)(n + 1), t2 = t * t, t3 = t2 * t;
                Vector3 pt = (2f * t3 - 3f * t2 + 1f) * a + (t3 - 2f * t2 + t) * m0 + (-2f * t3 + 3f * t2) * b + (t3 - t2) * m1;
                pt.y = yAt(pt.z);
                list.Add(pt);
            }
        }

        static float MaxTurnPerMeter(List<Vector3> pts)
        {
            float worst = 0f;
            for (int i = 1; i < pts.Count - 1; i++)
            {
                Vector3 d0 = pts[i] - pts[i - 1], d1 = pts[i + 1] - pts[i]; d0.y = 0; d1.y = 0;
                float seg = (d0.magnitude + d1.magnitude) * 0.5f; if (seg < 0.01f) continue;
                worst = Mathf.Max(worst, Vector3.Angle(d0, d1) / seg);
            }
            return worst;
        }

        static float Length(List<Vector3> pts) { float l = 0f; for (int i = 1; i < pts.Count; i++) l += Vector3.Distance(pts[i - 1], pts[i]); return l; }

        // ---- vat nap ----
        static void BuildCover(Transform parent, string name, string type, Vector3 enemyGround, float yawToCam)
        {
            Vector3 toCam = Quaternion.Euler(0, yawToCam, 0) * Vector3.forward;
            const float depth = 0.6f;
            Vector3 c = enemyGround + toCam * (depth * 0.5f + 0.5f);   // nua be day + 0.5 m: enemy khong chui vao vat the
            if (type == "CabinetTall")
            {
                Box(parent, name, c + Vector3.up * 0.7f, new Vector3(1.0f, 1.4f, depth), "Metal", yawToCam);
                for (int i = 0; i < 3; i++)
                    Box(parent, name + "_Drawer_" + i, c + Vector3.up * (0.3f + 0.42f * i) + toCam * 0.31f, new Vector3(0.8f, 0.05f, 0.02f), "GrayDark", yawToCam, false);
            }
            else
            {
                Box(parent, name, c + Vector3.up * 0.5f, new Vector3(1.2f, 1.0f, depth), "Metal", yawToCam);
                Box(parent, name + "_Papers", c + Vector3.up * 1.03f, new Vector3(0.4f, 0.06f, 0.3f), "White", yawToCam + 12f, false);
            }
        }

        // ---- canh hai H01: ten thu 3 nhay tu gac thap, vuong lan can, chui mat nam bat dong (GagFall, khong phai muc tieu) ----
        static void BuildGag(Transform parent)
        {
            var g = new GameObject("Gag_P2_W3_01");
            g.transform.SetParent(parent, false);
            // L3-HOST: dung giua 2 ten nhay (x 11.0 / 14.0). Than lay DUNG mesh/vi tri/scale/material cua Body trong Enemy.prefab
            // (truoc: capsule 0.5 x 1.8 m, nho hon enemy 1 x 2 m) -> 3 ten cung kich thuoc.
            const float gx = 12.5f;
            var start = new Vector3(gx, F + 3.0f, 171.0f);
            var face = Quaternion.Euler(0f, 180f, 0f);                         // nhin ve phia camera (-Z)
            g.transform.SetPositionAndRotation(start, face);
            var body = Marker(g.transform, "Body", start, face);
            Vector3 bodyLocalPos = new Vector3(0f, 1f, 0f), bodyLocalScale = Vector3.one; Material enemyMat = null;
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Enemies/Enemy.prefab");
            var srcBody = enemyPrefab != null ? enemyPrefab.transform.Find("Body") : null;
            if (srcBody != null)
            {
                bodyLocalPos = srcBody.localPosition; bodyLocalScale = srcBody.localScale;
                var sr = srcBody.GetComponent<Renderer>(); if (sr != null) enemyMat = sr.sharedMaterial;
            }
            var cap = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cap.name = "BodyMesh"; Object.DestroyImmediate(cap.GetComponent<Collider>());
            cap.transform.SetParent(body, false); cap.transform.localPosition = bodyLocalPos; cap.transform.localScale = bodyLocalScale;
            cap.GetComponent<MeshRenderer>().sharedMaterial = enemyMat != null ? enemyMat : M("Red");
            var gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gun.name = "Gun"; Object.DestroyImmediate(gun.GetComponent<Collider>());
            gun.transform.SetParent(g.transform, false);
            gun.transform.SetPositionAndRotation(start + face * new Vector3(0.45f, 1.15f, 0.5f), face);   // tay cam sung (cung cho JusticeMarker cua Enemy)
            gun.transform.localScale = new Vector3(0.08f, 0.14f, 0.35f);
            gun.GetComponent<MeshRenderer>().sharedMaterial = M("Black");
            var hop = Marker(g.transform, "Hop", new Vector3(gx, F + 3.7f, 170.2f), face);
            var land = Marker(g.transform, "Land", new Vector3(gx, F + 0.5f, 169.4f), face);       // nam sap: chan cao = ban kinh than (0.5 m), than dai 2 m ve -Z
            var gunLand = Marker(g.transform, "GunLand", new Vector3(gx + 1.0f, F + 0.05f, 166.6f), face);
            var gf = g.AddComponent<GagFall>();
            var so = new SerializedObject(gf);
            so.FindProperty("body").objectReferenceValue = body;
            so.FindProperty("gun").objectReferenceValue = gun.transform;
            so.FindProperty("hop").objectReferenceValue = hop;
            so.FindProperty("land").objectReferenceValue = land;
            so.FindProperty("gunLand").objectReferenceValue = gunLand;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---- hinh khoi ----

        /// <summary>Chi dung khi choi rieng Level_03.unity: phan sanh/cau thang/tang lung cua Level 2 ma ray P1_S1 di qua. Chuoi go bo group nay.</summary>
        static void BuildApproach(Transform g)
        {
            Box(g, "Approach_Floor", new Vector3(7f, -0.25f, 91.5f), new Vector3(26f, 0.5f, 43f), "Stone");
            for (int i = 0; i < 22; i++)
            {
                float h = 0.2f * (i + 1);
                Box(g, "Approach_Step_" + (i + 1), new Vector3(11.5f, h * 0.5f, 92f + 0.727f * (i + 0.5f)), new Vector3(4f, h, 0.727f), "Stone");
            }
            Box(g, "Approach_Mezzanine", new Vector3(9f, 3.6f, 110.25f), new Vector3(20f, 1.6f, 4.5f), "Stone");
        }

        static void BuildShell(Transform g)
        {
            // Tuong sau sanh L2 (thay Wall_Back + Mezzanine_Backwall cua L2 trong chuoi): cua vao hanh lang x 10..13 o cao do tang lung.
            Box(g, "Hall_BackWall_W", new Vector3(-4.5f, 4.5f, 113.5f), new Vector3(29f, 9f, 1f), "White");
            Box(g, "Hall_BackWall_E", new Vector3(16f, 4.5f, 113.5f), new Vector3(6f, 9f, 1f), "White");
            Box(g, "Hall_BackWall_Lintel", new Vector3(11.5f, 8.2f, 113.5f), new Vector3(3f, 1.6f, 1f), "White");
            Box(g, "Hall_BackWall_Low", new Vector3(11.5f, 2.2f, 113.5f), new Vector3(3f, 4.4f, 1f), "White");
            // San van phong (cao F)
            Box(g, "Office_Floor", new Vector3(23.5f, F - 0.15f, 144.2f), new Vector3(43f, 0.3f, 63.6f), "Gray");
            Box(g, "Office_Floor_Strip", new Vector3(11.5f, F + 0.005f, 130.75f), new Vector3(2.2f, 0.01f, 34.5f), "Wood", 0f, false);
        }

        static void BuildP1(Transform geo, Transform decor)
        {
            // Hanh lang 6 m x 34.5 m, hai day cua van phong
            Box(geo, "Corridor_Wall_W", new Vector3(8.35f, F + 1.75f, 130.75f), new Vector3(0.3f, 3.5f, 34.5f), "White");
            Box(geo, "Corridor_Wall_E", new Vector3(14.65f, F + 1.75f, 130.75f), new Vector3(0.3f, 3.5f, 34.5f), "White");
            Box(decor, "Corridor_Skirting_W", new Vector3(8.52f, F + 0.06f, 130.75f), new Vector3(0.04f, 0.12f, 34.5f), "Wood", 0f, false);
            Box(decor, "Corridor_Skirting_E", new Vector3(14.48f, F + 0.06f, 130.75f), new Vector3(0.04f, 0.12f, 34.5f), "Wood", 0f, false);
            // Cua cho enemy (o den) - Door_Enemy_<P>_<W>_<NN>
            DoorAt(decor, "Door_Enemy_P1_W2_01", new Vector3(8.5f, F, 132.5f), Vector3.right, true);
            DoorAt(decor, "Door_Enemy_P1_W3_01", new Vector3(14.5f, F, 135.5f), Vector3.left, true);
            DoorAt(decor, "Door_Enemy_P1_W6_01", new Vector3(14.5f, F, 141.5f), Vector3.left, true);
            // Cua van phong dong
            int k = 0;
            foreach (var z in new[] { 120f, 126f, 145.5f }) DoorAt(decor, "Door_Office_" + (++k).ToString("00"), new Vector3(8.5f, F, z), Vector3.right, false);
            foreach (var z in new[] { 121f, 128f, 146f }) DoorAt(decor, "Door_Office_" + (++k).ToString("00"), new Vector3(14.5f, F, z), Vector3.left, false);
            // May loc nuoc, tu ho so sat tuong (khong collider: khong chan tam nhin), bang ten
            Box(decor, "Water_Cooler", new Vector3(13.95f, F + 0.6f, 130.2f), new Vector3(0.4f, 1.2f, 0.4f), "White", 0f, false);
            Box(decor, "Water_Cooler_Bottle", new Vector3(13.95f, F + 1.4f, 130.2f), new Vector3(0.3f, 0.4f, 0.3f), "Blue", 0f, false);
            Box(decor, "Cabinet_Wall_E", new Vector3(14.15f, F + 0.5f, 124f), new Vector3(0.6f, 1.0f, 1.2f), "Metal", 0f, false);
            Box(decor, "Notice_Board", new Vector3(8.55f, F + 1.7f, 137f), new Vector3(0.05f, 0.9f, 1.4f), "Cardboard", 0f, false);
            for (int i = 0; i < 5; i++)
                Box(decor, "Ceiling_Light_" + i, new Vector3(11.5f, F + 3.45f, 117f + 7f * i), new Vector3(1.2f, 0.05f, 0.4f), "White", 0f, false);
            Box(decor, "Paper_Scatter_1", new Vector3(10.4f, F + 0.01f, 129f), new Vector3(0.3f, 0.01f, 0.4f), "White", 25f, false);
            Box(decor, "Paper_Scatter_2", new Vector3(12.6f, F + 0.01f, 137.5f), new Vector3(0.3f, 0.01f, 0.4f), "White", -40f, false);
        }

        static void BuildP2(Transform geo, Transform decor)
        {
            // Phong hop: tuong truoc kinh (z=148), mieng hanh lang x 8.5..14.5
            Box(decor, "Meeting_Glass_Front_W", new Vector3(5.1f, F + 2.75f, 148f), new Vector3(6.2f, 5.5f, 0.06f), "Glass", 0f, false);
            Box(decor, "Meeting_Glass_Front_E", new Vector3(16.9f, F + 2.75f, 148f), new Vector3(4.2f, 5.5f, 0.06f), "Glass", 0f, false);
            Box(geo, "Meeting_Front_Lintel", new Vector3(11.5f, F + 4.5f, 148f), new Vector3(6.6f, 2f, 0.3f), "White");
            Box(geo, "Meeting_Wall_W", new Vector3(1.85f, F + 2.75f, 162f), new Vector3(0.3f, 5.5f, 28f), "White");
            Box(geo, "Meeting_Wall_E_S", new Vector3(19.15f, F + 2.75f, 153.75f), new Vector3(0.3f, 5.5f, 11.5f), "White");
            Box(geo, "Meeting_Wall_E_N", new Vector3(19.15f, F + 2.75f, 170.25f), new Vector3(0.3f, 5.5f, 11.5f), "White");
            Box(geo, "Meeting_Wall_E_Lintel", new Vector3(19.15f, F + 4.25f, 162f), new Vector3(0.3f, 2.5f, 5f), "White");
            Box(geo, "Meeting_Wall_Back", new Vector3(10.5f, F + 2.75f, 176.15f), new Vector3(17.3f, 5.5f, 0.3f), "White");
            // Gac thap (mezzanine) z 170..176, mat san F+3.0, lan can kinh o z=170.1
            Box(geo, "Loft_Slab", new Vector3(10.5f, F + 2.8f, 173f), new Vector3(17f, 0.4f, 6f), "Wood");
            Box(decor, "Loft_Rail_Glass", new Vector3(10.5f, F + 3.5f, 170.1f), new Vector3(17f, 1.0f, 0.05f), "Glass", 0f, false);
            Box(decor, "Loft_Handrail", new Vector3(10.5f, F + 4.02f, 170.1f), new Vector3(17f, 0.06f, 0.1f), "Metal", 0f, false);
            for (int i = 0; i < 6; i++)
                Box(decor, "Loft_Post_" + i, new Vector3(2.3f + 3.3f * i, F + 3.5f, 170.1f), new Vector3(0.08f, 1.0f, 0.08f), "Metal", 0f, false);
            // Van phong kinh duoi gac: cua kinh ban vo duoc (PropSlot_Glass) truoc enemy W1, cua sau cho enemy
            Marker(decor, "PropSlot_Glass_P2_W2_01", new Vector3(8.4f, F + 1.3f, 170.4f), Quaternion.identity).localScale = new Vector3(2.4f, 2.6f, 1f);
            Box(decor, "Office_Glass_W", new Vector3(4.6f, F + 1.3f, 170.4f), new Vector3(5.2f, 2.6f, 0.05f), "Glass", 0f, false);
            Box(decor, "Office_Glass_E", new Vector3(14.3f, F + 1.3f, 170.4f), new Vector3(9.4f, 2.6f, 0.05f), "Glass", 0f, false);
            Box(decor, "Office_Mullion_1", new Vector3(7.15f, F + 1.3f, 170.4f), new Vector3(0.1f, 2.6f, 0.1f), "Metal", 0f, false);
            Box(decor, "Office_Mullion_2", new Vector3(9.65f, F + 1.3f, 170.4f), new Vector3(0.1f, 2.6f, 0.1f), "Metal", 0f, false);
            DoorAt(decor, "Door_Enemy_P2_W2_01", new Vector3(8.4f, F, 176f), Vector3.back, true);
            // Ban hop dai + ghe xoay, man hinh
            Box(geo, "Meeting_Table", new Vector3(8.4f, F + 0.375f, 164.5f), new Vector3(3.6f, 0.75f, 1.2f), "Wood");
            foreach (var x in new[] { 7.4f, 9.4f })
            {
                Box(decor, "Chair_Back_" + x, new Vector3(x, F + 0.45f, 166.3f), new Vector3(0.5f, 0.9f, 0.5f), "GrayDark", 0f, false);
                Box(decor, "Chair_Front_" + x, new Vector3(x, F + 0.3f, 163.4f), new Vector3(0.45f, 0.6f, 0.45f), "GrayDark", 0f, false);
            }
            Box(decor, "Meeting_Screen", new Vector3(2.05f, F + 2.0f, 164.5f), new Vector3(0.08f, 1.2f, 2.0f), "Blue", 0f, false);
            // Thang may tuong Dong (cua truot, mo luc canh hai xong - chuong thang may / giam tai)
            Box(decor, "Elevator_Cab", new Vector3(18.97f, F + 1.15f, 152.75f), new Vector3(0.04f, 2.3f, 2.0f), "Yellow", 0f, false);
            Box(decor, "Elevator_Frame", new Vector3(18.95f, F + 2.4f, 152.75f), new Vector3(0.1f, 0.2f, 2.2f), "Metal", 0f, false);
            Movable(Box(decor, "Elevator_Door_L", new Vector3(18.88f, F + 1.15f, 152.25f), new Vector3(0.06f, 2.3f, 1.0f), "Metal", 0f, false));
            Movable(Box(decor, "Elevator_Door_R", new Vector3(18.88f, F + 1.15f, 153.25f), new Vector3(0.06f, 2.3f, 1.0f), "Metal", 0f, false));
            Box(decor, "Elevator_Light", new Vector3(18.9f, F + 2.7f, 152.75f), new Vector3(0.06f, 0.15f, 0.4f), "LightRed", 0f, false);
            Box(decor, "Paper_Scatter_3", new Vector3(13.5f, F + 0.01f, 160f), new Vector3(0.3f, 0.01f, 0.4f), "White", 60f, false);
        }

        static void BuildP3(Transform geo, Transform decor)
        {
            // Phong an ninh x 19..45, z 153..172; cua ham o tuong Dong (x=45, z 161..164)
            Box(geo, "Security_Wall_N", new Vector3(32f, F + 1.75f, 172.15f), new Vector3(26.6f, 3.5f, 0.3f), "GrayDark");
            Box(geo, "Security_Wall_S", new Vector3(32f, F + 1.75f, 152.85f), new Vector3(26.6f, 3.5f, 0.3f), "GrayDark");
            Box(geo, "Security_Wall_E_S", new Vector3(45.15f, F + 1.75f, 157f), new Vector3(0.3f, 3.5f, 8f), "GrayDark");
            Box(geo, "Security_Wall_E_N", new Vector3(45.15f, F + 1.75f, 168f), new Vector3(0.3f, 3.5f, 8f), "GrayDark");
            Box(geo, "Security_Wall_E_Lintel", new Vector3(45.15f, F + 3.15f, 162.5f), new Vector3(0.3f, 0.7f, 3f), "GrayDark");
            // Tuong man hinh camera
            for (int i = 0; i < 4; i++)
            {
                Box(decor, "Monitor_S_" + i, new Vector3(44.95f, F + 1.3f + 0.8f * (i % 2), 154.5f + 3f * (i / 2) + (i % 2) * 1.2f), new Vector3(0.06f, 0.7f, 1.2f), "Blue", 0f, false);
                Box(decor, "Monitor_N_" + i, new Vector3(44.95f, F + 1.3f + 0.8f * (i % 2), 165.5f + 3f * (i / 2) + (i % 2) * 1.2f), new Vector3(0.06f, 0.7f, 1.2f), "Blue", 0f, false);
            }
            // Ban dieu khien (thap) + tu server sat tuong
            Box(geo, "Control_Desk", new Vector3(DeskX, F + 0.45f, 162.5f), new Vector3(2f * DeskHalf, 0.9f, 5.0f), "GrayDark");
            // L3-FIX: man hinh ban dieu khien cu (Control_Desk_Screens, xanh, cao F+0.9..1.3, rong 3.6 m) chan than moi enemy P3.
            // Thay bang mat console nam phang tren mat ban (cao toi F+0.96): khong che tam nhin camera -> enemy.
            Box(decor, "Control_Desk_Console", new Vector3(40.30f, F + 0.93f, 162.5f), new Vector3(0.6f, 0.06f, 3.6f), "Blue", 0f, false);
            foreach (var x in new[] { 33f, 35.5f, 38f })
            {
                Box(geo, "Server_Rack_S_" + x, new Vector3(x, F + 1.1f, 154.0f), new Vector3(1.2f, 2.2f, 0.8f), "Metal");
                Box(geo, "Server_Rack_N_" + x, new Vector3(x, F + 1.1f, 171.0f), new Vector3(1.2f, 2.2f, 0.8f), "Metal");
            }
            Box(decor, "Hostage_Chair", new Vector3(44.6f, F + 0.45f, 165.6f), new Vector3(0.5f, 0.9f, 0.5f), "Wood", 0f, false);
            // Cua ham: ban le o z=164, la cua xoay ra ngoai (DoorOpener_Vault trong scene). Sau cua: chieu nghi + cau thang xuong (noi Level 4).
            var pivot = new GameObject("VaultDoor_Pivot").transform;
            pivot.SetParent(geo, false); pivot.position = new Vector3(45f, F, 164.0f);
            var leaf = Box(pivot, "VaultDoor_Leaf", Vector3.zero, new Vector3(0.35f, 2.8f, 3.0f), "Metal");
            leaf.transform.localPosition = new Vector3(0.0f, 1.4f, -1.5f);
            Movable(leaf);
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "VaultDoor_Wheel"; Object.DestroyImmediate(wheel.GetComponent<Collider>());
            wheel.transform.SetParent(pivot, false); wheel.transform.localPosition = new Vector3(-0.22f, 1.4f, -1.5f);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); wheel.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
            wheel.GetComponent<MeshRenderer>().sharedMaterial = M("Yellow");
            // L3-L4: dau cau thang bao mat sau cua ham - DUNG kich thuoc/cao do cau thang cua Level04Builder (chieu nghi x 45.3..46, bac 12/22 m x 0.2 m,
            // tuong z 160.35/164.65, tran doc, den khan cap) nhung chi 6 bac roi man den. Chuoi go ca group VaultStair_Stub va dat cau thang day du cua Level 4.
            // Nguong cua ham (lap khe san x 45.0..45.3 duoi o cua - giu ca trong chuoi)
            Box(geo, "VaultDoor_Threshold", new Vector3(45.15f, F - 0.15f, 162.5f), new Vector3(0.4f, 0.3f, 3.0f), "Metal");
            var stub = Group(geo.parent, "VaultStair_Stub");
            Box(stub, "Vault_Landing", new Vector3(45.65f, F - 0.15f, 162.5f), new Vector3(0.7f, 0.3f, 4f), "GrayDark");
            float run = 12f / 22f;
            for (int i = 0; i < 6; i++)
            {
                float top = F - 0.2f * (i + 1), h = top + 0.3f;
                Box(stub, "Vault_Stair_" + (i + 1), new Vector3(46f + run * (i + 0.5f), -0.3f + h * 0.5f, 162.5f), new Vector3(run, h, 4f), "Stone");
                Box(stub, "Vault_Stair_Nose_" + (i + 1), new Vector3(46f + run * i + 0.04f, top + 0.006f, 162.5f), new Vector3(0.08f, 0.012f, 3.9f), "Yellow", 0f, false);
            }
            float xEnd = 46f + run * 6f;
            Box(stub, "Vault_Side_N", new Vector3((45.3f + xEnd + 0.2f) * 0.5f, 4.0f, 164.65f), new Vector3(xEnd + 0.2f - 45.3f, 8f, 0.3f), "GrayDark");
            Box(stub, "Vault_Side_S", new Vector3((45.3f + xEnd + 0.2f) * 0.5f, 4.0f, 160.35f), new Vector3(xEnd + 0.2f - 45.3f, 8f, 0.3f), "GrayDark");
            Box(stub, "Vault_Dark", new Vector3(xEnd + 0.1f, 4.0f, 162.5f), new Vector3(0.2f, 8f, 4.0f), "Black", 0f, false);
            Level04Builder.StairCeiling(stub, "Vault_Ceiling", 45.3f, xEnd + 0.2f);
            Box(stub, "Vault_EmergencyLight_S", new Vector3(46.6f, F + 2.3f, 160.525f),new Vector3(0.45f, 0.18f, 0.05f), "LightRed", 0f, false);
            // Den bao + bien B1 tren tuong phong an ninh quanh cua ham (nhin tu goc cuoi Level 3)
            Box(decor, "Vault_Beacon_S", new Vector3(44.96f, F + 2.95f, 160.55f), new Vector3(0.06f, 0.3f, 0.3f), "LightRed", 0f, false);
            Box(decor, "Vault_Beacon_N", new Vector3(44.96f, F + 2.95f, 164.45f), new Vector3(0.06f, 0.3f, 0.3f), "LightRed", 0f, false);
            Box(decor, "Vault_Sign_Plate", new Vector3(44.97f, F + 3.17f, 162.5f), new Vector3(0.04f, 0.5f, 1.8f), "Green", 0f, false);
            Level04Builder.Label(decor, "Vault_Sign_Text", "B1  VAULT", new Vector3(44.94f, F + 3.17f, 162.5f), 90f, 0.38f, Color.white);
        }
    }
}
