using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.RankScore;

namespace ClaudeCop.UI
{
    /// <summary>
    /// GameStateChanged: Win/GameOver hien man tuong ung; trang thai khac an ca hai (vd. sau Restart).
    /// Rank: doc ScoreRankBoard.Last neu da co luc vao Win, neu chua thi cho RankEvaluated; qua rankWaitSeconds (unscaled) thi bo khoi rank.
    /// </summary>
    public class EndScreenPresenter : MonoBehaviour
    {
        [SerializeField] WinView winView;
        [SerializeField] GameOverView gameOverView;
        [SerializeField] UIConfig config;

        bool waitingRank;
        float waited;

        public bool WaitingRank => waitingRank;

        void OnEnable()
        {
            GameEvents.GameStateChanged += Apply;
            ScoreRankBoard.RankEvaluated += OnRank;
            Apply(GameEvents.Current.State);
        }

        void Start() { Apply(GameEvents.Current.State); } // sau Awake cua view (Awake goi Hide)

        void OnDisable()
        {
            GameEvents.GameStateChanged -= Apply;
            ScoreRankBoard.RankEvaluated -= OnRank;
        }

        void Update()
        {
            if (!waitingRank) return;
            waited += Time.unscaledDeltaTime;
            float limit = config != null ? config.rankWaitSeconds : UIConfig.Fallback.rankWaitSeconds;
            if (waited >= limit) waitingRank = false; // het han: chi hien diem
        }

        void OnRank(ScoreRankResult r)
        {
            if (winView != null && winView.IsVisible) { winView.ShowRank(r); waitingRank = false; }
        }

        void Apply(GameState state)
        {
            int score = GameEvents.Current.Score;
            if (state == GameState.Win)
            {
                if (gameOverView != null) gameOverView.Hide();
                if (winView != null)
                {
                    winView.Show(score);
                    if (ScoreRankBoard.HasResult) { winView.ShowRank(ScoreRankBoard.Last); waitingRank = false; }
                    else { winView.HideRank(); waitingRank = true; waited = 0f; }
                }
            }
            else if (state == GameState.GameOver)
            {
                waitingRank = false;
                if (winView != null) winView.Hide();
                if (gameOverView != null) gameOverView.Show(score);
            }
            else
            {
                waitingRank = false;
                if (winView != null) winView.Hide();
                if (gameOverView != null) gameOverView.Hide();
            }
        }
    }
}
