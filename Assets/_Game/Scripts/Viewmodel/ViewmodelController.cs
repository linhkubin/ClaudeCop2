using System.Collections.Generic;
using ClaudeCop.Combat;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Viewmodel
{
    /// <summary>
    /// Viewmodel sung goc nhin thu nhat (chi sung, khong tay). Nghe CombatEvents (Core): doi sung, ban, reload, het dan.
    /// Dat o goc duoi man hinh theo aspect (neo viewport trong config). Animator chay clip co khi (Fire/Reload/Equip/DryFire),
    /// con giat/nghieng theo diem tap la spring thu cap (ViewmodelMotion). Khong ai tham chieu module nay.
    /// Dat component nay tren ViewmodelRoot (con cua Main Camera); ViewmodelCamera (URP Overlay) la anh em cua no.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ViewmodelController : MonoBehaviour
    {
        public const string StateIdle = "Idle";
        public const string StateFire = "Fire";
        public const string StateReload = "Reload";
        public const string StateEquip = "Equip";
        public const string StateDryFire = "DryFire";
        public const string ClipReload = "VM_Reload";

        static readonly int IdleHash = Animator.StringToHash(StateIdle);
        static readonly int FireHash = Animator.StringToHash(StateFire);
        static readonly int ReloadHash = Animator.StringToHash(StateReload);
        static readonly int EquipHash = Animator.StringToHash(StateEquip);
        static readonly int DryFireHash = Animator.StringToHash(StateDryFire);

        [SerializeField] ViewmodelConfig config;
        [Tooltip("ViewmodelCamera (URP Overlay). Config dat FOV/near/far cho camera nay.")]
        [SerializeField] Camera overlayCamera;
        [Tooltip("Tuy chon. Trong thi tu tim 1 lan o Start. Dung de doc WeaponData.ReloadTime/FireInterval.")]
        [SerializeField] TapShooter tapShooter;
        [Tooltip("Layer gan cho moi doi tuong viewmodel khi khoi tao (Viewmodel).")]
        [SerializeField] string layerName = "Viewmodel";
        [Header("Vo dan")]
        [SerializeField] bool ejectShells = true;
        [SerializeField, Min(0.1f)] float shellLife = 1.5f;

        sealed class Slot
        {
            public ViewmodelWeaponEntry entry;
            public GameObject go;
            public Animator animator;
            public GameObject flash;
            public Transform muzzle;
            public float reloadClipLength;
        }

        readonly Dictionary<WeaponKind, Slot> slots = new Dictionary<WeaponKind, Slot>(3);
        Slot active;
        ViewmodelMotion motion;
        readonly ViewmodelAimState aim = new ViewmodelAimState();
        Transform aimRoot;      // CAM-VC2: cha cua cac sung, xoay de nong chi ve diem trung (khong dung vao Animator cua sung)
        int aimFrame = -1;      // frame da ngam chinh xac cho phat hien tai
        Vector3 aimOrigin;      // diem xuat phat vet dan (the gioi) cua phat hien tai
        Vector3 aimTarget;      // diem trung (the gioi) cua phat hien tai
        bool aimDiagPending;
        Vector3 prevCamPos; float prevCamYaw; bool hasPrevCam;
        Camera baseCamera; // camera chinh (cha): aspect cua stack
        float flashTimer;
        float lastShotTime = -10f;
        bool initialized;

        public WeaponKind? ActiveKind { get; private set; }

        /// <summary>Do kiem (bao cao/debug): bat de ghi lai sai lech huong nong - vet dan.</summary>
        public bool RecordDiagnostics;
        public int AimCount, AimClampedCount;
        /// <summary>Goc (do) lech tren man hinh giua huong nong va vet dan: ngay luc ngam / cuoi frame ban (da tinh giat) / lon nhat.</summary>
        public float AimErrorAtShotDeg, AimErrorRenderedDeg, AimErrorMaxDeg;
        /// <summary>Goc 3D (do) giua truc nong va vet dan (khac 0 chi do FOV overlay khac FOV camera chinh).</summary>
        public float AimError3DDeg;
        public float LastAimAngleDeg;
        public Vector3 LastAimOrigin => aimOrigin;
        public Vector3 LastAimTarget => aimTarget;
        public Transform ActiveMuzzle => active != null ? active.muzzle : null;

        void Awake() { EnsureInit(); }

        void EnsureInit()
        {
            if (initialized) return;
            initialized = true;
            if (config == null) { Debug.LogError("[Viewmodel] Thieu ViewmodelConfig.", this); enabled = false; return; }
            motion = new ViewmodelMotion(config.motion);
            baseCamera = GetComponentInParent<Camera>();
            if (overlayCamera != null)
            {
                overlayCamera.fieldOfView = config.overlayFov;
                overlayCamera.nearClipPlane = config.overlayNear;
                overlayCamera.farClipPlane = config.overlayFar;
            }
            int layer = LayerMask.NameToLayer(layerName);
            var ar = new GameObject("AimRoot");
            aimRoot = ar.transform;
            aimRoot.SetParent(transform, false);
            if (layer >= 0) ar.layer = layer;
            for (int i = 0; i < config.weapons.Length; i++)
            {
                var e = config.weapons[i];
                if (e == null || e.prefab == null || slots.ContainsKey(e.kind)) continue;
                var go = Instantiate(e.prefab, aimRoot);
                go.name = e.prefab.name;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                if (layer >= 0) SetLayerRecursive(go.transform, layer);
                var s = new Slot { entry = e, go = go, animator = go.GetComponent<Animator>() };
                var muzzle = go.transform.Find("Pivot/Body/Muzzle");
                if (muzzle == null) muzzle = FindDeep(go.transform, "Muzzle");
                s.muzzle = muzzle;
                if (muzzle != null)
                {
                    var f = muzzle.Find("Flash");
                    if (f != null) { s.flash = f.gameObject; s.flash.SetActive(false); }
                }
                s.reloadClipLength = ResolveReloadClipLength(s);
                go.SetActive(false);
                slots[e.kind] = s;
            }
        }

        static Transform FindDeep(Transform t, string name)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i);
                if (c.name == name) return c;
                var r = FindDeep(c, name);
                if (r != null) return r;
            }
            return null;
        }

        static void SetLayerRecursive(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            for (int i = 0; i < t.childCount; i++) SetLayerRecursive(t.GetChild(i), layer);
        }

        static float ResolveReloadClipLength(Slot s)
        {
            if (s.animator != null)
            {
                var rc = s.animator.runtimeAnimatorController;
                if (rc is AnimatorOverrideController oc)
                {
                    var clip = oc[ClipReload];
                    if (clip != null && clip.length > 0.01f) return clip.length;
                }
                else if (rc != null)
                {
                    var clips = rc.animationClips;
                    for (int i = 0; i < clips.Length; i++)
                        if (clips[i] != null && clips[i].name == ClipReload && clips[i].length > 0.01f) return clips[i].length;
                }
            }
            return s.entry.fallbackReloadClipLength;
        }

        void Start()
        {
            if (tapShooter == null) tapShooter = FindFirstObjectByType<TapShooter>();
        }

        void OnEnable()
        {
            EnsureInit();
            if (!initialized || config == null) return;
            CombatEvents.WeaponChanged += OnWeaponChanged;
            CombatEvents.ShotFired += OnShotFired;
            CombatEvents.ReloadStateChanged += OnReloadStateChanged;
            CombatEvents.OutOfAmmo += OnOutOfAmmo;
            MuzzleAnchor.SetAimHandler(HandleAim);
            hasPrevCam = false;

            var snap = CombatEvents.Current;
            SetActiveWeapon(snap.Weapon, true);
            if (snap.Reloading) OnReloadStateChanged(true);
            ApplyTransform();
        }

        void OnDisable()
        {
            CombatEvents.WeaponChanged -= OnWeaponChanged;
            CombatEvents.ShotFired -= OnShotFired;
            CombatEvents.ReloadStateChanged -= OnReloadStateChanged;
            CombatEvents.OutOfAmmo -= OnOutOfAmmo;
            MuzzleAnchor.ClearAimHandler(HandleAim);
            aim.Clear();
            if (aimRoot != null) aimRoot.localRotation = Quaternion.identity;
            if (active != null) { MuzzleAnchor.Clear(active.muzzle); active.go.SetActive(false); }
            active = null;
            ActiveKind = null;
        }

        // ---------- Su kien ----------

        void OnWeaponChanged(WeaponKind kind) { SetActiveWeapon(kind, true); }

        void SetActiveWeapon(WeaponKind kind, bool playEquip)
        {
            if (!slots.TryGetValue(kind, out var s)) return;
            if (active != null && active != s) { MuzzleAnchor.Clear(active.muzzle); active.go.SetActive(false); }
            active = s;
            if (s.muzzle != null) MuzzleAnchor.Set(s.muzzle);
            ActiveKind = kind;
            s.go.SetActive(true);
            if (s.flash != null) s.flash.SetActive(false);
            flashTimer = 0f;
            motion.Reset();
            aim.Clear();
            if (aimRoot != null) aimRoot.localRotation = Quaternion.identity;
            if (s.animator != null)
            {
                s.animator.speed = 1f;
                if (playEquip) s.animator.Play(EquipHash, 0, 0f); else s.animator.Play(IdleHash, 0, 0f);
            }
        }

        void OnShotFired(WeaponKind kind, Vector2 screenPos)
        {
            if (active == null || kind != ActiveKind) return;
            lastShotTime = Time.unscaledTime;
            var e = active.entry;
            motion.OnShot(screenPos, Screen.width, Screen.height, e.recoilImpulseScale);
            PreAim(screenPos);

            var w = tapShooter != null ? tapShooter.CurrentWeapon : null;
            bool playClip = w == null || !w.HoldToFire || w.FireInterval >= e.fireClipMinInterval;
            if (playClip && active.animator != null) { active.animator.speed = 1f; active.animator.Play(FireHash, 0, 0f); }

            EjectCasing(active);

            if (active.flash != null)
            {
                float sc = Random.Range(config.flashScaleRange.x, config.flashScaleRange.y);
                active.flash.transform.localScale = Vector3.one * sc;
                active.flash.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                active.flash.SetActive(true);
                flashTimer = config.flashDuration;
            }
        }

        // ---------- Vo dan bay ra khoi sung ----------
        sealed class Casing { public Transform t; public Vector3 vel; public Vector3 spin; public float life; }
        readonly List<Casing> casings = new List<Casing>(16);
        static Material brassMat, shellMat;

        static Material MakeMat(Color c, float metallic, float smooth)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard"); if (sh == null) sh = Shader.Find("Sprites/Default");
            var m = new Material(sh) { color = c };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            return m;
        }

        /// <summary>Vo dan (hinh tru nho, thau/do cho shotgun) bay ra tu cua nap dan cua sung theo cung len-sang-phai, roi roi xuong theo trong luc va xoay. Mo phong trong khong gian cua viewmodel (di chuyen theo camera).</summary>
        void EjectCasing(Slot s)
        {
            if (!ejectShells || s == null || s.muzzle == null) return;
            WeaponKind kind = ActiveKind ?? WeaponKind.Pistol;
            bool shotgun = kind == WeaponKind.Shotgun;
            float len = shotgun ? 0.057f : (kind == WeaponKind.MachineGun ? 0.033f : 0.039f), rad = shotgun ? 0.013f : (kind == WeaponKind.MachineGun ? 0.0067f : 0.0073f);
            if (brassMat == null) brassMat = MakeMat(new Color(0.9f, 0.68f, 0.22f), 0.9f, 0.65f);
            if (shellMat == null) shellMat = MakeMat(new Color(0.8f, 0.12f, 0.08f), 0.1f, 0.4f);

            Casing c = null;
            for (int i = 0; i < casings.Count; i++) if (casings[i].life <= 0f) { c = casings[i]; break; }
            if (c == null)
            {
                if (casings.Count >= 16) { c = casings[0]; casings.RemoveAt(0); casings.Add(c); }
                else
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    var col = go.GetComponent<Collider>(); if (col != null) Destroy(col);
                    go.name = "Casing";
                    go.transform.SetParent(transform, false);
                    int layer = LayerMask.NameToLayer(layerName); if (layer >= 0) go.layer = layer;
                    c = new Casing { t = go.transform };
                    casings.Add(c);
                }
            }
            var mz = s.muzzle;
            // Cua nap dan: lui ve sau tu nong ~ nua khoang nong->goc sung, lech len/phai
            Vector3 portWorld = mz.position - mz.forward * (Mathf.Abs(mz.localPosition.z) * 0.5f) + mz.up * 0.028f + mz.right * 0.015f;
            Vector3 velWorld = mz.right * Random.Range(0.65f, 1.05f) + mz.up * Random.Range(0.5f, 0.85f) - mz.forward * Random.Range(0.1f, 0.3f);
            c.t.gameObject.SetActive(true);
            c.t.GetComponent<Renderer>().sharedMaterial = shotgun ? shellMat : brassMat;
            c.t.localScale = new Vector3(rad * 2f, len * 0.5f, rad * 2f);
            c.t.localPosition = transform.InverseTransformPoint(portWorld);
            c.t.localRotation = Quaternion.LookRotation(transform.InverseTransformDirection(mz.right)) * Quaternion.Euler(90f, 0f, 0f);
            c.vel = transform.InverseTransformDirection(velWorld);
            c.spin = new Vector3(Random.Range(-450f, 450f), Random.Range(-450f, 450f), Random.Range(-450f, 450f));
            c.life = shellLife;
        }

        void TickCasings(float dt)
        {
            for (int i = 0; i < casings.Count; i++)
            {
                var c = casings[i];
                if (c.life <= 0f) continue;
                c.life -= dt;
                if (c.life <= 0f) { c.t.gameObject.SetActive(false); continue; }
                c.vel += Vector3.down * 3.2f * dt;
                c.t.localPosition += c.vel * dt;
                c.t.Rotate(c.spin * dt, Space.Self);
            }
        }

        void OnReloadStateChanged(bool reloading)
        {
            if (active == null || active.animator == null) return;
            var a = active.animator;
            if (reloading)
            {
                float t = tapShooter != null && tapShooter.CurrentWeapon != null ? tapShooter.CurrentWeapon.ReloadTime : active.reloadClipLength;
                float speed = t > 0.001f ? active.reloadClipLength / t : config.reloadSpeedRange.y;
                a.speed = Mathf.Clamp(speed, config.reloadSpeedRange.x, config.reloadSpeedRange.y);
                a.CrossFadeInFixedTime(ReloadHash, 0.03f, 0, 0f);
            }
            else
            {
                a.speed = 1f;
                // Reload tu ket thuc -> state tu ve Idle. Neu bi huy giua chung (doi sung) thi ve Idle ngay.
                if (a.GetCurrentAnimatorStateInfo(0).shortNameHash == ReloadHash && a.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.98f)
                    a.CrossFadeInFixedTime(IdleHash, 0.08f, 0, 0f);
            }
        }

        void OnOutOfAmmo()
        {
            if (active == null || active.animator == null) return;
            if (Time.unscaledTime - lastShotTime < 0.05f) return; // vien cuoi: Fire vua chay, khong chen DryFire
            if (CombatEvents.Current.Reloading) return;
            active.animator.speed = 1f;
            active.animator.CrossFadeInFixedTime(DryFireHash, 0.02f, 0, 0f);
        }

        // ---------- Moi frame ----------

        void LateUpdate() { Advance(Time.unscaledDeltaTime); TickCasings(Time.unscaledDeltaTime); }

        /// <summary>Tien chuyen dong thu cap + dat lai transform (LateUpdate goi moi frame; test/sandbox co the goi truc tiep).</summary>
        public void Advance(float dt)
        {
            if (active == null) return;
            if (flashTimer > 0f)
            {
                flashTimer -= dt;
                if (flashTimer <= 0f && active.flash != null) active.flash.SetActive(false);
            }
            motion.Tick(dt);
            SampleMove(dt);
            ApplyTransform();
            aim.Tick(dt);
            ApplyAim();
            if (aimDiagPending) { aimDiagPending = false; if (RecordDiagnostics) AimErrorRenderedDeg = MeasureScreenError(out _); }
        }

        // ---------- CAM-VC2: huong theo di chuyen ----------

        /// <summary>Toc do xoay yaw + toc do tien cua camera chinh -> nghieng sung theo di chuyen (cut/blend lon bi bo qua).</summary>
        void SampleMove(float dt)
        {
            if (baseCamera == null || dt <= 1e-5f) { motion.TickMove(dt, 0f, 0f); return; }
            Transform ct = baseCamera.transform;
            Vector3 pos = ct.position;
            float yaw = ct.eulerAngles.y;
            float yawRate = 0f, speed = 0f;
            if (hasPrevCam)
            {
                float dy = Mathf.DeltaAngle(prevCamYaw, yaw);
                float dp = (pos - prevCamPos).magnitude;
                if (Mathf.Abs(dy) < 20f && dp < 3f) { yawRate = dy / dt; speed = dp / dt; } // Cut/hitch: bo qua
            }
            prevCamPos = pos; prevCamYaw = yaw; hasPrevCam = true;
            motion.TickMove(dt, yawRate, speed);
        }

        // ---------- CAM-VC2: ngam theo vet dan ----------

        void ApplyAim()
        {
            if (aimRoot == null) return;
            aimRoot.localRotation = config != null && config.aimEnabled ? aim.Current(config.aimHoldTime, config.aimReturnTime) : Quaternion.identity;
        }

        /// <summary>Luc ShotFired chi biet diem tap: ngam so bo theo tia camera (de sung quay ngay ca khi khong co FX). FX se tinh chinh bang MuzzleAnchor.TryAimAt.</summary>
        void PreAim(Vector2 screenPos)
        {
            if (config == null || !config.aimEnabled || active == null || active.muzzle == null || baseCamera == null) return;
            Ray ray = baseCamera.ScreenPointToRay(screenPos);
            int mask = config.aimRayMask.value;
            int vl = LayerMask.NameToLayer(layerName);
            if (vl >= 0) mask &= ~(1 << vl);
            Vector3 target = Physics.Raycast(ray, out RaycastHit hit, config.aimRayDistance, mask, QueryTriggerInteraction.Ignore)
                ? hit.point : ray.origin + ray.direction * config.aimRayDistance;
            SolveAim(target, out aimOrigin);
            aimFrame = -1; // chua tinh chinh: lan TryAimAt dau tien cua phat nay se ngam lai cho dung diem trung that
        }

        /// <summary>MuzzleAnchor.TryAimAt: lan dau trong frame ban thi ngam lai chinh xac; cac lan sau (Shotgun nhieu ket qua) giu nguyen.</summary>
        bool HandleAim(Vector3 targetWorld, out Vector3 originWorld)
        {
            originWorld = default;
            if (config == null || !config.aimEnabled || active == null || active.muzzle == null || baseCamera == null) return false;
            if (aimFrame == Time.frameCount) { originWorld = aimOrigin; return true; }
            aimFrame = Time.frameCount;
            SolveAim(targetWorld, out aimOrigin);
            originWorld = aimOrigin;
            return true;
        }

        void SolveAim(Vector3 targetWorld, out Vector3 originWorld)
        {
            Camera ov = overlayCamera != null ? overlayCamera : baseCamera;
            aimTarget = targetWorld;
            Transform m = active.muzzle;

            // Diem ngam trong khong gian camera overlay: cung vi tri tren man hinh voi diem trung (camera chinh), o do sau xa -> nong chi dung theo vet tren man hinh.
            Vector3 vp = baseCamera.WorldToViewportPoint(targetWorld);
            Vector3 far;
            if (vp.z < 0.1f) far = ov.transform.position + ov.transform.forward * config.aimDepth;
            else
            {
                vp.x = Mathf.Clamp(vp.x, -0.5f, 1.5f); vp.y = Mathf.Clamp(vp.y, -0.5f, 1.5f);
                far = ov.ViewportToWorldPoint(new Vector3(vp.x, vp.y, config.aimDepth));
            }

            // Xoay AimRoot (quanh goc sung) tu tu the nghi; lap vi vi tri nong doi theo goc xoay.
            aimRoot.localRotation = Quaternion.identity;
            for (int i = 0; i < 3; i++)
            {
                Vector3 want = far - m.position;
                if (want.sqrMagnitude < 1e-8f) break;
                aimRoot.rotation = Quaternion.FromToRotation(m.forward, want.normalized) * aimRoot.rotation;
            }
            Quaternion d = ViewmodelAimState.ClampAngle(aimRoot.localRotation, config.aimMaxAngle);
            bool clamped = Quaternion.Angle(d, aimRoot.localRotation) > 0.01f;
            aimRoot.localRotation = d;
            aim.Set(d);

            // Diem xuat phat vet dan: vi tri nong tren man hinh (camera overlay) quy ve camera chinh o cung do sau -> vet bat dau dung o dau nong.
            Vector3 mv = ov.WorldToViewportPoint(m.position);
            originWorld = ov == baseCamera ? m.position : baseCamera.ViewportToWorldPoint(mv);

            AimCount++; if (clamped) AimClampedCount++;
            LastAimAngleDeg = Quaternion.Angle(Quaternion.identity, d);
            aimOrigin = originWorld;
            if (RecordDiagnostics)
            {
                AimErrorAtShotDeg = MeasureScreenError(out AimError3DDeg);
                AimErrorMaxDeg = Mathf.Max(AimErrorMaxDeg, AimErrorAtShotDeg);
                aimDiagPending = true;
            }
        }

        /// <summary>Goc tren man hinh giua huong nong (camera overlay) va doan vet dan (camera chinh) tu aimOrigin toi aimTarget.</summary>
        float MeasureScreenError(out float angle3D)
        {
            angle3D = 0f;
            if (active == null || active.muzzle == null || baseCamera == null) return 0f;
            Camera ov = overlayCamera != null ? overlayCamera : baseCamera;
            Transform m = active.muzzle;
            Vector2 b0 = ov.WorldToScreenPoint(m.position), b1 = ov.WorldToScreenPoint(m.position + m.forward * 5f);
            Vector2 t0 = baseCamera.WorldToScreenPoint(aimOrigin), t1 = baseCamera.WorldToScreenPoint(aimTarget);
            angle3D = Vector3.Angle(m.forward, aimTarget - aimOrigin);
            Vector2 db = b1 - b0, dt2 = t1 - t0;
            if (db.sqrMagnitude < 1e-6f || dt2.sqrMagnitude < 1e-6f) return 0f;
            return Vector2.Angle(db, dt2);
        }

        void ApplyTransform()
        {
            if (config == null || active == null) return;
            float aspect = baseCamera != null && baseCamera.aspect > 0.01f ? baseCamera.aspect : (Screen.height > 0 ? (float)Screen.width / Screen.height : 0.5625f);
            config.Layout(aspect, out Vector2 anchor, out float scale);
            float fov = overlayCamera != null ? overlayCamera.fieldOfView : config.overlayFov;
            Vector3 basePos = ViewmodelMotion.AnchorLocalPosition(anchor, config.depth, fov, aspect);

            float amp = (UserSettings.ReduceMotion ? config.motion.reduceMotionScale : 1f) * active.entry.recoilVisualScale;
            Vector3 pos = basePos + motion.PositionOffset(amp);
            Quaternion rot = Quaternion.Euler(config.baseEuler) * Quaternion.Euler(motion.EulerOffset(amp));
            transform.localPosition = pos;
            transform.localRotation = rot;
            transform.localScale = Vector3.one * scale;
        }
    }
}
