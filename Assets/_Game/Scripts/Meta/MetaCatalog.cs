using System.Collections.Generic;
using ClaudeCop.Core;

namespace ClaudeCop.Meta
{
    /// <summary>Nhanh nang cap cua sung (note: ong ngam, giam thanh, ngoai hinh).</summary>
    public enum GunUpgrade { Scope, Silencer }

    /// <summary>Mon tren ban (man Home): chon mon nao thi nang cap mon do.</summary>
    public enum GearItem { Vest, Helmet, Gloves, Glasses, Radio }

    /// <summary>Chi so goc cua mot sung mua duoc (khong can asset: LoadoutBuilder tao WeaponData luc chay).</summary>
    public sealed class GunDef
    {
        public string Id, Name;
        public WeaponKind Kind;
        public int Price;                 // 0 = co san
        public int Magazine;
        public float HitRadiusPx, ShotsPerSecond, ReloadTime, ImpulseScale;
        public int MaxTargets = 1;
        public bool HoldToFire;
        /// <summary>Gia thue 1 tran bang xu.</summary>
        public int RentPrice => Price / 5;
    }

    public sealed class SkinDef
    {
        public string Id, Name;
        public int CoinPrice;             // > 0: mua bang xu
        public int BadgePrice;            // > 0: mua bang huy hieu (rank S)
    }

    /// <summary>Bang gia + so lieu cua tat ca do trong man Home. Mot nguon duy nhat; chinh can bang o day.</summary>
    public sealed class MetaCatalog
    {
        public readonly List<GunDef> Guns = new List<GunDef>();
        public readonly List<SkinDef> Skins = new List<SkinDef>();
        public string DefaultGunId = "pistol";

        // Nang cap sung: gia moi cap (do dai = so cap toi da).
        public int[] ScopePrices = { 150, 250, 400, 650, 1000 };
        public int[] SilencerPrices = { 200, 350, 550, 800, 1200 };
        public float ScopeRadiusPerLevel = 0.06f;      // +6% vung trung moi cap
        public float SilencerReticlePerLevel = 0.04f;  // vong target cua enemy thu cham hon 4% moi cap

        // Trang bi tren ban: gia moi cap.
        public int[] VestPrices = { 600, 1500 };              // giap nap lai moi Stage = cap
        public int[] HelmetPrices = { 1200, 3000 };           // tim them = cap
        public int[] GlovesPrices = { 400, 900, 1600 };       // so lan truot khong mat combo moi Stage = cap
        public int[] GlassesPrices = { 300, 700, 1300 };      // ban kinh Justice +GlassesJusticePerLevel moi cap
        public int[] RadioPrices = { 500, 1200, 2500 };       // +RadioCoinPerLevel xu cuoi tran moi cap
        public float GlassesJusticePerLevel = 0.15f;
        public float RadioCoinPerLevel = 0.10f;

        // Xu cuoi moi level.
        public int ScorePerCoin = 50;
        public int WinBonus = 200;
        public int NoDamageBonus = 150;
        public int PerJustice = 20;
        public int ComboBonus = 100;
        public float ComboBonusAt = 5f;
        public int AdMultiplier = 2;
        /// <summary>Huy hieu: lan dau dat rank S o moi level.</summary>
        public int BadgesPerFirstS = 1;
        /// <summary>Giap them khi xem quang cao truoc tran.</summary>
        public int AdArmor = 1;

        public GunDef Gun(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var g in Guns) if (g.Id == id) return g;
            return null;
        }

        public SkinDef Skin(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var s in Skins) if (s.Id == id) return s;
            return null;
        }

        public GunDef DefaultGun => Gun(DefaultGunId) ?? (Guns.Count > 0 ? Guns[0] : null);

        public int[] Prices(GunUpgrade u) => u == GunUpgrade.Scope ? ScopePrices : SilencerPrices;

        public int[] Prices(GearItem g)
        {
            switch (g)
            {
                case GearItem.Vest: return VestPrices;
                case GearItem.Helmet: return HelmetPrices;
                case GearItem.Gloves: return GlovesPrices;
                case GearItem.Glasses: return GlassesPrices;
                default: return RadioPrices;
            }
        }

        static MetaCatalog def;
        /// <summary>Bang gia mac dinh cua game.</summary>
        public static MetaCatalog Default => def ?? (def = CreateDefault());

        public static MetaCatalog CreateDefault()
        {
            var c = new MetaCatalog();
            // Pistol khop Weapon_Pistol.asset (6 vien, 90 px, thay dan 0.5 s).
            c.Guns.Add(new GunDef { Id = "pistol", Name = "Pistol", Kind = WeaponKind.Pistol, Price = 0, Magazine = 6, HitRadiusPx = 90f, ShotsPerSecond = 10f, ReloadTime = 0.5f, ImpulseScale = 1f });
            c.Guns.Add(new GunDef { Id = "revolver", Name = "Revolver", Kind = WeaponKind.Revolver, Price = 1500, Magazine = 6, HitRadiusPx = 110f, ShotsPerSecond = 3f, ReloadTime = 0.9f, ImpulseScale = 1.4f, MaxTargets = 2 });
            c.Guns.Add(new GunDef { Id = "smg", Name = "SMG", Kind = WeaponKind.SMG, Price = 3000, Magazine = 20, HitRadiusPx = 75f, ShotsPerSecond = 8f, ReloadTime = 0.8f, ImpulseScale = 0.6f, HoldToFire = true });
            c.Skins.Add(new SkinDef { Id = "matte", Name = "Đen nhám", CoinPrice = 300 });
            c.Skins.Add(new SkinDef { Id = "camo", Name = "Rằn ri", CoinPrice = 600 });
            c.Skins.Add(new SkinDef { Id = "gold", Name = "Vàng", BadgePrice = 3 });
            return c;
        }
    }
}
