using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// T-501: them diem xuat hien enemy o tren cao cho moi goc giao tranh cua Level_01 (man doc).
    /// Ban tia tu CamPoint toi mat tuong/nha phia truoc, dung khoi blockout (cua so, ban cong, mep mai, day thả)
    /// gan vao mat tuong do, dat EnemySpawn_P*_W*_NN + con Peek, roi chay lai Level01Assembler.
    /// Chay lai an toan: xoa nhom "T501_MultiFloor" cu truoc khi dung.
    /// </summary>
    public static class MultiFloorBuilder
    {
        const string PrefabPath = "Assets/_Game/Level/Level_01.prefab";
        const string ScenePath = "Assets/_Game/Scenes/Gameplay/Level_01.unity";
        const string MatDir = "Assets/_Game/Level/Materials/";
        const string GroupName = "T501_MultiFloor";

        enum Kind { Window2, Window3, Balcony, RoofEdge, Rappel }

        // Moi goc giao tranh: danh sach kieu muon them (moi muc = 1 enemy). Kieu khong dat duoc thi thu Window2 roi Balcony.
        static readonly Dictionary<string, Kind[]> Plan = new Dictionary<string, Kind[]>
        {
            { "P1_S2", new Kind[0] },                              // dot dau: giu nhe
            { "P1_S3", new[] { Kind.Window2 } },
            { "P1_S5", new[] { Kind.Window2 } },
            { "P1_S6", new[] { Kind.RoofEdge } },
            { "P2_S2", new[] { Kind.Balcony } },
            { "P2_S3", new[] { Kind.Rappel } },
            { "P2_S5", new[] { Kind.Balcony, Kind.Window2 } },
            { "P2_S6", new[] { Kind.Rappel } },
            { "P3_S2", new[] { Kind.Window3, Kind.Window2 } },
            { "P3_S3", new[] { Kind.Rappel } },
            { "P3_S5", new[] { Kind.RoofEdge, Kind.Window3 } },
            { "P3_S6", new[] { Kind.Rappel, Kind.Window3 } },
        };

        static readonly float[] Yaws = { 0f, -6f, 6f, -12f, 12f, -17f, 17f };
        const float ProbeHeight = 6f;       // cao tren san: tren lan can/xe/container, duoi mai nha pho (8 m)
        const float MinDist = 7f, MaxDist = 32f;
        const float MaxPitch = 34f;         // goc ngang toi diem ngam (man doc con thua)
        const float MinSeparation = 7f;     // cach cac muc tieu khac (do)
        const float AimHeight = 1.5f;

        struct Spec
        {
            public Kind kind; public string wave;
            public Vector3 hit, normal; public float ground, top; public bool ceiling;
            public Vector3 hide, peek; public Quaternion rot;
        }

        [MenuItem("ClaudeCop/Level/Build Multi-floor Enemies (T-501)")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // 1) Xoa nhom cu trong prefab de tia khong trung khoi da dung.
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            var old = root.transform.Find(GroupName);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.UnloadPrefabContents(root);

            // 2) Tinh vi tri trong scene gameplay (co collider cua level).
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject level = null;
            foreach (var g in scene.GetRootGameObjects()) if (g.name == "Level_01") level = g;
            if (level == null) { Debug.LogError("[T-501] Khong thay Level_01 trong scene."); return; }
            foreach (var t in level.GetComponentsInChildren<Transform>(true)) if (t.name.StartsWith("Area_")) t.gameObject.SetActive(true);
            Physics.SyncTransforms();

            var points = new Dictionary<string, Transform>();
            foreach (var t in level.GetComponentsInChildren<Transform>(true)) points[t.name] = t;

            var specs = new List<Spec>();
            var nextIndex = new Dictionary<string, int>();
            foreach (var kv in Plan)
            {
                if (!points.TryGetValue("CamPoint_" + kv.Key, out var cp)) { Debug.LogWarning("[T-501] Thieu CamPoint_" + kv.Key); continue; }
                string wave = kv.Key.Replace("_S", "_W");
                var taken = ExistingAngles(cp, points, wave, out int count);
                nextIndex[wave] = count + 1;
                foreach (var want in kv.Value)
                {
                    if (TryPlace(cp, want, wave, taken, out var spec) ||
                        (want != Kind.Window2 && TryPlace(cp, Kind.Window2, wave, taken, out spec)) ||
                        (want != Kind.Balcony && TryPlace(cp, Kind.Balcony, wave, taken, out spec)))
                    {
                        specs.Add(spec);
                        taken.Add(Yaw(cp, spec.peek));
                    }
                    else Debug.LogWarning("[T-501] " + kv.Key + ": khong tim duoc cho dat " + want);
                }
            }

            // 3) Dung khoi + diem spawn trong prefab (toa do prefab = toa do world qua nghich dao instance).
            Matrix4x4 toLocal = level.transform.worldToLocalMatrix;
            Quaternion rotToLocal = Quaternion.Inverse(level.transform.rotation);
            root = PrefabUtility.LoadPrefabContents(PrefabPath);
            var group = new GameObject(GroupName).transform;
            group.SetParent(root.transform, false);
            var mats = new Materials();
            foreach (var s in specs)
            {
                int idx = nextIndex[s.wave]++;
                BuildSpec(s, idx, group, mats, toLocal, rotToLocal);
            }
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log("[T-501] Da them " + specs.Count + " enemy tren cao. Dung lai Level_01...");

            // 4) Noi lai wave + frameTargets.
            Level01Assembler.Assemble();
        }

        // ---------- tinh toan ----------

        static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude < 1e-6f ? Vector3.forward : v.normalized; }

        static float Yaw(Transform cp, Vector3 p)
        {
            Vector3 f = Flat(cp.forward), d = Flat(p - cp.position);
            return Vector3.SignedAngle(f, d, Vector3.up);
        }

        static List<float> ExistingAngles(Transform cp, Dictionary<string, Transform> pts, string wave, out int enemyCount)
        {
            var list = new List<float>(); enemyCount = 0;
            foreach (var kv in pts)
            {
                bool enemy = kv.Key.StartsWith("EnemySpawn_" + wave + "_");
                if (!enemy && !kv.Key.StartsWith("HostageSpawn_" + wave + "_") && !kv.Key.StartsWith("PickupSpawn_" + wave + "_")) continue;
                if (enemy) enemyCount++;
                var peek = kv.Value.Find("Peek");
                list.Add(Yaw(cp, peek != null ? peek.position : kv.Value.position));
            }
            return list;
        }

        static bool TryPlace(Transform cp, Kind kind, string wave, List<float> taken, out Spec spec)
        {
            spec = default;
            Vector3 cam = cp.position;
            float ground = Physics.Raycast(cam + Vector3.up * 0.5f, Vector3.down, out var gh, 10f, ~0, QueryTriggerInteraction.Ignore) ? gh.point.y : cam.y - 1.6f;
            bool hasCeiling = Physics.Raycast(cam, Vector3.up, out var ch, 30f, ~0, QueryTriggerInteraction.Ignore);
            float ceiling = hasCeiling ? ch.point.y : float.MaxValue;

            float bestScore = -1f;
            foreach (float yaw in Yaws)
            {
                Vector3 dir = Quaternion.AngleAxis(yaw, Vector3.up) * Flat(cp.forward);
                Vector3 origin = new Vector3(cam.x, ground + ProbeHeight, cam.z);
                if (!Physics.Raycast(origin, dir, out var hit, MaxDist, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (hit.distance < MinDist) continue;
                Vector3 n = Flat(hit.normal);
                if (Vector3.Dot(n, -dir) < 0.6f) continue;                 // mat tuong phai huong ve camera
                float top = Mathf.Min(hit.collider.bounds.max.y, ceiling);

                var s = new Spec { kind = kind, wave = wave, hit = hit.point, normal = n, ground = ground, top = top, ceiling = hasCeiling };
                if (!Layout(ref s)) continue;

                // Ngam thay duoc tu camera va nam trong khung doc.
                Vector3 aim = s.peek + Vector3.up * AimHeight;
                Vector3 toAim = aim - cam;
                float pitch = Mathf.Atan2(toAim.y, new Vector2(toAim.x, toAim.z).magnitude) * Mathf.Rad2Deg;
                if (Mathf.Abs(pitch) > MaxPitch) continue;
                if (Physics.Raycast(cam, toAim.normalized, out var block, toAim.magnitude - 0.3f, ~0, QueryTriggerInteraction.Ignore)) continue;

                float a = Yaw(cp, s.peek), sep = 180f;
                foreach (var t in taken) sep = Mathf.Min(sep, Mathf.Abs(Mathf.DeltaAngle(a, t)));
                if (sep < MinSeparation) continue;
                float score = sep - Mathf.Abs(a) * 0.5f;                     // xa muc tieu khac, gan giua khung
                if (score > bestScore) { bestScore = score; spec = s; }
            }
            if (bestScore < 0f) return false;
            spec.rot = Quaternion.LookRotation(Flat(cam - spec.peek), Vector3.up);
            return true;
        }

        /// <summary>Tinh vi tri nap/lo ra theo kieu; false neu mat tuong khong du cao.</summary>
        static bool Layout(ref Spec s)
        {
            Vector3 p = s.hit, n = s.normal;
            float h = s.top - s.ground;
            switch (s.kind)
            {
                case Kind.Window2:
                case Kind.Window3:
                {
                    float sill = s.ground + (s.kind == Kind.Window2 ? 3.2f : 6.4f);
                    if (s.top < sill + 2.8f) return false;
                    s.hide = Ground(p - n * 0.7f, sill);                      // trong nha, sau tuong
                    s.peek = Ground(p + n * 0.45f, sill);                     // buoc ra bau cua so
                    return true;
                }
                case Kind.Balcony:
                {
                    float floor = s.ground + 3.4f;
                    if (s.top < floor + 2.4f) return false;
                    s.hide = Ground(p + n * 0.8f, floor - 1.2f);              // ngoi sau lan can
                    s.peek = Ground(p + n * 0.8f, floor);
                    return true;
                }
                case Kind.RoofEdge:
                {
                    if (s.ceiling || h < 5f || h > 12.5f) return false;
                    s.hide = Ground(p - n * 0.6f, s.top - 1.2f);              // nam sau tuong chan mai
                    s.peek = Ground(p - n * 0.6f, s.top);
                    return true;
                }
                case Kind.Rappel:
                {
                    if (h < 6.5f) return false;
                    if (s.ceiling)
                    {
                        // Trong nha: tha tu tran xuong.
                        s.hide = Ground(p + n * 1.2f, s.top + 0.3f);          // tren tran (an trong mai)
                        s.peek = Ground(p + n * 1.2f, s.ground + 3.6f);
                    }
                    else
                    {
                        s.hide = Ground(p - n * 0.6f, s.top - 1.2f);          // sau tuong chan mai
                        s.peek = Ground(p + n * 0.45f, s.top - 3.4f);         // treo tren mat tien
                    }
                    return true;
                }
            }
            return false;
        }

        static Vector3 Ground(Vector3 xz, float y) { xz.y = y; return xz; }

        // ---------- dung ----------

        sealed class Materials
        {
            public readonly Material dark = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Blockout_GrayDark.mat");
            public readonly Material gray = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Blockout_Gray.mat");
            public readonly Material red = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Blockout_Red.mat");
        }

        static void BuildSpec(Spec s, int idx, Transform group, Materials m, Matrix4x4 toLocal, Quaternion rotToLocal)
        {
            string tag = s.wave + "_" + idx.ToString("00");
            var node = new GameObject("T501_" + s.kind + "_" + tag).transform;
            node.SetParent(group, false);
            Vector3 n = s.normal, r = Vector3.Cross(Vector3.up, n);
            Quaternion face = Quaternion.LookRotation(-n, Vector3.up);   // truc z vao tuong

            void Box(string name, Vector3 center, Vector3 size, Material mat, SurfaceMaterial surf)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = name;
                go.transform.SetParent(node, false);
                go.transform.SetPositionAndRotation(toLocal.MultiplyPoint3x4(center), rotToLocal * face);
                go.transform.localScale = size;
                if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
                var st = go.AddComponent<SurfaceMaterialTag>();
                var so = new SerializedObject(st);
                so.FindProperty("material").enumValueIndex = (int)surf;
                so.ApplyModifiedPropertiesWithoutUndo();
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            }

            Vector3 at(Vector3 basePt, float side, float up, float outN) => basePt + r * side + Vector3.up * up + n * outN;
            Vector3 wall = s.hit;
            switch (s.kind)
            {
                case Kind.Window2:
                case Kind.Window3:
                {
                    float sill = s.peek.y;
                    Vector3 b = Ground(wall, sill);
                    Box("Opening", at(b, 0f, 1.15f, 0.03f), new Vector3(1.8f, 2.3f, 0.06f), m.dark, SurfaceMaterial.Concrete);
                    Box("Sill", at(b, 0f, -0.1f, 0.45f), new Vector3(2.4f, 0.2f, 0.9f), m.gray, SurfaceMaterial.Concrete);
                    Box("JambL", at(b, -1.05f, 1.15f, 0.3f), new Vector3(0.3f, 2.5f, 0.6f), m.gray, SurfaceMaterial.Concrete);
                    Box("JambR", at(b, 1.05f, 1.15f, 0.3f), new Vector3(0.3f, 2.5f, 0.6f), m.gray, SurfaceMaterial.Concrete);
                    Box("Lintel", at(b, 0f, 2.45f, 0.3f), new Vector3(2.4f, 0.3f, 0.6f), m.gray, SurfaceMaterial.Concrete);
                    break;
                }
                case Kind.Balcony:
                {
                    float floor = s.peek.y;
                    Vector3 b = Ground(wall, floor);
                    Box("Door", at(b, 0f, 1.1f, 0.03f), new Vector3(1.4f, 2.2f, 0.06f), m.dark, SurfaceMaterial.Concrete);
                    Box("Slab", at(b, 0f, -0.1f, 0.8f), new Vector3(3.0f, 0.2f, 1.6f), m.gray, SurfaceMaterial.Concrete);
                    Box("RailFront", at(b, 0f, 0.55f, 1.55f), new Vector3(3.0f, 1.1f, 0.08f), m.red, SurfaceMaterial.Metal);
                    Box("RailL", at(b, -1.46f, 0.55f, 0.8f), new Vector3(0.08f, 1.1f, 1.6f), m.red, SurfaceMaterial.Metal);
                    Box("RailR", at(b, 1.46f, 0.55f, 0.8f), new Vector3(0.08f, 1.1f, 1.6f), m.red, SurfaceMaterial.Metal);
                    break;
                }
                case Kind.RoofEdge:
                {
                    Vector3 b = Ground(wall, s.top);
                    Box("Parapet", at(b, 0f, 0.45f, -0.12f), new Vector3(3.0f, 0.9f, 0.25f), m.gray, SurfaceMaterial.Concrete);
                    Box("Ledge", at(b, 0f, -0.05f, -0.6f), new Vector3(3.0f, 0.1f, 1.2f), m.dark, SurfaceMaterial.Concrete);
                    break;
                }
                case Kind.Rappel:
                {
                    Vector3 rope = Ground(s.peek, 0f);
                    float topY = s.ceiling ? s.top : s.top + 0.3f, botY = s.peek.y + 1.8f;
                    Box("Rope", Ground(rope, (topY + botY) * 0.5f), new Vector3(0.05f, Mathf.Max(0.1f, topY - botY), 0.05f), m.dark, SurfaceMaterial.Wood);
                    if (!s.ceiling)
                        Box("Parapet", at(Ground(wall, s.top), 0f, 0.45f, -0.12f), new Vector3(3.0f, 0.9f, 0.25f), m.gray, SurfaceMaterial.Concrete);
                    else
                        Box("Hatch", Ground(rope, s.top - 0.05f), new Vector3(1.4f, 0.1f, 1.4f), m.dark, SurfaceMaterial.Metal);
                    break;
                }
            }

            var spawn = new GameObject("EnemySpawn_" + tag).transform;
            spawn.SetParent(node, false);
            spawn.SetPositionAndRotation(toLocal.MultiplyPoint3x4(s.hide), rotToLocal * s.rot);
            var peek = new GameObject("Peek").transform;
            peek.SetParent(spawn, false);
            peek.SetPositionAndRotation(toLocal.MultiplyPoint3x4(s.peek), rotToLocal * s.rot);
        }
    }
}
