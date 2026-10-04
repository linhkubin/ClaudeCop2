using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using ClaudeCop.Core;

namespace ClaudeCop.Camera
{
    [Serializable]
    public class RailPhase
    {
        public string title = "Phase";
        public List<CameraShot> shots = new List<CameraShot>();
    }

    /// <summary>
    /// Chay man choi: Phase -> Shot tuan tu. Move: di theo spline (ease) roi sang Shot ke. Combat: Begin encounter, cho Cleared,
    /// nghi restAfterClear, sang Shot ke. Raise RailEvents. Blend/Cut giua cac Shot va goc phu: CombatPauseSignal.Push("CameraBlend").
    /// Rig: 1 CinemachineBrain tren Main Camera; 1 CinemachineCamera "Rail" (SplineDolly) tai su dung cho moi Shot Move
    /// (doi Spline); moi Shot Combat (va goc phu) co CinemachineCamera rieng, chuyen bang Priority (10 = active, 0 = khac).
    /// </summary>
    public class PhaseDirector : MonoBehaviour
    {
        public const string PauseReason = "CameraBlend";
        const int ActivePriority = 10;

        [Header("Rig")]
        public CameraFeelProfile profile;
        public CinemachineBrain brain;
        [Tooltip("CinemachineCamera co CinemachineSplineDolly, dung chung cho cac Shot Move")] public CinemachineCamera railCamera;

        [Header("Man choi")]
        public List<RailPhase> phases = new List<RailPhase>();
        [Tooltip("Sandbox: tu chay o Start()")] public bool autoStart = false;
        [Tooltip("Tu chay khi GameEvents.GameStateChanged -> Playing lan dau (khong chay lai sau Revive)")] public bool startOnGamePlaying = true;

        RailCameraDriver driver;
        CinemachineVirtualCameraBase active;
        Coroutine routine;
        bool started;
        EncounterBase activeEnc;
        Action<EncounterBase, Vector3> activeOnCleared;
        bool pauseHeld;
        bool initialized;
        bool forceCutNext;          // shot dau cua Phase moi: Cut trong luc man den
        float transitionRemaining;  // giay unscaled con phai cho (hold + fadeIn) sau khi cut sang Phase moi
        bool transitionPending;

        public bool IsRunning => routine != null;
        public bool LevelFinished { get; private set; }
        public int CurrentPhaseIndex { get; private set; } = -1;
        public CameraShot CurrentShot { get; private set; }

        // ---------------- API cho Game / ghep scene ----------------
        /// <summary>Bat dau chay Phase 0. Chi chay mot lan.</summary>
        public void StartLevel()
        {
            if (started) return;
            Init();
            started = true;
            routine = StartCoroutine(Run());
        }

        void Awake() { Init(); }

        void Init()
        {
            if (initialized) return;
            initialized = true;
            if (profile == null) { Debug.LogError("[PhaseDirector] Thieu CameraFeelProfile.", this); profile = ScriptableObject.CreateInstance<CameraFeelProfile>(); }
            if (brain == null) brain = FindFirstObjectByType<CinemachineBrain>();
            if (brain == null) Debug.LogError("[PhaseDirector] Khong tim thay CinemachineBrain.", this);
            if (railCamera != null)
            {
                var dolly = railCamera.GetComponent<CinemachineSplineDolly>();
                if (dolly == null) Debug.LogError("[PhaseDirector] railCamera thieu CinemachineSplineDolly.", railCamera);
                else driver = new RailCameraDriver(railCamera, dolly, profile);
                var ap = railCamera.GetComponent<CameraFeelApplier>();
                if (ap == null) ap = railCamera.gameObject.AddComponent<CameraFeelApplier>();
                ap.profile = profile;
                railCamera.Priority.Enabled = true; railCamera.Priority.Value = 0;
            }
            for (int i = 0; i < phases.Count; i++)
                for (int j = 0; j < phases[i].shots.Count; j++)
                    if (phases[i].shots[j] != null) phases[i].shots[j].EnsureCameras(profile);
        }

