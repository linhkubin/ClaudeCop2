using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeCop.Meta
{
    [Serializable]
    public sealed class GunState
    {
        public string id;
        public List<string> skins = new List<string>();
        public string skin = "";
    }

    /// <summary>Cap cua mot nhanh nang cap, khoa = UpgradeKey (vd. "pistol/scope", "gear/vest").</summary>
    [Serializable]
    public sealed class UpgradeLevel
    {
        public string key;
        public int level;
    }

    /// <summary>Khoa luu cap nang cap trong ho so.</summary>
    public static class UpgradeKey
    {
        public static string Gun(string gunId, string upgradeId) => gunId + "/" + upgradeId;
        public static string Gear(string gearId) => "gear/" + gearId;
    }

    [Serializable]
    public sealed class LevelRecord
    {
        public int level;          // 0-based (0 = Level 1)
        public int plays;
        public int wins;
        public int losses;
        public int bestScore;
        /// <summary>Hang tot nhat: 0 = S, 1 = A, 2 = B, 3 = C; -1 = chua thang.</summary>
        public int bestRank = -1;
        public bool gotS;
    }

    /// <summary>Du lieu nguoi choi (luu JSON trong PlayerPrefs). Chi doi qua Shop / LevelProgress.</summary>
    [Serializable]
    public sealed class ProfileData
    {
        public int version = 2;
        public int coins;
        public int badges;
        public string equippedGun = "";
        public List<GunState> guns = new List<GunState>();
        public List<UpgradeLevel> upgrades = new List<UpgradeLevel>();
        /// <summary>Sung thue cho tran ke (rong = khong). Dung xong thi xoa.</summary>
        public string rentedGun = "";
        /// <summary>Giap them cho tran ke (xem quang cao). Dung xong thi ve 0.</summary>
        public int pendingArmor;
        public List<LevelRecord> levels = new List<LevelRecord>();

        public GunState Gun(string id)
        {
            foreach (var g in guns) if (g != null && g.id == id) return g;
            return null;
        }

        public bool OwnsGun(string id) => Gun(id) != null;

        public GunState AddGun(string id)
        {
            var g = Gun(id);
            if (g == null) { g = new GunState { id = id }; guns.Add(g); }
            return g;
        }

        public int GetLevel(string key)
        {
            var u = upgrades.Find(x => x.key == key);
            return u != null ? u.level : 0;
        }

        public void SetLevel(string key, int level)
        {
            var u = upgrades.Find(x => x.key == key);
            if (u == null) { u = new UpgradeLevel { key = key }; upgrades.Add(u); }
            u.level = level;
        }

        public LevelRecord Level(int level, bool create)
        {
            foreach (var r in levels) if (r != null && r.level == level) return r;
            if (!create) return null;
            var n = new LevelRecord { level = level };
            levels.Add(n);
            return n;
        }

        /// <summary>Sua du lieu thieu/hong; dam bao co sung mac dinh.</summary>
        public void Sanitize(MetaCatalog catalog)
        {
            if (guns == null) guns = new List<GunState>();
            if (upgrades == null) upgrades = new List<UpgradeLevel>();
            upgrades.RemoveAll(u => u == null || string.IsNullOrEmpty(u.key));
            if (levels == null) levels = new List<LevelRecord>();
            guns.RemoveAll(g => g == null || string.IsNullOrEmpty(g.id));
            foreach (var g in guns) { if (g.skins == null) g.skins = new List<string>(); if (g.skin == null) g.skin = ""; }
            levels.RemoveAll(l => l == null);
            if (coins < 0) coins = 0;
            if (badges < 0) badges = 0;
            if (pendingArmor < 0) pendingArmor = 0;
            if (rentedGun == null) rentedGun = "";
            if (catalog != null && catalog.DefaultGun != null) AddGun(catalog.DefaultGun.Id);
            if (equippedGun == null || !OwnsGun(equippedGun)) equippedGun = catalog != null && catalog.DefaultGun != null ? catalog.DefaultGun.Id : "";
        }

        public static ProfileData FromJson(string json, MetaCatalog catalog)
        {
            ProfileData d = null;
            if (!string.IsNullOrEmpty(json))
            {
                try { d = JsonUtility.FromJson<ProfileData>(json); }
                catch (Exception) { d = null; }
            }
            if (d == null) d = new ProfileData();
            d.Sanitize(catalog);
            return d;
        }

        public string ToJson() => JsonUtility.ToJson(this);
    }

    /// <summary>Ho so dang dung (doc lan dau tu PlayerPrefs). Moi thay doi goi Save() -> phat Changed.</summary>
    public static class PlayerProfile
    {
        public const string Key = "cc_profile_v1";
        static ProfileData data;
        public static event Action Changed;

        public static MetaCatalog Catalog => MetaCatalog.Default;

        public static ProfileData Data => data ?? (data = ProfileData.FromJson(PlayerPrefs.GetString(Key, ""), Catalog));

        public static void Save()
        {
            if (data == null) return;
            PlayerPrefs.SetString(Key, data.ToJson());
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        /// <summary>Xoa ho so (debug).</summary>
        public static void ResetAll()
        {
            data = ProfileData.FromJson("", Catalog);
            Save();
        }

        /// <summary>Test: dung du lieu trong bo nho, khong doc PlayerPrefs.</summary>
        public static void UseForTests(ProfileData d) { data = d; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { data = null; Changed = null; }
    }
}
