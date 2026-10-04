using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.UI
{
    /// <summary>GameStateChanged: Win/GameOver hien man tuong ung; trang thai khac an ca hai (vd. sau Restart).</summary>
    public class EndScreenPresenter : MonoBehaviour
    {
        [SerializeField] WinView winView;
        [SerializeField] GameOverView gameOverView;

        void OnEnable()
        {
            GameEvents.GameStateChanged += Apply;
            Apply(GameEvents.Current.State);
        }

        void Start() { Apply(GameEvents.Current.State); } // sau Awake cua view (Awake goi Hide)

        void OnDisable() { GameEvents.GameStateChanged -= Apply; }

        void Apply(GameState state)
        {
            int score = GameEvents.Current.Score;
            if (state == GameState.Win)
            {
                if (gameOverView != null) gameOverView.Hide();
                if (winView != null) winView.Show(score);
            }
            else if (state == GameState.GameOver)
            {
                if (winView != null) winView.Hide();
                if (gameOverView != null) gameOverView.Show(score);
            }
            else
            {
                if (winView != null) winView.Hide();
                if (gameOverView != null) gameOverView.Hide();
            }
        }
    }
}
