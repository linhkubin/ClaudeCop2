using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ClaudeCop.Ads;

namespace ClaudeCop.UI.Editor
{
    /// <summary>Tao sprite thu tuc, UIConfig, prefab UI va Sandbox scene. Menu: ClaudeCop/UI/Build M1 Assets.</summary>
    public static partial class UIAssetBuilder
    {
        const string Root = "Assets/_Game/UI";
        const string SpriteDir = Root + "/Sprites";
        const string PrefabDir = "Assets/_Game/Prefabs/UI";
        const string SandboxScene = "Assets/_Game/Scenes/Sandbox/ui-coder.unity";

        static TMP_FontAsset font;

        [MenuItem("ClaudeCop/UI/Build M1 Assets")]
        public static void BuildAll() { Build(true); }

        /// <summary>Chi tao lai sprite + prefab, khong dong vao scene dang mo.</summary>
        [MenuItem("ClaudeCop/UI/Build Prefabs Only")]
        public static void BuildPrefabsOnly() { Build(false); }

        static void Build(bool sandbox)
        {
            Directory.CreateDirectory(SpriteDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory("Assets/_Game/Scenes/Sandbox");
            font = GetUiFont();

            var ring = MakeSprite("Ring", 256, (x, y) => RingAlpha(x, y, 0.80f, 0.97f));
            var circle = MakeSprite("Circle", 128, (x, y) => Mathf.Clamp01((1f - Mathf.Sqrt(x * x + y * y)) * 64f));
            var heart = MakeSprite("Heart", 128, HeartAlpha);
            var white = MakeSprite("White", 8, (x, y) => 1f);

            var cfg = AssetDatabase.LoadAssetAtPath<UIConfig>(Root + "/UIConfig.asset");
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<UIConfig>();
                AssetDatabase.CreateAsset(cfg, Root + "/UIConfig.asset");
            }

            BuildHud(cfg, heart, circle);
            BuildReticle(cfg, ring);
            BuildDamageFlash(cfg, white);
            BuildEndPanel("WinPanel", "MISSION COMPLETE", new Color(0.1f, 0.35f, 0.15f, 0.85f), typeof(WinView), white, circle);
            BuildEndPanel("GameOverPanel", "GAME OVER", new Color(0.4f, 0.05f, 0.05f, 0.85f), typeof(GameOverView), white, circle);
            BuildAdOverlay(white);
            AssetDatabase.SaveAssets();
            BuildGameplayUI();
            AssetDatabase.SaveAssets();
            if (sandbox) BuildSandbox();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UIAssetBuilder] Build xong.");
        }

        // ---------- sprites ----------
        static float RingAlpha(float x, float y, float inner, float outer)
        {
            float r = Mathf.Sqrt(x * x + y * y);
            float a = Mathf.Clamp01((r - inner) * 128f) * Mathf.Clamp01((outer - r) * 128f);
            return a;
        }

        static float HeartAlpha(float x, float y)
        {
            // x,y trong [-1,1]; phuong trinh trai tim an
            float px = x * 1.25f, py = (y + 0.1f) * 1.3f;
            float f = Mathf.Pow(px * px + py * py - 1f, 3f) - px * px * py * py * py;
            return Mathf.Clamp01(-f * 40f);
        }

        static Sprite MakeSprite(string name, int size, Func<float, float, float> alpha)
        {
            string path = SpriteDir + "/" + name + ".png";
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            for (int j = 0; j < size; j++)
                for (int i = 0; i < size; i++)
                {
                    float x = (i + 0.5f) / size * 2f - 1f, y = (j + 0.5f) / size * 2f - 1f;
                    px[j * size + i] = new Color(1f, 1f, 1f, alpha(x, y));
                }
            tex.SetPixels(px); tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ---------- helpers ----------
        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }

