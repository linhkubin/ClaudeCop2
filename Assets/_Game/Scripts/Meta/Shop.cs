namespace ClaudeCop.Meta
{
    public enum ShopResult { Ok, NotEnoughCoins, NotEnoughBadges, AlreadyOwned, NotOwned, MaxLevel, Unknown }

    /// <summary>
    /// Logic cua hang thuan (khong luu): sung, nang cap (sung + trang bi tren ban, cung mot co che), skin, giap quang cao.
    /// Man Home goi roi PlayerProfile.Save().
    /// </summary>
    public static class Shop
    {
        // ---------- Nang cap (chung cho sung va trang bi) ----------

        /// <summary>Gia len cap ke; -1 neu da toi da.</summary>
        public static int NextPrice(ProfileData p, UpgradeDef u, string key) => u.PriceFrom(p.GetLevel(key));

        public static ShopResult Upgrade(ProfileData p, UpgradeDef u, string key)
        {
            int price = NextPrice(p, u, key);
            if (price < 0) return ShopResult.MaxLevel;
            if (p.coins < price) return ShopResult.NotEnoughCoins;
            p.coins -= price;
            p.SetLevel(key, p.GetLevel(key) + 1);
            return ShopResult.Ok;
        }

        /// <summary>Nang cap sung da so huu (vd. "pistol", "scope").</summary>
        public static ShopResult UpgradeGun(ProfileData p, MetaCatalog c, string gunId, string upgradeId)
        {
            var u = c.GunUpgrade(upgradeId);
            if (u == null || c.Gun(gunId) == null) return ShopResult.Unknown;
            if (!p.OwnsGun(gunId)) return ShopResult.NotOwned;
            return Upgrade(p, u, UpgradeKey.Gun(gunId, upgradeId));
        }

        /// <summary>Nang cap mon tren ban (vd. "vest").</summary>
        public static ShopResult UpgradeGear(ProfileData p, MetaCatalog c, string gearId)
        {
            var u = c.GearItem(gearId);
            return u == null ? ShopResult.Unknown : Upgrade(p, u, UpgradeKey.Gear(gearId));
        }

        // ---------- Sung ----------

        public static ShopResult BuyGun(ProfileData p, MetaCatalog c, string id)
        {
            var g = c.Gun(id);
            if (g == null) return ShopResult.Unknown;
            if (p.OwnsGun(id)) return ShopResult.AlreadyOwned;
            if (!Pay(p, g.Price, 0, out var fail)) return fail;
            p.AddGun(id);
            return ShopResult.Ok;
        }

        public static ShopResult EquipGun(ProfileData p, MetaCatalog c, string id)
        {
            if (c.Gun(id) == null) return ShopResult.Unknown;
            if (!p.OwnsGun(id)) return ShopResult.NotOwned;
            p.equippedGun = id;
            return ShopResult.Ok;
        }

        /// <summary>Thue sung chua co cho 1 tran: xem quang cao (mien phi) hoac tra RentPrice xu.</summary>
        public static ShopResult RentGun(ProfileData p, MetaCatalog c, string id, bool byAd)
        {
            var g = c.Gun(id);
            if (g == null) return ShopResult.Unknown;
            if (p.OwnsGun(id)) return ShopResult.AlreadyOwned;
            if (!byAd && !Pay(p, g.RentPrice, 0, out var fail)) return fail;
            p.rentedGun = id;
            return ShopResult.Ok;
        }

        // ---------- Ngoai hinh ----------

        public static ShopResult BuySkin(ProfileData p, MetaCatalog c, string gunId, string skinId)
        {
            var k = c.Skin(skinId);
            if (k == null || c.Gun(gunId) == null) return ShopResult.Unknown;
            var s = p.Gun(gunId);
            if (s == null) return ShopResult.NotOwned;
            if (s.skins.Contains(skinId)) return ShopResult.AlreadyOwned;
            if (!Pay(p, k.CoinPrice, k.BadgePrice, out var fail)) return fail;
            s.skins.Add(skinId);
            s.skin = skinId;
            return ShopResult.Ok;
        }

        /// <summary>Chon skin da co ("" = mac dinh).</summary>
        public static ShopResult EquipSkin(ProfileData p, string gunId, string skinId)
        {
            var s = p.Gun(gunId);
            if (s == null) return ShopResult.NotOwned;
            if (!string.IsNullOrEmpty(skinId) && !s.skins.Contains(skinId)) return ShopResult.NotOwned;
            s.skin = skinId ?? "";
            return ShopResult.Ok;
        }

        // ---------- Quang cao truoc tran ----------

        /// <summary>Them giap cho tran ke (khong cong don).</summary>
        public static ShopResult GrantAdArmor(ProfileData p, MetaCatalog c)
        {
            if (p.pendingArmor >= c.AdArmor) return ShopResult.AlreadyOwned;
            p.pendingArmor = c.AdArmor;
            return ShopResult.Ok;
        }

        // ---------- Tien ----------

        static bool Pay(ProfileData p, int coins, int badges, out ShopResult fail)
        {
            fail = ShopResult.Ok;
            if (p.coins < coins) fail = ShopResult.NotEnoughCoins;
            else if (p.badges < badges) fail = ShopResult.NotEnoughBadges;
            if (fail != ShopResult.Ok) return false;
            p.coins -= coins;
            p.badges -= badges;
            return true;
        }
    }
}
