namespace ClaudeCop.Meta
{
    public enum ShopResult { Ok, NotEnoughCoins, NotEnoughBadges, AlreadyOwned, NotOwned, MaxLevel, Unknown }

    /// <summary>
    /// Logic cua hang thuan (khong luu): mua/trang bi/thue sung, nang cap ong ngam/giam thanh, skin, trang bi tren ban, giap quang cao.
    /// Man Home goi roi PlayerProfile.Save().
    /// </summary>
    public static class Shop
    {
        // ---------- Sung ----------

        public static ShopResult BuyGun(ProfileData p, MetaCatalog c, string id)
        {
            var g = c.Gun(id);
            if (g == null) return ShopResult.Unknown;
            if (p.OwnsGun(id)) return ShopResult.AlreadyOwned;
            if (p.coins < g.Price) return ShopResult.NotEnoughCoins;
            p.coins -= g.Price;
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

        /// <summary>Thue sung chua co cho 1 tran. byAd = xem quang cao (mien phi), nguoc lai tra RentPrice xu.</summary>
        public static ShopResult RentGun(ProfileData p, MetaCatalog c, string id, bool byAd)
        {
            var g = c.Gun(id);
            if (g == null) return ShopResult.Unknown;
            if (p.OwnsGun(id)) return ShopResult.AlreadyOwned;
            if (!byAd)
            {
                if (p.coins < g.RentPrice) return ShopResult.NotEnoughCoins;
                p.coins -= g.RentPrice;
            }
            p.rentedGun = id;
            return ShopResult.Ok;
        }

        // ---------- Nang cap sung ----------

        public static int Level(ProfileData p, string gunId, GunUpgrade u)
        {
            var s = p.Gun(gunId);
            if (s == null) return 0;
            return u == GunUpgrade.Scope ? s.scope : s.silencer;
        }

        /// <summary>Gia cap ke tiep; -1 neu da toi da.</summary>
        public static int UpgradePrice(ProfileData p, MetaCatalog c, string gunId, GunUpgrade u)
        {
            var prices = c.Prices(u);
            int lv = Level(p, gunId, u);
            return lv < prices.Length ? prices[lv] : -1;
        }

        public static ShopResult UpgradeGun(ProfileData p, MetaCatalog c, string gunId, GunUpgrade u)
        {
            var s = p.Gun(gunId);
            if (s == null) return c.Gun(gunId) == null ? ShopResult.Unknown : ShopResult.NotOwned;
            int price = UpgradePrice(p, c, gunId, u);
            if (price < 0) return ShopResult.MaxLevel;
            if (p.coins < price) return ShopResult.NotEnoughCoins;
            p.coins -= price;
            if (u == GunUpgrade.Scope) s.scope++; else s.silencer++;
            return ShopResult.Ok;
        }

        // ---------- Ngoai hinh (skin) ----------

        public static ShopResult BuySkin(ProfileData p, MetaCatalog c, string gunId, string skinId)
        {
            var s = p.Gun(gunId);
            var k = c.Skin(skinId);
            if (k == null || c.Gun(gunId) == null) return ShopResult.Unknown;
            if (s == null) return ShopResult.NotOwned;
            if (s.skins.Contains(skinId)) return ShopResult.AlreadyOwned;
            if (p.coins < k.CoinPrice) return ShopResult.NotEnoughCoins;
            if (p.badges < k.BadgePrice) return ShopResult.NotEnoughBadges;
            p.coins -= k.CoinPrice;
            p.badges -= k.BadgePrice;
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

        // ---------- Trang bi tren ban ----------

        public static int GearPrice(ProfileData p, MetaCatalog c, GearItem item)
        {
            var prices = c.Prices(item);
            int lv = p.GearLevel(item);
            return lv < prices.Length ? prices[lv] : -1;
        }

        public static ShopResult UpgradeGear(ProfileData p, MetaCatalog c, GearItem item)
        {
            int price = GearPrice(p, c, item);
            if (price < 0) return ShopResult.MaxLevel;
            if (p.coins < price) return ShopResult.NotEnoughCoins;
            p.coins -= price;
            p.SetGearLevel(item, p.GearLevel(item) + 1);
            return ShopResult.Ok;
        }

        // ---------- Quang cao truoc tran ----------

        /// <summary>Xem quang cao truoc tran: them giap cho tran ke (cong don toi da 1 lan).</summary>
        public static ShopResult GrantAdArmor(ProfileData p, MetaCatalog c)
        {
            if (p.pendingArmor >= c.AdArmor) return ShopResult.AlreadyOwned;
            p.pendingArmor = c.AdArmor;
            return ShopResult.Ok;
        }
    }
}
