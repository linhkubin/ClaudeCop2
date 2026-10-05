using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClaudeCop.Core;
using ClaudeCop.RankScore;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Bang ket qua giua chuoi level: RailEvents.StageCompleted -> (sau showDelay) hien man giong MISSION COMPLETE (diem + rank + bang thong ke)
    /// nhung nut Restart doi thanh CONTINUE va HOME. Dung lai WinPanel.prefab (winPrefab) nen cung giao dien / rank voi man thang cuoi.
    /// CONTINUE -> GameCommands.RequestContinue (PhaseDirector chay tiep man ke ngay trong scene, khong load/ngat quang).
    /// HOME -> GameCommands.RequestHome (logic ve man hinh chinh them sau). Thieu winPrefab -> tu Continue de khong ket game.
    /// </summary>
    public class StageResultPresenter : MonoBehaviour
    {
        [SerializeField] WinView winPrefab;
        [SerializeField] float showDelay = 1.6f;

        WinView view;
        TMP_Text titleText;
        Coroutine routine;

        public bool IsVisible => view != null && view.IsVisible;

        void Awake() { Build(); }

        void OnEnable() { RailEvents.StageCompleted += OnStageCompleted; }

        void OnDisable()
        {
            RailEvents.StageCompleted -= OnStageCompleted;
            if (routine != null) { StopCoroutine(routine); routine = null; }
            if (view != null) view.Hide();
        }

        void OnStageCompleted(int phaseIndex, string title)
        {
            if (view == null) { Debug.LogWarning("[StageResultPresenter] Thieu winPrefab: bo qua bang ket qua, Continue ngay.", this); GameCommands.RequestContinue(); return; }
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(ShowRoutine(title));
        }

        IEnumerator ShowRoutine(string phaseTitle)
        {
            yield return new WaitForSecondsRealtime(showDelay);
            routine = null;
            if (titleText != null) titleText.text = LevelName(phaseTitle) + " COMPLETE";
            view.Show(GameEvents.Current.Score);
            if (ScoreRankBoard.HasResult) view.ShowRank(ScoreRankBoard.Last); else view.HideRank();
        }

        /// <summary>"STAGE 1-3" -> "LEVEL 1"; khong khop -> "LEVEL".</summary>
        public static string LevelName(string phaseTitle)
        {
            if (string.IsNullOrEmpty(phaseTitle)) return "LEVEL";
            int dash = phaseTitle.LastIndexOf('-');
            string head = dash > 0 ? phaseTitle.Substring(0, dash) : phaseTitle;
            int sp = head.LastIndexOf(' ');
            string num = sp >= 0 ? head.Substring(sp + 1) : "";
            return num.Length > 0 ? "LEVEL " + num : "LEVEL";
        }

        void OnContinue()
        {
            view.Hide();
            GameCommands.RequestContinue();
        }

        void OnHome() { GameCommands.RequestHome(); }

        void Build()
        {
            if (winPrefab == null) return;
            view = Instantiate(winPrefab, transform);
            view.name = "StageResultPanel";
            var content = view.transform.Find("Content");
            if (content == null) return;
            var t = content.Find("Title");
            if (t != null) titleText = t.GetComponent<TMP_Text>();
            var restart = content.Find("RestartButton");
            if (restart == null) return;
            var cont = restart.GetComponent<Button>();
            cont.onClick.RemoveAllListeners(); // WinView.Awake gan Restart: bo, doi thanh Continue
            cont.onClick.AddListener(OnContinue);
            SetButton(restart, "CONTINUE", new Vector2(0f, -660f), new Vector2(560f, 140f), new Color(0.15f, 0.55f, 0.25f, 1f));
            var home = Instantiate(restart.gameObject, restart.parent);
            home.name = "HomeButton";
            var hb = home.GetComponent<Button>();
            hb.onClick.RemoveAllListeners(); hb.onClick.AddListener(OnHome);
            SetButton((RectTransform)home.transform, "HOME", new Vector2(0f, -830f), new Vector2(560f, 130f), new Color(0.3f, 0.34f, 0.4f, 1f));
        }

        static void SetButton(Transform tr, string label, Vector2 pos, Vector2 size, Color color)
        {
            var r = (RectTransform)tr;
            r.anchoredPosition = pos; r.sizeDelta = size;
            var img = tr.GetComponent<Image>(); if (img != null) img.color = color;
            var txt = tr.GetComponentInChildren<TMP_Text>(true); if (txt != null) txt.text = label;
        }
    }
}
