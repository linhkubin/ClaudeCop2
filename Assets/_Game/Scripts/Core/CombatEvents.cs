using System;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>Anh chup trang thai chien dau de UI doc khi OnEnable.</summary>
    public struct CombatSnapshot
    {
        public WeaponKind Weapon;
        public int Ammo;
        public int MaxAmmo;
        public bool Reloading;
        public int ComboStreak;
        public float ComboMultiplier;
    }

    /// <summary>
    /// C16. Su kien chien dau. Chi Combat (TapShooter, ComboSystem) duoc Raise; UI, Game, RankScore nghe.
    /// Current luon cap nhat truoc khi phat event.
    /// </summary>
    public static class CombatEvents
    {
        static CombatSnapshot current = new CombatSnapshot { ComboMultiplier = 1f };
        public static CombatSnapshot Current => current;

        /// <summary>(weapon, screenPos)</summary>
        public static event Action<WeaponKind, Vector2> ShotFired;
        public static event Action<ShotResult> ShotResolved;
        /// <summary>(cur, max, weapon)</summary>
        public static event Action<int, int, WeaponKind> AmmoChanged;
        public static event Action<WeaponKind> WeaponChanged;
        public static event Action<bool> ReloadStateChanged;
        /// <summary>(streak, multiplier)</summary>
        public static event Action<int, float> ComboChanged;
        public static event Action OutOfAmmo;

        public static void RaiseShotFired(WeaponKind weapon, Vector2 screenPos) => ShotFired?.Invoke(weapon, screenPos);
        public static void RaiseShotResolved(ShotResult result) => ShotResolved?.Invoke(result);
        public static void RaiseAmmoChanged(int cur, int max, WeaponKind weapon)
        {
            current.Ammo = cur; current.MaxAmmo = max; current.Weapon = weapon;
            AmmoChanged?.Invoke(cur, max, weapon);
        }
        public static void RaiseWeaponChanged(WeaponKind weapon)
        {
            current.Weapon = weapon;
            WeaponChanged?.Invoke(weapon);
        }
        public static void RaiseReloadStateChanged(bool reloading)
        {
            current.Reloading = reloading;
            ReloadStateChanged?.Invoke(reloading);
        }
        public static void RaiseComboChanged(int streak, float multiplier)
        {
            current.ComboStreak = streak; current.ComboMultiplier = multiplier;
            ComboChanged?.Invoke(streak, multiplier);
        }
        public static void RaiseOutOfAmmo() => OutOfAmmo?.Invoke();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            current = new CombatSnapshot { ComboMultiplier = 1f };
            ShotFired = null; ShotResolved = null; AmmoChanged = null; WeaponChanged = null;
            ReloadStateChanged = null; ComboChanged = null; OutOfAmmo = null;
        }
    }
}
