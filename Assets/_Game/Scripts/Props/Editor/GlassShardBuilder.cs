using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.Props.Editor
{
    /// <summary>
    /// Chia quad 1x1 (mat phang XY) thanh N manh tam giac (N trong [6,10], mac dinh 8) dang long kinh mong, luu mesh + prefab
    /// Prop_GlassShards. Chay lai an toan (ghi de). Menu: ClaudeCop/Props/Build Glass Shards.
    /// </summary>
    public static class GlassShardBuilder
    {
        public const int MinShards = 6, MaxShards = 10, DefaultShards = 8;
        public const float Thickness = 0.02f;
        public const string Dir = "Assets/_Game/Prefabs/Props";
        public const string PrefabPath = Dir + "/Prop_GlassShards.prefab";

        [MenuItem("ClaudeCop/Props/Build Glass Shards")]
        public static void BuildMenu() { Build(DefaultShards); }

        /// <summary>Cac tam giac (3 dinh moi tam giac, mat phang XY, quad [-0.5,0.5]) - ham thuan de test.</summary>
        public static List<Vector2[]> Slice(int count, int seed = 7)
        {
            count = Mathf.Clamp(count, MinShards, MaxShards);
            var rng = new System.Random(seed);
            // Diem chu vi theo tham so t in [0,4): 4 goc + (count-4) diem them, sap xep.
            var ts = new List<float> { 0f, 1f, 2f, 3f };
            while (ts.Count < count)
            {
                float t = (float)(rng.NextDouble() * 4.0);
                float frac = t - Mathf.Floor(t);
                if (frac < 0.15f || frac > 0.85f) continue; // tranh tam giac qua manh
                ts.Add(t);
            }
            ts.Sort();
            var pts = new List<Vector2>(count);
            foreach (var t in ts) pts.Add(Perimeter(t));
            var center = new Vector2((float)(rng.NextDouble() - 0.5) * 0.2f, (float)(rng.NextDouble() - 0.5) * 0.2f);
            var tris = new List<Vector2[]>(count);
            for (int i = 0; i < pts.Count; i++)
                tris.Add(new[] { center, pts[i], pts[(i + 1) % pts.Count] });
            return tris;
        }

        static Vector2 Perimeter(float t)
        {
            int side = Mathf.FloorToInt(t) % 4; float f = t - Mathf.Floor(t);
            switch (side)
            {
                case 0: return new Vector2(-0.5f + f, -0.5f);
                case 1: return new Vector2(0.5f, -0.5f + f);
                case 2: return new Vector2(0.5f - f, 0.5f);
                default: return new Vector2(-0.5f, 0.5f - f);
            }
        }

        static Mesh PrismMesh(Vector2[] tri, Vector2 centroid, string name)
        {
            // Dam bao thu tu nguoc chieu kim dong ho nhin tu -Z (mat truoc quay ve -Z nhu Quad).
            var a = tri[0] - centroid; var b = tri[1] - centroid; var c = tri[2] - centroid;
            float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
            if (cross > 0f) { var tmp = b; b = c; c = tmp; } // lam cho tam giac CW (nhin tu +Z) => mat truoc huong -Z
            float h = Thickness * 0.5f;
            var front = new[] { new Vector3(a.x, a.y, -h), new Vector3(b.x, b.y, -h), new Vector3(c.x, c.y, -h) };
            var back = new[] { new Vector3(a.x, a.y, h), new Vector3(b.x, b.y, h), new Vector3(c.x, c.y, h) };
            var v = new List<Vector3>(); var idx = new List<int>(); var uv = new List<Vector2>();
            void Tri(Vector3 p0, Vector3 p1, Vector3 p2)
            {
                int i0 = v.Count; v.Add(p0); v.Add(p1); v.Add(p2);
                uv.Add(new Vector2(p0.x + 0.5f, p0.y + 0.5f)); uv.Add(new Vector2(p1.x + 0.5f, p1.y + 0.5f)); uv.Add(new Vector2(p2.x + 0.5f, p2.y + 0.5f));
                idx.Add(i0); idx.Add(i0 + 1); idx.Add(i0 + 2);
            }
            Tri(front[0], front[1], front[2]);
            Tri(back[0], back[2], back[1]);
            for (int i = 0; i < 3; i++)
            {
                int j = (i + 1) % 3;
                Tri(front[i], back[i], back[j]);
                Tri(front[i], back[j], front[j]);
            }
            var m = new Mesh { name = name };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(idx, 0);
            m.RecalculateNormals(); m.RecalculateBounds();
            return m;
        }

        public static GameObject Build(int count)
        {
            count = Mathf.Clamp(count, MinShards, MaxShards);
            Directory.CreateDirectory(Dir + "/Meshes");
            Directory.CreateDirectory(Dir + "/Materials");
            var mat = PropAssetBuilder.GlassMaterial();

            // Xoa mesh cu (chay lai an toan).
            foreach (var guid in AssetDatabase.FindAssets("GlassShard_ t:Mesh", new[] { Dir + "/Meshes" }))
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));

            var tris = Slice(count);
            var root = new GameObject("Prop_GlassShards");
            for (int i = 0; i < tris.Count; i++)
            {
                var t = tris[i];
                var centroid = (t[0] + t[1] + t[2]) / 3f;
                var mesh = PrismMesh(t, centroid, "GlassShard_" + i);
                AssetDatabase.CreateAsset(mesh, Dir + "/Meshes/GlassShard_" + i + ".asset");

                var go = new GameObject("Shard_" + i);
                go.transform.SetParent(root.transform, false);
                go.transform.localPosition = new Vector3(centroid.x, centroid.y, 0f);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh; mc.convex = true;
                var rb = go.AddComponent<Rigidbody>();
                rb.mass = 0.1f;
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Props] Built " + count + " glass shards -> " + PrefabPath);
            return prefab;
        }
    }
}
