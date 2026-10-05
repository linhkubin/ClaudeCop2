using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClaudeCop.UI
{
    /// <summary>
    /// View bang debug RankScore (T-411): chi hien thi + phat su kien nut RANKSCORE. Khi an, panelRoot tat (khong chan tap);
    /// nut RANKSCORE la Graphic raycast duy nhat luc an, luc hien chi nen panel chan.
    /// </summary>
    public class RankScoreDebugPanelView : MonoBehaviour
    {
        public const int BarCount = 3;

        [SerializeField] GameObject panelRoot;
        [SerializeField] GameObject toggleButtonRoot;
        [SerializeField] Button toggleButton;
        [SerializeField] TMP_Text questionText;
        [SerializeField] TMP_Text choiceText;
        [SerializeField] TMP_Text sourceText;
        [SerializeField] TMP_Text confidenceText;
        [SerializeField] RectTransform confidenceFill;
        [SerializeField] RectTransform confidenceThresholdMarker;
        [SerializeField] TMP_Text timeText;
        [SerializeField] TMP_Text statsText;
        [SerializeField] TMP_Text historyText;
        [SerializeField] TMP_Text[] barLabels = new TMP_Text[BarCount];
        [SerializeField] Image[] barFills = new Image[BarCount];

        public event Action ToggleClicked;

        public bool IsVisible => panelRoot != null && panelRoot.activeSelf;
        public bool ToggleButtonVisible => toggleButtonRoot != null && toggleButtonRoot.activeSelf;
        public string QuestionString => questionText != null ? questionText.text : null;
        public string ChoiceString => choiceText != null ? choiceText.text : null;
        public string SourceString => sourceText != null ? sourceText.text : null;
        public Color SourceColor => sourceText != null ? sourceText.color : default;
        public string HistoryString => historyText != null ? historyText.text : null;
        public string StatsString => statsText != null ? statsText.text : null;
        public Button ToggleButton => toggleButton;

        void Awake()
        {
            if (toggleButton != null) toggleButton.onClick.AddListener(OnToggle);
        }

        void OnToggle() => ToggleClicked?.Invoke();

        public void SetVisible(bool v) { if (panelRoot != null && panelRoot.activeSelf != v) panelRoot.SetActive(v); }
        public void SetToggleButtonVisible(bool v) { if (toggleButtonRoot != null) toggleButtonRoot.SetActive(v); }

        public void SetEmpty()
        {
            Txt(questionText, "Câu hỏi: —");
            Txt(choiceText, "Chưa có quyết định");
            Txt(sourceText, "Nguồn: —");
            Txt(confidenceText, "Confidence: —");
            SetFill(confidenceFill, 0f);
            Txt(timeText, "");
            Txt(statsText, "");
            Txt(historyText, "");
            for (int i = 0; i < BarCount; i++)
            {
                if (i < barLabels.Length) Txt(barLabels[i], "");
                if (i < barFills.Length && barFills[i] != null) SetFill(barFills[i].rectTransform, 0f);
            }
        }

        public void SetQuestion(string s) => Txt(questionText, s);
        public void SetChoice(string s) => Txt(choiceText, s);
        public void SetSource(string s, Color c)
        {
            Txt(sourceText, s);
            if (sourceText != null) sourceText.color = c;
        }
        public void SetConfidence(string s, float value01, float threshold01)
        {
            Txt(confidenceText, s);
            SetFill(confidenceFill, value01);
            if (confidenceThresholdMarker != null)
            {
                var a = confidenceThresholdMarker.anchorMin; a.x = Mathf.Clamp01(threshold01);
                confidenceThresholdMarker.anchorMin = a;
                var b = confidenceThresholdMarker.anchorMax; b.x = a.x;
                confidenceThresholdMarker.anchorMax = b;
            }
        }
        public void SetTime(string s) => Txt(timeText, s);
        public void SetStats(string s) => Txt(statsText, s);
        public void SetHistory(string s) => Txt(historyText, s);

        public void SetBar(int i, string label, float value01, Color color)
        {
            if (i < 0 || i >= BarCount) return;
            if (i < barLabels.Length) Txt(barLabels[i], label);
            if (i < barFills.Length && barFills[i] != null)
            {
                SetFill(barFills[i].rectTransform, value01);
                barFills[i].color = color;
            }
        }

        static void Txt(TMP_Text t, string s) { if (t != null && t.text != s) t.text = s; }

        static void SetFill(RectTransform r, float v01)
        {
            if (r == null) return;
            var m = r.anchorMax; m.x = Mathf.Clamp01(v01); r.anchorMax = m;
        }
    }
}
