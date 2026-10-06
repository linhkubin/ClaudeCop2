using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ClaudeCop.Enemy;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// Level 4 (Kho tien, GDD vong 10-11) - blockout primitive + marker, luu Assets/_Game/Level/Level_04.prefab.
    /// Toa do LOCAL TRUNG voi Level_02/Level_03: trong chuoi Level_04 dat dung transform cua Level_03.
    /// Ray P1_S1 bat dau DUNG goc camera cuoi Level 3 (CamPoint_P3_S6 cua Level_03.prefab), qua cua ham (x=45) sau phong an ninh,
    /// xuong cau thang bao mat (san F=4.4 -> san kho B=0) toi cua thang may bao mat (cua truot mo san).
    /// P1 sanh kiem soat cua vault (khien nguoi dau tien), P2 phong ket hai tang (san luoi, ban cong xa thu, cua sap ket san),
    /// P3 dai sanh vault + loi vang (khien doi, xa thu 34 m, "Don dap loi vang" 5 enemy, cua cuon nap tien noi Level 5).
    /// Moi Phase co goc quay doi giua stage: S2 -> S3 xoay ~26 do, S5 -> S6 xoay ~26 do (validator: cap Combat <= 30 do, <= 1 m),
    /// wave moi xuat hien ngay o goc moi. Enemy dat theo toa do cuc tu camera (khoang cach + lech ngang) -> yaw shot = giua cum.
    /// Cuoi Build do bang so (khong anh): tia nhin toi moi muc tieu (camera lech +-0.6 m) chan boi BAT KY renderer nao (ca vat khong collider),
    /// khoang cach ray toi renderer, duong chay cua enemy chay vao, khoang cach / lech ngang / tach con tin. Chay lai an toan (ghi de prefab).
    /// </summary>
    public static class Level04Builder
    {
        public const string PrefabPath = "Assets/_Game/Level/Level_04.prefab";
        const string MatDir = "Assets/_Game/Level/Materials/";
        public const float F = 4.4f;          // san van phong Level 3 (dinh cau thang)
        public const float B = 0.0f;          // san kho tien
        const float Eye = 1.65f, Aim = 1.5f;
        const float StairX0 = 46f, StairX1 = 58f, LaneZ = 162.5f;
        const float BalconyY = 4.0f;          // ban cong / san luoi P2
        const float P3X = 136f;               // truc giua dai sanh vault P3 (x)

        // ---- camera (xz) ----
        static readonly Vector2 C12 = new Vector2(64.0f, 162.5f), C13 = new Vector2(64.3f, 162.5f);
        static readonly Vector2 C15 = new Vector2(78.0f, 162.5f), C16 = new Vector2(78.3f, 162.5f);
        static readonly Vector2 C22 = new Vector2(110.0f, 168.0f), C23 = new Vector2(110.3f, 168.0f);
        static readonly Vector2 C25 = new Vector2(120.0f, 168.0f), C26 = new Vector2(120.3f, 168.0f);
        static readonly Vector2 C32 = new Vector2(P3X, 138.0f), C33 = new Vector2(P3X, 137.7f);
        static readonly Vector2 C35 = new Vector2(P3X, 132.0f), C36 = new Vector2(P3X, 131.7f);

        /// <summary>Huong danh nghia cua goc giao tranh (yaw that = giua cum muc tieu, tinh luc Build).</summary>
        static float NomYaw(int p, int s)
        {
            switch (p * 10 + s)
            {
                case 12: return 90f; case 13: return 116f; case 15: return 90f; case 16: return 64f;
                case 22: return 90f; case 23: return 64f; case 25: return 90f; case 26: return 116f;
                case 32: return 180f; case 33: return 206f; case 35: return 180f; case 36: return 154f;
            }
            return 0f;
        }

        static Vector2 CamXZ(int p, int s)
        {
            switch (p * 10 + s)
            {
                case 12: return C12; case 13: return C13; case 15: return C15; case 16: return C16;
                case 22: return C22; case 23: return C23; case 25: return C25; case 26: return C26;
                case 32: return C32; case 33: return C33; case 35: return C35; case 36: return C36;
            }
            return Vector2.zero;
        }
        static Vector3 CamPos(int p, int s) { var c = CamXZ(p, s); return new Vector3(c.x, B + Eye, c.y); }

        /// <summary>Diem cach camera (p,s) d m (ngang), lech goc bear do so voi huong danh nghia (duong = ben phai camera).</summary>
        static Vector2 Pol(int p, int s, float d, float bear)
        {
            float r = (NomYaw(p, s) + bear) * Mathf.Deg2Rad;
            return CamXZ(p, s) + new Vector2(Mathf.Sin(r), Mathf.Cos(r)) * d;
        }

        // ---- du lieu ----
        // kind: E enemy, K khien nguoi (ShieldSpawn), H con tin. code: ma loai GDD (T, C, D, X, S, K, H) chi de bao cao.
        struct Sp { public int p, w; public char kind; public string code; public Vector2 xz; public float y; public string cover; public EnemyActor.EntryStyle style; public float drop; }
        static Sp E(int p, int w, string code, float d, float bear, EnemyActor.EntryStyle st = EnemyActor.EntryStyle.Auto, string cover = null, float y = B, float drop = 0f)
            => new Sp { p = p, w = w, kind = 'E', code = code, xz = Pol(p, w, d, bear), y = y, cover = cover, style = st, drop = drop };
        static Sp K(int p, int w, float d, float bear, EnemyActor.EntryStyle st = EnemyActor.EntryStyle.Auto)
            => new Sp { p = p, w = w, kind = 'K', code = "K", xz = Pol(p, w, d, bear), y = B, style = st };
        static Sp H(int p, int w, float d, float bear, string cover = null)
            => new Sp { p = p, w = w, kind = 'H', code = "H", xz = Pol(p, w, d, bear), y = B, cover = cover };

        const EnemyActor.EntryStyle Auto = EnemyActor.EntryStyle.Auto, Door = EnemyActor.EntryStyle.Door, Drop = EnemyActor.EntryStyle.Drop,
            Vault = EnemyActor.EntryStyle.Vault, Slide = EnemyActor.EntryStyle.Slide;

        // 27 enemy (P1 7 = 1,1,2,3; P2 9 = 2,3,2,2; P3 11 = 2,3,5,1; gom 4 khien nguoi) + 4 con tin. W = so shot (2,3,5,6).
        static readonly Sp[] Spawns =
        {
            // ---- P1 sanh kiem soat cua vault (x 58..104, z 150..175) ----
            E(1,2, "T", 20f, 0f, Auto, "Barrier"),                         // W1: 1 ten lo sau rao kiem soat (mau quen)
            K(1,3, 16f, 0f, Door),                                          // W2 (xoay phai 26 do): khien nguoi dau tien ra tu phong kiem soat
            H(1,5, 19f, -4.8f, "Counter"),                                  // W3: nhan vien lo sau quay kiem soat roi tu cui
            E(1,5, "T", 22f, 0.8f, Auto, "Counter"),                        //     1 ten lo sau quay
            E(1,5, "C", 22.5f, 4.6f, Door),                                 //     1 ten ra tu cua phong kiem soat (tuong Dong)
            E(1,6, "C", 20f, -4f, Door),                                    // W4 (xoay trai 26 do): 3 ten chay ra tu hai cua chop phia Bac
            E(1,6, "C", 22f, 0.8f, Door),
            E(1,6, "C", 27f, 4.3f, Door),
            // ---- P2 phong ket hai tang (x 104..156, z 152..184), san luoi phia Bac, ban cong phia Dong (cao 4 m) ----
            E(2,2, "T", 19f, -5.0f, Auto, "BoxRow"),                        // W1: 2 ten lo giua hang ket thap
            E(2,2, "T", 26f, 4.9f, Auto, "BoxRow"),
            H(2,2, 15f, 0f),                                                //     con tin CHUI LEN tu cua sap ket san (khong vat nap)
            E(2,3, "D", 18f, -4f, Drop, null, B, BalconyY),                 // W2 (xoay trai 26 do, bat ngo): 2 ten nhay tu san luoi
            E(2,3, "D", 20f, 1.5f, Drop, null, B, BalconyY),
            E(2,3, "X", 14.5f, 5f, Door),                                   //     1 ten xung phong tu cua duoi san luoi (tam: Door sau lung)
            K(2,5, 16f, -4f),                                               // W3: khien nguoi o ket dang mo + xa thu tren ban cong ~31 m
            E(2,5, "S", 31f, 2f, Auto, null, BalconyY),
            H(2,6, 16f, -4.8f, "Cart"),                                     // W4 (xoay phai 26 do): nhan vien lo/cui canh cau thang (thay chay ngang)
            E(2,6, "C", 20f, 1.1f, Door),                                   //     2 ten chay xuong tu chan cau thang sat
            E(2,6, "C", 26f, 4.7f, Door),
            // ---- P3 dai sanh vault (x 106..154, z 90..144), loi vang x 124..136 z 100..112 ----
            K(3,2, 15f, -4f),                                               // W1: khien doi (2 nhan vien)
            K(3,2, 18f, 4f),
            H(3,3, 28f, -5.2f, "Cart"),                                     // W2 (xoay phai 26 do): con tin xa lo roi tu cui
            E(3,3, "T", 26f, 0f, Auto, "Cart"),                             //     T sau xe day tien
            E(3,3, "S", 34f, 2.6f),                                         //     xa thu cuoi dai sanh ~34 m
            E(3,3, "X", 14.5f, 5.2f, Door),                                 //     xung phong tu cua long tien (tam: Door sau lung)
            E(3,5, "C", 16.5f, -5f, Slide),                                 // W3 "Don dap loi vang": 5 ten (2 loi ben + cua vault), toi da 4
            E(3,5, "C", 17.5f, 5f, Slide),
            E(3,5, "C", 18.5f, 0f, Door),
            E(3,5, "C", 15f, -2.5f, Door),
            E(3,5, "C", 15.5f, 2.5f, Door),
            E(3,6, "T", 18f, 0f, Auto, "Cart"),                             // W4 (xoay trai 26 do): ten cam dau sau xe day vang, kill-zoom
        };

        // Cua cho enemy (Door_Enemy_<P>_<W>_<NN>): vi tri san tren mat tuong + phap tuyen (vao phong).
        static (string name, Vector2 xz, Vector2 n)[] EnemyDoors() => new[]
        {
            ("Door_Enemy_P1_W3_01", new Vector2(78.7f, 150.15f), new Vector2(0f, 1f)),      // phong kiem soat (tuong Nam)
            ("Door_Enemy_P1_W5_01", new Vector2(103.85f, 158.0f), new Vector2(-1f, 0f)),    // phong kiem soat Dong
            ("Door_Enemy_P1_W6_01", new Vector2(95.0f, 174.85f), new Vector2(0f, -1f)),     // cua chop Bac 1
            ("Door_Enemy_P1_W6_02", new Vector2(101.0f, 174.85f), new Vector2(0f, -1f)),    // cua chop Bac 2
            ("Door_Enemy_P2_W3_01", new Vector2(131.0f, 179.35f), new Vector2(0f, -1f)),    // vach duoi san luoi (X xung phong)
            ("Door_Enemy_P2_W6_01", new Vector2(141.0f, 152.15f), new Vector2(0f, 1f)),     // chan cau thang sat
            ("Door_Enemy_P3_W3_01", Pol(3, 3, 23.5f, 5.4f), Vector2.zero),                   // cua long tien dung tu do (X xung phong)
            ("Door_Enemy_P3_W5_01", new Vector2(P3X, 112.02f), new Vector2(0f, 1f)),      // sau cua vault tron
        };

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

        static void Movable(GameObject go) { GameObjectUtility.SetStaticEditorFlags(go, 0); }

        /// <summary>Chu 3D blockout (TextMesh, font co san LegacyRuntime): bien tang B1... Doc duoc khi camera nhin theo +Z cua chu (yaw). height = cao 1 dong (m).</summary>
        internal static GameObject Label(Transform parent, string name, string text, Vector3 pos, float yaw, float height, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            var tm = go.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.font = font; tm.text = text; tm.fontSize = 64; tm.characterSize = height / 6.4f;
            tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.fontStyle = FontStyle.Bold; tm.color = color;
            if (font != null) go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return go;
        }

        // ---- tran cau thang bao mat (L3-L4): phang (mat tren 8.0) tren chieu nghi + la cua ham mo (cao F + 2.8), roi doc xuong B + 5.0 tai x = StairX1 ----
        const float CeilX0 = 45.3f, CeilXk = 49.0f, CeilTop0 = 8.0f, CeilTop1 = B + 5.0f, CeilThick = 0.2f;
        /// <summary>Mat duoi tran cau thang tai x.</summary>
        internal static float CeilingBottom(float x) => (x <= CeilXk ? CeilTop0 : CeilTop0 + (CeilTop1 - CeilTop0) * (x - CeilXk) / (StairX1 - CeilXk)) - CeilThick;
        static float CeilingPitch => Mathf.Atan2(CeilTop0 - CeilTop1, StairX1 - CeilXk) * Mathf.Rad2Deg;

        /// <summary>Tran cau thang (x0..x1): phan phang + phan doc - dung chung cho Level 4 (day du) va doan dau cau thang trong Level 3 (VaultStair_Stub).</summary>
        internal static void StairCeiling(Transform g, string name, float x0, float x1)
        {
            float a = x0, b = Mathf.Min(x1, CeilXk);
            if (b > a + 0.01f)
                Box(g, name + "_Flat", new Vector3((a + b) * 0.5f, CeilTop0 - CeilThick * 0.5f, LaneZ), new Vector3(b - a, CeilThick, 4.3f), "GrayDark", 0f, false);
            a = Mathf.Max(x0, CeilXk); b = x1;
            if (b > a + 0.01f)
            {
                float xm = (a + b) * 0.5f, len = (b - a) / Mathf.Cos(CeilingPitch * Mathf.Deg2Rad);
                var c = Box(g, name + "_Slope", new Vector3(xm, CeilingBottom(xm) + CeilThick * 0.5f, LaneZ), new Vector3(len, CeilThick, 4.3f), "GrayDark", 0f, false);
                c.transform.rotation = Quaternion.Euler(0f, 0f, -CeilingPitch);
            }
        }

        /// <summary>Mat bac thang cao nhat duoi x (bac 22 x 0.2 m, x 46..58).</summary>
        static float StepTop(float x)
        {
            const int n = 22; float run = (StairX1 - StairX0) / n;
            int i = Mathf.Clamp(Mathf.FloorToInt((x - StairX0) / run), 0, n - 1);
            return F - (F - B) * (i + 1) / n;
        }

        static Transform Marker(Transform parent, string name, Vector3 pos, Quaternion rot)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); t.SetPositionAndRotation(pos, rot); return t;
        }

        static float YawToward(Vector2 from, Vector2 to) => Mathf.Atan2(to.x - from.x, to.y - from.y) * Mathf.Rad2Deg;
        static Vector3 Fwd(float yawDeg) => Quaternion.Euler(0f, yawDeg, 0f) * Vector3.forward;
        static Vector3 V3(Vector2 xz, float y) => new Vector3(xz.x, y, xz.y);

        /// <summary>Cua tren tuong: khung + o den (cua mo cho enemy). Group dat sat mat tuong, nhin theo normal (vao phong).</summary>
        static Transform DoorAt(Transform parent, string name, Vector3 wallFloorPos, Vector3 normal, string panel = "Black")
        {
            var g = new GameObject(name).transform;
            g.SetParent(parent, false);
            g.SetPositionAndRotation(wallFloorPos + normal * 0.02f, Quaternion.LookRotation(normal));
            void Part(string n, Vector3 local, Vector3 size, string mat)
            {
                var b = Box(g, n, Vector3.zero, size, mat, 0f, false);
                b.transform.localPosition = local; b.transform.localRotation = Quaternion.identity;
            }
            Part("DoorJamb_L", new Vector3(-0.62f, 1.12f, 0.05f), new Vector3(0.12f, 2.24f, 0.1f), "Metal");
            Part("DoorJamb_R", new Vector3(0.62f, 1.12f, 0.05f), new Vector3(0.12f, 2.24f, 0.1f), "Metal");
            Part("DoorHead", new Vector3(0f, 2.3f, 0.05f), new Vector3(1.36f, 0.14f, 0.12f), "Metal");
            Part("DoorPanel", new Vector3(0f, 1.06f, 0.01f), new Vector3(1.12f, 2.12f, 0.03f), panel);
            return g;
        }

        /// <summary>Tuong doc truc X (mat phang x = const) tu z0 toi z1, co cac o mo (tam z, rong, cao; cao >= h = mo het).</summary>
        static void WallX(Transform geo, string name, float x, float z0, float z1, float h, string mat, params (float c, float w, float oh)[] openings)
        {
            var cuts = new List<(float a, float b, float oh)>();
            foreach (var o in openings) cuts.Add((o.c - o.w * 0.5f, o.c + o.w * 0.5f, o.oh));
            cuts.Sort((p, q) => p.a.CompareTo(q.a));
            float cur = z0; int k = 0;
            foreach (var c in cuts)
            {
                if (c.a > cur + 0.01f) Box(geo, name + "_" + (++k), new Vector3(x, h * 0.5f + B, (cur + c.a) * 0.5f), new Vector3(0.3f, h, c.a - cur), mat);
                if (c.oh < h) Box(geo, name + "_Lintel_" + k, new Vector3(x, B + c.oh + (h - c.oh) * 0.5f, (c.a + c.b) * 0.5f), new Vector3(0.3f, h - c.oh, c.b - c.a), mat);
                cur = c.b;
            }
            if (z1 > cur + 0.01f) Box(geo, name + "_" + (++k), new Vector3(x, h * 0.5f + B, (cur + z1) * 0.5f), new Vector3(0.3f, h, z1 - cur), mat);
        }

        /// <summary>Tuong doc truc Z (mat phang z = const) tu x0 toi x1, co cac o mo.</summary>
        static void WallZ(Transform geo, string name, float z, float x0, float x1, float h, string mat, params (float c, float w, float oh)[] openings)
        {
            var cuts = new List<(float a, float b, float oh)>();
            foreach (var o in openings) cuts.Add((o.c - o.w * 0.5f, o.c + o.w * 0.5f, o.oh));
            cuts.Sort((p, q) => p.a.CompareTo(q.a));
            float cur = x0; int k = 0;
            foreach (var c in cuts)
            {
                if (c.a > cur + 0.01f) Box(geo, name + "_" + (++k), new Vector3((cur + c.a) * 0.5f, h * 0.5f + B, z), new Vector3(c.a - cur, h, 0.3f), mat);
                if (c.oh < h) Box(geo, name + "_Lintel_" + k, new Vector3((c.a + c.b) * 0.5f, B + c.oh + (h - c.oh) * 0.5f, z), new Vector3(c.b - c.a, h - c.oh, 0.3f), mat);
                cur = c.b;
            }
            if (x1 > cur + 0.01f) Box(geo, name + "_" + (++k), new Vector3((cur + x1) * 0.5f, h * 0.5f + B, z), new Vector3(x1 - cur, h, 0.3f), mat);
        }

        // ---- ray ----

        /// <summary>Do cao mat san duoi camera tren cau thang bao mat (F -> B, x 46..58): duong cong van toc hinh thang (dau/cuoi em 25%).</summary>
        static float StairFloor(float x)
        {
            float u = Mathf.Clamp01((x - StairX0) / (StairX1 - StairX0));
            const float r = 0.25f; float vmax = 1f / (1f - r);
            float s;
            if (u < r) s = vmax * u * u / (2f * r);
            else if (u > 1f - r) { float w = 1f - u; s = 1f - vmax * w * w / (2f * r); }
            else s = vmax * (u - r * 0.5f);
            return F - (F - B) * s;
        }

        /// <summary>Do cao camera tren san phong an ninh L3: nang HopRise qua ban dieu khien (plateau = ban +-0.5 m, doc len/xuong HopRamp m
        /// theo smootherstep C2 -> van toc/pitch doi lien tuc), ngoai doan do = F + Eye.</summary>
        // Profile (cao do tuong doi so voi mat F + Eye): 0 -> +HopRise (len HopRamp m, dinh phang tren ban +-0.5 m)
        // -> ha NHANH ngay sau mep ban xuong HopDip (HopDrop m) -> tu tu ve 0 tai nguong cua ham (x = HopEndX).
        // Moi doan noi bang smootherstep (dao ham bac 1, 2 = 0 tai moc) -> cao do, van toc dung lien tuc, khong gap khuc.
        internal const float HopRise = 0.5f, HopRamp = 3.0f, HopDip = -0.2f, HopDrop = 1.8f, HopEndX = 45.6f;
        internal static float DeskHop(float x) => F + Eye + DeskHopOffset(x);
        internal static float DeskHopOffset(float x)
        {
            float a = Level03Builder.DeskX - Level03Builder.DeskHalf - 0.5f, b = Level03Builder.DeskX + Level03Builder.DeskHalf + 0.5f;
            float[] kx = { a - HopRamp, a, b, b + HopDrop, HopEndX };
            float[] ky = { 0f, HopRise, HopRise, HopDip, 0f };
            if (x <= kx[0] || x >= kx[kx.Length - 1]) return 0f;
            for (int i = 1; i < kx.Length; i++)
                if (x <= kx[i])
                {
                    float t = Mathf.Clamp01((x - kx[i - 1]) / (kx[i] - kx[i - 1]));
                    float s = t * t * t * (t * (6f * t - 15f) + 10f);
                    return Mathf.Lerp(ky[i - 1], ky[i], s);
                }
            return 0f;
        }

        static List<Vector3> LinkRail(int p, Dictionary<int, float> camYaw, Vector3 l3End, float l3Yaw)
        {
            var end = CamPos(p, 2); float ny = camYaw[p * 10 + 2];
            var list = new List<Vector3>();
            if (p == 1)
            {
                // Tu goc cuoi Level 3 (phong an ninh) cong mem toi cua ham (x=45, z 161..164), xuong cau thang bao mat, qua cua thang may.
                list.Add(l3End);
                var lead = l3End + Fwd(l3Yaw) * 1.5f;
                list.Add(lead);
                var door = new Vector3(45.6f, F + Eye, LaneZ);
                // Ban dieu khien L3 chan ngang lan: camera "nhay qua ban" - diem day (~0.58 m) de giu dung vong cung cao do DeskHop.
                AddHermite(list, lead, l3Yaw, door, 90f, 31, DeskHop);
                for (float x = 47f; x < StairX1 - 0.01f; x += 1.5f) list.Add(new Vector3(x, StairFloor(x) + Eye, LaneZ));
                list.Add(new Vector3(StairX1, B + Eye, LaneZ));
                // StairX1 = end - 6 m (thang hang yaw 90, pitch 0)
            }
            else
            {
                var start = CamPos(p - 1, 6); float py = camYaw[(p - 1) * 10 + 6];
                var lead = start + Fwd(py) * 1.5f;
                list.Add(start); list.Add(lead);
                AddHermite(list, lead, py, end - Fwd(ny) * 6f, ny, 6, x => B + Eye);
            }
            if (Vector3.Distance(list[list.Count - 1], end - Fwd(ny) * 6f) > 0.05f) list.Add(end - Fwd(ny) * 6f);
            list.Add(end - Fwd(ny) * 3f);
            list.Add(end);
            return list;
        }

        static List<Vector3> MidRail(int p)
        {
            Vector3 a = CamPos(p, 3), b = CamPos(p, 5);
            return new List<Vector3> { a, Vector3.Lerp(a, b, 0.33f), Vector3.Lerp(a, b, 0.66f), b };
        }

        /// <summary>Them n diem Hermite giua (tiep tuyen theo yaw dau/cuoi, he so 1.0) roi diem b. Do cao theo yAt(x).</summary>
        static void AddHermite(List<Vector3> list, Vector3 a, float yawA, Vector3 b, float yawB, int n, System.Func<float, float> yAt)
        {
            float len = Vector3.Distance(new Vector3(a.x, 0, a.z), new Vector3(b.x, 0, b.z));
            Vector3 m0 = Fwd(yawA) * len, m1 = Fwd(yawB) * len;
            for (int i = 1; i <= n + 1; i++)
            {
                float t = i / (float)(n + 1), t2 = t * t, t3 = t2 * t;
                Vector3 pt = (2f * t3 - 3f * t2 + 1f) * a + (t3 - 2f * t2 + t) * m0 + (-2f * t3 + 3f * t2) * b + (t3 - t2) * m1;
                pt.y = yAt(pt.x);
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

        /// <summary>Diem ray cat mat phang z = const (noi suy tuyen tinh giua hai diem hint), tra ve x.</summary>
        static float CrossAtZ(List<Vector3> pts, float z)
        {
            for (int i = 1; i < pts.Count; i++)
                if ((pts[i - 1].z - z) * (pts[i].z - z) <= 0f && Mathf.Abs(pts[i].z - pts[i - 1].z) > 1e-4f)
                    return Mathf.Lerp(pts[i - 1].x, pts[i].x, (z - pts[i - 1].z) / (pts[i].z - pts[i - 1].z));
            return float.NaN;
        }
        static float CrossAtX(List<Vector3> pts, float x)
        {
            for (int i = 1; i < pts.Count; i++)
                if ((pts[i - 1].x - x) * (pts[i].x - x) <= 0f && Mathf.Abs(pts[i].x - pts[i - 1].x) > 1e-4f)
                    return Mathf.Lerp(pts[i - 1].z, pts[i].z, (x - pts[i - 1].x) / (pts[i].x - pts[i - 1].x));
            return float.NaN;
        }

        // ---- Build ----
        [MenuItem("ClaudeCop/Game/Build Level_04 Prefab")]
        public static void Build()
        {
            mats.Clear();
            var root = new GameObject("Level_04");
            var approach = Group(root.transform, "Approach_Standalone");
            var shell = Group(root.transform, "Shared_VaultLevel");
            var a1 = Group(root.transform, "Area_P1_Checkpoint");
            var a2 = Group(root.transform, "Area_P2_SafeDeposit");
            var a3 = Group(root.transform, "Area_P3_VaultHall");
            var areas = new[] { a1, a2, a3 };

            // Goc camera cuoi Level 3 (diem bat dau ray P1_S1)
            Vector3 l3End = new Vector3(30.2f, F + Eye, 162.7f); float l3Yaw = 99f;
            var l3 = AssetDatabase.LoadAssetAtPath<GameObject>(Level03Builder.PrefabPath);
            // L3-L4: uu tien goc dwell "nhin cua ham" (CamPoint_P3_S7, cung vi tri S6, nhin thang vao cua) -> ray di thang qua cua, khong cong chu S.
            if (l3 != null)
            {
                Transform s6 = null, s7 = null;
                foreach (var t in l3.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "CamPoint_P3_S6") s6 = t;
                    else if (t.name == Level03Builder.ExitCamPoint) s7 = t;
                }
                var e = s7 != null ? s7 : s6;
                if (e != null) { l3End = e.position; l3Yaw = e.eulerAngles.y; }
            }

            // 1) yaw shot = giua cum muc tieu (khong lam tron: |lech ngang| doi xung)
            var camYaw = new Dictionary<int, float>();
            for (int p = 1; p <= 3; p++)
                foreach (int s in new[] { 2, 3, 5, 6 })
                {
                    var c = CamXZ(p, s); float mn = 999f, mx = -999f, ny = NomYaw(p, s);
                    foreach (var sp in Spawns)
                    {
                        if (sp.p != p || sp.w != s) continue;
                        float b = Mathf.DeltaAngle(ny, YawToward(c, sp.xz)); mn = Mathf.Min(mn, b); mx = Mathf.Max(mx, b);
                    }
                    camYaw[p * 10 + s] = mn > 900f ? ny : ny + (mn + mx) * 0.5f;
                }

            // 2) ray
            var rails = new Dictionary<int, List<Vector3>>();
            for (int p = 1; p <= 3; p++) { rails[p * 10 + 1] = LinkRail(p, camYaw, l3End, l3Yaw); rails[p * 10 + 4] = MidRail(p); }
            float zP12 = CrossAtX(rails[21], 104f);      // o mo P1 -> P2 (tuong x = 104)
            float xP23 = CrossAtZ(rails[31], 152f);      // o mo P2 -> hanh lang (tuong z = 152)
            float xP3In = CrossAtZ(rails[31], 144f);     // o mo hanh lang -> dai sanh (z = 144)
            if (float.IsNaN(zP12)) zP12 = 168f; if (float.IsNaN(xP23)) xP23 = 130f; if (float.IsNaN(xP3In)) xP3In = 130f;

            // 3) hinh khoi
            BuildApproach(approach);
            BuildStair(shell);
            BuildP1(Group(a1, "Geometry"), Group(a1, "Decor"), zP12);
            BuildP2(Group(a2, "Geometry"), Group(a2, "Decor"), zP12, xP23, xP3In);
            BuildP3(Group(a3, "Geometry"), Group(a3, "Decor"), xP23, xP3In);
            var doorGroups = new[] { Group(a1, "EnemyDoors"), Group(a2, "EnemyDoors"), Group(a3, "EnemyDoors") };
            foreach (var d in EnemyDoors())
            {
                int p = d.name[12] - '0';
                Vector2 n = d.n;
                if (n == Vector2.zero) { var c = CamXZ(3, 3); n = (c - d.xz).normalized; }   // cua dung tu do: quay ve camera
                DoorAt(doorGroups[p - 1], d.name, V3(d.xz, B), V3(n, 0f));
            }
            BuildCageDoor(doorGroups[2]);

            // 4) CamPoints + RailHints
            for (int p = 1; p <= 3; p++)
            {
                var camPts = Group(areas[p - 1], "CamPoints");
                var hints = Group(areas[p - 1], "RailHints");
                foreach (int s in new[] { 2, 3, 5, 6 })
                    Marker(camPts, "CamPoint_P" + p + "_S" + s, CamPos(p, s), Quaternion.Euler(0f, camYaw[p * 10 + s], 0f));
                foreach (int s in new[] { 1, 4 })
                {
                    var pts = rails[p * 10 + s];
                    Marker(camPts, "CamPoint_P" + p + "_S" + s, pts[0], Quaternion.identity);
                    for (int i = 0; i < pts.Count; i++)
                        Marker(hints, "RailHint_P" + p + "_S" + s + "_" + (i + 1).ToString("00"), pts[i], Quaternion.identity);
                }
            }

            // 5) spawn / khien nguoi / con tin + kieu xuat hien + vat nap
            var spawnGroups = new[] { Group(a1, "Spawns"), Group(a2, "Spawns"), Group(a3, "Spawns") };
            var covers = new[] { Group(a1, "Cover"), Group(a2, "Cover"), Group(a3, "Cover") };
            var counters = new Dictionary<string, int>();
            var spawnMarkers = new List<(Sp sp, Transform t)>();
            foreach (var sp in Spawns)
            {
                string key = "P" + sp.p + "_W" + sp.w + "_";
                string tag = sp.kind == 'H' ? "HostageSpawn_" : sp.kind == 'K' ? "ShieldSpawn_" : "EnemySpawn_";
                counters.TryGetValue(tag + key, out int n); counters[tag + key] = ++n;
                string name = tag + key + n.ToString("00");
                var cam = CamXZ(sp.p, sp.w);
                var ground = V3(sp.xz, sp.y);
                float yaw = YawToward(sp.xz, cam);
                var rot = Quaternion.Euler(0f, yaw, 0f);
                // Con tin khong vat nap (cua sap san): diem nap sau 2.1 m de khong lo dau tren mat san khi chua/het lo (khong dung im).
                float hideDepth = sp.kind == 'H' && string.IsNullOrEmpty(sp.cover) ? 2.1f : 1.15f;
                var t = Marker(spawnGroups[sp.p - 1], name, ground + Vector3.down * hideDepth, rot);
                Marker(t, "Peek", ground + Vector3.up * 0.05f, rot);
                if (sp.kind != 'H' && (sp.style != Auto || sp.drop > 0f))
                {
                    var spe = t.gameObject.AddComponent<SpawnPointEntry>();
                    var so = new SerializedObject(spe);
                    so.FindProperty("style").enumValueIndex = (int)sp.style;
                    so.FindProperty("dropHeight").floatValue = sp.drop;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                if (!string.IsNullOrEmpty(sp.cover)) BuildCover(covers[sp.p - 1], name.Replace(tag, "Cover_" + sp.cover + "_"), sp.cover, ground, yaw);
                if (sp.p == 2 && sp.w == 2 && sp.kind == 'H') BuildFloorHatch(covers[1], ground);
                spawnMarkers.Add((sp, t));
            }
            // Hop hoi mau dau P3 (chua co code runtime pickup mau): marker goi y
            Marker(Group(a3, "Hints"), "HealthHint_P3_S2", V3(Pol(3, 2, 6f, -25f), B + 0.4f), Quaternion.identity);

            // 6) do bang so + luu
            string report = Check(root, rails, camYaw, spawnMarkers);
            System.IO.Directory.CreateDirectory("Assets/_Game/Level");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            var sb = new System.Text.StringBuilder();
            foreach (var kv in camYaw) sb.Append("P" + kv.Key / 10 + "S" + kv.Key % 10 + "=" + kv.Value.ToString("0.0") + " ");
            string full = "[Level04Builder] yaw cam: " + sb + "| o mo P1/P2 z=" + zP12.ToString("0.0") + ", P2/hanh lang x=" + xP23.ToString("0.0")
                + ", hanh lang/P3 x=" + xP3In.ToString("0.0") + "\n" + report + "\n- da luu " + PrefabPath + " (" + Spawns.Length + " diem).";
            System.IO.File.WriteAllText("Temp/Level04Build.txt", full);   // bao cao day du (Temp/, khong commit)
            Debug.Log(full.Substring(0, full.IndexOf('\n')) + " | " + report.Substring(0, report.IndexOf('\n')) + " (day du: Temp/Level04Build.txt)");
        }

        // ---- kiem tra bang so ----
        static string Check(GameObject root, Dictionary<int, List<Vector3>> rails, Dictionary<int, float> camYaw, List<(Sp sp, Transform t)> markers)
        {
            Physics.SyncTransforms();
            var rends = new List<Renderer>(root.GetComponentsInChildren<Renderer>(true));
            var sb = new System.Text.StringBuilder();
            int blocked = 0, rays = 0;
            // (1) tia nhin: camera (+-0.6 m ngang) -> diem ngam, chan boi bat ky renderer nao (ca vat khong collider). Bo qua kinh va chinh marker.
            foreach (var m in markers)
            {
                var cam = CamPos(m.sp.p, m.sp.w); float yaw = camYaw[m.sp.p * 10 + m.sp.w];
                Vector3 right = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
                Vector3 aim = m.t.Find("Peek").position + Vector3.up * Aim;
                foreach (float off in new[] { -0.6f, 0f, 0.6f })
                {
                    Vector3 from = cam + right * off, d = aim - from; float dist = d.magnitude; var ray = new Ray(from, d / dist);
                    rays++;
                    foreach (var r in rends)
                    {
                        if (r.transform.IsChildOf(m.t) || r.name.Contains("Glass")) continue;
                        if (r.bounds.IntersectRay(ray, out float hd) && hd < dist - 0.3f && hd > 0.05f)
                        {
                            blocked++; sb.Append("  CHAN " + m.t.name + " (off " + off + ") boi " + r.name + " @" + hd.ToString("0.0") + "\n"); break;
                        }
                    }
                }
            }
            // (2) khoang cach ray -> renderer (>= 0.5 m; bo qua san/bac thang/tran duoi camera)
            float worst = 99f; string worstN = "";
            foreach (var kv in rails)
            {
                var pts = kv.Value;
                for (int i = 1; i < pts.Count; i++)
                    for (float t = 0f; t < 1f; t += 0.1f)
                    {
                        var q = Vector3.Lerp(pts[i - 1], pts[i], t);
                        foreach (var r in rends)
                        {
                            string n = r.name;
                            if (n.Contains("Floor") || n.StartsWith("Step") || n.Contains("Landing") || n.Contains("Ceiling")) continue; // tran doc: do rieng (AABB bao ca ray)
                            float dd = Mathf.Sqrt(r.bounds.SqrDistance(q));
                            if (dd < worst) { worst = dd; worstN = "Rail S" + kv.Key + " -> " + n; }
                        }
                    }
            }
            // tran doc cau thang: khoang dung (mat duoi tran - camera) tren ray P1_S1
            float ceilGap = 99f, deskGap = 99f, hopMax = -99f, hopMin = 99f, floorGap = 99f;
            {
                var pts = rails[11];
                for (int i = 1; i < pts.Count; i++)
                    for (float t = 0f; t < 1f; t += 0.1f)
                    {
                        var q = Vector3.Lerp(pts[i - 1], pts[i], t);
                        if (q.x >= CeilX0 && q.x <= StairX1) ceilGap = Mathf.Min(ceilGap, CeilingBottom(q.x) - q.y);
                        if (Mathf.Abs(q.x - Level03Builder.DeskX) <= Level03Builder.DeskHalf + 0.6f) deskGap = Mathf.Min(deskGap, q.y - Level03Builder.DeskTop);
                        if (q.x < HopEndX) { hopMax = Mathf.Max(hopMax, q.y - (F + Eye)); hopMin = Mathf.Min(hopMin, q.y - (F + Eye)); floorGap = Mathf.Min(floorGap, q.y - F); }
                    }
            }
            sb.Insert(0, "Ray gan renderer nhat: " + worst.ToString("0.00") + " m (" + worstN + ")" + (worst < 0.5f ? " FAIL" : "")
                + "; tran cau thang tren camera >= " + ceilGap.ToString("0.00") + " m" + (ceilGap < 0.5f ? " FAIL" : "")
                + "; camera tren mat ban dieu khien L3 >= " + deskGap.ToString("0.00") + " m" + (deskGap < 1.0f ? " FAIL" : "")
                + "; nhay qua ban: cao do tuong doi " + hopMin.ToString("+0.00;-0.00") + ".." + hopMax.ToString("+0.00;-0.00") + " m, camera tren san phong an ninh >= "
                + floorGap.ToString("0.00") + " m" + (floorGap < 0.5f ? " FAIL" : "") + "\n");
            // (3) duong chay vao (Auto khong vat nap, Slide): doan ngang tu Peek ra mep man hinh + 1.2 m, cao 0.3..1.7 m
            int runBad = 0;
            foreach (var m in markers)
            {
                if (m.sp.kind == 'H' || m.sp.cover != null || !(m.sp.style == Auto || m.sp.style == Slide)) continue;
                var cam = CamPos(m.sp.p, m.sp.w); float yaw = camYaw[m.sp.p * 10 + m.sp.w];
                Vector3 fwd = Fwd(yaw), right = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
                Vector3 peek = m.t.Find("Peek").position; Vector3 toE = peek - cam;
                float side = Vector3.Dot(toE, right) >= 0f ? 1f : -1f, depth = Vector3.Dot(toE, fwd);
                float edge = depth * Mathf.Tan(20f * Mathf.Deg2Rad) * (9f / 16f);
                float run = Mathf.Clamp(edge - Mathf.Abs(Vector3.Dot(toE, right)) + 1.2f + (m.sp.style == Slide ? 2f : 0f), 2f, 16f);
                foreach (float h in new[] { 0.3f, 1.0f, 1.7f })
                {
                    var a = peek + Vector3.up * h; var ray = new Ray(a, right * side);
                    foreach (var r in rends)
                    {
                        if (r.transform.IsChildOf(m.t)) continue;
                        if (r.bounds.IntersectRay(ray, out float hd) && hd < run && !r.name.Contains("Floor"))
                        { runBad++; sb.Append("  CHAY " + m.t.name + " cat " + r.name + " @" + hd.ToString("0.0") + "/" + run.ToString("0.0") + "\n"); break; }
                    }
                }
            }
            // (4) khoang cach, lech ngang, tach con tin, cua gan nhat
            var doors = EnemyDoors();
            int near = 0, mid = 0, far = 0; float maxH = 0f, minSep = 99f, maxD = 0f, minD = 99f;
            foreach (var m in markers)
            {
                var cam = CamPos(m.sp.p, m.sp.w); float yaw = camYaw[m.sp.p * 10 + m.sp.w];
                Vector3 aim = m.t.Find("Peek").position + Vector3.up * Aim; float d = Vector3.Distance(cam, aim);
                float h = Mathf.DeltaAngle(yaw, YawToward(new Vector2(cam.x, cam.z), m.sp.xz));
                maxH = Mathf.Max(maxH, Mathf.Abs(h));
                if (m.sp.kind != 'H') { if (d < 18f) near++; else if (d < 26f) mid++; else far++; maxD = Mathf.Max(maxD, d); minD = Mathf.Min(minD, d); }
                sb.Append("  " + m.t.name + " [" + m.sp.code + (m.sp.style != Auto ? "/" + m.sp.style : "") + "] " + d.ToString("0.0") + " m, h " + h.ToString("0.0"));
                if (m.sp.style == Door)
                {
                    string best = "-"; float bd = 15f;
                    foreach (var dr in doors) { float dd = Vector2.Distance(dr.xz, m.sp.xz); if (dd < bd) { bd = dd; best = dr.name; } }
                    sb.Append(", cua " + best + " " + bd.ToString("0.0") + " m");
                }
                if (m.sp.kind == 'H')
                    foreach (var o in markers)
                    {
                        if (o.sp.kind == 'H' || o.sp.p != m.sp.p || o.sp.w != m.sp.w) continue;
                        float sep = Mathf.Abs(Mathf.DeltaAngle(YawToward(new Vector2(cam.x, cam.z), m.sp.xz), YawToward(new Vector2(cam.x, cam.z), o.sp.xz)));
                        minSep = Mathf.Min(minSep, sep);
                    }
                sb.Append("\n");
            }
            string head = "Tia chan " + blocked + "/" + rays + ", duong chay cat " + runBad + ", enemy gan<18 " + near + " / vua " + mid + " / xa>=26 " + far
                + ", khoang cach " + minD.ToString("0.0") + ".." + maxD.ToString("0.0") + " m, |h| max " + maxH.ToString("0.0") + ", tach con tin-enemy min " + minSep.ToString("0.0") + " do\n";
            var turn = new System.Text.StringBuilder();
            foreach (var kv in rails) turn.Append("S" + kv.Key + " " + Length(kv.Value).ToString("0.0") + " m / " + MaxTurnPerMeter(kv.Value).ToString("0.0") + " do/m; ");
            return head + "Ray: " + turn + "\n" + sb;
        }

        // ---- vat nap ----
        static void BuildCover(Transform parent, string name, string type, Vector3 enemyGround, float yawToCam)
        {
            Vector3 toCam = Quaternion.Euler(0, yawToCam, 0) * Vector3.forward;
            const float depth = 0.6f;
            Vector3 c = enemyGround + toCam * (depth * 0.5f + 0.5f);   // nua be day + 0.5 m: enemy khong chui vao vat the
            c.y = B;
            switch (type)
            {
                case "Barrier":   // rao kiem soat thep
                    Box(parent, name, c + Vector3.up * 0.65f, new Vector3(2.0f, 1.3f, depth), "Metal", yawToCam);
                    Box(parent, name + "_Stripe", c + Vector3.up * 1.1f + toCam * 0.31f, new Vector3(1.9f, 0.12f, 0.02f), "Yellow", yawToCam, false);
                    break;
                case "Counter":   // quay kiem soat
                    Box(parent, name, c + Vector3.up * 0.65f, new Vector3(1.6f, 1.3f, depth), "GrayDark", yawToCam);
                    Box(parent, name + "_Top", c + Vector3.up * 1.32f, new Vector3(1.7f, 0.04f, depth + 0.1f), "Metal", yawToCam, false);
                    break;
                case "BoxRow":    // hang ket thap
                    Box(parent, name, c + Vector3.up * 0.65f, new Vector3(1.6f, 1.3f, depth), "Metal", yawToCam);
                    for (int i = 0; i < 3; i++)
                        Box(parent, name + "_Lock_" + i, c + Vector3.up * (0.3f + 0.4f * i) + toCam * 0.31f, new Vector3(1.4f, 0.03f, 0.02f), "GrayDark", yawToCam, false);
                    break;
                default:          // xe day tien / vang
                    Box(parent, name, c + Vector3.up * 0.7f, new Vector3(1.5f, 1.2f, depth), "Metal", yawToCam);
                    Box(parent, name + "_Load", c + Vector3.up * 1.33f, new Vector3(1.2f, 0.06f, depth - 0.1f), type == "Cart" ? "Yellow" : "Wood", yawToCam, false);
                    break;
            }
        }

        /// <summary>Cua sap ket san (con tin chui len): o den sat mat san + nap mo nghieng ben canh (khong collider).</summary>
        static void BuildFloorHatch(Transform parent, Vector3 ground)
        {
            Box(parent, "FloorHatch_P2_W2_Hole", ground + Vector3.up * 0.01f, new Vector3(1.3f, 0.02f, 1.3f), "Black", 0f, false);
            Box(parent, "FloorHatch_P2_W2_Rim", ground + Vector3.up * 0.015f, new Vector3(1.5f, 0.01f, 1.5f), "Yellow", 0f, false);
            Box(parent, "FloorHatch_P2_W2_Lid", ground + new Vector3(0f, 0.03f, 1.5f), new Vector3(1.3f, 0.05f, 1.3f), "Metal", 0f, false);   // nap mo nam phang
        }

        /// <summary>Long tien dung tu do quanh cua Door_Enemy_P3_W3_01 (X xung phong): cot mong phia SAU cua (khong che tam nhin).</summary>
        static void BuildCageDoor(Transform parent)
        {
            var c = CamXZ(3, 3); var d = Pol(3, 3, 23.5f, 5.4f);
            Vector2 back = (d - c).normalized, side = new Vector2(back.y, -back.x);
            for (int i = 0; i < 3; i++)
                for (int j = -1; j <= 1; j += 2)
                    Box(parent, "MoneyCage_Post_" + i + "_" + j, V3(d + back * (0.2f + 1.4f * i) + side * j * 0.75f, B + 1.2f), new Vector3(0.06f, 2.4f, 0.06f), "Metal", 0f, false);
            Box(parent, "MoneyCage_Roof", V3(d + back * 1.6f, B + 2.42f), new Vector3(1.6f, 0.05f, 3.0f), "Metal", YawToward(c, d), false);
            Box(parent, "MoneyCage_Bags", V3(d + back * 2.2f, B + 0.4f), new Vector3(1.1f, 0.8f, 1.0f), "Cardboard", YawToward(c, d), false);
        }

        // ---- hinh khoi ----

        /// <summary>Chi dung khi choi rieng Level_04.unity: san phong an ninh + tuong Dong co cua ham cua Level 3. Chuoi go group nay.</summary>
        static void BuildApproach(Transform g)
        {
            Box(g, "Approach_Floor", new Vector3(36.5f, F - 0.15f, 162.5f), new Vector3(17.6f, 0.3f, 10f), "Gray");
            Box(g, "Approach_Wall_E_S", new Vector3(45.15f, F + 1.75f, 158.5f), new Vector3(0.3f, 3.5f, 5f), "GrayDark");
            Box(g, "Approach_Wall_E_N", new Vector3(45.15f, F + 1.75f, 166.5f), new Vector3(0.3f, 3.5f, 5f), "GrayDark");
            Box(g, "Approach_Wall_E_Lintel", new Vector3(45.15f, F + 3.15f, 162.5f), new Vector3(0.3f, 0.7f, 3f), "GrayDark");
            // L3-L4: cua ham Level 3 da mo san (ban le z=164, xoay -90 do nhu DoorOpener_Vault) + den bao/bien B1 nhu Level 3 -> choi rieng Level 4 nhin giong chuoi.
            var pivot = new GameObject("Approach_VaultDoor_Pivot").transform;
            pivot.SetParent(g, false); pivot.SetPositionAndRotation(new Vector3(45f, F, 164.0f), Quaternion.Euler(0f, -90f, 0f));
            var leaf = Box(pivot, "Approach_VaultDoor_Leaf", Vector3.zero, new Vector3(0.35f, 2.8f, 3.0f), "Metal");
            leaf.transform.localPosition = new Vector3(0f, 1.4f, -1.5f); leaf.transform.localRotation = Quaternion.identity;
            // Ban dieu khien nhu Level 3 (ray "nhay qua ban" - choi rieng nhin giong chuoi)
            Box(g, "Approach_Control_Desk", new Vector3(Level03Builder.DeskX, F + 0.45f, 162.5f), new Vector3(2f * Level03Builder.DeskHalf, 0.9f, 5.0f), "GrayDark");
            Box(g, "Approach_Control_Desk_Console", new Vector3(Level03Builder.DeskX, F + 0.93f, 162.5f), new Vector3(0.6f, 0.06f, 3.6f), "Blue", 0f, false);
            Box(g, "Approach_Vault_Beacon_S", new Vector3(44.96f, F + 2.95f, 160.55f), new Vector3(0.06f, 0.3f, 0.3f), "LightRed", 0f, false);
            Box(g, "Approach_Vault_Beacon_N", new Vector3(44.96f, F + 2.95f, 164.45f), new Vector3(0.06f, 0.3f, 0.3f), "LightRed", 0f, false);
            Box(g, "Approach_Vault_Sign_Plate", new Vector3(44.97f, F + 3.17f, 162.5f), new Vector3(0.04f, 0.5f, 1.8f), "Green", 0f, false);
            Label(g, "Approach_Vault_Sign_Text", "B1  VAULT", new Vector3(44.94f, F + 3.17f, 162.5f), 90f, 0.38f, Color.white);
        }

        /// <summary>Cau thang bao mat sau cua ham (x 45.3..58, z 160.5..164.5): chieu nghi F, 22 bac xuong san kho B, cua thang may truot mo.</summary>
        static void BuildStair(Transform g)
        {
            Box(g, "Stair_Landing", new Vector3(45.65f, F - 0.15f, LaneZ), new Vector3(0.7f, 0.3f, 4f), "GrayDark");
            const int n = 22; float run = (StairX1 - StairX0) / n;
            for (int i = 0; i < n; i++)
            {
                float top = F - (F - B) * (i + 1) / n;
                float h = Mathf.Max(0.05f, top - (B - 0.3f));
                Box(g, "Step_" + (i + 1).ToString("00"), new Vector3(StairX0 + run * (i + 0.5f), (B - 0.3f) + h * 0.5f, LaneZ), new Vector3(run, h, 4f), "Stone");
            }
            Box(g, "Stair_Wall_S", new Vector3((45.3f + StairX1) * 0.5f, B + 4.0f, 160.35f), new Vector3(StairX1 - 45.3f, 8.0f, 0.3f), "GrayDark");
            Box(g, "Stair_Wall_N", new Vector3((45.3f + StairX1) * 0.5f, B + 4.0f, 164.65f), new Vector3(StairX1 - 45.3f, 8.0f, 0.3f), "GrayDark");
            // L3-L4: hinh khoi "di XUONG tang ham": mep bac vang, lan can hai ben + tru, tran doc, den tran, den khan cap do, bien B1 treo.
            {
                const int nn = 22; float rr = (StairX1 - StairX0) / nn;
                for (int i = 0; i < nn; i++)
                    Box(g, "Step_Nose_" + (i + 1).ToString("00"), new Vector3(StairX0 + rr * i + 0.04f, F - (F - B) * (i + 1) / nn + 0.006f, LaneZ), new Vector3(0.08f, 0.012f, 3.9f), "Yellow", 0f, false);
            }
            float railSlope = Mathf.Atan2(F - B, StairX1 - StairX0) * Mathf.Rad2Deg;
            float RailY(float x) => F + 1.0f - (F - B) * (x - StairX0) / (StairX1 - StairX0);
            // Lan can Bac bat dau sau la cua ham mo (la nam doc tuong Bac x 45..48.1)
            foreach (var (side, z, xa) in new[] { ("S", 160.62f, StairX0), ("N", 164.38f, 48.4f) })
            {
                float railLen = (StairX1 - xa) / Mathf.Cos(railSlope * Mathf.Deg2Rad);
                var rail = Box(g, "Stair_Rail_" + side, Vector3.zero, new Vector3(railLen, 0.06f, 0.06f), "Metal", 0f, false);
                rail.transform.SetPositionAndRotation(new Vector3((xa + StairX1) * 0.5f, RailY((xa + StairX1) * 0.5f), z), Quaternion.Euler(0f, 0f, -railSlope));
                for (float x = xa + 0.3f; x < StairX1; x += 2.3f)
                {
                    float top = StepTop(x), h = RailY(x) - top;
                    Box(g, "Stair_RailPost_" + side + "_" + x.ToString("0.0"), new Vector3(x, top + h * 0.5f, z), new Vector3(0.05f, h, 0.05f), "Metal", 0f, false);
                }
            }
            StairCeiling(g, "Stair_Ceiling", CeilX0, StairX1);
            for (int i = 0; i < 4; i++)
            {
                float x = 47.5f + 3f * i;
                var l = Box(g, "Stair_Light_" + i, new Vector3(x, CeilingBottom(x) - 0.03f, LaneZ), new Vector3(0.5f, 0.05f, 1.2f), "White", 0f, false);
                l.transform.rotation = Quaternion.Euler(0f, 0f, x <= CeilXk ? 0f : -CeilingPitch);
                Box(g, "Stair_EmergencyLight_N_" + i, new Vector3(x + 1.2f, StepTop(x + 1.2f) + 2.3f, 164.475f), new Vector3(0.4f, 0.15f, 0.05f), "LightRed", 0f, false);
                Box(g, "Stair_EmergencyLight_S_" + i, new Vector3(x + 0.3f, StepTop(x + 0.3f) + 2.3f, 160.525f), new Vector3(0.4f, 0.15f, 0.05f), "LightRed", 0f, false);
            }
            // Bien B1 treo tran giua cau thang (nhin tu tren xuong doc duoc), thanh treo
            {
                const float sx = 51.5f; float cy = CeilingBottom(sx) - 0.45f;
                Box(g, "Stair_Sign_B1_Plate", new Vector3(sx, cy, LaneZ), new Vector3(0.04f, 0.45f, 1.6f), "Green", 0f, false);
                Label(g, "Stair_Sign_B1_Text", "B1  VAULT", new Vector3(sx - 0.03f, cy, LaneZ), 90f, 0.32f, Color.white);
                foreach (float dz in new[] { -0.6f, 0.6f })
                    Box(g, "Stair_Sign_B1_Hanger_" + (dz < 0 ? "S" : "N"), new Vector3(sx, (cy + 0.225f + CeilingBottom(sx)) * 0.5f, LaneZ + dz), new Vector3(0.02f, CeilingBottom(sx) - cy - 0.225f + 0.05f, 0.02f), "Metal", 0f, false);
                // so tang tren tuong hai ben dau cau thang (mat trong tuong) va tren cua thang may
                // (tuong Bac dau cau thang bi la cua ham mo che -> 1F o tuong Nam)
                Label(g, "Stair_Wall_Label_1F", "1F", new Vector3(46.6f, F + 1.9f, 160.52f), 180f, 0.45f, new Color(0.95f, 0.85f, 0.2f));
                Label(g, "Stair_Wall_Label_B1", "B1", new Vector3(StairX1 - 1.2f, B + 1.9f, 164.48f), 0f, 0.6f, new Color(0.95f, 0.85f, 0.2f));
                Label(g, "Lift_Sign_Text", "B1", new Vector3(StairX1 + 0.17f, B + 4.6f, LaneZ), 90f, 0.26f, Color.white);
            }
            // Cua thang may bao mat o chan cau thang (truot mo san vao hoc tuong), den trang lanh
            Box(g, "Lift_Frame_Head", new Vector3(StairX1 + 0.1f, B + 4.3f, LaneZ), new Vector3(0.3f, 0.4f, 4.4f), "Metal", 0f, false);
            Movable(Box(g, "Lift_Door_S", new Vector3(StairX1 + 0.25f, B + 2.0f, 159.4f), new Vector3(0.08f, 4.0f, 2.0f), "Metal", 0f, false));
            Movable(Box(g, "Lift_Door_N", new Vector3(StairX1 + 0.25f, B + 2.0f, 165.6f), new Vector3(0.08f, 4.0f, 2.0f), "Metal", 0f, false));
            Box(g, "Lift_Sign", new Vector3(StairX1 + 0.2f, B + 4.6f, LaneZ), new Vector3(0.05f, 0.3f, 1.6f), "LightRed", 0f, false);
        }

        static void BuildP1(Transform geo, Transform decor, float zP12)
        {
            const float h = 5f;
            Box(geo, "P1_Floor", new Vector3(81f, B - 0.15f, 162.5f), new Vector3(46f, 0.3f, 25f), "Gray");
            Box(decor, "P1_Floor_Lane", new Vector3(81f, B + 0.005f, LaneZ), new Vector3(46f, 0.01f, 1.2f), "Yellow", 0f, false);
            WallX(geo, "P1_Wall_W", 58f, 150f, 175f, h, "GrayDark", (LaneZ, 4f, h));
            WallZ(geo, "P1_Wall_S", 150f, 58f, 104f, h, "GrayDark");
            WallZ(geo, "P1_Wall_N", 175f, 58f, 104f, h, "GrayDark");
            // Tuong chung P1/P2 (x = 104, z 150..184, cao 8.5): o mo cua chop ket nua chung (cao 2.6)
            WallX(geo, "P12_Wall", 104f, 150f, 184f, 8.5f, "GrayDark", (zP12, 4f, 2.6f));
            Movable(Box(decor, "P12_Shutter_Jammed", new Vector3(104f, B + 2.9f, zP12), new Vector3(0.12f, 0.6f, 4f), "Metal", 0f, false));
            for (int i = 0; i < 4; i++) Box(decor, "P12_Shutter_Slat_" + i, new Vector3(103.92f, B + 2.66f + 0.15f * i, zP12), new Vector3(0.02f, 0.03f, 4f), "GrayDark", 0f, false);
            // Cua chop hai ben tuong Bac (W4) - khung cua chop kem cua enemy (Door_Enemy_P1_W6_*)
            foreach (var x in new[] { 95f, 101f })
                Box(decor, "P1_Shutter_Box_" + x, new Vector3(x, B + 2.9f, 174.8f), new Vector3(2.0f, 0.5f, 0.12f), "Metal", 0f, false);
            // Phong kiem soat: cua so kinh tuong Nam (khong collider, sat tuong)
            Box(decor, "P1_Control_Window", new Vector3(86f, B + 1.8f, 150.17f), new Vector3(8f, 1.4f, 0.02f), "Blue", 0f, false);
            // Den tran trang lanh, camera tran
            for (int i = 0; i < 6; i++) Box(decor, "P1_Ceiling_Light_" + i, new Vector3(62f + 7.5f * i, B + 4.85f, LaneZ), new Vector3(1.2f, 0.05f, 0.4f), "White", 0f, false);
            Box(decor, "P1_CeilingCam_N", new Vector3(70f, B + 4.6f, 174.6f), new Vector3(0.3f, 0.25f, 0.5f), "GrayDark", 0f, false);
            Box(decor, "P1_CeilingCam_S", new Vector3(92f, B + 4.6f, 150.4f), new Vector3(0.3f, 0.25f, 0.5f), "GrayDark", 0f, false);
            // Den bao dong do tren tuong
            Box(decor, "P1_Alarm_N", new Vector3(80f, B + 4.2f, 174.83f), new Vector3(0.6f, 0.25f, 0.05f), "LightRed", 0f, false);
            Box(decor, "P1_Alarm_S", new Vector3(66f, B + 4.2f, 150.17f), new Vector3(0.6f, 0.25f, 0.05f), "LightRed", 0f, false);
        }

        static void BuildP2(Transform geo, Transform decor, float zP12, float xP23, float xP3In)
        {
            const float h = 8.5f;
            Box(geo, "P2_Floor", new Vector3(130f, B - 0.15f, 168f), new Vector3(52f, 0.3f, 32f), "Stone");
            WallX(geo, "P2_Wall_E", 156f, 152f, 184f, h, "GrayDark");
            WallZ(geo, "P2_Wall_N", 184f, 104f, 156f, h, "GrayDark");
            WallZ(geo, "P2_Wall_S", 152f, 104f, 156f, h, "GrayDark", (xP23, 8f, 3.4f));
            // San luoi phia Bac (x 116..150, z 179..184, mat san 4 m) + vach duoi san luoi (cua X)
            Box(geo, "P2_Mezz_Slab", new Vector3(133f, BalconyY - 0.15f, 181.5f), new Vector3(34f, 0.3f, 5f), "Metal");
            WallZ(geo, "P2_Mezz_Partition", 179.5f, 116f, 150f, BalconyY - 0.3f, "GrayDark");
            Box(decor, "P2_Mezz_Handrail", new Vector3(133f, BalconyY + 1.0f, 179.05f), new Vector3(34f, 0.06f, 0.06f), "Metal", 0f, false);
            for (int i = 0; i < 9; i++) Box(decor, "P2_Mezz_Post_" + i, new Vector3(117f + 4f * i, BalconyY + 0.5f, 179.05f), new Vector3(0.05f, 1.0f, 0.05f), "Metal", 0f, false);
            // Ban cong tang hai phia Dong (x 150..156, z 152..184)
            Box(geo, "P2_Balcony_Slab", new Vector3(153f, BalconyY - 0.15f, 168f), new Vector3(6f, 0.3f, 32f), "Metal");
            Box(decor, "P2_Balcony_Handrail", new Vector3(150.05f, BalconyY + 1.0f, 168f), new Vector3(0.06f, 0.06f, 32f), "Metal", 0f, false);
            for (int i = 0; i < 8; i++) Box(decor, "P2_Balcony_Post_" + i, new Vector3(150.05f, BalconyY + 0.5f, 153.5f + 4.2f * i), new Vector3(0.05f, 1.0f, 0.05f), "Metal", 0f, false);
            Box(decor, "P2_Sniper_Lamp", new Vector3(155.8f, BalconyY + 2.6f, 167f), new Vector3(0.05f, 0.6f, 2.4f), "White", 0f, false);
            // Cau thang sat tu ban cong xuong (doc tuong Nam, x 141..150): bac khong collider sat tuong
            for (int i = 0; i < 12; i++)
                Box(decor, "P2_IronStair_" + i, new Vector3(149.6f - 0.75f * i, BalconyY - (i + 0.5f) * BalconyY / 12f, 153.0f), new Vector3(0.75f, 0.06f, 1.4f), "Metal", 0f, false);
            Box(decor, "P2_IronStair_Rail", new Vector3(145.2f, 2.6f, 153.7f), new Vector3(9.5f, 0.05f, 0.05f), "Metal", 0f, false).transform.rotation = Quaternion.Euler(0f, 0f, 22.8f);
            // Tuong ket (mat ket sat tuong, khong collider) tuong Tay/Bac tren san luoi
            for (int i = 0; i < 6; i++)
            {
                if (i < 4) Box(decor, "P2_Lockers_S_" + i, new Vector3(108f + 4f * i, B + 1.6f, 152.18f), new Vector3(3.8f, 3.0f, 0.03f), "Metal", 0f, false);
                Box(decor, "P2_Lockers_N_" + i, new Vector3(108f + 8f * i, B + 6.0f, 183.8f), new Vector3(7.6f, 3.6f, 0.06f), "Metal", 0f, false);
            }
            for (int i = 0; i < 6; i++) Box(decor, "P2_Ceiling_Light_" + i, new Vector3(110f + 8f * i, B + 8.3f, 168f), new Vector3(1.2f, 0.05f, 0.5f), "White", 0f, false);
            Box(decor, "P2_Alarm", new Vector3(104.2f, B + 6.0f, 176f), new Vector3(0.05f, 0.3f, 0.8f), "LightRed", 0f, false);
            // Hanh lang P2 -> P3 (rong 6 m, z 144..152), cua kho nang mo sat tuong (dong sam sau khi camera qua - canh trang tri)
            float cx = (xP23 + xP3In) * 0.5f;
            Box(geo, "P23_Floor", new Vector3(cx, B - 0.15f, 148f), new Vector3(9f, 0.3f, 8f), "Stone");
            Box(geo, "P23_Wall_W", new Vector3(cx - 4.65f, B + 2.0f, 148f), new Vector3(0.3f, 4f, 8f), "GrayDark");
            Box(geo, "P23_Wall_E", new Vector3(cx + 4.65f, B + 2.0f, 148f), new Vector3(0.3f, 4f, 8f), "GrayDark");
            Box(decor, "P23_StorageDoor_Leaf", new Vector3(cx + 4.35f, B + 1.7f, 149.6f), new Vector3(0.25f, 3.4f, 3.0f), "Metal", 0f, false);
        }

        static void BuildP3(Transform geo, Transform decor, float xP23, float xIn)
        {
            const float h = 8f;
            Box(geo, "P3_Floor", new Vector3(P3X, B - 0.15f, 117f), new Vector3(48f, 0.3f, 54f), "Stone");
            Box(decor, "P3_Floor_Ring", new Vector3(P3X, B + 0.006f, 117f), new Vector3(36f, 0.01f, 0.4f), "Yellow", 0f, false);
            Box(decor, "P3_Floor_Ring2", new Vector3(P3X, B + 0.006f, 117f), new Vector3(0.4f, 0.01f, 36f), "Yellow", 0f, false);
            WallX(geo, "P3_Wall_W", P3X - 24f, 90f, 144f, h, "GrayDark");
            WallX(geo, "P3_Wall_E", P3X + 24f, 90f, 144f, h, "GrayDark");
            WallZ(geo, "P3_Wall_N", 144f, P3X - 24f, P3X + 24f, h, "GrayDark", (xIn, 8f, 4f));
            // Tuong Nam: cua cuon nap tien (noi Level 5) - o mo x 147.5..152.5, la cua truot len khi ha ten cuoi
            float shX = P3X + 20.2f;
            WallZ(geo, "P3_Wall_S", 90f, P3X - 24f, P3X + 24f, h, "GrayDark", (shX, 5f, 4.2f));
            var pivot = new GameObject("LoadingShutter_Pivot").transform;
            pivot.SetParent(geo, false); pivot.position = new Vector3(shX, B, 90f);
            var leaf = Box(pivot, "LoadingShutter_Leaf", Vector3.zero, new Vector3(5f, 4.2f, 0.2f), "Metal");
            leaf.transform.localPosition = new Vector3(0f, 2.1f, 0f); Movable(leaf);
            Box(decor, "LoadingShutter_Dark", new Vector3(shX, B + 2.1f, 89.0f), new Vector3(5f, 4.2f, 0.1f), "Black", 0f, false);
            Box(decor, "LoadingShutter_Stripe", new Vector3(shX, B + 4.4f, 90.17f), new Vector3(5.2f, 0.2f, 0.03f), "Yellow", 0f, false);
            // Loi vang: khoi x 124..136, z 100..112, cao 6; cua vault tron o mat Bac (quay mo truoc dot don dap)
            Box(geo, "GoldCore_Block", new Vector3(P3X, B + 3f, 106f), new Vector3(12f, 6f, 12f), "Metal");
            var opening = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            opening.name = "VaultDoor_Opening"; Object.DestroyImmediate(opening.GetComponent<Collider>());
            opening.transform.SetParent(decor, false); opening.transform.SetPositionAndRotation(new Vector3(P3X, B + 2.6f, 112.01f), Quaternion.Euler(90f, 0f, 0f));
            opening.transform.localScale = new Vector3(4.6f, 0.01f, 4.6f); opening.GetComponent<MeshRenderer>().sharedMaterial = M("Black");
            var vp = new GameObject("VaultDoor_Pivot").transform;
            vp.SetParent(geo, false); vp.position = new Vector3(P3X - 2.6f, B, 112.25f);
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "VaultDoor_Disc"; Object.DestroyImmediate(disc.GetComponent<Collider>());
            disc.transform.SetParent(vp, false); disc.transform.localPosition = new Vector3(2.6f, 2.6f, 0f); disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            disc.transform.localScale = new Vector3(5.2f, 0.12f, 5.2f); disc.GetComponent<MeshRenderer>().sharedMaterial = M("Metal");
            Movable(disc);
            var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "VaultDoor_Hub"; Object.DestroyImmediate(hub.GetComponent<Collider>());
            hub.transform.SetParent(vp, false); hub.transform.localPosition = new Vector3(2.6f, 2.6f, 0.2f); hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            hub.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f); hub.GetComponent<MeshRenderer>().sharedMaterial = M("Yellow");
            Movable(hub);
            // Thoi vang xep bac trong loi (nhin thay qua cua vault khi mo)
            for (int i = 0; i < 3; i++) Box(decor, "GoldCore_Bars_" + i, new Vector3(P3X, B + 0.3f + 0.6f * i, 110.4f - 1.0f * i), new Vector3(3.6f - 0.8f * i, 0.6f, 0.9f), "Yellow", 0f, false);
            Box(decor, "GoldCore_Stripe_N", new Vector3(P3X, B + 5.6f, 112.02f), new Vector3(12f, 0.2f, 0.03f), "Yellow", 0f, false);
            // Den xoay vang tren loi (bat khi cua vault mo), den chieu xa thu
            Box(decor, "GoldCore_Beacon_W", new Vector3(P3X - 5f, B + 6.2f, 111.5f), new Vector3(0.4f, 0.4f, 0.4f), "Yellow", 0f, false);
            Box(decor, "GoldCore_Beacon_E", new Vector3(P3X + 5f, B + 6.2f, 111.5f), new Vector3(0.4f, 0.4f, 0.4f), "Yellow", 0f, false);
            Box(decor, "P3_Sniper_Lamp", new Vector3(P3X - 23.8f, B + 3.2f, 107f), new Vector3(0.05f, 1.2f, 3.0f), "White", 0f, false);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 2; j++)
                    Box(decor, "P3_Ceiling_Light_" + i + "_" + j, new Vector3(P3X - 16f + 10.7f * i, B + 7.8f, 98f + 36f * j), new Vector3(1.2f, 0.05f, 0.5f), "White", 0f, false);
            // Lan sat tuong (ket tien) + bao tien chat do canh tuong Tay (khong tren tia nhin)
            for (int i = 0; i < 4; i++) Box(decor, "P3_Lockers_W_" + i, new Vector3(P3X - 23.82f, B + 1.6f, 124f + 5f * i), new Vector3(0.03f, 3.0f, 4.6f), "Metal", 0f, false);
            Box(decor, "P3_MoneyBags_E", new Vector3(P3X + 22.5f, B + 0.4f, 128f), new Vector3(2f, 0.8f, 3f), "Cardboard", 0f, false);
        }
    }
}
