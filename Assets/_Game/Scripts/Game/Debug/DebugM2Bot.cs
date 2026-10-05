#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Combat;
using ClaudeCop.Camera;
using ClaudeCop.RankScore;

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
        [Tooltip("W7: ban thung no (ExplosiveBarrel) khi nhin thay, moi thung 1 lan.")] public bool shootBarrel;
        [Tooltip("W7: ban roi luu dan dang bay.")] public bool shootGrenade;
        public bool barrelNeedsEnemy = true, pullBarrel;
        [Tooltip("W7: khong ban Grenadier de no nem luu dan (tu tat sau khi da thu bo mac + ban roi).")] public bool grenadierWait;
        [Tooltip("W7: bo mac luu dan (khong ban) de mat 1 mang.")] public bool ignoreGrenade;
        public bool shootHostageOnce, pickupFirst = true, autoStart = true, shots = true;
        [Tooltip("Bot do: tat Giam chuyen dong luc runtime (khong ghi PlayerPrefs) de do cam giac day du.")] public bool reduceMotionOff = false;
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
        System.Action<int, Vector3, bool, float> hAwarded; System.Action<RankScoreDecision> hRankScore; System.Action<bool> hReload; System.Action<BlastReport> hBlast;

        public void Log(string m)
        {
            string line = "[M2 " + Time.realtimeSinceStartup.ToString("F2") + "|t" + Time.time.ToString("F2") + "] " + m;
            Debug.Log(line);
            if (!string.IsNullOrEmpty(logFile)) File.AppendAllText(logFile, line + "\n");
        }

        void OnEnable()
        {
            Application.runInBackground = true;
            if (reduceMotionOff) UserSettings.SetRuntimeOnly(false);
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
            hRes = r => { if (r.TargetKind == TargetKind.Grenade && r.Outcome == TapOutcome.Kill) grenadierWait = false; Log("RESOLVED " + r.Outcome + " kind=" + r.TargetKind + " weapon=" + r.Weapon + " combo=x" + r.ComboMultiplier.ToString("F1") + " hits=" + r.TargetsHit + " prog=" + r.ReticleProgress.ToString("F2")); };
            hAmmo = (c, m, w) => Log("AMMO " + c + "/" + m + " " + w);
            hWeapon = w => Log("WEAPON " + w);
            hCombo = (s, m) => Log("COMBO streak=" + s + " x" + m.ToString("F1"));
            hDmg = (src, p, left) => { Log("DAMAGED " + src + " livesLeft=" + left); if (src == DamageSource.Explosion) ignoreGrenade = false; };
            hAwarded = (pts, pos, j, mul) => Log("AWARD " + pts + " justice=" + j + " mul=x" + mul.ToString("F1"));
            hRankScore = d => Log("RANKSCORE " + d.Question + " choice=" + d.Choice + " conf=" + d.Confidence.ToString("F2") + " reticle=" + d.ReticleTime.ToString("F2") + " src=" + d.Source + " stats=" + d.Stats);
            hBlast = b => Log("BLAST kills=" + b.EnemiesKilled + " hostages=" + b.HostagesHit + " center=" + b.Center + " r=" + b.Radius);
            hReload = b => Log("RELOADING " + b);
            RailEvents.PhaseStarted += hPhase; RailEvents.MoveSegmentStarted += hMove; RailEvents.EncounterStarted += hStart;
            RailEvents.EncounterCleared += hClear; RailEvents.LevelCompleted += hLevel;
            GameEvents.GameStateChanged += hState; GameEvents.LivesChanged += hLives; GameEvents.ScoreChanged += hScore;
            GameEvents.PlayerDamaged += hDmg; GameEvents.ScoreAwarded += hAwarded;
            CombatEvents.ShotResolved += hRes; CombatEvents.AmmoChanged += hAmmo; CombatEvents.WeaponChanged += hWeapon; CombatEvents.ComboChanged += hCombo;
            CombatEvents.ReloadStateChanged += hReload;
            RankScoreDecisionLog.DecisionMade += hRankScore; BlastEvents.Blasted += hBlast;
        }

        void OnDisable()
        {
            RailEvents.PhaseStarted -= hPhase; RailEvents.MoveSegmentStarted -= hMove; RailEvents.EncounterStarted -= hStart;
            RailEvents.EncounterCleared -= hClear; RailEvents.LevelCompleted -= hLevel;
            GameEvents.GameStateChanged -= hState; GameEvents.LivesChanged -= hLives; GameEvents.ScoreChanged -= hScore;
            GameEvents.PlayerDamaged -= hDmg; GameEvents.ScoreAwarded -= hAwarded;
            CombatEvents.ShotResolved -= hRes; CombatEvents.AmmoChanged -= hAmmo; CombatEvents.WeaponChanged -= hWeapon; CombatEvents.ComboChanged -= hCombo;
            CombatEvents.ReloadStateChanged -= hReload;
            RankScoreDecisionLog.DecisionMade -= hRankScore; BlastEvents.Blasted -= hBlast;
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
            if (pick == null && !ignoreGrenade && shootGrenade)
                foreach (var t in buf) if (t != null && t.IsTargetable && t.Kind == TargetKind.Grenade) { pick = t; Log("BOT SHOOT GRENADE"); break; }
            if (pick == null && shootBarrel && TryShootBarrel(cam)) { next = Time.time + interval; return; }
            if (pick == null)
                foreach (var t in buf) if (t != null && t.IsTargetable && t.Kind == TargetKind.Enemy && t.ExposedTime >= tapDelay && !(grenadierWait && IsGrenadier(t)) && !HoldForBarrel(t)) { pick = t; break; }
            if (pick == null) return;
            var sp = cam.WorldToScreenPoint(pick.HasJusticePoint && pick.Kind == TargetKind.Enemy ? pick.JusticePoint : pick.AimPoint);
            if (sp.z <= 0f) return;
            shooter.FireAt(new Vector2(sp.x, sp.y));
            next = Time.time + interval;
        }

        readonly HashSet<int> shotBarrels = new HashSet<int>(), pulled = new HashSet<int>(), occludedLogged = new HashSet<int>(), occludedNow = new HashSet<int>();
        float barrelScan; MonoBehaviour[] barrelCache;

        bool TryShootBarrel(UnityEngine.Camera cam)
        {
            if (Time.time >= barrelScan) { barrelScan = Time.time + 1f; barrelCache = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None); }
            if (barrelCache == null) return false;
            foreach (var m in barrelCache)
            {
                if (m == null || !m.gameObject.activeInHierarchy || (m.GetType().Name != "ExplosiveBarrel" && m.GetType().Name != "BreakableGlass")) continue;
                if (m.GetType().Name == "ExplosiveBarrel" && barrelNeedsEnemy)
                {
                    ITapTarget nearT = null; float nd = 99f;
                    foreach (var t in buf) if (t != null && t.IsTargetable && t.Kind == TargetKind.Enemy) { float d = (t.AimPoint - BarrelCenter(m)).magnitude; if (d < nd) { nd = d; nearT = t; } }
                    if (nearT == null || nd > 2.9f || nearT.ExposedTime < 0.15f) continue;
                    if (nd > 2.4f && pullBarrel && pulled.Add(m.GetInstanceID()))
                    {
                        var dv = nearT.AimPoint - m.transform.position; dv.y = 0f;
                        Log("BOT PULL BARREL by " + (dv.magnitude * 0.4f).ToString("F2") + "m (runtime only, marker dist=" + nd.ToString("F2") + ")");
                        m.transform.position += dv.normalized * (dv.magnitude * 0.4f);
                    }
                }
                int id = m.GetInstanceID();
                if (shotBarrels.Contains(id)) continue;
                var col = m.GetComponentInChildren<Collider>();
                Vector3 wp = col != null ? col.bounds.center : m.transform.position;
                var sp = cam.WorldToScreenPoint(wp);
                if (sp.z <= 0f || sp.x < 0f || sp.y < 0f || sp.x > Screen.width || sp.y > Screen.height) continue;
                if (sp.z > 25f) continue;
                if (Physics.Raycast(cam.transform.position, (wp - cam.transform.position).normalized, out var rh, sp.z + 1f, ~0, QueryTriggerInteraction.Collide)
                    && rh.collider.GetComponentInParent<MonoBehaviour>() is MonoBehaviour hm && hm != m && rh.collider.transform.root != m.transform.root && !rh.collider.transform.IsChildOf(m.transform))
                {
                    occludedNow.Add(id); if (occludedLogged.Add(id)) Log("BARREL OCCLUDED " + m.name + " by " + rh.collider.name + " dist=" + rh.distance.ToString("F1") + " camPos=" + cam.transform.position + " shot=" + lastShot);
                    continue;
                }
                occludedNow.Remove(id);
                shotBarrels.Add(id);
                Log("BOT SHOOT BARREL " + m.name);
                if (shots) StartCoroutine(Shot("prop_" + m.name, 0.3f));
                shooter.FireAt(new Vector2(sp.x, sp.y));
                return true;
            }
            return false;
        }

        static Vector3 BarrelCenter(MonoBehaviour m) { var c = m.GetComponentInChildren<Collider>(); return c != null ? c.bounds.center : m.transform.position; }
        bool HoldForBarrel(ITapTarget t)
        {
            if (!shootBarrel || !barrelNeedsEnemy || barrelCache == null || t.ExposedTime > 1.0f) return false;
            foreach (var m in barrelCache)
                if (m != null && m.gameObject.activeInHierarchy && m.GetType().Name == "ExplosiveBarrel" && !shotBarrels.Contains(m.GetInstanceID()) && !occludedNow.Contains(m.GetInstanceID())
                    && (t.AimPoint - BarrelCenter(m)).magnitude < 2.9f) return true;
            return false;
        }
        static bool IsGrenadier(ITapTarget t) { var c = t as Component; return c != null && c.name.Contains("Grenadier"); }
        static bool SceneNameIsTitle() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Title";
    }
}
#endif
