using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Meta;

namespace ClaudeCop.Meta.Tests
{
    public class MetaTests
    {
        MetaCatalog c;
        ProfileData p;

        [SetUp]
        public void SetUp()
        {
            c = MetaCatalog.CreateDefault();
            p = ProfileData.FromJson("", c);
        }

        [Test]
        public void NewProfile_OwnsAndEquipsDefaultPistol()
        {
            Assert.IsTrue(p.OwnsGun("pistol"));
            Assert.AreEqual("pistol", p.equippedGun);
            Assert.AreEqual(0, p.coins);
        }

        [Test]
        public void CorruptJson_FallsBackToDefault()
        {
            var d = ProfileData.FromJson("{not json", c);
            Assert.AreEqual("pistol", d.equippedGun);
        }

        [Test]
        public void Json_RoundTrip_KeepsData()
        {
            p.coins = 1234; p.badges = 2;
            p.AddGun("revolver").scope = 3;
            p.SetGearLevel(GearItem.Gloves, 2);
            p.Level(1, true).bestRank = 0;
            var d = ProfileData.FromJson(p.ToJson(), c);
            Assert.AreEqual(1234, d.coins);
            Assert.AreEqual(2, d.badges);
            Assert.AreEqual(3, d.Gun("revolver").scope);
            Assert.AreEqual(2, d.GearLevel(GearItem.Gloves));
            Assert.AreEqual(0, d.Level(1, false).bestRank);
        }

        [Test]
        public void BuyGun_NeedsCoins_ThenOwnsAndEquips()
        {
            Assert.AreEqual(ShopResult.NotEnoughCoins, Shop.BuyGun(p, c, "revolver"));
            p.coins = 2000;
            Assert.AreEqual(ShopResult.Ok, Shop.BuyGun(p, c, "revolver"));
            Assert.AreEqual(500, p.coins);
            Assert.AreEqual(ShopResult.AlreadyOwned, Shop.BuyGun(p, c, "revolver"));
            Assert.AreEqual(ShopResult.Ok, Shop.EquipGun(p, c, "revolver"));
            Assert.AreEqual(ShopResult.NotOwned, Shop.EquipGun(p, c, "smg"));
            Assert.AreEqual(ShopResult.Unknown, Shop.BuyGun(p, c, "nope"));
        }

        [Test]
        public void UpgradeScope_UntilMax()
        {
            p.coins = 100000;
            for (int i = 0; i < c.ScopePrices.Length; i++) Assert.AreEqual(ShopResult.Ok, Shop.UpgradeGun(p, c, "pistol", GunUpgrade.Scope));
            Assert.AreEqual(ShopResult.MaxLevel, Shop.UpgradeGun(p, c, "pistol", GunUpgrade.Scope));
            Assert.AreEqual(-1, Shop.UpgradePrice(p, c, "pistol", GunUpgrade.Scope));
            Assert.AreEqual(ShopResult.NotOwned, Shop.UpgradeGun(p, c, "smg", GunUpgrade.Silencer));
        }

        [Test]
        public void Skin_CoinAndBadgePrices()
        {
            Assert.AreEqual(ShopResult.NotEnoughCoins, Shop.BuySkin(p, c, "pistol", "matte"));
            p.coins = 300;
            Assert.AreEqual(ShopResult.Ok, Shop.BuySkin(p, c, "pistol", "matte"));
            Assert.AreEqual("matte", p.Gun("pistol").skin);
            Assert.AreEqual(ShopResult.NotEnoughBadges, Shop.BuySkin(p, c, "pistol", "gold"));
            p.badges = 3;
            Assert.AreEqual(ShopResult.Ok, Shop.BuySkin(p, c, "pistol", "gold"));
            Assert.AreEqual(0, p.badges);
            Assert.AreEqual(ShopResult.Ok, Shop.EquipSkin(p, "pistol", ""));
        }

        [Test]
        public void Rent_ByCoinsOrAd_UsedOnceThenConsumed()
        {
            Assert.AreEqual(ShopResult.NotEnoughCoins, Shop.RentGun(p, c, "smg", false));
            Assert.AreEqual(ShopResult.Ok, Shop.RentGun(p, c, "smg", true));
            var l = LoadoutBuilder.Build(p, c);
            Assert.IsTrue(l.Rented);
            Assert.AreEqual(WeaponKind.SMG, l.Gun.Kind);
            Assert.IsTrue(LoadoutBuilder.Consume(p));
            Assert.AreEqual(WeaponKind.Pistol, LoadoutBuilder.Build(p, c).Gun.Kind);
            Assert.AreEqual(ShopResult.AlreadyOwned, Shop.RentGun(p, c, "pistol", true));
        }

        [Test]
        public void Loadout_AppliesUpgradesAndGear()
        {
            var g = p.Gun("pistol"); g.scope = 2; g.silencer = 3;
            p.SetGearLevel(GearItem.Vest, 1);
            p.SetGearLevel(GearItem.Helmet, 1);
            p.SetGearLevel(GearItem.Gloves, 2);
            p.SetGearLevel(GearItem.Glasses, 1);
            p.SetGearLevel(GearItem.Radio, 1);
            Assert.AreEqual(ShopResult.Ok, Shop.GrantAdArmor(p, c));
            var l = LoadoutBuilder.Build(p, c);
            Assert.AreEqual(90f * (1f + 2 * c.ScopeRadiusPerLevel), l.HitRadiusPx, 1e-3f);
            Assert.AreEqual(1f + 3 * c.SilencerReticlePerLevel, l.EnemyReticleScale, 1e-4f);
            Assert.AreEqual(1, l.ArmorPerStage);
            Assert.AreEqual(1, l.ExtraLives);
            Assert.AreEqual(2, l.MissForgiveness);
            Assert.AreEqual(1f + c.GlassesJusticePerLevel, l.JusticeRadiusScale, 1e-4f);
            Assert.AreEqual(c.RadioCoinPerLevel, l.CoinBonus, 1e-4f);
            Assert.AreEqual(c.AdArmor, l.StartArmor);
            var w = l.CreateWeapon();
            Assert.AreEqual(6, w.MagazineSize);
            Assert.AreEqual(l.HitRadiusPx, w.HitRadiusPx, 1e-3f);
            Object.DestroyImmediate(w);
        }

