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

        InputAction tapAction;
        InputAction tapPositionAction;
        WeaponData weapon;
        int ammo;
        bool reloading;
        float reloadEndTime;
        float nextFireTime;
        Camera cam;
        int holdTouchId = -1;

        readonly List<TargetHit> hits = new List<TargetHit>(8);
        readonly List<ShotResult> pending = new List<ShotResult>(8);
        Func<Vector3, Vector2?> projectFunc;

        public WeaponData CurrentWeapon => weapon;
        public int Ammo => ammo;
        public bool IsReloading => reloading;
        /// <summary>Vu khi khoi dau (Pistol): quay ve khi het dan vu khi dac biet.</summary>
        public WeaponData StartingWeapon => startingWeapon;

        void Awake() { projectFunc = Project; }

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
            if (startingWeapon != null) Equip(startingWeapon, true);
        }

        bool holdLocked;

        void OnDisable()
        {
            if (tapAction != null) tapAction.performed -= OnTapPerformed;
            GameCommands.ReloadRequested -= StartReload;
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
            if (!anyTouch) holdTouchId = -1;

            // Chuot/but: giu de ban (Sung may).
            bool mousePressed = !anyTouch && tapAction != null && tapAction.IsPressed();
            if (mousePressed && weapon.HoldToFire && !holdLocked)
                TryFire(tapPositionAction.ReadValue<Vector2>());

            // F-202: sau khi nhat thung, bo qua lan giu hien tai den khi nha tay.
            if (holdLocked && !anyTouch && !mousePressed) holdLocked = false;
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
                nextFireTime = Time.time + 0.2f; // tranh spam khi giu
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

        /// <returns>Vu khi cua thung vua nhat (neu co), nguoc lai null.</returns>
        WeaponData ResolveShot(Vector2 screenPos)
        {
            if (cam == null || !cam.isActiveAndEnabled) cam = Camera.main;
            if (cam == null) { Debug.LogWarning("[TapShooter] Khong co Camera.main.", this); return null; }
            if (projectFunc == null) projectFunc = Project;

            float scale = Mathf.Min(Screen.width, Screen.height) / (config != null ? config.ReferenceScreenHeight : 1080f);
            float radius = weapon.HitRadiusPx * scale;
            float justice = (config != null ? config.JusticeRadiusPx : 35f) * scale;

            int count = TargetSelector.Select(TargetRegistry.Targets, screenPos, radius, justice,
                weapon.MaxTargetsPerShot, projectFunc, hits);

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
                Vector3 point = h.Justice ? t.JusticePoint : t.AimPoint;
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

        void ResolveEnvironment(Vector2 screenPos)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            float maxDist = config != null ? config.MaxRayDistance : 100f;
            int mask = config != null ? config.EnvironmentMask.value : ~0;
            TapOutcome outcome = TapOutcome.Miss;
            Vector3 point = ray.origin + ray.direction * maxDist;
            bool shootableHit = false;

            if (Physics.Raycast(ray, out RaycastHit hit, maxDist, mask, QueryTriggerInteraction.Ignore))
            {
                outcome = TapOutcome.Environment;
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

            // Ban vat IShootable giu combo (plan muc 14); tuong tro/khong trung gi thi reset.
            float mult = combo != null ? combo.RegisterShot(outcome, shootableHit) : CombatEvents.Current.ComboMultiplier;
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
