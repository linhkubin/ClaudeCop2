using UnityEditor;
using UnityEngine;
using ClaudeCop.Meta;

namespace ClaudeCop.Game.Editor
{
    /// <summary>Menu thu nhanh ho so man Home (xu, huy hieu, mua/nang cap) khi chua co giao dien.</summary>
    public static class MetaDebugMenu
    {
        [MenuItem("ClaudeCop/Meta/Add 5000 Coins + 3 Badges")]
        static void AddCoins()
        {
            var p = PlayerProfile.Data;
            p.coins += 5000; p.badges += 3;
            PlayerProfile.Save();
            Log();
        }

        [MenuItem("ClaudeCop/Meta/Buy + Equip Revolver (scope 2, silencer 1)")]
        static void Revolver()
        {
            var p = PlayerProfile.Data; var c = PlayerProfile.Catalog;
            Shop.BuyGun(p, c, "revolver");
            Shop.EquipGun(p, c, "revolver");
            Shop.UpgradeGun(p, c, "revolver", "scope");
            Shop.UpgradeGun(p, c, "revolver", "scope");
            Shop.UpgradeGun(p, c, "revolver", "silencer");
            PlayerProfile.Save();
            Log();
        }

        [MenuItem("ClaudeCop/Meta/Rent SMG (ad) + Ad Armor for next run")]
        static void RentSmg()
        {
            var p = PlayerProfile.Data; var c = PlayerProfile.Catalog;
            Shop.RentGun(p, c, "smg", true);
            Shop.GrantAdArmor(p, c);
            PlayerProfile.Save();
            Log();
        }

        [MenuItem("ClaudeCop/Meta/Upgrade All Gear +1")]
        static void Gear()
        {
            var p = PlayerProfile.Data; var c = PlayerProfile.Catalog;
            foreach (var g in c.Gear) Shop.UpgradeGear(p, c, g.Id);
            PlayerProfile.Save();
            Log();
        }

        [MenuItem("ClaudeCop/Meta/Log Profile")]
        static void Log() { Debug.Log("[Meta] " + PlayerProfile.Data.ToJson()); }

        [MenuItem("ClaudeCop/Meta/Reset Profile")]
        static void ResetProfile()
        {
            if (!EditorUtility.DisplayDialog("Reset Profile", "Xoa toan bo xu, sung, trang bi, ket qua level?", "Xoa", "Huy")) return;
            PlayerProfile.ResetAll();
            Log();
        }
    }
}
