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
    /// Chay man choi: Phase -> Shot tuan tu. Doi Phase mac dinh LIEN MACH (profile.seamlessPhaseTransitions): khong fade den, khong Cut. Move: di theo spline (ease) roi sang Shot ke. Combat: Begin encounter, cho Cleared,
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
        string pendingBannerTitle;  // SEAMLESS: tieu de Phase cho hien khi ray vao Phase moi bat dau chay
        readonly PhaseBannerGate bannerGate = new PhaseBannerGate(); // moi Phase chi hien chu stage dung 1 lan
        CameraPoseSmoother smoother;
        CameraReaction reaction;
        CameraKick kick;
        bool reactionOk;       // true tu luc toi diem (ArmFeel) den luc Activate camera ke tiep
        float comboTarget, comboVel;

        /// <summary>CAM-LIVELY: logic reaction (do dem/bien do).</summary>
        public CameraReaction Reaction => reaction;

        /// <summary>CAM-VC2: logic giat khi ban (do dem/bien do).</summary>
        public CameraKick Kick => kick;

        /// <summary>Luoi an toan van toc/gia toc cuoi pipeline (CAM-SMOOTH). Dung de do/debug.</summary>
        public CameraPoseSmoother Smoother => smoother;

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
            bannerGate.Reset(); bannerGate.TryShow(0); // Phase 0: UI tu hien qua PhaseStarted
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
            if (brain != null) brain.UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate; // rail/shot khong co target: tranh SmartUpdate doi nhip
            if (railCamera != null)
            {
                var dolly = railCamera.GetComponent<CinemachineSplineDolly>();
                if (dolly == null) Debug.LogError("[PhaseDirector] railCamera thieu CinemachineSplineDolly.", railCamera);
                else driver = new RailCameraDriver(railCamera, dolly, profile);
                var ap = railCamera.GetComponent<CameraFeelApplier>();
                if (ap == null) ap = railCamera.gameObject.AddComponent<CameraFeelApplier>();
                ap.profile = profile; ap.isRail = true;
                railCamera.Priority.Enabled = true; railCamera.Priority.Value = 0;
            }
            outCam = brain != null ? brain.GetComponent<UnityEngine.Camera>() : UnityEngine.Camera.main;
            if (brain != null && smoother == null) { smoother = new CameraPoseSmoother(brain, profile); smoother.Enable(); }
            if (reaction == null) reaction = new CameraReaction(profile);
            if (kick == null) kick = new CameraKick(profile);
            ReframeShots();
        }

        UnityEngine.Camera outCam;
        float framedAspect;

        float CurrentAspect() => outCam != null ? outCam.aspect : (Screen.height > 0 ? (float)Screen.width / Screen.height : 0f);

        /// <summary>Can khung lai moi Shot Combat theo ti le man hien tai (goi luc Init va khi aspect doi, chi khi camera Shot khong dang live).</summary>
        void ReframeShots()
        {
            float aspect = CurrentAspect();
            bool liveDone = true;
            for (int i = 0; i < phases.Count; i++)
                for (int j = 0; j < phases[i].shots.Count; j++)
                {
                    var shot = phases[i].shots[j];
                    if (shot == null) continue;
                    shot.EnsureCameras(profile);
                    if (shot.VCam != null && shot.VCam == active)
                    {
                        // dang live: can khung lai mem (lerp), lap lai moi frame cho den khi toi dich
                        if (!shot.AutoFrameSmooth(profile, aspect, Time.unscaledDeltaTime)) liveDone = false;
                        continue;
                    }
                    shot.AutoFrame(profile, aspect); // man doc: can khung theo ti le man that
                }
            if (liveDone) framedAspect = aspect;
        }

        void Start()
        {
            if (autoStart || (startOnGamePlaying && GameEvents.Current.State == GameState.Playing)) StartLevel();
        }

        void OnEnable()
        {
            if (smoother != null) smoother.Enable();
            GameEvents.PlayerDamaged += OnPlayerDamaged;
            BlastEvents.Blasted += OnBlasted;
            GameEvents.GameStateChanged += OnGameStateChanged;
            CombatEvents.ShotResolved += OnShotResolved;
            CombatEvents.ShotFired += OnShotFired;
            CombatEvents.ComboChanged += OnComboChanged;
            TargetRegistry.Registered += OnTargetRegistered;
        }

        void OnDisable()
        {
            if (smoother != null) smoother.Disable();
            GameEvents.PlayerDamaged -= OnPlayerDamaged;
            BlastEvents.Blasted -= OnBlasted;
            GameEvents.GameStateChanged -= OnGameStateChanged;
            CombatEvents.ShotResolved -= OnShotResolved;
            CombatEvents.ShotFired -= OnShotFired;
            CombatEvents.ComboChanged -= OnComboChanged;
            TargetRegistry.Registered -= OnTargetRegistered;
            reaction?.Cancel(); kick?.Cancel(); CameraFeelState.KickTarget = Vector2.zero; CameraFeelState.ReactTarget = Vector3.zero; CameraFeelState.ComboDolly = 0f;
            ReleasePause();
            ReleaseTransitionPause();
            SlowZoom.EndKill();
            // F-205: coroutine bi Unity dung ngam khi disable -> don dang ky, cho phep StartLevel lai.
            if (activeEnc != null && activeOnCleared != null) activeEnc.Cleared -= activeOnCleared;
            activeEnc = null; activeOnCleared = null;
            routine = null;
            started = false;
        }

        /// <summary>CAM-VC2: giat nhe khi ban. Tap dung Camera.main luc nay (giat chi ap sau, bien do nho); tat/giam khi Giam chuyen dong.</summary>
        void OnShotFired(WeaponKind weapon, Vector2 screenPos)
        {
            if (profile == null || kick == null) return;
            kick.Fire(Time.time, weapon, UserSettings.ReduceMotion ? profile.kickReduceMotionScale : 1f);
        }

        void OnShotResolved(ShotResult r)
        {
            if (profile == null || UserSettings.ReduceMotion) return;
            if (r.TargetKind == TargetKind.Grenade) return;
            if (r.Outcome == TapOutcome.Kill || r.Outcome == TapOutcome.JusticeKill) SlowZoom.Punch(profile, Time.time, r.Outcome == TapOutcome.JusticeKill);
        }

        void OnComboChanged(int streak, float mult)
        {
            if (profile == null) return;
            comboTarget = Mathf.Min(profile.comboDollyFov, Mathf.Max(0, streak - 1) * profile.comboDollyPerCombo);
        }

        /// <summary>Enemy MOI lo ra (targetable lan dau) lech truc camera: gop vao reaction (giat minh quay sang).</summary>
        void OnTargetRegistered(ITapTarget t)
        {
            if (reaction == null || !reactionOk || t == null || t.Kind != TargetKind.Enemy || outCam == null) return;
            Vector3 local = Quaternion.Inverse(outCam.transform.rotation) * (t.AimPoint - outCam.transform.position);
            if (local.z < 1f) return;
            reaction.Notify(Time.time, Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, Mathf.Atan2(local.y, local.z) * Mathf.Rad2Deg);
        }

        void TickLiveliness()
        {
            bool reduce = UserSettings.ReduceMotion;
            // dolly-in theo combo (ease ra khi het dot), tat khi Giam chuyen dong
            float tgt = (reduce || !CameraFeelState.Combat) ? 0f : comboTarget;
            CameraFeelState.ComboDolly = Mathf.SmoothDamp(CameraFeelState.ComboDolly, tgt, ref comboVel, profile.comboDollySmooth, Mathf.Infinity, Time.deltaTime);
            if (reaction == null) return;
            bool allowed = reactionOk && CameraFeelState.Combat && brain != null && !brain.IsBlending && !pauseHeld && !transitionPauseHeld && !transitionPending && !SlowZoom.KillZoomActive;
            reaction.Tick(Time.time, allowed, reduce ? profile.reactReduceMotionScale : 1f);
            Vector3 t = reaction.Target;
            // khong chong len punch ha enemy: giam punch FOV cua reaction theo do lech punch dang chay
            if (profile.punchFov > 0f) t.z *= 1f - Mathf.Clamp01(SlowZoom.PunchOffset(Time.time) / profile.punchFov);
            CameraFeelState.ReactTarget = t;

            // CAM-VC2: giat khi ban. Giam khi reaction / kill-zoom dang chay (uu tien chung: khong chong len nhau).
            float busy = Mathf.Clamp01((Mathf.Abs(t.x) + Mathf.Abs(t.y)) / Mathf.Max(0.1f, profile.kickReactRef));
            if (SlowZoom.KillZoomActive) busy = 1f;
            float damp = 1f - busy * (1f - profile.kickReactDamp);
            CameraFeelState.KickTarget = kick != null && profile.kickEnabled ? kick.Evaluate(Time.time, damp) : Vector2.zero;
        }

        void OnGameStateChanged(GameState s)
        {
            if (startOnGamePlaying && s == GameState.Playing && !started) StartLevel();
        }

        /// <summary>W7: rung khi co vu no (explosionShake*); tat khi Giam chuyen dong neu profile bat co.</summary>
        public void OnBlasted(BlastReport b)
        {
            if (profile == null) return;
            if (UserSettings.ReduceMotion && profile.explosionShakeOffWhenReduceMotion) return;
            CameraFeelState.TriggerShake(profile.explosionShakeDuration, profile.explosionShakeScale);
        }

        void OnPlayerDamaged(DamageSource src, Vector3 pos, int livesLeft)
        {
            if (profile != null) CameraFeelState.TriggerShake(profile.hitShakeDuration);
        }

        void Update()
        {
            if (profile == null) return;
            if (initialized && Mathf.Abs(CurrentAspect() - framedAspect) > 0.01f) ReframeShots();
            driver?.Tick(Time.deltaTime);
            CameraFeelState.MoveSpeed = driver != null && driver.Active ? driver.CurrentSpeed : 0f;
            CameraFeelState.Tick(Time.deltaTime, profile);
            TickLiveliness();
        }

        // ---------------- Luong chay ----------------
        IEnumerator Run()
        {
            for (int pi = 0; pi < phases.Count; pi++)
            {
                CurrentPhaseIndex = pi;
                var phase = phases[pi];
                RailEvents.RaisePhaseStarted(pi, phase.title);
                if (pi > 0)
                {
                    if (profile.seamlessPhaseTransitions) { if (bannerGate.TryShow(pi)) pendingBannerTitle = phase.title; } // lien mach: khong fade, khong Cut
                    else yield return BeginPhaseTransition(phase.title);
                }

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
                FlushPhaseBanner();
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

        /// <summary>SEAMLESS: hien tieu de Phase chong len canh dang chay (goi khi ray bat dau chay / toi diem Combat dau Phase).</summary>
        void FlushPhaseBanner()
        {
            if (pendingBannerTitle == null) return;
            string t = pendingBannerTitle; pendingBannerTitle = null;
            RailEvents.RaisePhaseBanner(t);
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

            // CAM-SMOOTH: Cut CHI khi vao level lan dau hoac luc man den giua 2 Phase. Khong con Cut theo goc/ShotEntry.Cut.
            // Rail -> Rail (cung camera, khong blend duoc): giu huong cu, ray tu xoay dan.
            bool snap = !hasPrev || forceCutNext;
            bool sameCam = active == railCamera;
            bool cut = snap || sameCam;
            forceCutNext = false;
            // Camera ray dat san theo huong tiep tuyen dau ray; viec xoay tu goc Combat sang huong ray do blend Cinemachine
            // (EaseInOut, thoi gian tu co gian theo goc) thuc hien - truoc day ray tu xoay sau khi toi noi voi toc do/gia toc cao.
            bool keyed = shot.lookKeys != null && shot.lookKeys.Count > 0; // huong nhin theo moc: giu huong camera luc vao ray, khong xoay ve tiep tuyen
            if (!sameCam && !keyed) driver.Prepare(shot.spline, shot.speedOverride, Quaternion.LookRotation(startFwd));
            if (snap) smoother?.RequestSnap(railCamera);
            float bt = ScaleBlend(shot.ResolveBlend(profile), railCamera, cut);
            Activate(railCamera, cut, bt);
            yield return WaitTransition(cut, bt);
            SlowZoom.EndKill(); // camera cu da blend xong
            if (transitionPending) yield return FinishPhaseTransition();

            // CAM-VC2: nhin truoc shot Combat ke trong cung Phase (khop dung huong shot khi toi)
            var nextShot = si + 1 < phases[pi].shots.Count ? phases[pi].shots[si + 1] : null;
            if (nextShot != null && nextShot.kind == ShotKind.Combat && nextShot.VCam != null)
            {
                Vector3 nf = nextShot.VCam.transform.forward;
                driver.SetNextLook(Mathf.Atan2(nf.x, nf.z) * Mathf.Rad2Deg, -Mathf.Asin(Mathf.Clamp(nf.y, -1f, 1f)) * Mathf.Rad2Deg);
            }
            driver.SetLookKeys(keyed ? shot.lookKeys : null);
            driver.StartMoving();
            FlushPhaseBanner();
            while (!driver.Finished) yield return null;
        }

        IEnumerator RunCombat(CameraShot shot, ShotKind prevShotKind)
        {
            var cam = shot.VCam;
            if (cam == null) { Debug.LogError("[PhaseDirector] Shot Combat '" + shot.name + "' thieu CinemachineCamera.", shot); yield break; }
            driver?.Deactivate();
            // Camera ray khong con la live sau Activate nen giu driver o trang thai cuoi cho blend: chi dung Tick khi Active.
            CameraFeelState.Combat = true;

            bool hasPrev = active != null && brain != null;
            bool cut = !hasPrev || forceCutNext; // CAM-SMOOTH: chi Cut khi vao level / man den giua Phase
            forceCutNext = false;
            if (cut) smoother?.RequestSnap(cam);
            float bt = ScaleBlend(shot.ResolveBlend(profile), cam, cut);
            Activate(cam, cut, bt);
            yield return WaitTransition(cut, bt);
            SlowZoom.EndKill(); // camera cu da blend xong
            if (transitionPending) yield return FinishPhaseTransition();
            ArmFeel(cam); // toi diem: bat dau settle -> giu khung -> push-in (chi zoom-in)
            FlushPhaseBanner();

            var enc = shot.encounter;
            if (enc == null && shot.dwell > 0f)
            {
                // Nhip giam tai: giu goc nay mot luc (reload tu nhien), khong co encounter.
                comboTarget = 0f;
                yield return new WaitForSeconds(shot.dwell);
                reactionOk = false;
                yield break;
            }
            if (enc == null)
            {
                Debug.LogWarning("[PhaseDirector] Shot Combat '" + shot.name + "' khong co EncounterBase, bo qua.", shot);
                yield break;
            }

            // Giao tranh noi tiep giao tranh (khong co doan Move truoc): van hoi RankScore truoc Begin() (T-403).
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
            comboTarget = 0f; reactionOk = false; // het dot: ease ra dolly combo, khong reaction nua
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
            const bool cut = false;
            float bt = ScaleBlend(a.blendTime >= 0f ? a.blendTime : profile.subAngleBlend, angleCam, cut);
            Activate(angleCam, cut, bt);
            yield return WaitTransition(cut, bt);
            ArmFeel(angleCam);

            float held = 0f;
            while (held < a.hold && !isCleared()) { held += Time.deltaTime; yield return null; }
            if (isCleared()) yield break;

            const bool cutBack = false;
            bt = ScaleBlend(bt, shot.VCam, cutBack);
            Activate(shot.VCam, cutBack, bt);
            yield return WaitTransition(cutBack, bt);
        }

        // ---------------- Tien ich ----------------
        /// <summary>
        /// Blend du dai de van toc VA gia toc (xoay + vi tri) khong vuot gioi han. EaseInOut co dinh (S-curve): dinh van toc = 1.5 x trung binh,
        /// gia toc dinh = 6*delta/T^2 -> T >= sqrt(6*delta/aMax). Khong con tran thoi gian ep nho hon nhu cau (maxBlendTime chi la tran an toan).
        /// </summary>
        float ScaleBlend(float bt, CinemachineVirtualCameraBase to, bool cut)
        {
            if (cut || brain == null || to == null) return bt;
            float dist = Vector3.Distance(brain.transform.position, to.transform.position);
            float ang = Vector3.Angle(brain.transform.forward, to.transform.forward);
            float need = 0f;
            if (profile.maxBlendSpeed > 0f) need = Mathf.Max(need, dist / profile.maxBlendSpeed);
            if (profile.maxBlendAngularSpeed > 0f) need = Mathf.Max(need, ang / profile.maxBlendAngularSpeed);
            if (profile.blendMaxAccel > 0f) need = Mathf.Max(need, Mathf.Sqrt(6f * dist / profile.blendMaxAccel));
            if (profile.blendMaxAngAccel > 0f) need = Mathf.Max(need, Mathf.Sqrt(6f * ang / profile.blendMaxAngAccel));
            if (UserSettings.ReduceMotion) need /= Mathf.Max(0.2f, profile.smoothReduceMotionScale); // Giam chuyen dong: cham hon, KHONG Cut
            return Mathf.Clamp(Mathf.Max(bt, need), bt, Mathf.Max(bt, profile.maxBlendTime));
        }

        void Activate(CinemachineVirtualCameraBase cam, bool cut, float blendTime)
        {
            if (brain != null)
                brain.DefaultBlend = new CinemachineBlendDefinition(cut ? CinemachineBlendDefinition.Styles.Cut : profile.blendStyle, blendTime);
            reactionOk = false; reaction?.Cancel();
            if (active != null) active.Priority.Value = 0;
            cam.Priority.Value = ActivePriority;
            active = cam;
        }

        IEnumerator WaitTransition(bool cut, float blendTime)
        {
            PushPause();
            yield return null;
            yield return null;
            float timeout = (cut ? 0f : blendTime) + profile.blendWaitMargin;
            float t = 0f;
            while (brain != null && brain.IsBlending && t < timeout) { t += Time.deltaTime; yield return null; }
            ReleasePause();
        }

        void ArmFeel(CinemachineVirtualCameraBase cam)
        {
            if (cam == null) return;
            reactionOk = true;
            var ap = cam.GetComponent<CameraFeelApplier>();
            if (ap != null) ap.Arm(Time.time);
        }

        void PushPause() { if (pauseHeld) return; pauseHeld = true; CombatPauseSignal.Push(PauseReason); }
        void ReleasePause() { if (!pauseHeld) return; pauseHeld = false; CombatPauseSignal.Pop(PauseReason); }

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