        void Start()
        {
            if (autoStart || (startOnGamePlaying && GameEvents.Current.State == GameState.Playing)) StartLevel();
        }

        void OnEnable()
        {
            GameEvents.PlayerDamaged += OnPlayerDamaged;
            GameEvents.GameStateChanged += OnGameStateChanged;
            CombatEvents.ShotResolved += OnShotResolved;
        }

        void OnDisable()
        {
            GameEvents.PlayerDamaged -= OnPlayerDamaged;
            GameEvents.GameStateChanged -= OnGameStateChanged;
            CombatEvents.ShotResolved -= OnShotResolved;
            ReleasePause();
            ReleaseTransitionPause();
            SlowZoom.EndKill();
            // F-205: coroutine bi Unity dung ngam khi disable -> don dang ky, cho phep StartLevel lai.
            if (activeEnc != null && activeOnCleared != null) activeEnc.Cleared -= activeOnCleared;
            activeEnc = null; activeOnCleared = null;
            routine = null;
            started = false;
        }

        void OnShotResolved(ShotResult r)
        {
            if (profile == null || UserSettings.ReduceMotion) return;
            if (r.Outcome == TapOutcome.Kill || r.Outcome == TapOutcome.JusticeKill) SlowZoom.Punch(profile, Time.time);
        }

        void OnGameStateChanged(GameState s)
        {
            if (startOnGamePlaying && s == GameState.Playing && !started) StartLevel();
        }

        void OnPlayerDamaged(DamageSource src, Vector3 pos, int livesLeft)
        {
            if (profile != null) CameraFeelState.TriggerShake(profile.hitShakeDuration);
        }

        void Update()
        {
            if (profile == null) return;
            driver?.Tick(Time.deltaTime);
            CameraFeelState.MoveSpeed = driver != null && driver.Active ? driver.CurrentSpeed : 0f;
            CameraFeelState.Tick(Time.deltaTime, profile);
        }

        // ---------------- Luong chay ----------------
        IEnumerator Run()
        {
            for (int pi = 0; pi < phases.Count; pi++)
            {
                CurrentPhaseIndex = pi;
                var phase = phases[pi];
                RailEvents.RaisePhaseStarted(pi, phase.title);
                if (pi > 0) yield return BeginPhaseTransition(phase.title);

                ShotKind prevKind = ShotKind.Move;
                for (int si = 0; si < phase.shots.Count; si++)
                {
                    var shot = phase.shots[si];
                    if (shot == null) { Debug.LogWarning("[PhaseDirector] Shot null o Phase " + pi + " #" + si, this); continue; }
                    CurrentShot = shot;
                    if (shot.kind == ShotKind.Move) yield return RunMove(shot, pi, si);
                    else yield return RunCombat(shot, prevKind);
                    prevKind = shot.kind;
                }
                if (transitionPending) yield return FinishPhaseTransition(); // Phase khong co Shot chay duoc
            }
            CurrentShot = null;
            LevelFinished = true;
            routine = null;
            RailEvents.RaiseLevelCompleted();
        }

        /// <summary>
        /// Chuyen Phase: phat PhaseTransition (UI fade den + tieu de), CombatPauseSignal "PhaseTransition", cho fadeOut (unscaled).
        /// Shot dau cua Phase moi se Cut sang vi tri moi luc man den roi goi FinishPhaseTransition (cho hold + fadeIn).
        /// </summary>
        IEnumerator BeginPhaseTransition(string title)
        {
            bool reduce = UserSettings.ReduceMotion;
            float fo = reduce ? profile.reduceMotionFade : profile.phaseFadeOut;
            float hold = profile.phaseHold;
            float fi = reduce ? profile.reduceMotionFade : profile.phaseFadeIn;
            PushTransitionPause();
            RailEvents.RaisePhaseTransition(title, fo, hold, fi);
            yield return new WaitForSecondsRealtime(fo);
            forceCutNext = true;
            transitionRemaining = hold + fi;
            transitionPending = true;
        }

