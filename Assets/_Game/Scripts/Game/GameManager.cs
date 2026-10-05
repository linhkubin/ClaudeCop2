using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>
    /// Dieu phoi game: state, targetFrameRate, thang/thua, revive, restart. Khong tham chieu Camera/UI.
    /// Camera (PhaseDirector) tu StartLevel khi nghe GameStateChanged -> Playing.
    /// Game la chu Time.timeScale: RevivePrompt -> 0 (dat TRUOC khi phat state nen popup UI thay timeScale=0 va khong tu dong bang),
    /// revive / ket thuc -> 1.
    /// Khoi dong: GameConfig.StartImmediately, hoac GameSession.PendingStart (Title Start / Restart), hoac Editor chay thang scene gameplay.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        public const string EndPauseReason = "GameEnd";
        public const string RevivePauseReason = "Revive";
        public const string GracePauseReason = "ReviveGrace";
        public const string TitleScene = "Title";

        [SerializeField] GameConfig config;
        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] ScoreSystem scoreSystem;

        readonly GameFlow flow = new GameFlow();
        bool endPausePushed;
        bool revivePausePushed;
        bool gracePausePushed;
        Coroutine graceRoutine;

        public GameState State => flow.State;

        void OnEnable()
        {
            RailEvents.LevelCompleted += OnLevelCompleted;
            GameCommands.RestartRequested += OnRestartRequested;
            GameCommands.StartGameRequested += OnStartGameRequested;
            GameCommands.ReviveRequested += OnReviveRequested;
            GameCommands.ReviveDeclined += OnReviveDeclined;
            GameCommands.HomeRequested += ReturnToTitle;
            playerHealth.OutOfLives += OnOutOfLives;
        }

        void OnDisable()
        {
            RailEvents.LevelCompleted -= OnLevelCompleted;
            GameCommands.RestartRequested -= OnRestartRequested;
            GameCommands.StartGameRequested -= OnStartGameRequested;
            GameCommands.ReviveRequested -= OnReviveRequested;
            GameCommands.ReviveDeclined -= OnReviveDeclined;
            GameCommands.HomeRequested -= ReturnToTitle;
            if (playerHealth != null) playerHealth.OutOfLives -= OnOutOfLives;
            ReleaseAllPauses();
            Time.timeScale = 1f;
        }

        void Start()
        {
            Application.targetFrameRate = config.TargetFrameRate;
            ReleaseAllPauses();
            Time.timeScale = 1f;
            bool pending = GameSession.ConsumePendingStart();
            bool editorDirect = Application.isEditor && !GameSession.TitleVisited;
            if (config.StartImmediately || pending || editorDirect) BeginRun();
        }

        void BeginRun()
        {
            ReleaseAllPauses();
            Time.timeScale = 1f;
            flow.ReviveEnabled = config.RevivesPerRun > 0;
            flow.Begin(config.RevivesPerRun);
            playerHealth.ResetLives();
            scoreSystem.ResetScore();
            GameEvents.RaiseReviveAvailabilityChanged(flow.RevivesRemaining);
            GameEvents.RaiseGameStateChanged(GameState.Playing);
        }

        void OnStartGameRequested()
        {
            if (flow.State == GameState.Title) BeginRun();
        }

        void OnOutOfLives()
        {
            var s = flow.OnOutOfLives();
            if (s == GameState.RevivePrompt)
            {
                PushRevivePause();
                Time.timeScale = 0f;                 // truoc khi phat state: popup UI se khong tu freeze/restore
                GameEvents.RaiseGameStateChanged(s);
            }
            else if (s == GameState.GameOver) EndRun(s);
        }

        void OnLevelCompleted()
        {
            var s = flow.OnLevelCompleted();
            if (s == GameState.Win) EndRun(s);
        }

        void OnReviveRequested()
        {
            if (!flow.AcceptRevive()) return;
            PopRevivePause();
            Time.timeScale = 1f;
            playerHealth.ResetLives();
            float grace = config.ReviveGraceSeconds;
            playerHealth.GrantInvulnerability(grace);
            GameEvents.RaiseReviveAvailabilityChanged(flow.RevivesRemaining);
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            if (grace > 0f)
            {
                if (graceRoutine != null) StopCoroutine(graceRoutine);
                graceRoutine = StartCoroutine(GraceRoutine(grace));
            }
        }

        IEnumerator GraceRoutine(float seconds)
        {
            if (!gracePausePushed) { gracePausePushed = true; CombatPauseSignal.Push(GracePauseReason); }
            yield return new WaitForSecondsRealtime(seconds);
            graceRoutine = null;
            PopGracePause();
        }

        void OnReviveDeclined()
        {
            var s = flow.DeclineRevive();
            if (s != GameState.GameOver) return;
            PopRevivePause();
            Time.timeScale = 1f;
            EndRun(s);
        }

        void EndRun(GameState state)
        {
            if (!endPausePushed)
            {
                CombatPauseSignal.Push(EndPauseReason);
                endPausePushed = true;
            }
            GameEvents.RaiseGameStateChanged(state);
        }

        void OnRestartRequested()
        {
            ReleaseAllPauses();
            Time.timeScale = 1f;
            // Restart = nap lai scene gameplay hien tai va bat dau ngay (KHONG ve Title).
            // Snapshot ve Title chi de UI scene moi khong doc nham Win/GameOver luc OnEnable; scene moi Start() -> BeginRun phat Playing.
            GameSession.RequestStartOnLoad();
            flow.ReturnToTitle();
            GameEvents.RaiseGameStateChanged(GameState.Title);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>Ve man Title (huy luot choi). Chua co lenh Core tuong ung nen UI/debug goi truc tiep neu can.</summary>
        public void ReturnToTitle()
        {
            ReleaseAllPauses();
            Time.timeScale = 1f;
            flow.ReturnToTitle();
            GameEvents.RaiseGameStateChanged(GameState.Title);
            SceneManager.LoadScene(TitleScene);
        }

        // ---------------- pause helpers ----------------
        void PushRevivePause() { if (revivePausePushed) return; revivePausePushed = true; CombatPauseSignal.Push(RevivePauseReason); }
        void PopRevivePause() { if (!revivePausePushed) return; revivePausePushed = false; CombatPauseSignal.Pop(RevivePauseReason); }
        void PopGracePause() { if (!gracePausePushed) return; gracePausePushed = false; CombatPauseSignal.Pop(GracePauseReason); }
        void ReleaseEndPause()
        {
            if (!endPausePushed) return;
            endPausePushed = false;
            if (CombatPauseSignal.HasReason(EndPauseReason)) CombatPauseSignal.Pop(EndPauseReason);
        }

        void ReleaseAllPauses()
        {
            if (graceRoutine != null) { StopCoroutine(graceRoutine); graceRoutine = null; }
            ReleaseEndPause();
            if (revivePausePushed) { revivePausePushed = false; if (CombatPauseSignal.HasReason(RevivePauseReason)) CombatPauseSignal.Pop(RevivePauseReason); }
            if (gracePausePushed) { gracePausePushed = false; if (CombatPauseSignal.HasReason(GracePauseReason)) CombatPauseSignal.Pop(GracePauseReason); }
        }
    }
}
