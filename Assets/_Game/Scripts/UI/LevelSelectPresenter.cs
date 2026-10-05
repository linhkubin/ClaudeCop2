using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClaudeCop.Core;
using ClaudeCop.Meta;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Dat duoi Content cua TitlePanel: hang nut chon level (LEVEL 1, LEVEL 2, ...) + nhan "CHON MAN". Bam -> GameCommands.RequestSelectLevel(i)
    /// (TitleLauncher nap scene, PhaseDirector bat dau tu level do). UI tu dung bang code, nam trong Content nen an/hien cung man Title.
    /// Moi nut hien rank + diem tot nhat da luu (PlayerProfile): choi lai de cai thien, chi luu khi tot hon.
    /// </summary>
    public class LevelSelectPresenter : MonoBehaviour
    {
        [SerializeField] int levelCount = 2;
        [SerializeField] Vector2 buttonSize = new Vector2(330f, 130f);
        [SerializeField] float spacing = 40f;

        TMP_Text[] bestLabels;

        void Awake()
        {
            var rt = (RectTransform)transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1000f, 300f);
            Label("Header", "SELECT LEVEL", 44, new Vector2(0f, 110f), new Vector2(1000f, 70f), new Color(1f, 1f, 1f, 0.85f), null);
            bestLabels = new TMP_Text[levelCount];
            float total = levelCount * buttonSize.x + (levelCount - 1) * spacing, x0 = -total * 0.5f + buttonSize.x * 0.5f;
            for (int i = 0; i < levelCount; i++)
            {
                int level = i;
                var go = new GameObject("Level" + (i + 1) + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(transform, false);
                var r = (RectTransform)go.transform;
                r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
                r.anchoredPosition = new Vector2(x0 + i * (buttonSize.x + spacing), 0f); r.sizeDelta = buttonSize;
                var img = go.GetComponent<Image>(); img.color = new Color(0.15f, 0.45f, 0.9f, 1f);
                var b = go.GetComponent<Button>(); b.targetGraphic = img; b.onClick.AddListener(() => GameCommands.RequestSelectLevel(level));
                Label("Label", "LEVEL " + (i + 1), 46, new Vector2(0f, 20f), buttonSize, Color.white, go.transform);
                bestLabels[i] = Label("Best", "", 28, new Vector2(0f, -38f), buttonSize, new Color(1f, 0.85f, 0.2f), go.transform);
            }
        }

        void OnEnable() { PlayerProfile.Changed += Refresh; Refresh(); }
        void OnDisable() { PlayerProfile.Changed -= Refresh; }

        void Refresh()
        {
            if (bestLabels == null) return;
            var p = PlayerProfile.Data;
            for (int i = 0; i < bestLabels.Length; i++)
            {
                var r = p.Level(i, false);
                bestLabels[i].text = r == null || r.bestRank < 0
                    ? (r != null && r.plays > 0 ? "NOT CLEARED" : "NEW")
                    : "BEST " + LevelRecord.RankLabel(r.bestRank) + " - " + r.bestScore;
            }
        }

        TMP_Text Label(string name, string text, float size, Vector2 pos, Vector2 box, Color color, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent != null ? parent : transform, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.alignment = TextAlignmentOptions.Center; t.color = color; t.fontStyle = FontStyles.Bold; t.raycastTarget = false;
            var r = t.rectTransform; r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f); r.anchoredPosition = pos; r.sizeDelta = box;
            return t;
        }
    }
}
