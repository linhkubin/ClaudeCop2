using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using ClaudeCop.Ads;

namespace ClaudeCop.UI.Editor
{
    /// <summary>
    /// W3 (T-311/T-312): font TMP (dynamic, co tieng Viet), migrate Text -> TMP tai cho (giu nguyen node/GUID prefab),
    /// tao RevivePopup/PhaseFade/FloatingScore/TitlePanel va gan vao GameplayUI. Idempotent. KHONG dong vao scene dang mo.
    /// Menu: ClaudeCop/UI/W3 - Build Assets.
    /// </summary>
    public static partial class UIAssetBuilder
    {
        const string FontDir = Root + "/Fonts";
        const string FontPath = FontDir + "/ClaudeCop UI SDF.asset";
        const string SourceFontPath = "Assets/TextMesh Pro/Fonts/LiberationSans.ttf";

        [MenuItem("ClaudeCop/UI/W3 - Build Assets")]
        public static void BuildW3()
        {
            Directory.CreateDirectory(PrefabDir);
            font = GetUiFont();
            MigrateTextToTmp();
            var cfg = AssetDatabase.LoadAssetAtPath<UIConfig>(Root + "/UIConfig.asset");
            var white = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + "/White.png");
            var circle = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + "/Circle.png");
            AddComboToHud(cfg);
            BuildRevivePopup(cfg, white);
            BuildPhaseFade(cfg, white);
            BuildFloatingScore(cfg);
            BuildTitlePanel(white, circle);
            AssetDatabase.SaveAssets();
            AddW3ToGameplayUI();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UIAssetBuilder] W3 build xong.");
        }

        // ---------- font ----------
        /// <summary>Font SDF dong tu LiberationSans (co Latin Extended Additional -> du dau tieng Viet). Tao neu chua co.</summary>
        public static TMP_FontAsset GetUiFont()
        {
            var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (fa != null) return fa;
            var src = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (src == null) { Debug.LogError("[UIAssetBuilder] Thieu " + SourceFontPath + " (import TMP Essentials)."); return null; }
            Directory.CreateDirectory(FontDir);
            fa = TMP_FontAsset.CreateFontAsset(src, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            fa.name = "ClaudeCop UI SDF";
            AssetDatabase.CreateAsset(fa, FontPath);
            if (fa.material != null) { fa.material.name = "ClaudeCop UI SDF Material"; AssetDatabase.AddObjectToAsset(fa.material, fa); }
            if (fa.atlasTextures != null)
                foreach (var tex in fa.atlasTextures) { if (tex != null) { tex.name = "ClaudeCop UI SDF Atlas"; AssetDatabase.AddObjectToAsset(tex, fa); } }
            EditorUtility.SetDirty(fa);
            AssetDatabase.SaveAssets();
            return fa;
        }

        static TMPro.TextAlignmentOptions Map(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }

        // ---------- migrate ----------
        [MenuItem("ClaudeCop/UI/W3 - Migrate Text to TMP only")]
        public static void MigrateTextToTmp()
        {
            font = GetUiFont();
            Migrate("HUD", root =>
            {
                var v = root.GetComponent<HudView>();
                Relink(v, "scoreText", root, "SafeArea/ScoreText");
                Relink(v, "ammoText", root, "SafeArea/AmmoText");
                Relink(v, "weaponLabel", root, "SafeArea/WeaponLabel");
                Relink(v, "reloadLabel", root, "SafeArea/ReloadButton/Label");
            });
            Migrate("WinPanel", root => Relink(root.GetComponent<WinView>(), "scoreText", root, "Content/ScoreText"));
            Migrate("GameOverPanel", root => Relink(root.GetComponent<GameOverView>(), "scoreText", root, "Content/ScoreText"));
            Migrate("FakeAdOverlay", root =>
            {
                var v = root.GetComponent<FakeRewardedAd>();
                Relink(v, "countdownText", root, "Content/Countdown");
                Relink(v, "titleText", root, "Content/Title");
                var t = root.transform.Find("Content/Title");
                if (t != null) t.GetComponent<TMP_Text>().text = "QUẢNG CÁO";
            });
        }

        static void Migrate(string prefabName, System.Action<GameObject> relink)
        {
            string path = PrefabDir + "/" + prefabName + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var olds = root.GetComponentsInChildren<Text>(true);
                if (olds.Length == 0 && root.GetComponentInChildren<TMP_Text>(true) != null) return;
                foreach (var t in olds)
                {
                    var go = t.gameObject;
                    string s = t.text; int size = t.fontSize; Color col = t.color; var al = Map(t.alignment);
                    var sh = go.GetComponent<Shadow>(); if (sh != null) Object.DestroyImmediate(sh);
                    Object.DestroyImmediate(t);
                    var tmp = go.AddComponent<TextMeshProUGUI>();
                    if (font != null) tmp.font = font;
                    tmp.text = s; tmp.fontSize = size; tmp.color = col; tmp.alignment = al;
                    tmp.textWrappingMode = TextWrappingModes.NoWrap; tmp.overflowMode = TextOverflowModes.Overflow;
                    tmp.raycastTarget = false;
                }
                relink(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        static void Relink(Object owner, string field, GameObject root, string childPath)
        {
            var child = root.transform.Find(childPath);
            if (owner == null || child == null) { Debug.LogError("[UIAssetBuilder] Relink that bai: " + field + " <- " + childPath); return; }
            Set(owner, field, child.GetComponent<TMP_Text>());
        }

        // ---------- HUD combo ----------
        static void AddComboToHud(UIConfig cfg)
        {
            string path = PrefabDir + "/HUD.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var safe = root.transform.Find("SafeArea");
                if (safe.Find("ComboText") != null) return;
                var t = NewText("ComboText", safe, "x2 COMBO", 64, TextAlignmentOptions.Top, new Color(1f, 0.85f, 0.2f));
                var r = t.rectTransform;
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 1f); r.pivot = new Vector2(0.5f, 1f);
                r.sizeDelta = new Vector2(600, 90); r.anchoredPosition = new Vector2(0, -150); // man doc: duoi hang diem/tim
                t.gameObject.SetActive(false);
                Set(root.GetComponent<HudView>(), "comboText", t);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        // ---------- helpers ----------
        static void Center(RectTransform r, Vector2 size, Vector2 pos)
        {
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f); r.sizeDelta = size; r.anchoredPosition = pos;
        }

        // ---------- RevivePopup ----------
        static void BuildRevivePopup(UIConfig cfg, Sprite white)
        {
            var root = NewCanvas("RevivePopup", 80);   // tren RankScoreDebugPanel (70), duoi PhaseFade (90)/FakeAdOverlay (100)
            var content = NewImage("Content", root.transform, white, new Color(0f, 0f, 0f, 0.7f), true);   // chan tap xuyen xuong game
            Stretch(content.rectTransform);
            var panel = NewImage("Panel", content.transform, white, new Color(0.08f, 0.1f, 0.16f, 0.95f));
            Center(panel.rectTransform, new Vector2(1000, 640), Vector2.zero);
            var title = NewText("Title", panel.transform, "HỒI SINH?", 96, TextAlignmentOptions.Center, Color.white);
            Center(title.rectTransform, new Vector2(900, 120), new Vector2(0, 240));
            var count = NewText("Countdown", panel.transform, "10", 200, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.2f));
            Center(count.rectTransform, new Vector2(500, 240), new Vector2(0, 90));
            var watch = NewButton("WatchButton", panel.transform, white, new Color(0.15f, 0.55f, 0.25f, 1f), "Xem quảng cáo", 56, out _);
            Center((RectTransform)watch.transform, new Vector2(760, 130), new Vector2(0, -80));
            var skip = NewButton("SkipButton", panel.transform, white, new Color(0.35f, 0.35f, 0.4f, 1f), "Bỏ qua", 48, out _);
            Center((RectTransform)skip.transform, new Vector2(420, 100), new Vector2(0, -230));
            var ad = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/FakeAdOverlay.prefab"), root.transform);
            ad.transform.SetAsLastSibling();

            var view = root.AddComponent<RevivePopupView>();
            Set(view, "content", content.gameObject);
            Set(view, "titleText", title);
            Set(view, "countdownText", count);
            Set(view, "watchButton", watch);
            Set(view, "skipButton", skip);
            var pr = root.AddComponent<RevivePopupPresenter>();
            Set(pr, "view", view);
            Set(pr, "config", cfg);
            Set(pr, "adBehaviour", ad.GetComponent<FakeRewardedAd>());
            Set(pr, "freezeTimeScale", true);
            content.gameObject.SetActive(false);
            Save(root, "RevivePopup");
        }

        // ---------- PhaseFade ----------
        static void BuildPhaseFade(UIConfig cfg, Sprite white)
        {
            var root = NewCanvas("PhaseFade", 90);
            Destroy(root.GetComponent<GraphicRaycaster>());
            var content = NewRect("Content", root.transform);
            Stretch(content);
            var black = NewImage("Black", content, white, Color.black);
            Stretch(black.rectTransform);
            var bg = black.gameObject.AddComponent<CanvasGroup>(); bg.alpha = 0f; bg.blocksRaycasts = false; bg.interactable = false;
            var title = NewText("Title", content, "STAGE 1-1", 120, TextAlignmentOptions.Center, Color.white);
            Center(title.rectTransform, new Vector2(1000, 240), Vector2.zero);
            AutoSize(title);
            var tg = title.gameObject.AddComponent<CanvasGroup>(); tg.alpha = 0f; tg.blocksRaycasts = false; tg.interactable = false;
            var view = root.AddComponent<PhaseTransitionView>();
            Set(view, "config", cfg);
            Set(view, "content", content.gameObject);
            Set(view, "blackGroup", bg);
            Set(view, "titleGroup", tg);
            Set(view, "titleText", title);
            var pr = root.AddComponent<PhaseTransitionPresenter>();
            Set(pr, "view", view);
            Set(pr, "config", cfg);
            content.gameObject.SetActive(false);
            Save(root, "PhaseFade");
        }

        // ---------- FloatingScore ----------
        static void BuildFloatingScore(UIConfig cfg)
        {
            var root = NewCanvas("FloatingScore", 5);
            Destroy(root.GetComponent<GraphicRaycaster>());
            var layer = NewRect("Layer", root.transform);
            Stretch(layer);
            var item = NewRect("ItemTemplate", layer);
            Center(item, new Vector2(700, 200), Vector2.zero);
            var label = item.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.fontSize = cfg != null ? cfg.floatingNormalSize : 54; label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap; label.overflowMode = TextOverflowModes.Overflow;
            label.fontStyle = FontStyles.Bold; label.raycastTarget = false; label.text = "+100";
            var it = item.gameObject.AddComponent<FloatingScoreItem>();
            Set(it, "label", label);
            item.gameObject.SetActive(false);
            var view = root.AddComponent<FloatingScoreView>();
            Set(view, "config", cfg);
            Set(view, "layer", layer);
            Set(view, "template", it);
            Set(view, "poolSize", 12);
            var pr = root.AddComponent<FloatingScorePresenter>();
            Set(pr, "view", view);
            Save(root, "FloatingScore");
        }

        // ---------- TitlePanel ----------
        static void BuildTitlePanel(Sprite white, Sprite circle)
        {
            var root = NewCanvas("TitlePanel", 0);
            var content = NewImage("Content", root.transform, white, new Color(0.05f, 0.07f, 0.12f, 1f), true);
            Stretch(content.rectTransform);
            var safe = NewRect("SafeArea", content.transform);
            Stretch(safe); safe.gameObject.AddComponent<SafeAreaPanel>();
            var title = NewText("GameTitle", safe, "CLAUDE COP 2", 130, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f));
            Center(title.rectTransform, new Vector2(1000, 260), new Vector2(0, 230));
            AutoSize(title);
            var sub = NewText("Subtitle", safe, "Rail shooter", 56, TextAlignmentOptions.Center, new Color(0.8f, 0.85f, 0.95f));
            Center(sub.rectTransform, new Vector2(1000, 90), new Vector2(0, 80));
            var start = NewButton("StartButton", safe, white, new Color(0.15f, 0.55f, 0.25f, 1f), "BẮT ĐẦU", 80, out _);
            Center((RectTransform)start.transform, new Vector2(640, 170), new Vector2(0, -120));

            // Toggle "Giam chuyen dong": vung bam rong (nen trong suot) + o vuong + dau tich + nhan
            var tgo = NewImage("ReduceMotionToggle", safe, white, new Color(1, 1, 1, 0.05f), true);
            Center(tgo.rectTransform, new Vector2(760, 110), new Vector2(0, -330));
            var box = NewImage("Background", tgo.transform, white, new Color(0.9f, 0.92f, 1f, 1f), true);
            box.rectTransform.anchorMin = box.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            box.rectTransform.pivot = new Vector2(0f, 0.5f); box.rectTransform.sizeDelta = new Vector2(70, 70); box.rectTransform.anchoredPosition = new Vector2(24, 0);
            var check = NewImage("Checkmark", box.transform, circle, new Color(0.1f, 0.6f, 0.25f, 1f));
            Center(check.rectTransform, new Vector2(46, 46), Vector2.zero);
            var lbl = NewText("Label", tgo.transform, "Giảm chuyển động", 52, TextAlignmentOptions.Left, Color.white);
            lbl.rectTransform.anchorMin = Vector2.zero; lbl.rectTransform.anchorMax = Vector2.one;
            lbl.rectTransform.offsetMin = new Vector2(120, 0); lbl.rectTransform.offsetMax = Vector2.zero;
            var toggle = tgo.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = box; toggle.graphic = check; toggle.isOn = false;

            var es = new GameObject("EventSystem", typeof(EventSystem));
            es.transform.SetParent(root.transform, false);
            es.AddComponent(FindInputModuleType() ?? typeof(StandaloneInputModule));

            var view = root.AddComponent<TitleView>();
            Set(view, "content", content.gameObject);
            Set(view, "titleText", title);
            Set(view, "startButton", start);
            Set(view, "reduceMotionToggle", toggle);
            var pr = root.AddComponent<TitlePresenter>();
            Set(pr, "view", view);
            Save(root, "TitlePanel");
        }

        // ---------- GameplayUI ----------
        // Them con (instance prefab) vao GameplayUI.prefab bang cach sua asset - khong dung toi node/fileID da co.
        static void AddW3ToGameplayUI()
        {
            string path = PrefabDir + "/GameplayUI.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                bool changed = false;
                foreach (var n in new[] { "FloatingScore", "PhaseFade", "RevivePopup" })
                {
                    if (root.transform.Find(n) != null) continue;
                    var p = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + n + ".prefab");
                    PrefabUtility.InstantiatePrefab(p, root.transform);
                    changed = true;
                }
                if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
