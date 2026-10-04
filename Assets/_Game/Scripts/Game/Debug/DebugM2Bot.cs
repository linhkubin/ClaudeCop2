#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Combat;
using ClaudeCop.Camera;
using ClaudeCop.Jev;

namespace ClaudeCop.Game.Debugging
{
    /// <summary>
    /// T-403: bot kiem Play mode, chi gan luc runtime (KHONG luu trong scene). Ban qua TapShooter.FireAt (sau doan doc input),
    /// log ra file + Console, chup Game view. mode = Kill | Idle.
    /// </summary>
    public sealed class DebugM2Bot : MonoBehaviour
    {
        public string mode = "Kill";
        public float tapDelay = 0.5f, interval = 0.25f;
        public bool shootHostageOnce, pickupFirst = true, autoStart = true, shots = true;
        public string dir = "";
        public string logFile = "";

        TapShooter shooter;
        public bool autoFxTest; bool fxDone;
        IEnumerator DelayedFx() { yield return new WaitForSeconds(1.2f); yield return FxTest(); }
        float next; float fpsSum, fpsMin = 999f; int fpsFrames;
        string lastShot = ""; float lastShotT;
        int phase = -1; PhaseDirector pd;
        readonly List<ITapTarget> buf = new List<ITapTarget>();
        System.Action<int, string> hPhase; System.Action<EncounterBase> hMove, hStart, hClear; System.Action hLevel;
        System.Action<GameState> hState; System.Action<int, int> hLives; System.Action<int> hScore;
        System.Action<ShotResult> hRes; System.Action<int, int, WeaponKind> hAmmo; System.Action<WeaponKind> hWeapon;
        System.Action<int, float> hCombo; System.Action<DamageSource, Vector3, int> hDmg;
        System.Action<int, Vector3, bool, float> hAwarded; System.Action<JevDecision> hJev; System.Action<bool> hReload;

        public void Log(string m)
        {
            string line = "[M2 " + Time.realtimeSinceStartup.ToString("F2") + "|t" + Time.time.ToString("F2") + "] " + m;
            Debug.Log(line);
            if (!string.IsNullOrEmpty(logFile)) File.AppendAllText(logFile, line + "\n");
        }

        void OnEnable()
        {
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(RateProbe());
            hPhase = (i, t) => { phase = i; Log("PHASE " + i + " " + t); };
            hMove = e => Log("MOVE_SEGMENT next=" + (e ? e.Id : "none"));
            hStart = e =>
            {
                Log("ENC_START " + e.Id + " (" + e.Description + ")");
                if (shots && e.Id.Contains("W2")) StartCoroutine(Shot("p" + (phase + 1) + "_combat_" + e.Id, 2.2f));
                if (shots && e.Id.Contains("W5") && phase == 2) StartCoroutine(Shot("p3_final_" + e.Id, 2.5f));
            };
            hClear = e => Log("ENC_CLEARED " + e.Id);
            hLevel = () => Log("LEVEL_COMPLETED maxMoveRate=" + maxMoveRate.ToString("F1") + " maxBlendRate=" + maxBlendRate.ToString("F1") + " maxOtherRate=" + maxOtherRate.ToString("F1"));
            hState = s =>
            {
                Log("STATE " + s + " score=" + GameEvents.Current.Score + " lives=" + GameEvents.Current.Lives + " timeScale=" + Time.timeScale);
                if (!shots) return;
                if (s == GameState.Win) StartCoroutine(Shot("win", 1.2f));
                if (s == GameState.GameOver) StartCoroutine(Shot("gameover", 1.0f));
                if (s == GameState.RevivePrompt) StartCoroutine(Shot("revive", 1.0f));
            };
            hLives = (c, m) => Log("LIVES " + c + "/" + m);
            hScore = s => Log("SCORE " + s);
            hRes = r => Log("RESOLVED " + r.Outcome + " kind=" + r.TargetKind + " weapon=" + r.Weapon + " combo=x" + r.ComboMultiplier.ToString("F1") + " hits=" + r.TargetsHit + " prog=" + r.ReticleProgress.ToString("F2"));
            hAmmo = (c, m, w) => Log("AMMO " + c + "/" + m + " " + w);
            hWeapon = w => Log("WEAPON " + w);
            hCombo = (s, m) => Log("COMBO streak=" + s + " x" + m.ToString("F1"));
            hDmg = (src, p, left) => Log("DAMAGED " + src + " livesLeft=" + left);
            hAwarded = (pts, pos, j, mul) => Log("AWARD " + pts + " justice=" + j + " mul=x" + mul.ToString("F1"));
            hJev = d => Log("JEV " + d.Question + " choice=" + d.Choice + " conf=" + d.Confidence.ToString("F2") + " reticle=" + d.ReticleTime.ToString("F2") + " src=" + d.Source + " stats=" + d.Stats);
            hReload = b => Log("RELOADING " + b);
            RailEvents.PhaseStarted += hPhase; RailEvents.MoveSegmentStarted += hMove; RailEvents.EncounterStarted += hStart;
            RailEvents.EncounterCleared += hClear; RailEvents.LevelCompleted += hLevel;
            GameEvents.GameStateChanged += hState; GameEvents.LivesChanged += hLives; GameEvents.ScoreChanged += hScore;
            GameEvents.PlayerDamaged += hDmg; GameEvents.ScoreAwarded += hAwarded;
            CombatEvents.ShotResolved += hRes; CombatEvents.AmmoChanged += hAmmo; CombatEvents.WeaponChanged += hWeapon; CombatEvents.ComboChanged += hCombo;
            CombatEvents.ReloadStateChanged += hReload;
            JevDecisionLog.DecisionMade += hJev;
        }

