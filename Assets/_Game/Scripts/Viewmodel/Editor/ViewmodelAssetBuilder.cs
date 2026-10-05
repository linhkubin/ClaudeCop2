using System.Collections.Generic;
using System.IO;
using ClaudeCop.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ClaudeCop.Viewmodel.Editor
{
    /// <summary>
    /// Sinh toan bo viewmodel sung bang code: material, 3 prefab (Viewmodel_Pistol/Shotgun/MachineGun), AnimationClip, VM_Base.controller,
    /// 3 AnimatorOverrideController va ViewmodelConfig. Menu: ClaudeCop/Viewmodel/Build Viewmodel.
    /// Hop dong node (de thay model sau nay): Viewmodel_&lt;Kind&gt; (Animator) &gt; Pivot &gt; Body &gt; { Slide|Pump|Bolt, Magazine|ShellTube, Muzzle &gt; Flash }.
    /// </summary>
    public static class ViewmodelAssetBuilder
    {
        public const string Dir = "Assets/_Game/Prefabs/Viewmodel";
        public const string LayerName = "Viewmodel";
        const string FxTex = "Assets/_Game/Prefabs/FX/Textures/FxSoft.png";

        static readonly string[] States = { "Idle", "Fire", "Reload", "Equip", "DryFire" };

        struct Part { public string name; public Transform t; public Vector3 rest; }

        // ---------- Menu ----------

        [MenuItem("ClaudeCop/Viewmodel/Build Viewmodel")]
        public static void Build()
        {
            EnsureLayer();
            Directory.CreateDirectory(Dir + "/Materials");
            Directory.CreateDirectory(Dir + "/Animations");
            int layer = LayerMask.NameToLayer(LayerName);

            var mats = new Dictionary<string, Material>
            {
                ["metal"] = Lit("VM_Metal", new Color(0.17f, 0.18f, 0.2f), 0.55f, 0.6f),
                ["slide"] = Lit("VM_Slide", new Color(0.3f, 0.31f, 0.34f), 0.6f, 0.7f),
                ["grip"] = Lit("VM_Grip", new Color(0.09f, 0.09f, 0.1f), 0.2f, 0f),
                ["wood"] = Lit("VM_Wood", new Color(0.42f, 0.27f, 0.14f), 0.3f, 0f),
                ["olive"] = Lit("VM_Olive", new Color(0.23f, 0.27f, 0.2f), 0.3f, 0.2f),
                ["mag"] = Lit("VM_Mag", new Color(0.12f, 0.13f, 0.14f), 0.35f, 0.4f),
                ["accent"] = Lit("VM_Accent", new Color(0.85f, 0.65f, 0.15f), 0.4f, 0.2f),
            };
            var flashMat = FlashMaterial();

            var baseCtrl = BuildBaseController();
            var cfg = LoadOrCreate<ViewmodelConfig>(Dir + "/ViewmodelConfig.asset");
            cfg.weapons = new ViewmodelWeaponEntry[3];

            var kinds = new[] { WeaponKind.Pistol, WeaponKind.Shotgun, WeaponKind.MachineGun };
            for (int i = 0; i < kinds.Length; i++)
            {
                var kind = kinds[i];
                var root = new GameObject("Viewmodel_" + kind);
                var parts = BuildModel(kind, root, mats, flashMat);
                SetLayerRecursive(root.transform, layer);
                var anim = root.AddComponent<Animator>();
                anim.applyRootMotion = false;
                anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                var clips = BuildClips(kind, parts);
                var oc = new AnimatorOverrideController(baseCtrl) { name = "VM_" + kind };
                var ovr = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                foreach (var baseClip in baseCtrl.animationClips)
                {
                    string key = baseClip.name.Substring("VM_".Length);
                    ovr.Add(new KeyValuePair<AnimationClip, AnimationClip>(baseClip, clips[key]));
                }
                oc.ApplyOverrides(ovr);
                string ocPath = Dir + "/VM_" + kind + ".overrideController";
                AssetDatabase.DeleteAsset(ocPath);
                AssetDatabase.CreateAsset(oc, ocPath);
                anim.runtimeAnimatorController = oc;

                string prefabPath = Dir + "/Viewmodel_" + kind + ".prefab";
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Object.DestroyImmediate(root);

                var entry = new ViewmodelWeaponEntry
                {
                    kind = kind,
                    prefab = prefab,
                    fallbackReloadClipLength = clips["Reload"].length,
                    fireClipMinInterval = 0.25f,
                    recoilImpulseScale = kind == WeaponKind.Shotgun ? 1.5f : (kind == WeaponKind.MachineGun ? 0.55f : 1f),
                    recoilVisualScale = kind == WeaponKind.Shotgun ? 1.2f : 1f,
                };
                cfg.weapons[i] = entry;
            }

            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Viewmodel] Build xong: " + Dir);
        }

        /// <summary>Them ViewmodelRoot + ViewmodelCamera (Overlay) vao Camera Stack cua camera chinh trong scene dang mo.</summary>
        [MenuItem("ClaudeCop/Viewmodel/Add To Active Scene")]
        public static void AddToActiveScene()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<ViewmodelConfig>(Dir + "/ViewmodelConfig.asset");
            if (cfg == null) { Debug.LogError("[Viewmodel] Chua co ViewmodelConfig - chay Build Viewmodel truoc."); return; }
            var main = Camera.main;
            if (main == null) { Debug.LogError("[Viewmodel] Khong co Camera.main trong scene."); return; }
            int layer = LayerMask.NameToLayer(LayerName);
            if (layer < 0) { EnsureLayer(); layer = LayerMask.NameToLayer(LayerName); }

            // Camera overlay: con Main Camera, KHONG tag MainCamera.
            var existingCam = main.transform.Find("ViewmodelCamera");
            GameObject camGo = existingCam != null ? existingCam.gameObject : new GameObject("ViewmodelCamera");
            camGo.transform.SetParent(main.transform, false);
            camGo.transform.localPosition = Vector3.zero;
            camGo.transform.localRotation = Quaternion.identity;
            camGo.tag = "Untagged";
            camGo.layer = 0;
            var cam = Ensure<Camera>(camGo);
            cam.cullingMask = 1 << layer;
            cam.fieldOfView = cfg.overlayFov;
            cam.nearClipPlane = cfg.overlayNear;
            cam.farClipPlane = cfg.overlayFar;
            cam.clearFlags = CameraClearFlags.Depth;
            cam.allowHDR = false;
            cam.allowMSAA = true;
            cam.depth = main.depth + 1;
            var data = Ensure<UniversalAdditionalCameraData>(camGo);
            data.renderType = CameraRenderType.Overlay;
            data.renderPostProcessing = false;
            data.requiresDepthOption = CameraOverrideOption.Off;
            data.requiresColorOption = CameraOverrideOption.Off;
            var listener = camGo.GetComponent<AudioListener>();
            if (listener != null) Object.DestroyImmediate(listener);

            // Camera Stack cua Main Camera + loai layer Viewmodel khoi culling mask.
            var mainData = Ensure<UniversalAdditionalCameraData>(main.gameObject);
            mainData.renderType = CameraRenderType.Base;
            if (!mainData.cameraStack.Contains(cam)) mainData.cameraStack.Add(cam);
            main.cullingMask &= ~(1 << layer);

            // ViewmodelRoot + controller.
            var existingRoot = main.transform.Find("ViewmodelRoot");
            GameObject rootGo = existingRoot != null ? existingRoot.gameObject : new GameObject("ViewmodelRoot");
            rootGo.transform.SetParent(main.transform, false);
            rootGo.layer = layer;
            var ctrl = Ensure<ViewmodelController>(rootGo);
            var so = new SerializedObject(ctrl);
            so.FindProperty("config").objectReferenceValue = cfg;
            so.FindProperty("overlayCamera").objectReferenceValue = cam;
            var ts = Object.FindFirstObjectByType<ClaudeCop.Combat.TapShooter>();
            if (ts != null) so.FindProperty("tapShooter").objectReferenceValue = ts;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(main);
            EditorUtility.SetDirty(camGo);
            EditorUtility.SetDirty(rootGo);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(main.gameObject.scene);
            Debug.Log("[Viewmodel] Da them ViewmodelRoot + ViewmodelCamera (Overlay) vao " + main.gameObject.scene.name);
        }

        static T Ensure<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        // ---------- Layer ----------

        public static void EnsureLayer()
        {
            if (LayerMask.NameToLayer(LayerName) >= 0) return;
            var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tm.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
            {
                var p = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(p.stringValue)) { p.stringValue = LayerName; break; }
            }
            tm.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        static void SetLayerRecursive(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            for (int i = 0; i < t.childCount; i++) SetLayerRecursive(t.GetChild(i), layer);
        }

        // ---------- Material ----------

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a != null) return a;
            a = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        static Material Lit(string name, Color c, float smooth, float metal)
        {
            string path = Dir + "/Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c);
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Metallic", metal);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material FlashMaterial()
        {
            string path = Dir + "/Materials/VM_Flash.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")); AssetDatabase.CreateAsset(m, path); }
            m.shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(FxTex);
            if (tex != null) m.SetTexture("_BaseMap", tex);
            m.SetColor("_BaseColor", new Color(1f, 0.8f, 0.35f, 1f));
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 2f);
            m.SetFloat("_SrcBlend", 5f);
            m.SetFloat("_DstBlend", 1f);
            m.SetFloat("_SrcBlendAlpha", 1f);
            m.SetFloat("_DstBlendAlpha", 1f);
            m.SetFloat("_ZWrite", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3000;
            m.SetShaderPassEnabled("ShadowCaster", false);
            EditorUtility.SetDirty(m);
            return m;
        }

        // ---------- Model ----------

        static Transform Node(Transform parent, string name, Vector3 pos)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            return g.transform;
        }

        static Transform Box(Transform parent, string name, Vector3 pos, Vector3 size, Material m, Vector3? euler = null)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = size;
            if (euler.HasValue) g.transform.localEulerAngles = euler.Value;
            var r = g.GetComponent<MeshRenderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return g.transform;
        }

        /// <summary>Tru dat doc theo Z (tu pos, dai len, duong kinh dia).</summary>
        static Transform Cyl(Transform parent, string name, Vector3 pos, float len, float diameter, Material m)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            g.name = name;
            Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            g.transform.localScale = new Vector3(diameter, len * 0.5f, diameter);
            var r = g.GetComponent<MeshRenderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return g.transform;
        }

        static void MakeMuzzle(Transform body, Vector3 pos, Material flashMat, float flashSize)
        {
            var muzzle = Node(body, "Muzzle", pos);
            var flash = Node(muzzle, "Flash", Vector3.zero);
            for (int i = 0; i < 2; i++)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Quad);
                g.name = "Quad" + i;
                Object.DestroyImmediate(g.GetComponent<Collider>());
                g.transform.SetParent(flash, false);
                g.transform.localPosition = new Vector3(0f, 0f, 0.01f * i);
                g.transform.localEulerAngles = new Vector3(0f, 0f, 45f * i);
                g.transform.localScale = Vector3.one * flashSize * (i == 0 ? 1f : 0.7f);
                var r = g.GetComponent<MeshRenderer>();
                r.sharedMaterial = flashMat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
            flash.gameObject.SetActive(false);
        }

        static Dictionary<string, Part> BuildModel(WeaponKind kind, GameObject root, Dictionary<string, Material> m, Material flashMat)
        {
            var parts = new Dictionary<string, Part>();
            var pivot = Node(root.transform, "Pivot", Vector3.zero);
            var body = Node(pivot, "Body", Vector3.zero);
            parts["Pivot"] = new Part { name = "Pivot", t = pivot, rest = Vector3.zero };

            void Reg(string path, Transform t) { parts[path] = new Part { name = path, t = t, rest = t.localPosition }; }

            switch (kind)
            {
                case WeaponKind.Pistol:
                    Box(body, "Frame", new Vector3(0f, -0.004f, 0.06f), new Vector3(0.034f, 0.04f, 0.17f), m["metal"]);
                    Reg("Slide", Box(body, "Slide", new Vector3(0f, 0.026f, 0.075f), new Vector3(0.036f, 0.034f, 0.2f), m["slide"]));
                    Box(body, "SightRear", new Vector3(0f, 0.047f, 0.0f), new Vector3(0.012f, 0.008f, 0.01f), m["metal"]);
                    Box(body, "SightFront", new Vector3(0f, 0.047f, 0.165f), new Vector3(0.006f, 0.01f, 0.008f), m["accent"]);
                    Box(body, "Grip", new Vector3(0f, -0.058f, -0.005f), new Vector3(0.034f, 0.095f, 0.045f), m["grip"], new Vector3(-12f, 0f, 0f));
                    Reg("Magazine", Box(body, "Magazine", new Vector3(0f, -0.068f, 0.0f), new Vector3(0.027f, 0.098f, 0.034f), m["mag"], new Vector3(-12f, 0f, 0f)));
                    Box(body, "TriggerGuard", new Vector3(0f, -0.032f, 0.04f), new Vector3(0.008f, 0.006f, 0.05f), m["metal"]);
                    MakeMuzzle(body, new Vector3(0f, 0.026f, 0.18f), flashMat, 0.1f);
                    break;
                case WeaponKind.Shotgun:
                    Box(body, "Receiver", new Vector3(0f, 0f, 0.06f), new Vector3(0.042f, 0.054f, 0.16f), m["metal"]);
                    Cyl(body, "Barrel", new Vector3(0f, 0.018f, 0.31f), 0.36f, 0.03f, m["slide"]);
                    Reg("ShellTube", Cyl(body, "ShellTube", new Vector3(0f, -0.014f, 0.25f), 0.28f, 0.026f, m["metal"]));
                    Reg("Pump", Box(body, "Pump", new Vector3(0f, -0.014f, 0.2f), new Vector3(0.046f, 0.04f, 0.11f), m["wood"]));
                    Box(body, "Stock", new Vector3(0f, -0.018f, -0.15f), new Vector3(0.038f, 0.075f, 0.22f), m["wood"], new Vector3(6f, 0f, 0f));
                    Box(body, "Grip", new Vector3(0f, -0.055f, -0.02f), new Vector3(0.032f, 0.08f, 0.04f), m["wood"], new Vector3(-14f, 0f, 0f));
                    Box(body, "SightFront", new Vector3(0f, 0.037f, 0.47f), new Vector3(0.006f, 0.01f, 0.008f), m["accent"]);
                    MakeMuzzle(body, new Vector3(0f, 0.018f, 0.5f), flashMat, 0.16f);
                    break;
                case WeaponKind.MachineGun:
                    Box(body, "Receiver", new Vector3(0f, 0f, 0.12f), new Vector3(0.046f, 0.06f, 0.3f), m["olive"]);
                    Cyl(body, "Shroud", new Vector3(0f, 0.012f, 0.35f), 0.18f, 0.04f, m["metal"]);
                    Cyl(body, "Barrel", new Vector3(0f, 0.012f, 0.47f), 0.3f, 0.018f, m["slide"]);
                    Cyl(body, "FlashHider", new Vector3(0f, 0.012f, 0.6f), 0.05f, 0.028f, m["metal"]);
                    Reg("Magazine", Box(body, "Magazine", new Vector3(0f, -0.075f, 0.13f), new Vector3(0.04f, 0.105f, 0.075f), m["mag"]));
                    Reg("Bolt", Box(body, "Bolt", new Vector3(0.029f, 0.026f, 0.1f), new Vector3(0.012f, 0.014f, 0.045f), m["accent"]));
                    Box(body, "Stock", new Vector3(0f, -0.012f, -0.15f), new Vector3(0.036f, 0.07f, 0.22f), m["olive"], new Vector3(5f, 0f, 0f));
                    Box(body, "Grip", new Vector3(0f, -0.06f, -0.01f), new Vector3(0.032f, 0.08f, 0.04f), m["grip"], new Vector3(-14f, 0f, 0f));
                    Box(body, "CarryHandle", new Vector3(0f, 0.04f, 0.15f), new Vector3(0.012f, 0.014f, 0.1f), m["metal"]);
                    MakeMuzzle(body, new Vector3(0f, 0.012f, 0.63f), flashMat, 0.17f);
                    break;
            }
            return parts;
        }

        // ---------- Animation ----------

        static void Smooth(AnimationCurve c) { for (int i = 0; i < c.length; i++) c.SmoothTangents(i, 0f); }

        /// <summary>Curve vi tri/goc: keys (t, offset) cong voi rest. Ghi du 3 truc.</summary>
        static void Curve(AnimationClip clip, string path, bool rotation, Vector3 rest, params (float t, Vector3 v)[] keys)
        {
            var cx = new AnimationCurve(); var cy = new AnimationCurve(); var cz = new AnimationCurve();
            foreach (var k in keys)
            {
                var v = rest + k.v;
                cx.AddKey(k.t, v.x); cy.AddKey(k.t, v.y); cz.AddKey(k.t, v.z);
            }
            Smooth(cx); Smooth(cy); Smooth(cz);
            string p = rotation ? "localEulerAnglesRaw" : "m_LocalPosition";
            clip.SetCurve(path, typeof(Transform), p + ".x", cx);
            clip.SetCurve(path, typeof(Transform), p + ".y", cy);
            clip.SetCurve(path, typeof(Transform), p + ".z", cz);
        }

        static string ChildPath(string part) { return part == "Pivot" ? "Pivot" : "Pivot/Body/" + part; }

        static AnimationClip NewClip(string name, bool loop)
        {
            var c = new AnimationClip { name = name, frameRate = 60f };
            var s = AnimationUtility.GetAnimationClipSettings(c);
            s.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(c, s);
            return c;
        }

        static Vector3 V(float x, float y, float z) { return new Vector3(x, y, z); }

        static AnimationClip Finish(AnimationClip c, string weapon, string state)
        {
            string path = Dir + "/Animations/" + weapon + "_" + state + ".anim";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(c, path);
            return c;
        }

        static Dictionary<string, AnimationClip> BuildClips(WeaponKind kind, Dictionary<string, Part> parts)
        {
            string w = kind.ToString();
            var d = new Dictionary<string, AnimationClip>();
            Vector3 z3 = Vector3.zero;

            // Idle (loop, nhun tho nhe) - chung moi sung
            var idle = NewClip("VM_Idle", true);
            Curve(idle, "Pivot", false, z3, (0f, z3), (1f, V(0f, 0.0025f, 0f)), (2f, z3));
            Curve(idle, "Pivot", true, z3, (0f, z3), (1f, V(0.6f, 0f, 0.3f)), (2f, z3));
            d["Idle"] = Finish(idle, w, "Idle");

            // Equip: nang sung tu duoi len, hoi vuot
            var equip = NewClip("VM_Equip", false);
            Curve(equip, "Pivot", false, z3, (0f, V(0.02f, -0.16f, -0.04f)), (0.22f, V(0f, 0.008f, 0f)), (0.34f, z3));
            Curve(equip, "Pivot", true, z3, (0f, V(32f, 0f, -8f)), (0.22f, V(-2.5f, 0f, 0f)), (0.34f, z3));
            d["Equip"] = Finish(equip, w, "Equip");

            // DryFire: click kho
            var dry = NewClip("VM_DryFire", false);
            Curve(dry, "Pivot", false, z3, (0f, z3), (0.03f, V(0f, 0.002f, -0.008f)), (0.14f, z3));
            Curve(dry, "Pivot", true, z3, (0f, z3), (0.03f, V(-1.2f, 0f, 0f)), (0.14f, z3));
            d["DryFire"] = Finish(dry, w, "DryFire");

            var fire = NewClip("VM_Fire", false);
            var reload = NewClip("VM_Reload", false);

            switch (kind)
            {
                case WeaponKind.Pistol:
                {
                    var slide = parts["Slide"]; var mag = parts["Magazine"];
                    Curve(fire, "Pivot", false, z3, (0f, z3), (0.03f, V(0f, 0.004f, -0.02f)), (0.16f, z3));
                    Curve(fire, "Pivot", true, z3, (0f, z3), (0.03f, V(-5f, 0f, 0f)), (0.16f, z3));
                    Curve(fire, ChildPath("Slide"), false, slide.rest, (0f, z3), (0.03f, V(0f, 0f, -0.045f)), (0.12f, z3));

                    // Reload 1.0 s: nghieng sung, slide lui, bang dan roi xuong, nhet bang moi, slide ve
                    Curve(reload, "Pivot", false, z3, (0f, z3), (0.12f, V(-0.01f, 0.02f, -0.02f)), (0.85f, V(-0.01f, 0.02f, -0.02f)), (1f, z3));
                    Curve(reload, "Pivot", true, z3, (0f, z3), (0.12f, V(12f, 6f, 18f)), (0.85f, V(10f, 4f, 15f)), (1f, z3));
                    Curve(reload, ChildPath("Slide"), false, slide.rest, (0f, z3), (0.14f, V(0f, 0f, -0.045f)), (0.78f, V(0f, 0f, -0.045f)), (0.88f, z3), (1f, z3));
                    Curve(reload, ChildPath("Magazine"), false, mag.rest, (0f, z3), (0.22f, z3), (0.36f, V(0f, -0.16f, 0f)), (0.5f, V(0f, -0.16f, 0f)), (0.7f, V(0f, -0.02f, 0f)), (0.76f, z3), (1f, z3));
                    break;
                }
                case WeaponKind.Shotgun:
                {
                    var pump = parts["Pump"]; var tube = parts["ShellTube"];
                    Curve(fire, "Pivot", false, z3, (0f, z3), (0.05f, V(0f, 0.01f, -0.06f)), (0.28f, V(0f, 0.003f, -0.01f)), (0.5f, z3));
                    Curve(fire, "Pivot", true, z3, (0f, z3), (0.05f, V(-9f, 0f, 2f)), (0.28f, V(-2f, 0f, 0f)), (0.5f, z3));
                    Curve(fire, ChildPath("Pump"), false, pump.rest, (0f, z3), (0.14f, z3), (0.26f, V(0f, 0f, -0.08f)), (0.38f, z3), (0.5f, z3));

                    // Reload 1.2 s: nghieng, nhet vo (ong dan rung) + bom 2 lan
                    Curve(reload, "Pivot", false, z3, (0f, z3), (0.14f, V(-0.015f, 0.02f, -0.03f)), (1.05f, V(-0.015f, 0.02f, -0.03f)), (1.2f, z3));
                    Curve(reload, "Pivot", true, z3, (0f, z3), (0.14f, V(9f, 4f, 12f)), (1.05f, V(7f, 3f, 10f)), (1.2f, z3));
                    Curve(reload, ChildPath("ShellTube"), false, tube.rest, (0f, z3), (0.2f, z3), (0.28f, V(0f, 0f, -0.01f)), (0.36f, z3), (0.44f, V(0f, 0f, -0.01f)), (0.52f, z3), (1.2f, z3));
                    Curve(reload, ChildPath("Pump"), false, pump.rest, (0f, z3), (0.6f, z3), (0.72f, V(0f, 0f, -0.08f)), (0.84f, z3), (0.92f, z3), (1.0f, V(0f, 0f, -0.08f)), (1.12f, z3), (1.2f, z3));
                    break;
                }
                case WeaponKind.MachineGun:
                {
                    var bolt = parts["Bolt"]; var mag = parts["Magazine"];
                    Curve(fire, "Pivot", false, z3, (0f, z3), (0.025f, V(0f, 0.003f, -0.018f)), (0.1f, z3));
                    Curve(fire, "Pivot", true, z3, (0f, z3), (0.025f, V(-2.5f, 0f, 0f)), (0.1f, z3));
                    Curve(fire, ChildPath("Bolt"), false, bolt.rest, (0f, z3), (0.03f, V(0f, 0f, -0.03f)), (0.1f, z3));

                    // Reload 1.4 s: thao hop dan xuong, lap hop moi, keo co nap
                    Curve(reload, "Pivot", false, z3, (0f, z3), (0.14f, V(-0.012f, 0.02f, -0.03f)), (1.25f, V(-0.012f, 0.02f, -0.03f)), (1.4f, z3));
                    Curve(reload, "Pivot", true, z3, (0f, z3), (0.14f, V(7f, 3f, 11f)), (1.25f, V(6f, 3f, 9f)), (1.4f, z3));
                    Curve(reload, ChildPath("Magazine"), false, mag.rest, (0f, z3), (0.2f, z3), (0.42f, V(0.03f, -0.18f, 0f)), (0.6f, V(0.03f, -0.18f, 0f)), (0.62f, V(-0.02f, -0.18f, 0f)), (0.9f, V(0f, -0.02f, 0f)), (0.98f, z3), (1.4f, z3));
                    Curve(reload, ChildPath("Bolt"), false, bolt.rest, (0f, z3), (1.0f, z3), (1.12f, V(0f, 0f, -0.07f)), (1.28f, z3), (1.4f, z3));
                    break;
                }
            }
            d["Fire"] = Finish(fire, w, "Fire");
            d["Reload"] = Finish(reload, w, "Reload");
            return d;
        }

        static AnimatorController BuildBaseController()
        {
            // Clip nen (placeholder, rong) - ten VM_<State>; override moi sung thay bang clip rieng.
            string ctrlPath = Dir + "/VM_Base.controller";
            AssetDatabase.DeleteAsset(ctrlPath);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            var sm = ctrl.layers[0].stateMachine;
            foreach (var st in sm.states) sm.RemoveState(st.state);

            AnimatorState idle = null;
            foreach (var s in States)
            {
                var clip = NewClip("VM_" + s, s == "Idle");
                clip.SetCurve("Pivot", typeof(Transform), "m_LocalPosition.x", AnimationCurve.Constant(0f, 1f, 0f));
                Directory.CreateDirectory(Dir + "/Animations/Base"); string cp = Dir + "/Animations/Base/VM_" + s + ".anim";
                AssetDatabase.DeleteAsset(cp);
                AssetDatabase.CreateAsset(clip, cp);
                var state = sm.AddState(s);
                state.motion = clip;
                state.writeDefaultValues = true;
                if (s == "Idle") { idle = state; sm.defaultState = state; }
            }
            foreach (var st in sm.states)
            {
                if (st.state == idle) continue;
                var t = st.state.AddTransition(idle);
                t.hasExitTime = true;
                t.exitTime = 1f;
                t.duration = 0.05f;
                t.hasFixedDuration = true;
            }
            EditorUtility.SetDirty(ctrl);
            AssetDatabase.SaveAssets();
            return ctrl;
        }
    }
}
