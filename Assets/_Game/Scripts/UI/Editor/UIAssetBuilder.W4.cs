using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClaudeCop.Jev;

namespace ClaudeCop.UI.Editor
{
    /// <summary>
    /// W4 (T-411): JevDebugPanel.prefab (nut JEV + panel an mac dinh) va them node o CUOI GameplayUI.prefab.
    /// Idempotent. KHONG dong vao scene. Menu: ClaudeCop/UI/W4 - Build Jev Debug Panel.
    /// </summary>
    public static partial class UIAssetBuilder
    {
        [MenuItem("ClaudeCop/UI/W4 - Build Jev Debug Panel")]
        public static void BuildW4()
        {
            System.IO.Directory.CreateDirectory(PrefabDir);
            font = GetUiFont();
            var cfg = AssetDatabase.LoadAssetAtPath<UIConfig>(Root + "/UIConfig.asset");
            var white = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + "/White.png");
            BuildJevDebugPanel(cfg, white);
            AssetDatabase.SaveAssets();
            AddJevPanelToGameplayUI();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UIAssetBuilder] W4 (JevDebugPanel) build xong.");
        }

        static TMP_Text JRow(string name, Transform parent, string text, int size, float y, float h, float x = 20f, float w = 780f,
            TextAlignmentOptions al = TextAlignmentOptions.TopLeft)
        {
            var t = NewText(name, parent, text, size, al, Color.white);
            var r = t.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
            r.sizeDelta = new Vector2(w, h); r.anchoredPosition = new Vector2(x, y);
            return t;
        }

        static RectTransform JTrack(string name, Transform parent, Sprite white, float x, float y, float w, float h, Color bg, out Image fill, Color fillColor)
        {
            var track = NewImage(name, parent, white, bg);
            var r = track.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
            r.sizeDelta = new Vector2(w, h); r.anchoredPosition = new Vector2(x, y);
            fill = NewImage("Fill", track.transform, white, fillColor);
            var f = fill.rectTransform;
            f.anchorMin = Vector2.zero; f.anchorMax = new Vector2(0f, 1f); f.pivot = new Vector2(0f, 0.5f);
            f.offsetMin = f.offsetMax = Vector2.zero;
            return r;
        }

        static void BuildJevDebugPanel(UIConfig cfg, Sprite white)
        {
            var root = NewCanvas("JevDebugPanel", 70);
            var safe = NewRect("SafeArea", root.transform);
            Stretch(safe); safe.gameObject.AddComponent<SafeAreaPanel>();

            // Nut JEV nho o goc duoi-trai (raycast chi tren nut)
            var btn = NewButton("JevButton", safe, white, new Color(0.1f, 0.1f, 0.15f, 0.75f), "JEV", 34, out _);
            Anchor((RectTransform)btn.transform, new Vector2(0f, 0f), new Vector2(130, 70), new Vector2(24, 260)); // tren cum vu khi/dan

            // Panel: KHONG chan raycast (chi nut JEV chan tap); thu nho ~0.56 => ~460px / 1920 (~24% chieu rong), nam tren nut JEV
            var panel = NewImage("Panel", safe, white, new Color(0.04f, 0.06f, 0.1f, 0.88f), false);
            Anchor(panel.rectTransform, new Vector2(0f, 0f), new Vector2(820, 770), new Vector2(24, 342));
            panel.rectTransform.localScale = new Vector3(0.56f, 0.56f, 1f);
            var p = panel.transform;

            var title = JRow("Title", p, "JEV DEBUG", 30, -8, 36);
            title.color = new Color(0.6f, 0.65f, 0.75f);
            var question = JRow("Question", p, "Câu hỏi: —", 32, -48, 44);
            var choice = JRow("Choice", p, "Chưa có quyết định", 36, -92, 48);
            choice.fontStyle = FontStyles.Bold;
            var source = JRow("Source", p, "Nguồn: —", 32, -140, 44);

            var conf = JRow("Confidence", p, "Confidence: —", 30, -184, 40);
            JTrack("ConfidenceTrack", p, white, 20, -226, 780, 22, new Color(1, 1, 1, 0.12f), out var confFill, new Color(0.4f, 0.8f, 1f));
            var marker = NewImage("ThresholdMarker", confFill.rectTransform.parent, white, new Color(1f, 0.3f, 0.3f, 1f));
            var mr = marker.rectTransform;
            mr.anchorMin = new Vector2(0.6f, 0f); mr.anchorMax = new Vector2(0.6f, 1f); mr.pivot = new Vector2(0.5f, 0.5f);
            mr.sizeDelta = new Vector2(4, 8); mr.anchoredPosition = Vector2.zero;

            var time = JRow("Time", p, "", 28, -256, 36);

            var labels = new TMP_Text[JevDebugPanelView.BarCount];
            var fills = new Image[JevDebugPanelView.BarCount];
            for (int i = 0; i < labels.Length; i++)
            {
                float y = -298 - i * 42;
                labels[i] = JRow("BarLabel" + i, p, "", 28, y, 36, 20, 250);
                JTrack("BarTrack" + i, p, white, 280, y - 6, 520, 24, new Color(1, 1, 1, 0.12f), out fills[i], cfg != null ? cfg.jevBarColor : Color.green);
            }

            var stats = JRow("Stats", p, "", 28, -430, 110);
            var history = JRow("History", p, "", 26, -548, 214);
            history.color = new Color(0.8f, 0.85f, 0.95f);

            var view = root.AddComponent<JevDebugPanelView>();
            Set(view, "panelRoot", panel.gameObject);
            Set(view, "toggleButtonRoot", btn.gameObject);
            Set(view, "toggleButton", btn);
            Set(view, "questionText", question);
            Set(view, "choiceText", choice);
            Set(view, "sourceText", source);
            Set(view, "confidenceText", conf);
            Set(view, "confidenceFill", confFill);
            Set(view, "confidenceThresholdMarker", mr);
            Set(view, "timeText", time);
            Set(view, "statsText", stats);
            Set(view, "historyText", history);
            Set(view, "barLabels", labels);
            Set(view, "barFills", fills);
            var pr = root.AddComponent<JevDebugPanelPresenter>();
            Set(pr, "view", view);
            Set(pr, "config", cfg);
            Set(pr, "jevConfig", AssetDatabase.LoadAssetAtPath<JevConfig>("Assets/_Game/Prefabs/Game/Data/JevConfig.asset"));
            panel.gameObject.SetActive(false);
            Save(root, "JevDebugPanel");
        }

        static void AddJevPanelToGameplayUI()
        {
            string path = PrefabDir + "/GameplayUI.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.transform.Find("JevDebugPanel") != null) return;
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/JevDebugPanel.prefab");
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(p, root.transform);
                inst.transform.SetAsLastSibling();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