        [Test]
        public void SmgAndRevolver_KeepTheirTraits()
        {
            p.AddGun("smg"); p.AddGun("revolver");
            p.equippedGun = "smg";
            var smg = LoadoutBuilder.Build(p, c).CreateWeapon();
            Assert.IsTrue(smg.HoldToFire);
            p.equippedGun = "revolver";
            var rev = LoadoutBuilder.Build(p, c).CreateWeapon();
            Assert.AreEqual(2, rev.MaxTargetsPerShot);
            Object.DestroyImmediate(smg); Object.DestroyImmediate(rev);
        }

        [Test]
        public void GearUpgrade_PricesAndMax()
        {
            p.coins = 100000;
            for (int i = 0; i < c.VestPrices.Length; i++) Assert.AreEqual(ShopResult.Ok, Shop.UpgradeGear(p, c, GearItem.Vest));
            Assert.AreEqual(ShopResult.MaxLevel, Shop.UpgradeGear(p, c, GearItem.Vest));
            Assert.AreEqual(c.VestPrices.Length, p.GearLevel(GearItem.Vest));
        }

        [Test]
        public void Rewards_Win_AllBonuses()
        {
            var s = new LevelRunStats { Score = 5000, DamageTaken = 0, JusticeKills = 3, MaxCombo = 5f };
            var r = LevelRewards.Compute(0, true, s, c, 0f);
            Assert.AreEqual(100, r.ScoreCoins);
            Assert.AreEqual(100 + c.WinBonus + c.NoDamageBonus + 3 * c.PerJustice + c.ComboBonus, r.Coins);
        }

        [Test]
        public void Rewards_Loss_OnlyScoreCoins_AndRadioBonus()
        {
            var s = new LevelRunStats { Score = 1000, DamageTaken = 3, JusticeKills = 5, MaxCombo = 5f };
            var r = LevelRewards.Compute(0, false, s, c, 0.1f);
            Assert.AreEqual(20, r.ScoreCoins);
            Assert.AreEqual(0, r.WinBonus + r.NoDamageBonus + r.JusticeBonus + r.ComboBonus);
            Assert.AreEqual(22, r.Coins);
        }

        [Test]
        public void Apply_RecordsWinLoss_BestRank_BadgeOnFirstSOnly()
        {
            var win = LevelRewards.Compute(1, true, new LevelRunStats { Score = 3000 }, c, 0f); win.Rank = 1;
            LevelRewards.Apply(p, c, win);
            var lose = LevelRewards.Compute(1, false, new LevelRunStats { Score = 500 }, c, 0f);
            LevelRewards.Apply(p, c, lose);
            var rec = p.Level(1, false);
            Assert.AreEqual(1, rec.wins); Assert.AreEqual(1, rec.losses);
            Assert.AreEqual(3000, rec.bestScore);
            Assert.AreEqual(1, rec.bestRank);
            Assert.AreEqual(0, p.badges);

            var s1 = LevelRewards.Compute(1, true, new LevelRunStats { Score = 4000 }, c, 0f); s1.Rank = 0;
            s1 = LevelRewards.Apply(p, c, s1);
            Assert.AreEqual(c.BadgesPerFirstS, s1.Badges);
            var s2 = LevelRewards.Compute(1, true, new LevelRunStats { Score = 4000 }, c, 0f); s2.Rank = 0;
            s2 = LevelRewards.Apply(p, c, s2);
            Assert.AreEqual(0, s2.Badges);
            Assert.AreEqual(c.BadgesPerFirstS, p.badges);
            Assert.AreEqual(0, p.Level(1, false).bestRank);
        }

        [Test]
        public void LateRank_UpdatesRecordAndBadge()
        {
            var r = LevelRewards.Apply(p, c, LevelRewards.Compute(0, true, new LevelRunStats { Score = 100 }, c, 0f));
            Assert.AreEqual(-1, p.Level(0, false).bestRank);
            r = LevelRewards.ApplyLateRank(p, c, r, 0);
            Assert.AreEqual(0, p.Level(0, false).bestRank);
            Assert.AreEqual(c.BadgesPerFirstS, p.badges);
            Assert.AreEqual(0, r.Rank);
        }

        [Test]
        public void Double_OnlyOnce()
        {
            var r = LevelRewards.Apply(p, c, LevelRewards.Compute(0, true, new LevelRunStats { Score = 0 }, c, 0f));
            int before = p.coins;
            Assert.IsTrue(LevelRewards.TryDouble(p, c, ref r));
            Assert.AreEqual(before * 2, p.coins);
            Assert.IsFalse(LevelRewards.TryDouble(p, c, ref r));
        }

        [Test]
        public void AdArmor_NotStackable()
        {
            Assert.AreEqual(ShopResult.Ok, Shop.GrantAdArmor(p, c));
            Assert.AreEqual(ShopResult.AlreadyOwned, Shop.GrantAdArmor(p, c));
        }
    }
}
