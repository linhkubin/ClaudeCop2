using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Combat;

namespace ClaudeCop.UI
{
    /// <summary>Bind GameEvents/CombatEvents vao HudView + gan TapShooter.PointerBlocker (chan tap len UI).</summary>
    public class HudPresenter : MonoBehaviour
    {
        [SerializeField] HudView view;

        void OnEnable()
        {
            if (view == null) { Debug.LogError("[HudPresenter] Thieu HudView.", this); return; }
            GameEvents.LivesChanged += OnLives;
            GameEvents.ScoreChanged += view.SetScore;
            CombatEvents.AmmoChanged += OnAmmo;
            CombatEvents.WeaponChanged += view.SetWeapon;
            CombatEvents.ReloadStateChanged += view.SetReloading;
            CombatEvents.OutOfAmmo += view.PulseReload;
            CombatEvents.ComboChanged += view.SetCombo;
            TapShooter.PointerBlocker = UiPointerBlocker.Delegate;

            var g = GameEvents.Current; var c = CombatEvents.Current;
            view.SetScore(g.Score);
            view.SetLives(g.Lives, g.MaxLives);
            view.SetAmmo(c.Ammo, c.MaxAmmo);
            view.SetWeapon(c.Weapon);
            view.SetReloading(c.Reloading);
            view.SetCombo(c.ComboStreak, c.ComboMultiplier);
        }

        void OnDisable()
        {
            GameEvents.LivesChanged -= OnLives;
            CombatEvents.AmmoChanged -= OnAmmo;
            if (view != null)
            {
                GameEvents.ScoreChanged -= view.SetScore;
                CombatEvents.WeaponChanged -= view.SetWeapon;
                CombatEvents.ReloadStateChanged -= view.SetReloading;
                CombatEvents.OutOfAmmo -= view.PulseReload;
                CombatEvents.ComboChanged -= view.SetCombo;
            }
            if (TapShooter.PointerBlocker == UiPointerBlocker.Delegate) TapShooter.PointerBlocker = null;
        }

        void OnLives(int cur, int max) => view.SetLives(cur, max);
        void OnAmmo(int cur, int max, WeaponKind w) => view.SetAmmo(cur, max);
    }
}