        void OnDisable()
        {
            RailEvents.PhaseStarted -= hPhase; RailEvents.MoveSegmentStarted -= hMove; RailEvents.EncounterStarted -= hStart;
            RailEvents.EncounterCleared -= hClear; RailEvents.LevelCompleted -= hLevel;
            GameEvents.GameStateChanged -= hState; GameEvents.LivesChanged -= hLives; GameEvents.ScoreChanged -= hScore;
            GameEvents.PlayerDamaged -= hDmg; GameEvents.ScoreAwarded -= hAwarded;
            CombatEvents.ShotResolved -= hRes; CombatEvents.AmmoChanged -= hAmmo; CombatEvents.WeaponChanged -= hWeapon; CombatEvents.ComboChanged -= hCombo;
            CombatEvents.ReloadStateChanged -= hReload;
            JevDecisionLog.DecisionMade -= hJev;
        }

        IEnumerator Shot(string name, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (string.IsNullOrEmpty(dir)) yield break;
            ScreenCapture.CaptureScreenshot(dir + "/" + name + ".png");
            Log("SCREENSHOT " + name);
        }

        /// <summary>Ban thu FX theo chat lieu tren ray/tuong dang thay (dung trong doan Move). Log 'FXTEST'.</summary>
        public IEnumerator FxTest()
        {
            var cam = UnityEngine.Camera.main;
            var mats = new[] { SurfaceMaterial.Concrete, SurfaceMaterial.Wood, SurfaceMaterial.Metal, SurfaceMaterial.Glass, SurfaceMaterial.Foliage };
            foreach (var want in mats)
            {
                Vector2? pt = null;
                for (int x = 60; x < Screen.width - 60 && pt == null; x += 30)
                    for (int y = 60; y < Screen.height - 60 && pt == null; y += 30)
                    {
                        var ray = cam.ScreenPointToRay(new Vector2(x, y));
                        if (Physics.Raycast(ray, out var h, 60f))
                        {
                            var tg = h.collider.GetComponentInParent<SurfaceMaterialTag>();
                            if (tg != null && tg.Material == want) pt = new Vector2(x, y);
                        }
                    }
                if (pt == null) { Log("FXTEST " + want + " no surface visible"); continue; }
                float fmin = 999f;
                var before = new HashSet<int>();
                foreach (var ps in FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) before.Add(ps.GetInstanceID());
                FireScreen(pt.Value.x, pt.Value.y);
                var names = new HashSet<string>(); int maxParticles = 0, maxPlaying = 0, marks = 0;
                for (int f = 0; f < 20; f++)
                {
                    yield return null;
                    fmin = Mathf.Min(fmin, 1f / Time.unscaledDeltaTime);
                    int pc = 0, pl = 0;
                    foreach (var ps in FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                    {
                        pc += ps.particleCount; pl++;
                        if (!before.Contains(ps.GetInstanceID()) || f < 3) { var n = ps.transform.parent != null ? ps.transform.parent.name : ps.name; names.Add(n); }
                    }
                    maxParticles = Mathf.Max(maxParticles, pc); maxPlaying = Mathf.Max(maxPlaying, pl);
                }
                foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name.StartsWith("BulletMark") && t.gameObject.activeInHierarchy) marks++;
                Log("FXTEST " + want + " at " + pt.Value + " fx=[" + string.Join(",", names) + "] playingPS=" + maxPlaying + " maxParticles=" + maxParticles + " marks=" + marks + " minFps=" + fmin.ToString("F0") + " reduceMotion=" + UserSettings.ReduceMotion);
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Do toc do xoay chinh xac: lay mau cuoi frame (sau moi LateUpdate/Brain), goc giua 2 quaternion / unscaledDt.
        public float maxMoveRate, maxBlendRate, maxOtherRate;
        public IEnumerator RateProbe()
        {
            Quaternion last = Quaternion.identity; bool have = false; float lastT = 0f;
            var brain = FindFirstObjectByType<Unity.Cinemachine.CinemachineBrain>();
            while (true)
            {
                yield return new WaitForEndOfFrame();
                var cam = UnityEngine.Camera.main; if (cam == null) { have = false; continue; }
                if (brain == null) brain = FindFirstObjectByType<Unity.Cinemachine.CinemachineBrain>();
                var q = cam.transform.rotation; float t = Time.realtimeSinceStartup;
                if (have && t - lastT > 0.001f && t - lastT < 0.1f && Time.timeScale > 0f)
                {
                    float rate = Quaternion.Angle(last, q) / (t - lastT);
                    bool blending = brain != null && brain.IsBlending;
                    bool move = pd != null && pd.CurrentShot != null && pd.CurrentShot.kind == ShotKind.Move;
                    if (rate < 400f)
                    {
                        if (move && !blending) maxMoveRate = Mathf.Max(maxMoveRate, rate);
                        else if (blending) maxBlendRate = Mathf.Max(maxBlendRate, rate);
                        else maxOtherRate = Mathf.Max(maxOtherRate, rate);
                        if (rate > 62f) Log("ROT_RATE " + rate.ToString("F0") + " deg/s shot=" + (pd != null && pd.CurrentShot != null ? pd.CurrentShot.name : "-") + " blending=" + blending + " move=" + move + " dt=" + (t - lastT).ToString("F3"));
                    }
                    else Log("ROT_JUMP " + rate.ToString("F0") + " shot=" + (pd != null && pd.CurrentShot != null ? pd.CurrentShot.name : "-"));
                }
                last = q; lastT = t; have = true;
            }
        }

        public void Snap(string name) { StartCoroutine(Shot(name, 0f)); }

        /// <summary>Ban vao diem man hinh (thu FX tuong/thung/kinh).</summary>
        public void FireScreen(float x, float y) { if (shooter != null) shooter.FireAt(new Vector2(x, y)); }

        void Update()
        {
            if (Time.unscaledDeltaTime > 0f && Time.timeScale > 0f) { fpsSum += Time.unscaledDeltaTime; fpsFrames++; fpsMin = Mathf.Min(fpsMin, 1f / Time.unscaledDeltaTime); }
            if (shooter == null) shooter = FindFirstObjectByType<TapShooter>();
            if (pd == null) pd = FindFirstObjectByType<PhaseDirector>();
            if (pd != null && pd.CurrentShot != null && pd.CurrentShot.name != lastShot)
            {
                if (lastShot != "") Log("SHOT_END " + lastShot + " dur=" + (Time.realtimeSinceStartup - lastShotT).ToString("F2") + " avgFps=" + (fpsFrames / Mathf.Max(0.001f, fpsSum)).ToString("F0") + " minFps=" + fpsMin.ToString("F0"));
                fpsSum = 0f; fpsFrames = 0; fpsMin = 999f;
                lastShot = pd.CurrentShot.name; lastShotT = Time.realtimeSinceStartup;
                Log("SHOT_BEGIN " + lastShot);
                if (autoFxTest && !fxDone && pd.CurrentShot.kind == ShotKind.Move) { fxDone = true; StartCoroutine(DelayedFx()); }
            }
            if (autoStart && GameEvents.Current.State == GameState.Title && SceneNameIsTitle()) { autoStart = false; Log("START (GameCommands)"); GameCommands.RequestStartGame(); return; }
            if (mode != "Kill" || shooter == null || GameEvents.Current.State != GameState.Playing) return;
            if (CombatPauseSignal.IsPaused || Time.time < next) return;

            var cs = CombatEvents.Current;
            if (cs.Ammo <= 0 && !cs.Reloading) { Log("BOT RELOAD"); GameCommands.RequestReload(); next = Time.time + 0.3f; return; }
            if (cs.Reloading) return;
            buf.Clear();
            for (int i = 0; i < TargetRegistry.Targets.Count; i++) buf.Add(TargetRegistry.Targets[i]);
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;
            ITapTarget pick = null;
            foreach (var t in buf)
            {
                if (t == null || !t.IsTargetable) continue;
                if (t.Kind == TargetKind.Hostage && shootHostageOnce && t.ExposedTime > 0.3f) { pick = t; shootHostageOnce = false; Log("SHOOTING HOSTAGE on purpose"); break; }
                if (t.Kind == TargetKind.Pickup && pickupFirst) { pick = t; break; }
            }
            if (pick == null)
                foreach (var t in buf) if (t != null && t.IsTargetable && t.Kind == TargetKind.Enemy && t.ExposedTime >= tapDelay) { pick = t; break; }
            if (pick == null) return;
            var sp = cam.WorldToScreenPoint(pick.HasJusticePoint && pick.Kind == TargetKind.Enemy ? pick.JusticePoint : pick.AimPoint);
            if (sp.z <= 0f) return;
            shooter.FireAt(new Vector2(sp.x, sp.y));
            next = Time.time + interval;
        }

        static bool SceneNameIsTitle() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Title";
    }
}
#endif
