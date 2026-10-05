using System;
using UnityEngine;
using ClaudeCop.Combat;

namespace ClaudeCop.Meta
{
    /// <summary>Tong chi so tu moi nang cap dang co hieu luc (trang bi tren ban + nang cap cua sung dang dung).</summary>
    public sealed class StatTotals
    {
        readonly float[] values = new float[Enum.GetValues(typeof(Stat)).Length];

        public float this[Stat s] => values[(int)s];
        public int Int(Stat s) => Mathf.RoundToInt(values[(int)s]);
        public void Add(UpgradeDef u, int level) { if (level > 0) values[(int)u.Stat] += u.PerLevel * level; }
    }

    /// <summary>Trang bi cho mot tran, tinh tu ho so (thuan, test duoc). Game.LoadoutApplier ap vao tran.</summary>
    public sealed class Loadout
    {
        public GunDef Gun;
        public bool Rented;
        public string Skin = "";
        public readonly StatTotals Stats = new StatTotals();

        // Chi so suy ra (cach ap moi Stat nam het o day).
        public int Magazine => Gun == null ? 0 : Gun.Magazine + Stats.Int(Stat.Magazine);
        public float HitRadiusPx => Gun == null ? 0f : Gun.HitRadiusPx * (1f + Stats[Stat.HitRadius]);
        public float ReloadTime => Gun == null ? 0f : Gun.ReloadTime * Mathf.Max(0.2f, 1f - Stats[Stat.ReloadSpeed]);
        public float EnemyReticleScale => 1f + Stats[Stat.EnemyReticle];
        public int ArmorPerStage => Stats.Int(Stat.ArmorPerStage);
        public int ExtraLives => Stats.Int(Stat.ExtraLives);
        public int MissForgiveness => Stats.Int(Stat.MissForgiveness);
        public float JusticeRadiusScale => 1f + Stats[Stat.JusticeRadius];
        public float CoinBonus => Stats[Stat.CoinBonus];
        /// <summary>Giap them cho ca tran (xem quang cao truoc tran).</summary>
        public int StartArmor;

        /// <summary>Tao WeaponData luc chay voi chi so da nang cap (khong can asset).</summary>
        public WeaponData CreateWeapon()
        {
            if (Gun == null) return null;
            var w = ScriptableObject.CreateInstance<WeaponData>();
            w.name = "Loadout_" + Gun.Id;
            w.Configure(Gun.Kind, Magazine, HitRadiusPx, Gun.MaxTargets, Gun.ShotsPerSecond, ReloadTime, Gun.ImpulseScale, Gun.HoldToFire);
            return w;
        }
    }

    public static class LoadoutBuilder
    {
        /// <summary>Tinh trang bi tu ho so; khong doi ho so (xem Consume).</summary>
        public static Loadout Build(ProfileData p, MetaCatalog c)
        {
            var l = new Loadout();
            bool rented = !string.IsNullOrEmpty(p.rentedGun) && c.Gun(p.rentedGun) != null && !p.OwnsGun(p.rentedGun);
            l.Gun = rented ? c.Gun(p.rentedGun) : (c.Gun(p.equippedGun) ?? c.DefaultGun);
            l.Rented = rented;
            l.StartArmor = p.pendingArmor;

            foreach (var u in c.Gear) l.Stats.Add(u, p.GetLevel(UpgradeKey.Gear(u.Id)));
            if (l.Gun != null)
            {
                foreach (var u in c.GunUpgrades) l.Stats.Add(u, p.GetLevel(UpgradeKey.Gun(l.Gun.Id, u.Id)));
                var state = p.Gun(l.Gun.Id);
                if (state != null) l.Skin = state.skin;
            }
            return l;
        }

        /// <summary>Dung het do mot lan (sung thue, giap quang cao) khi tran bat dau. Tra true neu ho so thay doi.</summary>
        public static bool Consume(ProfileData p)
        {
            bool changed = !string.IsNullOrEmpty(p.rentedGun) || p.pendingArmor != 0;
            p.rentedGun = "";
            p.pendingArmor = 0;
            return changed;
        }
    }
}
