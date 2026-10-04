#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Combat;

namespace ClaudeCop.Game.Debugging
{
    /// <summary>
    /// Chi de kiem tra tu dong viec ghep scene Level_01 (them luc runtime, KHONG luu vao scene). Ghi log tien to [PT].
    /// Kich ban: (1) ban pause: Push ly do gia, kiem tra vong target dung va tap bi chan; (2) Idle den GameOver, Restart; (3) Kill: tap that qua TapShooter.
    /// </summary>
    public sealed class DebugPlaytest : MonoBehaviour
    {
        public string shotDir = "";
        public bool tapEnemies;          // Kill mode
        public bool autoRestartOnGameOver = true;
        public bool testPauseOnce = true;
        public bool testReloadOnce = true;
        public float tapDelay = 0.6f;
        public float tapInterval = 0.3f;

        TapShooter shooter;
        MethodInfo tryFire;
        float nextTap;
        bool pauseTested, reloadTested, shotS2, shotS3, shotS5;
        int kills;
        readonly List<ITapTarget> buf = new List<ITapTarget>();

        void Log(string m) { Debug.Log("[PT " + Time.time.ToString("F2") + "] " + m); }

        // Handler luu thanh field de huy dang ky duoc (lambda moi khong -= duoc; object bi destroy van con listener).
        System.Action<GameState> hState;
        System.Action<int, int> hLives;
        System.Action<DamageSource, Vector3, int> hDamaged;
        System.Action<int> hScore;
        System.Action<int, string> hPhase;
        System.Action<EncounterBase> hMove, hEncStart, hEncClear;
        System.Action hLevel;
        System.Action<WeaponKind, Vector2> hFired;
        System.Action<ShotResult> hResolved;
        System.Action<bool> hReload, hPause;

        void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
            tryFire = typeof(TapShooter).GetMethod("TryFire", BindingFlags.NonPublic | BindingFlags.Instance);
            hState = s => { Log("STATE " + s + " score=" + GameEvents.Current.Score + " lives=" + GameEvents.Current.Lives); OnState(s); };
            hLives = (c, m) => Log("LIVES " + c + "/" + m);
            hDamaged = (src, p, left) => Log("DAMAGED by " + src + " left=" + left);
            hScore = s => Log("SCORE " + s);
            hPhase = (i, t) => Log("PHASE " + i + " " + t);
            hMove = e => Log("MOVE (next=" + (e != null ? e.name : "none") + ")");
            hEncStart = e => { Log("ENC START " + e.name); if (e.name.Contains("W5")) { shotS5 = true; shotS2 = false; } if (e.name.Contains("W3")) StartCoroutine(Shot("s3_angle", 1.6f)); };
            hEncClear = e => Log("ENC CLEARED " + e.name);
            hLevel = () => Log("LEVEL COMPLETED");
            hFired = (w, p) => Log("SHOT " + w + " ammo(before event)=" + CombatEvents.Current.Ammo);
            hResolved = r => Log("RESOLVED " + r.Outcome + " kind=" + r.TargetKind + " prog=" + r.ReticleProgress.ToString("F2"));
            hReload = b => Log("RELOADING " + b);
            hPause = p => Log("PAUSE " + p + (CombatPauseSignal.HasReason("CameraBlend") ? " (CameraBlend)" : ""));
            GameEvents.GameStateChanged += hState;
            GameEvents.LivesChanged += hLives;
            GameEvents.PlayerDamaged += hDamaged;
            GameEvents.ScoreChanged += hScore;
            RailEvents.PhaseStarted += hPhase;
            RailEvents.MoveSegmentStarted += hMove;
            RailEvents.EncounterStarted += hEncStart;
            RailEvents.EncounterCleared += hEncClear;
            RailEvents.LevelCompleted += hLevel;
            CombatEvents.ShotFired += hFired;
            CombatEvents.ShotResolved += hResolved;
            CombatEvents.ReloadStateChanged += hReload;
            CombatPauseSignal.Changed += hPause;
        }

        void OnDisable()
        {
            GameEvents.GameStateChanged -= hState;
            GameEvents.LivesChanged -= hLives;
            GameEvents.PlayerDamaged -= hDamaged;
            GameEvents.ScoreChanged -= hScore;
            RailEvents.PhaseStarted -= hPhase;
            RailEvents.MoveSegmentStarted -= hMove;
            RailEvents.EncounterStarted -= hEncStart;
            RailEvents.EncounterCleared -= hEncClear;
            RailEvents.LevelCompleted -= hLevel;
            CombatEvents.ShotFired -= hFired;
            CombatEvents.ShotResolved -= hResolved;
            CombatEvents.ReloadStateChanged -= hReload;
            CombatPauseSignal.Changed -= hPause;
        }

