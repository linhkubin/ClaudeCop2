using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClaudeCop.Core;
using ClaudeCop.RankScore;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Man thang. Show/Hide + diem; khoi rank (chu S/A/B/C dong dau, nhan xet, bang thong ke) hien khi co ket qua.
    /// Nut Restart gui GameCommands.RequestRestart. Chi hien thi; EndScreenPresenter quyet dinh khi nao.
    /// </summary>
    public class WinView : MonoBehaviour
    {
        [SerializeField] UIConfig config;
        [SerializeField] GameObject content;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] Button restartButton;
        [SerializeField] GameObject rankBlock;
        [SerializeField] TMP_Text rankText;
        [SerializeField] TMP_Text weaknessText;
        [SerializeField] TMP_Text statsValues;

        float stampT = -1f;
        UIConfig Cfg => config != null ? config : UIConfig.Fallback;

        void Awake()
        {
            if (restartButton != null) restartButton.onClick.AddListener(GameCommands.RequestRestart);
            Hide();
        }

        public void Show(int score)
        {
            if (scoreText != null) scoreText.SetText("ĐIỂM  {0:0}", score);
            if (content != null) content.SetActive(true);
        }

        /// <summary>Hien khoi rank. Giam chuyen dong -> hien thang, khong dong dau.</summary>
        public void ShowRank(ScoreRankResult r)
        {
            if (rankBlock == null) return;
            var c = Cfg;
            if (rankText != null) { rankText.text = UIConfig.RankLetter(r.Rank); rankText.color = c.RankColor(r.Rank); }
            if (weaknessText != null) weaknessText.text = string.IsNullOrEmpty(r.WeaknessText) ? "" : r.WeaknessText;
            if (statsValues != null)
                statsValues.SetText("{0:0}%\n{1:2}s\n{2}\n{3}\n{4}", r.Accuracy * 100f, r.AvgReactionTime, r.DamageTaken, r.HostageHits, r.BlastKills);
            rankBlock.SetActive(true);
            if (rankText != null)
            {
                if (UserSettings.ReduceMotion || c.rankStampDuration <= 0f) { stampT = -1f; ApplyStamp(1f); }
                else { stampT = 0f; ApplyStamp(0f); }
            }
        }

        public void HideRank()
        {
            stampT = -1f;
            if (rankBlock != null) rankBlock.SetActive(false);
        }

        public void Hide() { HideRank(); if (content != null) content.SetActive(false); }
        public bool IsVisible => content != null && content.activeSelf;
        public bool RankVisible => rankBlock != null && rankBlock.activeSelf;
        public string RankString => rankText != null ? rankText.text : null;
        public Color RankColor => rankText != null ? rankText.color : default;
        public string WeaknessString => weaknessText != null ? weaknessText.text : null;
        public string StatsString => statsValues != null ? statsValues.text : null;
        public float StampScale => rankText != null ? rankText.rectTransform.localScale.x : 1f;

        void Update()
        {
            if (stampT < 0f) return;
            stampT += Time.unscaledDeltaTime;
            float d = Mathf.Max(0.01f, Cfg.rankStampDuration);
            float k = Mathf.Clamp01(stampT / d);
            ApplyStamp(k);
            if (k >= 1f) stampT = -1f;
        }

        void ApplyStamp(float k)
        {
            if (rankText == null) return;
            float e = 1f - (1f - k) * (1f - k) * (1f - k);
            float s = Mathf.Lerp(Cfg.rankStampStartScale, 1f, e);
            rankText.rectTransform.localScale = new Vector3(s, s, 1f);
            rankText.alpha = Mathf.Clamp01(k * 4f);
        }
    }
}
