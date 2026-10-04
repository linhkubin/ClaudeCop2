using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.FX.Editor
{
    /// <summary>Sinh vat lieu, prefab FX, FxConfig va FxSystems.prefab (khong can art). Menu: ClaudeCop/FX/Build FX Assets.</summary>
    public static class FxAssetBuilder
    {
        const string Dir = "Assets/_Game/Prefabs/FX";

        struct Spec
        {
            public string name; public bool soft; public bool additive;
            public Color c0, c1; public int count;
            public float speedMin, speedMax, sizeMin, sizeMax, life, gravity, cone, grow;
        }

        [MenuItem("ClaudeCop/FX/Build FX Assets")]
        public static void Build()
        {
            Directory.CreateDirectory(Dir + "/Materials");
            Directory.CreateDirectory(Dir + "/Textures");
            var softTex = MakeSoftTexture(Dir + "/Textures/FxSoft.png");
            var matSoft = MakeMaterial(Dir + "/Materials/FxSoft.mat", softTex, false);
            var matSoftAdd = MakeMaterial(Dir + "/Materials/FxSoftAdd.mat", softTex, true);
            var matHard = MakeMaterial(Dir + "/Materials/FxHard.mat", null, false);
            var matHardAdd = MakeMaterial(Dir + "/Materials/FxHardAdd.mat", null, true);
            var matMark = MakeMaterial(Dir + "/Materials/BulletMark.mat", softTex, false);

            var grey = new Color(0.62f, 0.6f, 0.58f, 0.8f);
            var cfg = ScriptableObject.CreateInstance<FxConfig>();
            cfg.surfaceImpacts = new FxEntry[6];

            cfg.surfaceImpacts[0] = Entry(Impact("Impact_Concrete", matSoft, matHard, matHardAdd,
                P("dust", true, false, grey, grey, 6, 0.4f, 1.2f, 0.25f, 0.45f, 0.7f, -0.05f, 40, 1.8f),
                P("chips", false, false, new Color(0.7f, 0.68f, 0.64f, 1f), new Color(0.5f, 0.48f, 0.45f, 1f), 5, 1.5f, 3.5f, 0.03f, 0.06f, 0.6f, 1.5f, 50, 0f)), 0.8f, 8);
            cfg.surfaceImpacts[1] = Entry(Impact("Impact_Wood", matSoft, matHard, matHardAdd,
                P("dust", true, false, new Color(0.7f, 0.55f, 0.35f, 0.7f), new Color(0.6f, 0.45f, 0.3f, 0.7f), 3, 0.3f, 0.9f, 0.18f, 0.3f, 0.5f, -0.05f, 40, 1.5f),
                P("splinters", false, false, new Color(0.65f, 0.45f, 0.22f, 1f), new Color(0.45f, 0.3f, 0.15f, 1f), 8, 1.5f, 4f, 0.03f, 0.07f, 0.7f, 2f, 55, 0f)), 0.8f, 8);
            cfg.surfaceImpacts[2] = Entry(Impact("Impact_Metal", matSoft, matHard, matHardAdd,
                P("smoke", true, false, new Color(0.5f, 0.5f, 0.5f, 0.5f), new Color(0.5f, 0.5f, 0.5f, 0.5f), 2, 0.2f, 0.6f, 0.12f, 0.2f, 0.4f, -0.05f, 30, 1.5f),
                P("sparks", false, true, new Color(1f, 0.9f, 0.5f, 1f), new Color(1f, 0.55f, 0.15f, 1f), 10, 3f, 7f, 0.02f, 0.04f, 0.3f, 3f, 60, 0f)), 0.5f, 8);
            cfg.surfaceImpacts[3] = Entry(Impact("Impact_Glass", matSoft, matHard, matHardAdd,
                null,
                P("shards", false, true, new Color(0.85f, 0.97f, 1f, 1f), new Color(0.6f, 0.85f, 1f, 1f), 10, 1.5f, 4.5f, 0.03f, 0.07f, 0.6f, 3f, 70, 0f)), 0.7f, 8);
            cfg.surfaceImpacts[4] = Entry(Impact("Impact_Foliage", matSoft, matHard, matHardAdd,
                null,
                P("leaves", false, false, new Color(0.35f, 0.65f, 0.2f, 1f), new Color(0.2f, 0.45f, 0.12f, 1f), 8, 0.6f, 2f, 0.05f, 0.1f, 1.0f, 0.6f, 70, 0f)), 1.0f, 8);
            cfg.surfaceImpacts[5] = Entry(Impact("Impact_Flesh", matSoft, matHard, matHardAdd,
                P("mist", true, false, new Color(0.75f, 0.05f, 0.05f, 0.8f), new Color(0.5f, 0.02f, 0.02f, 0.6f), 5, 0.4f, 1.2f, 0.12f, 0.22f, 0.4f, 0.2f, 45, 1.5f),
                P("drops", false, false, new Color(0.8f, 0.05f, 0.05f, 1f), new Color(0.5f, 0.02f, 0.02f, 1f), 6, 1.5f, 3.5f, 0.02f, 0.04f, 0.5f, 2.5f, 50, 0f)), 0.6f, 8);

            cfg.muzzle = Entry(Impact("MuzzleFlash", matSoft, matHard, matHardAdd,
                P("smoke", true, false, new Color(0.6f, 0.6f, 0.6f, 0.4f), new Color(0.6f, 0.6f, 0.6f, 0.4f), 3, 0.2f, 0.6f, 0.08f, 0.15f, 0.35f, -0.05f, 20, 2f),
                P("flash", true, true, new Color(1f, 0.95f, 0.7f, 1f), new Color(1f, 0.7f, 0.2f, 1f), 1, 0f, 0.01f, 0.35f, 0.4f, 0.07f, 0f, 1, 0.5f)), 0.4f, 4);
            cfg.enemyHit = Entry(Impact("Hit_Enemy", matSoft, matHard, matHardAdd,
                P("mist", true, false, new Color(0.8f, 0.05f, 0.05f, 0.85f), new Color(0.55f, 0.02f, 0.02f, 0.6f), 5, 0.5f, 1.5f, 0.15f, 0.28f, 0.35f, 0.2f, 60, 1.5f),
                P("drops", false, false, new Color(0.85f, 0.1f, 0.1f, 1f), new Color(0.5f, 0.02f, 0.02f, 1f), 8, 1.5f, 4f, 0.025f, 0.05f, 0.5f, 2.5f, 65, 0f)), 0.6f, 8);
            cfg.hostageHit = Entry(Impact("Hit_Hostage", matSoft, matHard, matHardAdd,
                P("flash", true, true, new Color(1f, 0.55f, 0.1f, 1f), new Color(1f, 0.3f, 0.05f, 1f), 6, 0.5f, 2f, 0.15f, 0.3f, 0.35f, 0f, 60, 1.5f),
                null), 0.6f, 4);
            cfg.justiceHit = Entry(Impact("Hit_Justice", matSoft, matHard, matHardAdd,
                P("glow", true, true, new Color(1f, 0.9f, 0.3f, 1f), new Color(1f, 0.7f, 0.1f, 1f), 3, 0f, 0.3f, 0.5f, 0.8f, 0.25f, 0f, 1, 1.6f),
                P("sparks", false, true, new Color(1f, 0.95f, 0.5f, 1f), new Color(1f, 0.75f, 0.1f, 1f), 14, 2f, 6f, 0.03f, 0.06f, 0.5f, 1.5f, 80, 0f)), 0.8f, 4);

            var mark = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mark.name = "BulletMark";
            Object.DestroyImmediate(mark.GetComponent<Collider>());
            mark.transform.localScale = Vector3.one * 0.1f;
            var mr = mark.GetComponent<MeshRenderer>();
            mr.sharedMaterial = matMark;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            var markPrefab = PrefabUtility.SaveAsPrefabAsset(mark, Dir + "/BulletMark.prefab");
            Object.DestroyImmediate(mark);
            cfg.bulletMark = new FxEntry { prefab = markPrefab, lifetime = 8f, maxInstances = 24 };

            cfg.maxActiveParticles = 300;
            cfg.reduceMotionScale = 0.4f;
            var cfgAsset = ReplaceAsset(cfg, Dir + "/FxConfig.asset");

            var root = new GameObject("FxSystems");
            var sys = root.AddComponent<FxSystem>();
            var so = new SerializedObject(sys);
            so.FindProperty("config").objectReferenceValue = cfgAsset;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Dir + "/FxSystems.prefab");
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[FX] Built FX assets in " + Dir);
        }

        static T ReplaceAsset<T>(T obj, string path) where T : Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null && obj is ScriptableObject)
            {
                EditorUtility.CopySerialized(obj, existing);
                Object.DestroyImmediate(obj);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            AssetDatabase.CreateAsset(obj, path);
            return obj;
        }

        static FxEntry Entry(GameObject prefab, float lifetime, int max) =>
            new FxEntry { prefab = prefab, lifetime = lifetime, maxInstances = max };

        static Spec P(string name, bool soft, bool add, Color c0, Color c1, int count, float smin, float smax,
                      float zmin, float zmax, float life, float grav, float cone, float grow) =>
            new Spec { name = name, soft = soft, additive = add, c0 = c0, c1 = c1, count = count, speedMin = smin, speedMax = smax,
                       sizeMin = zmin, sizeMax = zmax, life = life, gravity = grav, cone = cone, grow = grow };

        static GameObject Impact(string name, Material soft, Material hard, Material hardAdd, Spec? a, Spec? b)
        {
            // Tham so vat lieu duoc chon theo (soft, additive) cua tung Spec; softAdd lay tu asset.
            var root = new GameObject(name);
            var softAdd = AssetDatabase.LoadAssetAtPath<Material>(Dir + "/Materials/FxSoftAdd.mat");
            if (a.HasValue) AddPs(root, a.Value, soft, softAdd, hard, hardAdd);
            if (b.HasValue) AddPs(root, b.Value, soft, softAdd, hard, hardAdd);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Dir + "/" + name + ".prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void AddPs(GameObject root, Spec s, Material soft, Material softAdd, Material hard, Material hardAdd)
        {
            var go = new GameObject(s.name);
            go.transform.SetParent(root.transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(s.life * 0.7f, s.life);
            main.startSpeed = new ParticleSystem.MinMaxCurve(s.speedMin, Mathf.Max(s.speedMin, s.speedMax));
            main.startSize = new ParticleSystem.MinMaxCurve(s.sizeMin, s.sizeMax);
            main.startColor = new ParticleSystem.MinMaxGradient(s.c0, s.c1);
            main.gravityModifier = s.gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = s.count;
            main.stopAction = ParticleSystemStopAction.None;

            var em = ps.emission;
            em.enabled = true;
            em.rateOverTime = 0;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)s.count) });

            var sh = ps.shape;
            sh.enabled = true;
            sh.shapeType = ParticleSystemShapeType.Cone;
            sh.angle = s.cone;
            sh.radius = 0.01f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = g;

            if (s.grow > 1f)
            {
                var sz = ps.sizeOverLifetime;
                sz.enabled = true;
                sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.4f), new Keyframe(1f, s.grow)));
            }

            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.sharedMaterial = s.soft ? (s.additive ? softAdd : soft) : (s.additive ? hardAdd : hard);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        static Texture2D MakeSoftTexture(string path)
        {
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                    float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    a *= a;
                    tex.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Material MakeMaterial(string path, Texture2D tex, bool additive)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
            m.shader = shader;
            if (tex != null) m.SetTexture("_BaseMap", tex);
            m.SetColor("_BaseColor", Color.white);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", additive ? 2f : 0f);
            m.SetFloat("_SrcBlend", 5f);
            m.SetFloat("_DstBlend", additive ? 1f : 10f);
            m.SetFloat("_SrcBlendAlpha", 1f);
            m.SetFloat("_DstBlendAlpha", additive ? 1f : 10f);
            m.SetFloat("_ZWrite", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3000;
            m.SetShaderPassEnabled("ShadowCaster", false);
            EditorUtility.SetDirty(m);
            return m;
        }
    }
}
