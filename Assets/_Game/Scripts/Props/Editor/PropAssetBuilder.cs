using System.IO;
using ClaudeCop.Core;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.Props.Editor
{
    /// <summary>Sinh vat lieu, PropConfig, prefab Props (hop, lon, thung no, manh vo, kinh, bui kinh, PropSystems). Chay lai an toan.</summary>
    public static class PropAssetBuilder
    {
        const string Dir = GlassShardBuilder.Dir;

        [MenuItem("ClaudeCop/Props/Build Prop Assets")]
        public static void Build()
        {
            Directory.CreateDirectory(Dir + "/Data");
            Directory.CreateDirectory(Dir + "/Materials");

            var cfg = AssetDatabase.LoadAssetAtPath<PropConfig>(Dir + "/Data/PropConfig.asset");
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<PropConfig>();
                AssetDatabase.CreateAsset(cfg, Dir + "/Data/PropConfig.asset");
            }

            var wood = LitMaterial("Prop_Wood", new Color(0.55f, 0.38f, 0.2f));
            var metal = LitMaterial("Prop_Metal", new Color(0.6f, 0.62f, 0.65f));
            var red = LitMaterial("Prop_RedBarrel", new Color(0.75f, 0.08f, 0.06f));

            // Hop
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "Prop_Box";
            box.transform.localScale = Vector3.one * 0.6f;
            box.GetComponent<MeshRenderer>().sharedMaterial = wood;
            box.AddComponent<Rigidbody>().mass = 1f;
            SetEnum(box.AddComponent<SurfaceMaterialTag>(), SurfaceMaterial.Wood);
            WireProp(box.AddComponent<PhysicsProp>(), cfg);
            Save(box, "Prop_Box");

            // Lon
            var can = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            can.name = "Prop_Can";
            can.transform.localScale = new Vector3(0.12f, 0.08f, 0.12f);
            can.GetComponent<MeshRenderer>().sharedMaterial = metal;
            var canRb = can.AddComponent<Rigidbody>(); canRb.mass = 0.2f;
            SetEnum(can.AddComponent<SurfaceMaterialTag>(), SurfaceMaterial.Metal);
            WireProp(can.AddComponent<PhysicsProp>(), cfg);
            Save(can, "Prop_Can");

            // Manh vo thung (4 manh)
            var debris = new GameObject("Barrel_Debris");
            Vector3[] offs = { new Vector3(-0.15f, 0.15f, 0f), new Vector3(0.15f, 0.2f, 0.05f), new Vector3(0f, 0.55f, -0.1f), new Vector3(0.1f, 0.75f, 0.1f) };
            Vector3[] sizes = { new Vector3(0.3f, 0.3f, 0.12f), new Vector3(0.28f, 0.35f, 0.1f), new Vector3(0.3f, 0.25f, 0.1f), new Vector3(0.25f, 0.2f, 0.1f) };
            for (int i = 0; i < offs.Length; i++)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                p.name = "Piece_" + i;
                p.transform.SetParent(debris.transform, false);
                p.transform.localPosition = offs[i]; p.transform.localScale = sizes[i];
                p.GetComponent<MeshRenderer>().sharedMaterial = i % 2 == 0 ? red : metal;
                p.AddComponent<Rigidbody>().mass = 0.5f;
            }
            var debrisPrefab = Save(debris, "Barrel_Debris");

            // Thung no
            var barrel = new GameObject("Prop_Barrel");
            var vis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            vis.name = "Visual";
            vis.transform.SetParent(barrel.transform, false);
            vis.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            vis.transform.localScale = new Vector3(0.6f, 0.45f, 0.6f);
            vis.GetComponent<MeshRenderer>().sharedMaterial = red;
            SetEnum(barrel.AddComponent<SurfaceMaterialTag>(), SurfaceMaterial.Metal);
            var eb = barrel.AddComponent<ExplosiveBarrel>();
            var so = new SerializedObject(eb);
            so.FindProperty("config").objectReferenceValue = cfg;
            so.FindProperty("visual").objectReferenceValue = vis;
            so.FindProperty("debrisPrefab").objectReferenceValue = debrisPrefab;
            var colls = so.FindProperty("colliders"); colls.arraySize = 1;
            colls.GetArrayElementAtIndex(0).objectReferenceValue = vis.GetComponent<Collider>();
            so.ApplyModifiedPropertiesWithoutUndo();
            Save(barrel, "Prop_Barrel");

            // Kinh
            var shards = GlassShardBuilder.Build(GlassShardBuilder.DefaultShards);
            var dust = BuildGlassDust();
            var glass = new GameObject("Prop_Glass");
            var pane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            pane.name = "Pane";
            Object.DestroyImmediate(pane.GetComponent<Collider>());
            pane.transform.SetParent(glass.transform, false);
            pane.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // nhin tu -Z ra +Z (huong camera)
            var pr = pane.GetComponent<MeshRenderer>();
            pr.sharedMaterial = GlassMaterial();
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var bc = glass.AddComponent<BoxCollider>();
            bc.size = new Vector3(1f, 1f, 0.05f);
            SetEnum(glass.AddComponent<SurfaceMaterialTag>(), SurfaceMaterial.Glass);
            var bg = glass.AddComponent<BreakableGlass>();
            var gso = new SerializedObject(bg);
            gso.FindProperty("config").objectReferenceValue = cfg;
            gso.FindProperty("shardsPrefab").objectReferenceValue = shards;
            gso.FindProperty("dustPrefab").objectReferenceValue = dust;
            gso.FindProperty("visual").objectReferenceValue = pane;
            var gc = gso.FindProperty("colliders"); gc.arraySize = 1;
            gc.GetArrayElementAtIndex(0).objectReferenceValue = bc;
            gso.ApplyModifiedPropertiesWithoutUndo();
            Save(glass, "Prop_Glass");

            // PropSystems
            var sys = new GameObject("PropSystems");
            var ps = sys.AddComponent<PropSystem>();
            var sso = new SerializedObject(ps);
            sso.FindProperty("config").objectReferenceValue = cfg;
            var pw = sso.FindProperty("prewarmPrefabs"); pw.arraySize = 2;
            pw.GetArrayElementAtIndex(0).objectReferenceValue = debrisPrefab;
            pw.GetArrayElementAtIndex(1).objectReferenceValue = shards;
            sso.ApplyModifiedPropertiesWithoutUndo();
            Save(sys, "PropSystems");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Props] Built prop assets in " + Dir);
        }

        static void WireProp(PhysicsProp p, PropConfig cfg)
        {
            var so = new SerializedObject(p);
            so.FindProperty("config").objectReferenceValue = cfg;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetEnum(SurfaceMaterialTag tag, SurfaceMaterial m)
        {
            var so = new SerializedObject(tag);
            so.FindProperty("material").enumValueIndex = (int)m;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject Save(GameObject go, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, Dir + "/" + name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject BuildGlassDust()
        {
            var go = new GameObject("Fx_GlassDust");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false; main.playOnAwake = true; main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.85f, 0.95f, 1f, 0.7f), new Color(1f, 1f, 1f, 0.4f));
            main.gravityModifier = 0.15f;
            main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission;
            em.rateOverTime = 0f;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });
            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 35f; sh.radius = 0.25f;
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Prefabs/FX/Materials/FxSoft.mat");
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return Save(go, "Fx_GlassDust");
        }

        public static Material LitMaterial(string name, Color color)
        {
            string path = Dir + "/Materials/" + name + ".mat";
            Directory.CreateDirectory(Dir + "/Materials");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", 0.25f);
            ApplyLitRules(m);
            EditorUtility.SetDirty(m);
            return m;
        }

        // Luat T-404: tat Specular Highlights + Environment Reflections.
        static void ApplyLitRules(Material m)
        {
            m.SetFloat("_SpecularHighlights", 0f);
            m.SetFloat("_EnvironmentReflections", 0f);
            m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        }

        public static Material GlassMaterial()
        {
            string path = Dir + "/Materials/Prop_Glass.mat";
            Directory.CreateDirectory(Dir + "/Materials");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", new Color(0.7f, 0.9f, 1f, 0.35f));
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_Cull", 0f);
            m.SetFloat("_Smoothness", 0.5f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            ApplyLitRules(m);
            EditorUtility.SetDirty(m);
            return m;
        }
    }
}
