using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game.Tests
{
    /// <summary>
    /// F-208: test glue GameManager trong EditMode (OnEnable goi bang reflection vi MonoBehaviour khong tu chay OnEnable o edit mode).
    /// Khong kiem duoc o EditMode: Restart (SceneManager.LoadScene), coroutine ReviveGrace (WaitForSecondsRealtime), PhaseDirector -> can PlayMode/bam tay.
    /// </summary>
    public class GameManagerTests
    {
        GameObject go;
        GameManager gm;
        PlayerHealth health;
        ScoreSystem score;
        GameConfig cfg;

        static void Set(object o, string field, object v) =>
            o.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(o, v);
        static void Call(object o, string m) =>
            o.GetType().GetMethod(m, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(o, null);

        [SetUp]
        public void Setup()
        {
            typeof(CombatPauseSignal).GetMethod("ResetStatics", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            Time.timeScale = 1f;
            GameEvents.RaiseGameStateChanged(GameState.Title);
            cfg = ScriptableObject.CreateInstance<GameConfig>();
            Set(cfg, "startImmediately", false);
            Set(cfg, "revivesPerRun", 1);
            Set(cfg, "reviveGraceSeconds", 0f); // khong dung coroutine
            go = new GameObject("gm");
            health = go.AddComponent<PlayerHealth>(); Set(health, "config", cfg);
            score = go.AddComponent<ScoreSystem>(); Set(score, "config", cfg);
            gm = go.AddComponent<GameManager>();
            Set(gm, "config", cfg); Set(gm, "playerHealth", health); Set(gm, "scoreSystem", score);
            Call(score, "OnEnable");
            Call(gm, "OnEnable");
        }

        [TearDown]
        public void Teardown()
        {
            Call(gm, "OnDisable");
            Call(score, "OnDisable");
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(cfg);
            Time.timeScale = 1f;
        }

        void LoseAllLives()
        {
            for (int i = 0; i < 3; i++)
            {
                health.Damage(DamageSource.EnemyShot, Vector3.zero);
                health.GrantInvulnerability(-1f); // bo bat tu de dame lien tiep
            }
        }

        [Test]
        public void Start_EntersPlaying_FullLives()
        {
            GameCommands.RequestStartGame();
            Assert.AreEqual(GameState.Playing, gm.State);
            Assert.AreEqual(cfg.MaxLives, health.Lives);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void OutOfLives_FirstTime_FreezesAndPushesRevivePause()
        {
            GameCommands.RequestStartGame();
            LoseAllLives();
            Assert.AreEqual(GameState.RevivePrompt, gm.State);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(CombatPauseSignal.HasReason(GameManager.RevivePauseReason));
        }

        [Test]
        public void Revive_RestoresTimeScale_PopsPause_RefillsLives()
        {
            GameCommands.RequestStartGame();
            LoseAllLives();
            GameCommands.RequestRevive();
            Assert.AreEqual(GameState.Playing, gm.State);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(CombatPauseSignal.HasReason(GameManager.RevivePauseReason));
            Assert.AreEqual(cfg.MaxLives, health.Lives);
        }

        [Test]
        public void Revive_OnlyOncePerRun_SecondDeathIsGameOverWithEndPause()
        {
            GameCommands.RequestStartGame();
            LoseAllLives();
            GameCommands.RequestRevive();
            LoseAllLives();
            Assert.AreEqual(GameState.GameOver, gm.State);
            Assert.IsTrue(CombatPauseSignal.HasReason(GameManager.EndPauseReason));
        }

        [Test]
        public void DeclineRevive_GameOver_RestoresTimeScale()
        {
            GameCommands.RequestStartGame();
            LoseAllLives();
            GameCommands.DeclineRevive();
            Assert.AreEqual(GameState.GameOver, gm.State);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(CombatPauseSignal.HasReason(GameManager.RevivePauseReason));
            Assert.IsTrue(CombatPauseSignal.HasReason(GameManager.EndPauseReason));
        }

        [Test]
        public void LevelCompleted_Win_PushesEndPause()
        {
            GameCommands.RequestStartGame();
            RailEvents.RaiseLevelCompleted();
            Assert.AreEqual(GameState.Win, gm.State);
            Assert.IsTrue(CombatPauseSignal.HasReason(GameManager.EndPauseReason));
        }

        [Test]
        public void OnDisable_ReleasesAllPauses_AndTimeScale()
        {
            GameCommands.RequestStartGame();
            LoseAllLives(); // timeScale 0 + revive pause
            Call(gm, "OnDisable");
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(0, CombatPauseSignal.Count);
            Call(gm, "OnEnable"); // de Teardown goi OnDisable can doi
        }
    }
}