        IEnumerator FinishPhaseTransition()
        {
            transitionPending = false;
            if (transitionRemaining > 0f) yield return new WaitForSecondsRealtime(transitionRemaining);
            transitionRemaining = 0f;
            ReleaseTransitionPause();
        }

        bool transitionPauseHeld;
        public const string TransitionPauseReason = "PhaseTransition";
        void PushTransitionPause() { if (transitionPauseHeld) return; transitionPauseHeld = true; CombatPauseSignal.Push(TransitionPauseReason); }
        void ReleaseTransitionPause() { if (!transitionPauseHeld) return; transitionPauseHeld = false; CombatPauseSignal.Pop(TransitionPauseReason); }

        IEnumerator RunMove(CameraShot shot, int pi, int si)
        {
            if (shot.spline == null || driver == null)
            {
                Debug.LogError("[PhaseDirector] Shot Move '" + shot.name + "' thieu spline hoac railCamera.", shot);
                yield break;
            }
            CameraFeelState.Combat = false;
            RailEvents.RaiseMoveSegmentStarted(FindUpcomingEncounter(pi, si));

            bool hasPrev = active != null && brain != null;
            Quaternion fromRot = hasPrev ? brain.transform.rotation : Quaternion.identity;
            driver.Prepare(shot.spline, shot.speedOverride, hasPrev ? fromRot : Quaternion.LookRotation(Vector3.forward));
            Vector3 startFwd = driver.StartTangent();
            if (!hasPrev) driver.Prepare(shot.spline, shot.speedOverride, Quaternion.LookRotation(startFwd));

            bool cut = !hasPrev || forceCutNext || ShouldCut(fromRot * Vector3.forward, startFwd, shot.entry);
            forceCutNext = false;
            // Cut: camera ray dat thang huong tiep tuyen (khong xoay tu huong cu, tranh lia vuot gioi han sau Cut)
            if (cut && hasPrev) driver.Prepare(shot.spline, shot.speedOverride, Quaternion.LookRotation(startFwd));
            float bt = shot.ResolveBlend(profile);
            Activate(railCamera, cut, bt);
            yield return WaitTransition(cut, bt);
            SlowZoom.EndKill(); // camera cu da blend xong
            if (transitionPending) yield return FinishPhaseTransition();

            driver.StartMoving();
            while (!driver.Finished) yield return null;
        }

        IEnumerator RunCombat(CameraShot shot, ShotKind prevShotKind)
        {
            var cam = shot.VCam;
            if (cam == null) { Debug.LogError("[PhaseDirector] Shot Combat '" + shot.name + "' thieu CinemachineCamera.", shot); yield break; }
            driver?.Deactivate();
            // Camera ray khong con la live sau Activate nen giu driver o trang thai cuoi cho blend: chi dung Tick khi Active.
            CameraFeelState.Combat = true;
            CameraFeelState.ResetDolly();

            bool hasPrev = active != null && brain != null;
            bool cut = !hasPrev || forceCutNext || ShouldCut(brain.transform.forward, shot.transform.forward, shot.entry);
            forceCutNext = false;
            float bt = shot.ResolveBlend(profile);
            Activate(cam, cut, bt);
            yield return WaitTransition(cut, bt);
            SlowZoom.EndKill(); // camera cu da blend xong
            if (transitionPending) yield return FinishPhaseTransition();

            var enc = shot.encounter;
            if (enc == null)
            {
                Debug.LogWarning("[PhaseDirector] Shot Combat '" + shot.name + "' khong co EncounterBase, bo qua.", shot);
                yield break;
            }

            // Giao tranh noi tiep giao tranh (khong co doan Move truoc): van hoi Jev truoc Begin() (T-403).
            if (!enc.IsActive && !enc.IsCleared && prevShotKind == ShotKind.Combat) RailEvents.RaiseMoveSegmentStarted(enc);

            bool cleared = false;
            Vector3 lastKill = Vector3.zero;
            Action<EncounterBase, Vector3> onCleared = (e, pos) => { cleared = true; lastKill = pos; };
            enc.Cleared += onCleared;
            activeEnc = enc; activeOnCleared = onCleared;
            if (!enc.IsActive && !enc.IsCleared) enc.Begin();
            RailEvents.RaiseEncounterStarted(enc);
            if (enc.IsCleared) cleared = true;

            float elapsed = 0f;
            int angleIdx = 0;
            while (!cleared)
            {
                elapsed += Time.deltaTime;
                if (angleIdx < shot.subAngles.Count && elapsed >= shot.subAngles[angleIdx].startAfter)
                {
                    var a = shot.subAngles[angleIdx++];
                    if (a.cam != null)
                        yield return RunSubAngle(shot, a, () => cleared);
                }
                yield return null;
            }
            enc.Cleared -= onCleared;
            activeEnc = null; activeOnCleared = null;
            RailEvents.RaiseEncounterCleared(enc);
            // Zoom nhe vao vi tri kill cuoi roi moi chuyen goc (tat khi Giam chuyen dong)
            if (!UserSettings.ReduceMotion && profile.killZoomDuration > 0f)
            {
                SlowZoom.BeginKill(active, lastKill, profile, Time.time);
                yield return new WaitForSeconds(profile.killZoomDuration);
            }
            yield return new WaitForSeconds(profile.restAfterClear);
        }