        void OnState(GameState s)
        {
            if (s == GameState.GameOver) { StartCoroutine(Shot("gameover", 0.8f)); if (autoRestartOnGameOver) StartCoroutine(RestartLater()); }
            if (s == GameState.Win) StartCoroutine(Shot("win", 0.8f));
            if (s == GameState.Playing) { shotS2 = false; }
        }

        IEnumerator RestartLater()
        {
            yield return new WaitForSecondsRealtime(2.0f);
            Log("REQUEST RESTART");
            tapEnemies = true; // sau restart: choi that
            GameCommands.RequestRestart();
        }

        IEnumerator Shot(string name, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (string.IsNullOrEmpty(shotDir)) yield break;
            ScreenCapture.CaptureScreenshot(shotDir + "/" + name + ".png");
            Log("SCREENSHOT " + name);
        }

        void Update()
        {
            if (shooter == null) shooter = FindFirstObjectByType<TapShooter>();
            if (shooter == null || tryFire == null) return;

            // Tim muc tieu enemy ldang lo ra
            ITapTarget first = null;
            buf.Clear();
            for (int i = 0; i < TargetRegistry.Targets.Count; i++) buf.Add(TargetRegistry.Targets[i]);
            foreach (var t in buf) if (t != null && t.Kind == TargetKind.Enemy && t.ExposedTime > 0f) { first = t; break; }

            if (first != null && !shotS2 && first.ExposedTime > 1.2f && GameEvents.Current.State == GameState.Playing)
            {
                shotS2 = true;
                StartCoroutine(Shot(shotS5 ? "s5_combat" : "s2_combat", 0f));
                if (!shotS5 && tapEnemies) { }
            }

            if (testPauseOnce && !pauseTested && first != null && first.ExposedTime > 0.8f && GameEvents.Current.State == GameState.Playing)
            {
                pauseTested = true;
                StartCoroutine(PauseTest(first));
            }

            if (tapEnemies && !CombatPauseSignal.IsPaused && Time.time >= nextTap && GameEvents.Current.State == GameState.Playing)
            {
                foreach (var t in buf)
                {
                    if (t == null || !t.IsTargetable || t.Kind != TargetKind.Enemy || t.ExposedTime < tapDelay) continue;
                    var cam = UnityEngine.Camera.main;
                    var sp = cam.WorldToScreenPoint(t.AimPoint);
                    if (sp.z <= 0f) continue;
                    if (testReloadOnce && !reloadTested && CombatEvents.Current.Ammo <= 3)
                    {
                        reloadTested = true;
                        int scoreBefore = GameEvents.Current.Score;
                        Log("RELOAD TEST: ammo=" + CombatEvents.Current.Ammo + " score=" + scoreBefore);
                        GameCommands.RequestReload();
                        StartCoroutine(ReloadCheck(scoreBefore));
                        nextTap = Time.time + 1.0f;
                        break;
                    }
                    tryFire.Invoke(shooter, new object[] { new Vector2(sp.x, sp.y) });
                    nextTap = Time.time + tapInterval;
                    break;
                }
            }
        }

        IEnumerator ReloadCheck(int scoreBefore)
        {
            yield return new WaitForSecondsRealtime(1.0f);
            Log("RELOAD RESULT: ammo=" + CombatEvents.Current.Ammo + " reloading=" + CombatEvents.Current.Reloading + " score=" + GameEvents.Current.Score + " (was " + scoreBefore + ")");
        }

        IEnumerator PauseTest(ITapTarget t)
        {
            float p0 = t.ReticleProgress;
            int ammo0 = CombatEvents.Current.Ammo;
            CombatPauseSignal.Push("CameraBlend");
            yield return new WaitForSecondsRealtime(0.8f);
            float p1 = t.ReticleProgress;
            var cam = UnityEngine.Camera.main;
            var sp = cam.WorldToScreenPoint(t.AimPoint);
            tryFire.Invoke(shooter, new object[] { new Vector2(sp.x, sp.y) });
            int ammo1 = CombatEvents.Current.Ammo;
            CombatPauseSignal.Pop("CameraBlend");
            Log("PAUSE TEST: reticle " + p0.ToString("F3") + " -> " + p1.ToString("F3") + " (dung neu bang nhau); tap khi pause: ammo " + ammo0 + " -> " + ammo1 + " (chan neu bang nhau)");
        }
    }
}
#endif
