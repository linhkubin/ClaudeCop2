using UnityEngine;
using UnityEngine.SceneManagement;
using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>
    /// Dat trong Title.unity: nghe GameCommands.StartGameRequested (UI TitlePanel phat) -> nap scene gameplay.
    /// Scene gameplay co GameManager.startImmediately=false se tu bat dau nho GameSession.PendingStart.
    /// </summary>
    public sealed class TitleLauncher : MonoBehaviour
    {
        [SerializeField] string levelScene = "Level_01";
        bool loading;

        void Awake()
        {
            Application.targetFrameRate = 60; // F-207: Title cung 60 FPS tren Android
            Time.timeScale = 1f;
            GameSession.MarkTitleVisited();
            // Dat snapshot ve Title (cho UI doc khi OnEnable sau khi Restart/ve Title tu scene gameplay).
            if (GameEvents.Current.State != GameState.Title) GameEvents.RaiseGameStateChanged(GameState.Title);
        }

        void OnEnable() { GameCommands.StartGameRequested += OnStart; GameCommands.LevelSelectRequested += OnSelectLevel; }
        void OnDisable() { GameCommands.StartGameRequested -= OnStart; GameCommands.LevelSelectRequested -= OnSelectLevel; }

        void OnSelectLevel(int level) { Load(); } // SelectedLevel da duoc GameCommands.RequestSelectLevel dat

        void OnStart() { GameCommands.SelectedLevel = 0; Load(); }

        void Load()
        {
            if (loading) return;
            loading = true;
            GameSession.RequestStartOnLoad();
            SceneManager.LoadScene(levelScene);
        }
    }
}
