using UnityEngine;
using ClaudeCop.Combat;

namespace ClaudeCop.Meta
{
    /// <summary>Trang bi cho mot tran, tinh tu ho so (thuan, test duoc). Game (LoadoutApplier) ap vao TapShooter/PlayerHealth/ComboSystem/EnemyActor.</summary>
    public struct Loadout
    {
        public GunDef Gun;
        public bool Rented;
        public int ScopeLevel, SilencerLevel;
        public string Skin;
        public int Magazine;
        public float HitRadiusPx;
        /// <summary>He so thoi gian vong target cua enemy (giam thanh). 1 = chuan.</summary>
        public float EnemyReticleScale;
        public int ArmorPerStage;     // ao
        public int ExtraLives;        // mu
        public int MissForgiveness;   // gang tay (moi Stage)
        public float JusticeRadiusScale; // kinh
        public float CoinBonus;       // bo dam (+% xu cuoi tran)
        public int StartArmor;        // quang cao truoc tran

        /// <summary>Tao WeaponData luc chay voi chi so da nang cap (khong can asset).</summary>
        public WeaponData CreateWeapon()
        {
            if (Gun == null) return null;
            var w = ScriptableObject.CreateInstance<WeaponData>();
            w.name = "Loadout_" + Gun.Id;
            w.Configure(Gun.Kind, Magazine, HitRadiusPx, Gun.MaxTargets, Gun.ShotsPerSecond, Gun.ReloadTime, Gun.ImpulseScale, Gun.HoldToFire);
            return w;
        }
    }

    public static class LoadoutBuilder
    {
        /// <summary>Tinh trang bi; khong doi ho so (xem Consume).</summary>
        public static Loadout Build(ProfileData p, MetaCatalog c)
        {
            var l = new Loadout();
            bool rented = !string.IsNullOrEmpty(p.rentedGun) && c.Gun(p.rentedGun) != null && !p.OwnsGun(p.rentedGun);
            l.Gun = rented ? c.Gun(p.rentedGun) : (c.Gun(p.equippedGun) ?? c.DefaultGun);
            l.Rented = rented;
            var state = l.Gun != null ? p.Gun(l.Gun.Id) : null;
            l.ScopeLevel = state != null ? state.scope : 0;
            l.SilencerLevel = state != null ? state.silencer : 0;
            l.Skin = state != null ? state.skin : "";
            if (l.Gun != null)
            {
                l.Magazine = l.Gun.Magazine;
                l.HitRadiusPx = l.Gun.HitRadiusPx * (1f + c.ScopeRadiusPerLevel * l.ScopeLevel);
            }
            l.EnemyReticleScale = 1f + c.SilencerReticlePerLevel * l.SilencerLevel;
            l.ArmorPerStage = p.GearLevel(GearItem.Vest);
            l.ExtraLives = p.GearLevel(GearItem.Helmet);
            l.MissForgiveness = p.GearLevel(GearItem.Gloves);
            l.JusticeRadiusScale = 1f + c.GlassesJusticePerLevel * p.GearLevel(GearItem.Glasses);
            l.CoinBonus = c.RadioCoinPerLevel * p.GearLevel(GearItem.Radio);
            l.StartArmor = p.pendingArmor;
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