        IEnumerator RunSubAngle(CameraShot shot, SubAngle a, Func<bool> isCleared)
        {
            var angleCam = a.cam;
            float bt = a.blendTime >= 0f ? a.blendTime : profile.subAngleBlend;
            bool cut = ShouldCut(brain.transform.forward, a.view.forward, ShotEntry.Blend);
            Activate(angleCam, cut, bt);
            yield return WaitTransition(cut, bt);

            float held = 0f;
            while (held < a.hold && !isCleared()) { held += Time.deltaTime; yield return null; }
            if (isCleared()) yield break;

            bool cutBack = ShouldCut(a.view.forward, shot.transform.forward, ShotEntry.Blend);
            Activate(shot.VCam, cutBack, bt);
            yield return WaitTransition(cutBack, bt);
        }

        // ---------------- Tien ich ----------------
        void Activate(CinemachineVirtualCameraBase cam, bool cut, float blendTime)
        {
            if (brain != null)
                brain.DefaultBlend = new CinemachineBlendDefinition(cut ? CinemachineBlendDefinition.Styles.Cut : profile.blendStyle, blendTime);
            if (active != null) active.Priority.Value = 0;
            cam.Priority.Value = ActivePriority;
            active = cam;
        }

        IEnumerator WaitTransition(bool cut, float blendTime)
        {
            PushPause();
            yield return null;
            yield return null;
            float timeout = (cut ? 0f : blendTime) + 0.5f;
            float t = 0f;
            while (brain != null && brain.IsBlending && t < timeout) { t += Time.deltaTime; yield return null; }
            ReleasePause();
        }

        void PushPause() { if (pauseHeld) return; pauseHeld = true; CombatPauseSignal.Push(PauseReason); }
        void ReleasePause() { if (!pauseHeld) return; pauseHeld = false; CombatPauseSignal.Pop(PauseReason); }

        bool ShouldCut(Vector3 fromFwd, Vector3 toFwd, ShotEntry entry)
        {
            if (entry == ShotEntry.Cut) return true;
            float angle = Vector3.Angle(fromFwd, toFwd);
            if (angle > profile.cutAngle) return true;
            if (UserSettings.ReduceMotion && angle > profile.reduceMotionCutAngle) return true;
            return false;
        }

        EncounterBase FindUpcomingEncounter(int pi, int si)
        {
            for (int p = pi; p < phases.Count; p++)
            {
                var list = phases[p].shots;
                for (int s = (p == pi ? si + 1 : 0); s < list.Count; s++)
                {
                    if (list[s] == null) continue;
                    if (list[s].kind == ShotKind.Move) return null; // doan di chuyen khac chen giua
                    if (list[s].encounter != null) return list[s].encounter;
                }
            }
            return null;
        }
    }
}