        static void Anchor(RectTransform r, Vector2 anchor, Vector2 size, Vector2 pos)
        {
            r.anchorMin = r.anchorMax = anchor; r.pivot = anchor; r.sizeDelta = size; r.anchoredPosition = pos;
        }

        static Image NewImage(string name, Transform parent, Sprite sprite, Color color, bool raycast = false)
        {
            var r = NewRect(name, parent);
            var img = r.gameObject.AddComponent<Image>();
            img.sprite = sprite; img.color = color; img.raycastTarget = raycast;
            return img;
        }

        static TMP_Text NewText(string name, Transform parent, string text, int size, TextAlignmentOptions align, Color color)
        {
            var r = NewRect(name, parent);
            var t = r.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.text = text; t.fontSize = size; t.alignment = align; t.color = color;
            t.textWrappingMode = TextWrappingModes.NoWrap; t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>Man doc: chu lon tu thu nho (toi 40%) neu khong vua khung.</summary>
        static void AutoSize(TMP_Text t)
        {
            t.enableAutoSizing = true; t.fontSizeMax = t.fontSize; t.fontSizeMin = Mathf.Floor(t.fontSize * 0.4f);
        }

        static Button NewButton(string name, Transform parent, Sprite bg, Color color, string label, int labelSize, out TMP_Text labelText)
        {
            var img = NewImage(name, parent, bg, color, true);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            labelText = NewText("Label", img.transform, label, labelSize, TextAlignmentOptions.Center, Color.white);
            Stretch(labelText.rectTransform);
            return btn;
        }

        static GameObject NewCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = sortingOrder;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1080, 1920);
            s.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            s.matchWidthOrHeight = 0f; // man hinh doc: khop chieu rong
            return go;
        }

        static void Set(UnityEngine.Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError("Missing field " + field + " on " + target.GetType().Name); return; }
            switch (value)
            {
                case UnityEngine.Object o: p.objectReferenceValue = o; break;
                case float f: p.floatValue = f; break;
                case bool b: p.boolValue = b; break;
                case int n: p.intValue = n; break;
                case string str: p.stringValue = str; break;
                case UnityEngine.Object[] arr:
                    p.arraySize = arr.Length;
                    for (int i = 0; i < arr.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = arr[i];
                    break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject Save(GameObject go, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + "/" + name + ".prefab");
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        // ---------- prefabs ----------
        static void BuildHud(UIConfig cfg, Sprite heart, Sprite circle)
        {
            var root = NewCanvas("HUD", 0);
            var safe = NewRect("SafeArea", root.transform);
            Stretch(safe); safe.gameObject.AddComponent<SafeAreaPanel>();

            var reticleLayer = NewRect("ReticleLayer", root.transform);   // ngoai safe area: vong bam toan man hinh
            Stretch(reticleLayer);

            var score = NewText("ScoreText", safe, "0", 72, TextAlignmentOptions.TopLeft, Color.white);
            Anchor(score.rectTransform, new Vector2(0, 1), new Vector2(600, 100), new Vector2(40, -30));

            var heartsRoot = NewRect("Hearts", safe);
            Anchor(heartsRoot, new Vector2(1, 1), new Vector2(360, 110), new Vector2(-40, -30));
            var hl = heartsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 10; hl.childAlignment = TextAnchor.UpperRight;
            hl.childControlWidth = hl.childControlHeight = false; hl.childForceExpandWidth = hl.childForceExpandHeight = false;
            var hearts = new UnityEngine.Object[3];
            for (int i = 0; i < 3; i++)
            {
                var h = NewImage("Heart" + (i + 1), heartsRoot, heart, cfg.heartFull);
                h.rectTransform.sizeDelta = new Vector2(100, 100);
                hearts[i] = h;
            }

            var weaponIcon = NewImage("WeaponIcon", safe, circle, new Color(1, 1, 1, 0.9f));
            Anchor(weaponIcon.rectTransform, new Vector2(0, 0), new Vector2(110, 110), new Vector2(40, 40));
            weaponIcon.enabled = false;
            var weaponLabel = NewText("WeaponLabel", safe, "Pistol", 40, TextAlignmentOptions.BottomLeft, Color.white);
            Anchor(weaponLabel.rectTransform, new Vector2(0, 0), new Vector2(400, 60), new Vector2(40, 170));
            var ammo = NewText("AmmoText", safe, "6 / 6", 80, TextAlignmentOptions.BottomLeft, Color.white);
            Anchor(ammo.rectTransform, new Vector2(0, 0), new Vector2(500, 110), new Vector2(170, 40));

            var reload = NewButton("ReloadButton", safe, circle, new Color(0.15f, 0.45f, 0.9f, 0.9f), "RELOAD", 34, out var reloadLabel);
            Anchor((RectTransform)reload.transform, new Vector2(1, 0), new Vector2(180, 180), new Vector2(-50, 50));

            var view = root.AddComponent<HudView>();
            Set(view, "config", cfg);
            Set(view, "scoreText", score);
            Set(view, "hearts", hearts);
            Set(view, "ammoText", ammo);
            Set(view, "weaponLabel", weaponLabel);
            Set(view, "weaponIcon", weaponIcon);
            Set(view, "reloadButton", reload);
            Set(view, "reloadLabel", reloadLabel);
            Set(view, "reticleRoot", reticleLayer);
            Save(root, "HUD");
        }

        static void BuildReticle(UIConfig cfg, Sprite ring)
        {
            var go = new GameObject("TargetReticle", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(120, 120);
            var img = go.AddComponent<Image>();
            img.sprite = ring; img.raycastTarget = false; img.color = cfg.reticleGreen;
            var v = go.AddComponent<TargetReticleView>();
            Set(v, "config", cfg);
            Set(v, "ring", img);
            Save(go, "TargetReticle");
        }

        static void BuildDamageFlash(UIConfig cfg, Sprite white)
        {
            var root = NewCanvas("DamageFlash", 10);
            var cg = root.AddComponent<CanvasGroup>();
            Destroy(root.GetComponent<GraphicRaycaster>());
            var img = NewImage("Red", root.transform, white, new Color(1f, 0.05f, 0.05f, 1f));
            Stretch(img.rectTransform);
            var v = root.AddComponent<DamageFlashView>();
            Set(v, "config", cfg);
            Set(v, "group", cg);
            Save(root, "DamageFlash");
        }

        static void BuildEndPanel(string name, string title, Color tint, Type viewType, Sprite white, Sprite circle)
        {
            var root = NewCanvas(name, 20);
            var content = NewImage("Content", root.transform, white, tint, true);
            Stretch(content.rectTransform);
            var t = NewText("Title", content.transform, title, 120, TextAlignmentOptions.Center, Color.white);
            Anchor(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1000, 160), new Vector2(0, 140));
            AutoSize(t);
            var score = NewText("ScoreText", content.transform, "SCORE  0", 72, TextAlignmentOptions.Center, Color.white);
            Anchor(score.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1000, 100), new Vector2(0, 0));
            var btn = NewButton("RestartButton", content.transform, white, new Color(0.15f, 0.45f, 0.9f, 1f), "RESTART", 56, out _);
            Anchor((RectTransform)btn.transform, new Vector2(0.5f, 0.5f), new Vector2(420, 140), new Vector2(0, -200));
            var v = root.AddComponent(viewType);
            Set(v, "content", content.gameObject);
            Set(v, "scoreText", score);
            Set(v, "restartButton", btn);
            Save(root, name);
        }

        static void BuildAdOverlay(Sprite white)
        {
            var root = NewCanvas("FakeAdOverlay", 100);
            var content = NewImage("Content", root.transform, white, new Color(0f, 0f, 0f, 0.92f), true);
            Stretch(content.rectTransform);
            var title = NewText("Title", content.transform, "QUẢNG CÁO", 100, TextAlignmentOptions.Center, Color.white);
            Anchor(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1000, 140), new Vector2(0, 160));
            AutoSize(title);
            var count = NewText("Countdown", content.transform, "3", 220, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.2f));
            Anchor(count.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(600, 260), new Vector2(0, -80));
            var v = root.AddComponent<FakeRewardedAd>();
            Set(v, "content", content.gameObject);
            Set(v, "countdownText", count);
            Set(v, "titleText", title);
            Save(root, "FakeAdOverlay");
        }

        // Prefab gop cho scene gameplay: HUD + DamageFlash + Win + GameOver + presenters + EventSystem.
        // Khong gom FakeAdOverlay (RevivePopup T-311 se them).
        static void BuildGameplayUI()
        {
            T Inst<T>(Transform parent, string n) where T : Component
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + n + ".prefab");
                var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent);
                return go.GetComponent<T>();
            }

            var root = new GameObject("GameplayUI");
            var hud = Inst<HudView>(root.transform, "HUD");
            var flash = Inst<DamageFlashView>(root.transform, "DamageFlash");
            var win = Inst<WinView>(root.transform, "WinPanel");
            var over = Inst<GameOverView>(root.transform, "GameOverPanel");

            var es = new GameObject("EventSystem", typeof(EventSystem));
            es.transform.SetParent(root.transform, false);
            es.AddComponent(FindInputModuleType() ?? typeof(StandaloneInputModule));

            var hp = root.AddComponent<HudPresenter>(); Set(hp, "view", hud);
            var rp = root.AddComponent<TargetReticlePresenter>();
            Set(rp, "hud", hud);
            Set(rp, "reticlePrefab", AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/TargetReticle.prefab").GetComponent<TargetReticleView>());
            var fp = root.AddComponent<DamageFlashPresenter>(); Set(fp, "view", flash);
            var ep = root.AddComponent<EndScreenPresenter>(); Set(ep, "winView", win); Set(ep, "gameOverView", over);
            Save(root, "GameplayUI");
        }

        static Type FindInputModuleType()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                if (t != null) return t;
            }
            return null;
        }

        static void Destroy(UnityEngine.Object o) { UnityEngine.Object.DestroyImmediate(o); }

        // ---------- sandbox ----------
        static void BuildSandbox()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.2f, 0.25f, 0.3f);

            var es = new GameObject("EventSystem", typeof(EventSystem));
            Type inputModule = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                inputModule = asm.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                if (inputModule != null) break;
            }
            if (inputModule != null) es.AddComponent(inputModule); else es.AddComponent<StandaloneInputModule>();

            T Inst<T>(string n) where T : Component
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + n + ".prefab");
                var go = (GameObject)PrefabUtility.InstantiatePrefab(p);
                return go.GetComponent<T>();
            }

            var hud = Inst<HudView>("HUD");
            var reticles = new UnityEngine.Object[3];
            for (int i = 0; i < 3; i++)
            {
                var r = Inst<TargetReticleView>("TargetReticle");
                r.transform.SetParent(hud.ReticleRoot, false);
                r.name = "TargetReticle" + (i + 1);
                reticles[i] = r;
            }
            var flash = Inst<DamageFlashView>("DamageFlash");
            var win = Inst<WinView>("WinPanel");
            var over = Inst<GameOverView>("GameOverPanel");
            var ad = Inst<FakeRewardedAd>("FakeAdOverlay");

            var drv = new GameObject("UIDebugDriver").AddComponent<UIDebugDriver>();
            Set(drv, "hud", hud);
            Set(drv, "reticles", reticles);
            Set(drv, "flash", flash);
            Set(drv, "win", win);
            Set(drv, "gameOver", over);
            Set(drv, "ad", ad);
            EditorSceneManager.SaveScene(scene, SandboxScene);
        }
    }
}
