using System.Collections.Generic;
using ClaudeCop.Core;

namespace ClaudeCop.Meta
{
    /// <summary>Chi so ma nang cap tac dong. Them chi so moi: them vao day + cach ap trong Loadout.Build.</summary>
    public enum Stat
    {
        HitRadius,        // +% vung trung cua sung
        Magazine,         // +vien dan moi bang
        ReloadSpeed,      // -% thoi gian thay dan
        EnemyReticle,     // +% thoi gian vong target cua enemy (enemy ngam cham hon)
        ArmorPerStage,    // giap nap lai moi Stage
        ExtraLives,       // tim them
        MissForgiveness,  // so lan truot khong mat combo moi Stage
        JusticeRadius,    // +% vung Justice
        CoinBonus,        // +% xu cuoi level
    }

    /// <summary>Mot nhanh nang cap: moi cap cong PerLevel vao Stat. So cap toi da = so phan tu Prices.</summary>
    public sealed class UpgradeDef
    {
        public readonly string Id, Name;
        public readonly Stat Stat;
        public readonly float PerLevel;
        public readonly int[] Prices;

        public UpgradeDef(string id, string name, Stat stat, float perLevel, params int[] prices)
        {
            Id = id; Name = name; Stat = stat; PerLevel = perLevel; Prices = prices;
        }

        public int MaxLevel => Prices.Length;
        /// <summary>Gia de len cap ke tu cap hien tai; -1 neu da toi da.</summary>
        public int PriceFrom(int level) => level >= 0 && level < Prices.Length ? Prices[level] : -1;
    }

    /// <summary>Chi so goc cua mot sung (khong can asset: Loadout tao WeaponData luc chay).</summary>
    public sealed class GunDef
    {
        public string Id, Name;
        public WeaponKind Kind;
        public int Price;                 // 0 = co san
        public int Magazine;
        public float HitRadiusPx, ShotsPerSecond, ReloadTime, ImpulseScale;
        public int MaxTargets = 1;
        public bool HoldToFire;
        public int RentPrice => Price / 5;
    }

    public sealed class SkinDef
    {
        public string Id, Name;
        public int CoinPrice;             // > 0: mua bang xu
        public int BadgePrice;            // > 0: mua bang huy hieu (rank S)
    }

    /// <summary>
    /// Bang gia + so lieu cua man Home: mot nguon duy nhat. Them sung / nang cap / mon trang bi = them 1 dong o CreateDefault.
    /// </summary>
    public sealed class MetaCatalog
    {
        public readonly List<GunDef> Guns = new List<GunDef>();
        /// <summary>Nang cap ap cho moi sung (moi sung co cap rieng).</summary>
        public readonly List<UpgradeDef> GunUpgrades = new List<UpgradeDef>();
        /// <summary>Mon trang bi tren ban (moi mon la mot nhanh nang cap).</summary>
        public readonly List<UpgradeDef> Gear = new List<UpgradeDef>();
        public readonly List<SkinDef> Skins = new List<SkinDef>();
        public string DefaultGunId = "pistol";

        // Xu cuoi moi level.
        public int ScorePerCoin = 50;
        public int WinBonus = 200;
        public int NoDamageBonus = 150;
        public int PerJustice = 20;
        public int ComboBonus = 100;
        public float ComboBonusAt = 5f;
        public int AdMultiplier = 2;
        public int BadgesPerFirstS = 1;   // lan dau rank S o moi level
        public int AdArmor = 1;           // giap them khi xem quang cao truoc tran

        public GunDef Gun(string id) => Guns.Find(g => g.Id == id);
        public SkinDef Skin(string id) => Skins.Find(s => s.Id == id);
        public UpgradeDef GunUpgrade(string id) => GunUpgrades.Find(u => u.Id == id);
        public UpgradeDef GearItem(string id) => Gear.Find(u => u.Id == id);
        public GunDef DefaultGun => Gun(DefaultGunId) ?? (Guns.Count > 0 ? Guns[0] : null);

        static MetaCatalog def;
        public static MetaCatalog Default => def ?? (def = CreateDefault());

        public static MetaCatalog CreateDefault()
        {
            var c = new MetaCatalog();

            // Sung. Pistol khop Weapon_Pistol.asset.
            c.Guns.Add(new GunDef { Id = "pistol", Name = "Pistol", Kind = WeaponKind.Pistol, Price = 0, Magazine = 6, HitRadiusPx = 90f, ShotsPerSecond = 10f, ReloadTime = 0.5f, ImpulseScale = 1f });
            c.Guns.Add(new GunDef { Id = "revolver", Name = "Revolver", Kind = WeaponKind.Revolver, Price = 1500, Magazine = 6, HitRadiusPx = 110f, ShotsPerSecond = 3f, ReloadTime = 0.9f, ImpulseScale = 1.4f, MaxTargets = 2 });
            c.Guns.Add(new GunDef { Id = "smg", Name = "SMG", Kind = WeaponKind.SMG, Price = 3000, Magazine = 20, HitRadiusPx = 75f, ShotsPerSecond = 8f, ReloadTime = 0.8f, ImpulseScale = 0.6f, HoldToFire = true });

            // Nang cap sung:            id          ten            chi so               moi cap   gia tung cap
            c.GunUpgrades.Add(new UpgradeDef("scope",    "Ống ngắm",    Stat.HitRadius,     0.06f,    150, 250, 400, 650, 1000));
            c.GunUpgrades.Add(new UpgradeDef("silencer", "Giảm thanh",  Stat.EnemyReticle,  0.04f,    200, 350, 550, 800, 1200));

            // Trang bi tren ban:
            c.Gear.Add(new UpgradeDef("vest",    "Áo chống đạn", Stat.ArmorPerStage,   1f,    600, 1500));
            c.Gear.Add(new UpgradeDef("helmet",  "Mũ",           Stat.ExtraLives,      1f,    1200, 3000));
            c.Gear.Add(new UpgradeDef("gloves",  "Găng tay",     Stat.MissForgiveness, 1f,    400, 900, 1600));
            c.Gear.Add(new UpgradeDef("glasses", "Kính",         Stat.JusticeRadius,   0.15f, 300, 700, 1300));
            c.Gear.Add(new UpgradeDef("radio",   "Bộ đàm",       Stat.CoinBonus,       0.10f, 500, 1200, 2500));

            // Ngoai hinh sung:
            c.Skins.Add(new SkinDef { Id = "matte", Name = "Đen nhám", CoinPrice = 300 });
            c.Skins.Add(new SkinDef { Id = "camo", Name = "Rằn ri", CoinPrice = 600 });
            c.Skins.Add(new SkinDef { Id = "gold", Name = "Vàng", BadgePrice = 3 });
            return c;
        }
    }
}
