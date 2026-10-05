using UnityEditor;
using UnityEngine;
using TMPro;

namespace ClaudeCop.UI.Editor
{
    /// <summary>
    /// W7 (T-711): va tai cho WinPanel.prefab (khoi rank + bang thong ke), khong dung lai. Idempotent.
    /// Menu: ClaudeCop/UI/W7 - Patch Win Rank.
    /// </summary>
    public static partial class UIAssetBuilder
    {
        [MenuItem("ClaudeCop/UI/W7 - Patch Win Rank")]
        public static void PatchW7()
        {
            font = GetUiFont();
            var cfg = AssetDatabase.LoadAssetAtPath<UIConfig>(Root + "/UIConfig.asset");
            string path = PrefabDir + "/WinPanel.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var content = root.transform.Find("Content");
                var view = root.GetComponent<WinView>();

                // Bo cuc lai cac phan tu cu (man 1080x1920, tam man hinh).
                Anchor((RectTransform)content.Find("Title"), new Vector2(0.5f, 0.5f), new Vector2(1000, 150), new Vector2(0, 740));
                var score = (RectTransform)content.Find("ScoreText");
                Anchor(score, new Vector2(0.5f, 0.5f), new Vector2(1000, 110), new Vector2(0, -70));
                Anchor((RectTransform)content.Find("RestartButton"), new Vector2(0.5f, 0.5f), new Vector2(520, 150), new Vector2(0, -700));

                var old = content.Find("RankBlock");
                if (old != null) Object.DestroyImmediate(old.gameObject);
                var block = NewRect("RankBlock", content);
                Stretch(block);

                var rank = NewText("RankText", block, "S", 560, TextAlignmentOptions.Center, cfg.rankColorS);
                rank.fontStyle = FontStyles.Bold;
                Anchor(rank.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(700, 620), new Vector2(0, 400));

                var weak = NewText("WeaknessText", block, "", 52, TextAlignmentOptions.Center, Color.white);
                weak.textWrappingMode = TextWrappingModes.Normal;
                weak.overflowMode = TextOverflowModes.Overflow;
                Anchor(weak.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(920, 150), new Vector2(0, 120));
                AutoSize(weak);

                // Bang thong ke: cot nhan (trai) + cot gia tri (phai).
                var panel = NewImage("StatsPanel", block, AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/White.png"), new Color(0f, 0f, 0f, 0.35f));
                Anchor(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(820, 400), new Vector2(0, -360));
                var labels = NewText("StatsLabels", panel.transform, "Độ chính xác\nPhản xạ TB\nMất mạng\nCon tin trúng\nHạ bằng nổ", 48, TextAlignmentOptions.Left, Color.white);
                labels.lineSpacing = 20f;
                Anchor(labels.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(740, 360), Vector2.zero);
                var values = NewText("StatsValues", panel.transform, "0%\n0s\n0\n0\n0", 48, TextAlignmentOptions.Right, new Color(1f, 0.95f, 0.6f, 1f));
                values.lineSpacing = 20f;
                Anchor(values.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(740, 360), Vector2.zero);
                labels.alignment = TextAlignmentOptions.Left;
                // Canh dong: ca hai cot cung kich thuoc/khoang dong, can giua theo chieu doc.
                labels.verticalAlignment = VerticalAlignmentOptions.Middle;
                values.verticalAlignment = VerticalAlignmentOptions.Middle;

                // Diem lon hon, nam tren bang thong ke.
                score.GetComponent<TMP_Text>().fontSize = 76;

                Set(view, "config", cfg);
                Set(view, "rankBlock", block.gameObject);
                Set(view, "rankText", rank);
                Set(view, "weaknessText", weak);
                Set(view, "statsValues", values);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }

            // Gan config cho EndScreenPresenter trong GameplayUI.prefab.
            string gp = PrefabDir + "/GameplayUI.prefab";
            var g = PrefabUtility.LoadPrefabContents(gp);
            try
            {
                var ep = g.GetComponentInChildren<EndScreenPresenter>(true);
                if (ep != null) { Set(ep, "config", cfg); PrefabUtility.SaveAsPrefabAsset(g, gp); }
            }
            finally { PrefabUtility.UnloadPrefabContents(g); }

            AssetDatabase.SaveAssets();
            Debug.Log("[UIAssetBuilder] W7 patch xong.");
        }
    }
}
