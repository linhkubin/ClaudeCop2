using System;
using System.Collections.Generic;
using ClaudeCop.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ClaudeCop.Combat
{
    /// <summary>
    /// Doc tap (chuot/but qua Input Action; cam ung doc tung ngon qua Touchscreen), chon muc tieu qua <see cref="TargetSelector"/>,
    /// quan ly dan/reload/doi vu khi, phat CombatEvents. Quy tac uu tien xem TargetSelector.
    /// Khong tham chieu UI: UI co the gan <see cref="PointerBlocker"/> de chan tap len nut (vd. nut Reload).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TapShooter : MonoBehaviour
    {
        /// <summary>UI gan (vd. EventSystem.IsPointerOverGameObject). Tra true thi tap bi bo qua (khong ton dan).</summary>
        public static Func<Vector2, bool> PointerBlocker;

        [SerializeField] InputActionAsset inputActions;
        [SerializeField] WeaponData startingWeapon;
        [SerializeField] CombatConfig config;
        [Tooltip("Tuy chon. Co thi cap nhat combo truoc khi phat ShotResolved.")]
        [SerializeField] ComboSystem combo;
        [SerializeField] string actionMapName = "Gameplay";
        [SerializeField] string tapActionName = "Tap";
        [SerializeField] string tapPositionActionName = "TapPosition";
        [Header("Vuot xuong de thay dan (chi khi het dan)")]
        [Tooltip("Quang vuot xuong toi thieu theo ty le chieu cao man hinh")] [SerializeField, Range(0.05f, 0.5f)] float swipeReloadMinScreenFraction = 0.12f;

        InputAction tapAction;
        InputAction tapPositionAction;
        WeaponData weapon;
        int ammo;
        bool reloading;
        float reloadEndTime;
        float nextFireTime;
        Camera cam;
        int holdTouchId = -1;
        int swipeDoneTouchId = -1;          // ngon tay da kich hoat vuot (khong kich hoat lai)
        Vector2 mouseDownPos; bool prevMouseDown, mouseSwipeDone;

        readonly List<TargetHit> hits = new List<TargetHit>(8);
        readonly List<ShotResult> pending = new List<ShotResult>(8);
        Func<Vector3, Vector2?> projectFunc;
        Func<ITapTarget, Rect?> bodyRectFunc;
        float bodyPadPx;
        readonly Dictionary<int, Collider[]> bodyColliders = new Dictionary<int, Collider[]>();
        static readonly Collider[] NoColliders = new Collider[0];
        readonly List<Collider> colScratch = new List<Collider>(8);
        readonly List<Collider> keepScratch = new List<Collider>(8);
        Func<ITapTarget, float> depthFunc;
        Vector3 depthCamPos;
        float justiceRadiusScale = 1f;

        void OnTargetUnregistered(ITapTarget t) { if (t != null) bodyColliders.Remove(t.Id); }

        float Depth(ITapTarget t) { return (t.AimPoint - depthCamPos).sqrMagnitude; }

        public WeaponData CurrentWeapon => weapon;
        public int Ammo => ammo;
        public bool IsReloading => reloading;
        /// <summary>Vu khi khoi dau (Pistol): quay ve khi het dan vu khi dac biet.</summary>
        public WeaponData StartingWeapon => startingWeapon;

        /// <summary>Doi vu khi khoi dau (sung mua/thue/nang cap tu man Home). Dang cam vu khi khoi dau cu thi doi luon.</summary>
        public void SetStartingWeapon(WeaponData data)
        {
            if (data == null) return;
            bool holdingStart = weapon == null || weapon == startingWeapon;
            startingWeapon = data;
            if (holdingStart && isActiveAndEnabled) Equip(data, true);
        }

        /// <summary>He so ban kinh diem Justice (trang bi: kinh). 1 = chuan.</summary>
        public void SetJusticeRadiusScale(float scale) { justiceRadiusScale = Mathf.Max(0.1f, scale); }

        void Awake() { projectFunc = Project; bodyRectFunc = BodyRect; depthFunc = Depth; }

        void OnEnable()
        {
            if (inputActions != null)
            {
                var map = inputActions.FindActionMap(actionMapName, false);
                if (map != null)
                {
                    tapAction = map.FindAction(tapActionName, false);
                    tapPositionAction = map.FindAction(tapPositionActionName, false);
                    map.Enable();
                }
                if (tapAction == null || tapPositionAction == null)
                    Debug.LogError("[TapShooter] Thieu action " + actionMapName + "/" + tapActionName + " hoac " + tapPositionActionName, this);
                else
                    tapAction.performed += OnTapPerformed;
            }
            else Debug.LogError("[TapShooter] Chua gan InputActionAsset.", this);

            GameCommands.ReloadRequested += StartReload;
            TargetRegistry.Unregistered += OnTargetUnregistered;
            if (startingWeapon != null) Equip(startingWeapon, true);
        }

        bool holdLocked;

        void OnDisable()
        {
            if (tapAction != null) tapAction.performed -= OnTapPerformed;
            GameCommands.ReloadRequested -= StartReload;
            TargetRegistry.Unregistered -= OnTargetUnregistered;
            bodyColliders.Clear();
            if (reloading) { reloading = false; CombatEvents.RaiseReloadStateChanged(false); }
        }

        void Update()
        {
            if (reloading && Time.time >= reloadEndTime) FinishReload();
            if (weapon == null) return;

            // Cam ung: doc tung ngon (F-108). Ngon thu hai cham khi ngon dau con giu van ban duoc.
            bool anyTouch = false;
            var ts = Touchscreen.current;
            if (ts != null)
            {
                var touches = ts.touches;
                int holdIdx = -1, firstIdx = -1;
                for (int i = 0; i < touches.Count; i++)
                {
                    var t = touches[i];
                    if (!t.isInProgress) continue;
                    anyTouch = true;
                    int touchId = t.touchId.ReadValue();
                    if (touchId != swipeDoneTouchId && SwipeReloadAllowed && IsSwipeDown(t.startPosition.ReadValue(), t.position.ReadValue()))
                    {
                        swipeDoneTouchId = touchId;
                        StartReload();
                    }
                    if (!weapon.HoldToFire)
                    {
                        if (t.press.wasPressedThisFrame) TryFire(t.position.ReadValue());
                    }
                    else
                    {
                        if (firstIdx < 0) firstIdx = i;
                        if (holdTouchId >= 0 && t.touchId.ReadValue() == holdTouchId) holdIdx = i;
                    }
                }
                if (weapon.HoldToFire && anyTouch && !holdLocked)
                {
                    if (holdIdx < 0) { holdIdx = firstIdx; holdTouchId = touches[holdIdx].touchId.ReadValue(); }
                    TryFire(touches[holdIdx].position.ReadValue());
                }
            }
            if (!anyTouch) { holdTouchId = -1; swipeDoneTouchId = -1; }

            // Chuot/but: giu de ban (Sung may).
            bool mousePressed = !anyTouch && tapAction != null && tapAction.IsPressed();
            if (mousePressed && tapPositionAction != null)
            {
                Vector2 mp = tapPositionAction.ReadValue<Vector2>();
                if (!prevMouseDown) { mouseDownPos = mp; mouseSwipeDone = false; }
                if (!mouseSwipeDone && SwipeReloadAllowed && IsSwipeDown(mouseDownPos, mp)) { mouseSwipeDone = true; StartReload(); }
            }
            prevMouseDown = mousePressed;
            if (mousePressed && weapon.HoldToFire && !holdLocked)
                TryFire(tapPositionAction.ReadValue<Vector2>());

            // F-202: sau khi nhat thung, bo qua lan giu hien tai den khi nha tay.
            if (holdLocked && !anyTouch && !mousePressed) holdLocked = false;
        }

        /// <summary>Vuot xuong chi thay dan khi het dan (luc do tap chi la nhat khong, khong ton phat nao).</summary>
        bool SwipeReloadAllowed => weapon != null && !reloading && ammo <= 0 && !CombatPauseSignal.IsPaused;

        bool IsSwipeDown(Vector2 start, Vector2 now)
        {
            Vector2 d = now - start;
            return d.y <= -Screen.height * swipeReloadMinScreenFraction && Mathf.Abs(d.x) <= -d.y * 0.8f; // chu yeu doc
        }

        void OnTapPerformed(InputAction.CallbackContext ctx)
        {
            if (ctx.control != null && ctx.control.device is Touchscreen) return; // cam ung xu ly theo tung ngon trong Update
            if (weapon == null || weapon.HoldToFire) return;
            TryFire(tapPositionAction.ReadValue<Vector2>());
        }

        /// <summary>Ban nhu mot cu tap tai toa do man hinh (tuan thu pause/reload/dan/PointerBlocker). Dung cho debug/test.</summary>
        public void FireAt(Vector2 screenPos) { TryFire(screenPos); }

        // ---------- Vu khi / dan ----------

        /// <summary>Doi vu khi. refill = nap day bang dan.</summary>
        public void Equip(WeaponData data, bool refill = true)
        {
            if (data == null) return;
            if (reloading) { reloading = false; CombatEvents.RaiseReloadStateChanged(false); }
            bool changed = weapon != data;
            weapon = data;
            if (refill || ammo > data.MagazineSize) ammo = data.MagazineSize;
            nextFireTime = 0f;
            if (changed && data.HoldToFire) { holdLocked = true; holdTouchId = -1; }
            if (changed) CombatEvents.RaiseWeaponChanged(data.Kind);
            CombatEvents.RaiseAmmoChanged(ammo, data.MagazineSize, data.Kind);
        }

        public void StartReload()
        {
            if (weapon == null || reloading || ammo >= weapon.MagazineSize) return;
            reloading = true;
            reloadEndTime = Time.time + weapon.ReloadTime;
            CombatEvents.RaiseReloadStateChanged(true);
            if (weapon.ReloadTime <= 0f) FinishReload();
        }

        void FinishReload()
        {
            reloading = false;
            ammo = weapon.MagazineSize;
            CombatEvents.RaiseAmmoChanged(ammo, weapon.MagazineSize, weapon.Kind);
            CombatEvents.RaiseReloadStateChanged(false);
        }

        /// <summary>Het dan vu khi dac biet -> ve vu khi khoi dau day bang. Tra true neu da doi.</summary>
        bool TryRevertToStartingWeapon()
        {
            if (startingWeapon == null || weapon == startingWeapon) return false;
            if (config != null && !config.RevertToStartingWeaponWhenEmpty) return false;
            Equip(startingWeapon, true);
            return true;
        }

        // ---------- Ban ----------

        void TryFire(Vector2 screenPos)
        {
            if (weapon == null || Time.timeScale <= 0f) return;
            if (CombatPauseSignal.IsPaused || reloading) return;
            if (Time.time < nextFireTime) return;
            if (PointerBlocker != null && PointerBlocker(screenPos)) return;

            if (ammo <= 0)
            {
                CombatEvents.RaiseOutOfAmmo();
                nextFireTime = Time.time + (config != null ? config.EmptyClickCooldown : CombatConfig.DefaultEmptyClickCooldown); // tranh spam khi giu
                if (!TryRevertToStartingWeapon() && config != null && config.AutoReloadWhenEmpty) StartReload();
                return;
            }

            nextFireTime = weapon.HoldToFire ? Time.time + weapon.FireInterval : 0f;
            ammo--;
            CombatEvents.RaiseShotFired(weapon.Kind, screenPos);
            CombatEvents.RaiseAmmoChanged(ammo, weapon.MagazineSize, weapon.Kind);

            WeaponData collected = ResolveShot(screenPos);

            if (collected != null)
            {
                // Nhat thung khong ton dan (F-107): hoan lai vien vua ban, roi doi vu khi (day bang).
                ammo = Mathf.Min(ammo + 1, weapon.MagazineSize);
                Equip(collected, true);
                return;
            }

            if (ammo <= 0)
            {
                CombatEvents.RaiseOutOfAmmo();
                if (!TryRevertToStartingWeapon() && config != null && config.AutoReloadWhenEmpty) StartReload();
            }
        }

        Vector2? Project(Vector3 world)
        {
            if (cam == null || !cam.isActiveAndEnabled) cam = Camera.main;
            if (cam == null) return null;
            var sp = cam.WorldToScreenPoint(world);
            if (sp.z <= 0f) return null;
            return new Vector2(sp.x, sp.y);
        }

        /// <summary>Hinh chu nhat man hinh bao quanh cac collider dang bat cua enemy (bo collider thuoc ITapTarget khac, vd. con tin bi bat lam khien).</summary>
        Rect? BodyRect(ITapTarget t)
        {
            var comp = t as Component;
            if (comp == null) return null;
            if (!bodyColliders.TryGetValue(t.Id, out var cols) || cols == null || (cols.Length > 0 && cols[0] == null))
            {
                comp.GetComponentsInChildren(true, colScratch);
                keepScratch.Clear();
                for (int i = 0; i < colScratch.Count; i++)
                {
                    var owner = colScratch[i].GetComponentInParent<ITapTarget>();
                    if (owner == null || ReferenceEquals(owner, t) || owner.Id == t.Id) keepScratch.Add(colScratch[i]);
                }
                colScratch.Clear();
                cols = keepScratch.Count == 0 ? NoColliders : keepScratch.ToArray(); // cache ca ket qua rong
                bodyColliders[t.Id] = cols;
            }
            bool any = false;
            float minX = 0, minY = 0, maxX = 0, maxY = 0;
            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                if (c == null || !c.enabled || !c.gameObject.activeInHierarchy || c.isTrigger) continue;
                Bounds b = c.bounds;
                Vector3 mn = b.min, mx = b.max;
                for (int k = 0; k < 8; k++)
                {
                    var p = Project(new Vector3((k & 1) == 0 ? mn.x : mx.x, (k & 2) == 0 ? mn.y : mx.y, (k & 4) == 0 ? mn.z : mx.z));
                    if (!p.HasValue) return null; // cham/sau mat phang camera: bo qua than
                    var v = p.Value;
                    if (!any) { minX = maxX = v.x; minY = maxY = v.y; any = true; }
                    else { if (v.x < minX) minX = v.x; if (v.x > maxX) maxX = v.x; if (v.y < minY) minY = v.y; if (v.y > maxY) maxY = v.y; }
                }
            }
            if (!any) return null;
            return Rect.MinMaxRect(minX - bodyPadPx, minY - bodyPadPx, maxX + bodyPadPx, maxY + bodyPadPx);
        }

        // ---------- Thung no (uu tien) ----------
        float barrelPadPx;
        Ray barrelRay;
        Func<IPriorityShootable, Rect?> barrelRectFunc;
        Func<IPriorityShootable, bool> barrelOccludedFunc;
        Func<IPriorityShootable, float> barrelDepthFunc;
        readonly RaycastHit[] rayBuf = new RaycastHit[16];

        float BarrelDepth(IPriorityShootable b) { return (b.PriorityBounds.center - barrelRay.origin).sqrMagnitude; }

        Rect? BarrelRect(IPriorityShootable b)
        {
            Bounds bb = b.PriorityBounds;
            if (bb.size == Vector3.zero) return null;
            Vector3 mn = bb.min, mx = bb.max;
            float minX = 0, minY = 0, maxX = 0, maxY = 0; bool any = false;
            for (int k = 0; k < 8; k++)
            {
                var p = Project(new Vector3((k & 1) == 0 ? mn.x : mx.x, (k & 2) == 0 ? mn.y : mx.y, (k & 4) == 0 ? mn.z : mx.z));
                if (!p.HasValue) return null;
                var v = p.Value;
                if (!any) { minX = maxX = v.x; minY = maxY = v.y; any = true; }
                else { if (v.x < minX) minX = v.x; if (v.x > maxX) maxX = v.x; if (v.y < minY) minY = v.y; if (v.y > maxY) maxY = v.y; }
            }
            return Rect.MinMaxRect(minX - barrelPadPx, minY - barrelPadPx, maxX + barrelPadPx, maxY + barrelPadPx);
        }

        // Bi che neu tren tia tu camera toi thung co collider khac (khong phai thung, khong phai ITapTarget) o truoc thung.
        bool BarrelOccluded(IPriorityShootable b)
        {
            Vector3 toT = b.PriorityBounds.center - barrelRay.origin;
            float dist = toT.magnitude;
            if (dist < 0.01f) return false;
            int mask = config != null ? config.EnvironmentMask.value : ~0;
            int n = Physics.RaycastNonAlloc(new Ray(barrelRay.origin, toT / dist), rayBuf, dist, mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var col = rayBuf[i].collider;
                if (col == null) continue;
                if (ReferenceEquals(col.GetComponentInParent<IPriorityShootable>(), b)) continue;
                if (col.GetComponentInParent<ITapTarget>() != null) continue;
                return true;
            }
            return false;
        }

        void ResolveBarrel(Vector2 screenPos, IPriorityShootable barrel)
        {
            Vector3 point = barrel.PriorityBounds.center;
            Vector3 dir = (point - barrelRay.origin).normalized;
            barrel.OnShot(new ShotInfo
            {
                ScreenPosition = screenPos,
                HitPoint = point,
                HitNormal = -dir,
                Direction = dir,
                Weapon = weapon.Kind,
                ImpulseScale = weapon.ImpulseScale
            });
            EmitEnvironmentResult(screenPos, point, true, true);
        }

        /// <returns>Vu khi cua thung vua nhat (neu co), nguoc lai null.</returns>
        WeaponData ResolveShot(Vector2 screenPos)
        {
            if (cam == null || !cam.isActiveAndEnabled) cam = Camera.main;
            if (cam == null) { Debug.LogWarning("[TapShooter] Khong co Camera.main.", this); return null; }
            if (projectFunc == null) projectFunc = Project;

            float scale = Mathf.Min(Screen.width, Screen.height) / (config != null ? config.ReferenceScreenHeight : CombatConfig.DefaultReferenceScreenHeight);
            float radius = weapon.HitRadiusPx * scale;
            float justice = (config != null ? config.JusticeRadiusPx : CombatConfig.DefaultJusticeRadiusPx) * scale * justiceRadiusScale;

            bool bodyHit = config == null || config.EnemyBodyHit;
            bodyPadPx = (config != null ? config.BodyHitPaddingPx : CombatConfig.DefaultBodyHitPaddingPx) * scale;
            if (bodyRectFunc == null) bodyRectFunc = BodyRect;

            if (depthFunc == null) depthFunc = Depth;
            depthCamPos = cam.transform.position;
            bool nearest = config == null || config.NearestTargetFirst;
            int count = TargetSelector.Select(TargetRegistry.Targets, screenPos, radius, justice,
                weapon.MaxTargetsPerShot, projectFunc, hits, bodyHit ? bodyRectFunc : null, nearest ? depthFunc : null);

            bool barrelOn = config == null || config.BarrelPriority;
            if (BarrelPriority.Applies(barrelOn, weapon.MaxTargetsPerShot, count, count > 0 && hits[0].Justice))
            {
                barrelPadPx = (config != null ? config.BarrelHitPaddingPx : CombatConfig.DefaultBarrelHitPaddingPx) * scale;
                barrelRay = cam.ScreenPointToRay(screenPos);
                if (barrelRectFunc == null) { barrelRectFunc = BarrelRect; barrelOccludedFunc = BarrelOccluded; barrelDepthFunc = BarrelDepth; }
                var barrel = BarrelPriority.Pick(PriorityShootables.Items, screenPos, barrelRectFunc, barrelOccludedFunc, barrelDepthFunc);
                if (barrel != null) { ResolveBarrel(screenPos, barrel); return null; }
            }

            if (count == 0) { ResolveEnvironment(screenPos); return null; }

            Vector3 camPos = cam.transform.position;
            WeaponData collected = null;
            bool hostageDamaged = false;
            bool anyKill = false;
            TapOutcome shotOutcome = TapOutcome.Miss;
            pending.Clear();
            for (int i = 0; i < count; i++)
            {
                var h = hits[i];
                var t = h.Target;
                // Lay so lieu truoc OnTapHit (muc tieu co the reset/unregister sau khi trung).
                float reaction = t.ExposedTime;
                float progress = t.ReticleProgress;
                TargetKind kind = t.Kind;
                Vector3 point = h.Justice ? t.JusticePoint : TapPoint(t, screenPos);
                Vector3 dir = (point - camPos).normalized;
                var info = new ShotInfo
                {
                    ScreenPosition = screenPos,
                    HitPoint = point,
                    HitNormal = -dir,
                    Direction = dir,
                    Weapon = weapon.Kind,
                    ImpulseScale = weapon.ImpulseScale
                };
                BreakGlassBetween(camPos, point, screenPos);
                TapOutcome outcome = t.OnTapHit(info, h.Justice);

                if (outcome == TapOutcome.HostageHit && !hostageDamaged)
                {
                    hostageDamaged = true;
                    PlayerDamageService.Damage(DamageSource.HostageHit, point);
                }
                if (outcome == TapOutcome.PickupCollected && collected == null && t is WeaponPickup wp) collected = wp.Weapon;

                if (outcome == TapOutcome.Kill || outcome == TapOutcome.JusticeKill) { anyKill = true; shotOutcome = outcome; }
                else if (!anyKill) shotOutcome = outcome;

                pending.Add(new ShotResult
                {
                    Outcome = outcome,
                    TargetKind = kind,
                    WorldPoint = point,
                    ReticleProgress = progress,
                    ReactionTime = reaction,
                    Weapon = weapon.Kind,
                    TargetsHit = count
                });
            }

            // Combo tinh mot lan moi phat, TRUOC khi phat ShotResolved (ComboMultiplier = he so SAU phat).
            float mult = combo != null ? combo.RegisterShot(shotOutcome) : CombatEvents.Current.ComboMultiplier;
            for (int i = 0; i < pending.Count; i++)
            {
                var r = pending[i];
                r.ComboMultiplier = mult;
                CombatEvents.RaiseShotResolved(r);
            }
            return collected;
        }

        /// <summary>Diem dan trung = cho nguoi choi tap tren than muc tieu (giao tia camera voi collider), khong phai tam. Tap trong vung pad (khong cham collider) -> diem tren tia o do sau cua tam.</summary>
        Vector3 TapPoint(ITapTarget t, Vector2 screenPos)
        {
            Vector3 aim = t.AimPoint;
            var ray = cam.ScreenPointToRay(screenPos);
            var comp = t as Component;
            if (comp != null)
            {
                float maxDist = config != null ? config.MaxRayDistance : CombatConfig.DefaultMaxRayDistance;
                float best = float.MaxValue; Vector3 bestPt = aim; bool found = false;
                comp.GetComponentsInChildren(true, colScratch);
                for (int i = 0; i < colScratch.Count; i++)
                {
                    var c = colScratch[i];
                    if (c == null || !c.enabled || c.isTrigger || !c.gameObject.activeInHierarchy) continue;
                    var owner = c.GetComponentInParent<ITapTarget>();
                    if (owner != null && !ReferenceEquals(owner, t) && owner.Id != t.Id) continue;
                    if (c.Raycast(ray, out RaycastHit rh, maxDist) && rh.distance < best) { best = rh.distance; bestPt = rh.point; found = true; }
                }
                colScratch.Clear();
                if (found) return bestPt;
            }
            float depth = Vector3.Dot(aim - ray.origin, ray.direction);
            return depth > 0.1f ? ray.origin + ray.direction * depth : aim;
        }

        /// <summary>Muc tieu dang sau lop kinh (IShootable khong phai ITapTarget) tren tia camera -> muc tieu: ban trung muc tieu thi kinh vo.</summary>
        void BreakGlassBetween(Vector3 from, Vector3 to, Vector2 screenPos)
        {
            Vector3 d = to - from; float dist = d.magnitude;
            if (dist < 0.05f) return;
            Vector3 dir = d / dist;
            int mask = config != null ? config.EnvironmentMask.value : ~0;
            int n = Physics.RaycastNonAlloc(new Ray(from, dir), rayBuf, dist, mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var col = rayBuf[i].collider;
                if (col == null || col.GetComponentInParent<ITapTarget>() != null) continue;
                var shootable = col.GetComponentInParent<IShootable>();
                if (shootable == null) continue;
                shootable.OnShot(new ShotInfo
                {
                    ScreenPosition = screenPos,
                    HitPoint = rayBuf[i].point,
                    HitNormal = rayBuf[i].normal,
                    Direction = dir,
                    Weapon = weapon.Kind,
                    ImpulseScale = weapon.ImpulseScale
                });
            }
        }

        void ResolveEnvironment(Vector2 screenPos)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            float maxDist = config != null ? config.MaxRayDistance : CombatConfig.DefaultMaxRayDistance;
            int mask = config != null ? config.EnvironmentMask.value : ~0;
            Vector3 point = ray.origin + ray.direction * maxDist;
            bool rayHit = false, shootableHit = false;

            if (Physics.Raycast(ray, out RaycastHit hit, maxDist, mask, QueryTriggerInteraction.Ignore))
            {
                rayHit = true;
                point = hit.point;
                var shootable = hit.collider.GetComponentInParent<IShootable>();
                if (shootable != null)
                {
                    shootableHit = true;
                    shootable.OnShot(new ShotInfo
                    {
                        ScreenPosition = screenPos,
                        HitPoint = hit.point,
                        HitNormal = hit.normal,
                        Direction = ray.direction,
                        Weapon = weapon.Kind,
                        ImpulseScale = weapon.ImpulseScale
                    });
                }
            }

            EmitEnvironmentResult(screenPos, point, rayHit, shootableHit);
        }

        void EmitEnvironmentResult(Vector2 screenPos, Vector3 point, bool rayHit, bool shootableHit)
        {
            // C9: Environment chi khi trung IShootable (giu combo); tuong tro/khong trung gi = Miss (reset combo).
            TapOutcome outcome = ShotClassifier.ClassifyEnvironment(rayHit, shootableHit);
            float mult = combo != null ? combo.RegisterShot(outcome, ShotClassifier.KeepsCombo(outcome)) : CombatEvents.Current.ComboMultiplier;
            CombatEvents.RaiseShotResolved(new ShotResult
            {
                Outcome = outcome,
                TargetKind = TargetKind.Enemy, // khong co muc tieu; giu gia tri mac dinh
                WorldPoint = point,
                ReticleProgress = 0f,
                ReactionTime = 0f,
                Weapon = weapon.Kind,
                ComboMultiplier = mult,
                TargetsHit = 0
            });
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { PointerBlocker = null; }
    }
}
