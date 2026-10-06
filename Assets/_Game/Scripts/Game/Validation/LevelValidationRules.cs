using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeCop.Game.Validation
{
    [Serializable]
    public class MarkerNameRule
    {
        public string label = "EnemySpawn";
        [Tooltip("Ten bat dau bang tien to nay thi phai khop regex")] public string prefix = "EnemySpawn_";
        public string regex = @"^EnemySpawn_P\d+_W\d+_\d+$";
    }

    [Serializable]
    public class LevelRule
    {
        [Tooltip("Ap dung cho scene co ten chua chuoi nay (khong phan biet hoa thuong). Rule dau tien khop duoc dung.")]
        public string sceneNameContains = "Level_01";
        public bool forbidHostage = true;
        public bool forbidBarrel = true;
        public bool forbidWeaponCrate = true;
        public bool forbidHumanShield = true;
        public bool forbidGrenadier = true;
        [Min(0)] public int maxEnemiesPerWave = 4;
        [Min(0)] public int maxEnemiesTotal = 24;
        [Tooltip("Tran so enemy cung luc (maxConcurrent cua wave phai > 0 va <= tran nay)")] [Min(0)] public int maxConcurrentCap = 3;
        [Tooltip("> 0: scene chuoi level (vd. Level_Chain). Tong enemy toi da = gia tri nay x so level trong scene (thay maxEnemiesTotal); "
            + "moi level con con bi gioi han theo rule rieng cua no (vd. Level_02), level khong co rule rieng dung gia tri nay.")]
        [Min(0)] public int maxEnemiesPerLevel = 0;
    }

    /// <summary>Nguong cua Editor tool ClaudeCop/Validate Level. Khong hard-code trong validator.</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Level Validation Rules", fileName = "LevelValidationRules")]
    public class LevelValidationRules : ScriptableObject
    {
        [Header("(a) Khoang cach")]
        [Min(0)] public float minTargetDistance = 12f;
        [Tooltip("GDD vong 11: tran khoang cach muc tieu (xa thu toi 35 m). <= 0 = khong kiem.")] [Min(0)] public float maxTargetDistance = 35f;
        [Tooltip("Pickup noi duoc phep gan hon (WARN neu nho hon)")] [Min(0)] public float minPickupDistance = 8f;
        [Tooltip("Do cao ngam (m) cong vao diem Peek")] public float aimHeight = 1.5f;

        [Header("(b) Vua khung")]
        public float aspectPortrait = 9f / 16f;
        public float aspectTall = 9f / 19.5f;
        public float maxVerticalFov = 45f;
        public float fovMarginDeg = 5f;
        public float maxHorizontalOffsetDeg = 5.5f;
        public float maxClusterWidthDeg = 21f;

        [Header("(c) Tam nhin")]
        public LayerMask sightlineMask = ~0;
        [Tooltip("Bo qua vat can trong khoang nay truoc muc tieu (m)")] public float sightlineEndTolerance = 0.3f;
        public List<string> sightlineIgnorePrefixes = new List<string> { "Prop_Glass", "PropSlot_Glass" };

        [Header("(d) Noi shot/rail")]
        public float railJoinPosTolerance = 0.05f;
        public float railJoinAngleTolerance = 0.5f;
        public float combatPairMaxAngle = 30f;
        public float combatPairMaxDistance = 1f;

        [Header("(e) Thoi gian Move")]
        public float maxMoveSeconds = 6f;

        [Header("(f) Tuyen (theo tung Phase; Cut giua Phase)")]
        public float maxCumulativeYaw = 150f;
        public float turnReversalMinDeg = 15f;
        public int maxTurnReversals = 4;
        [Tooltip("Diem lien tiep gan hon nguong nay (m) duoc gop truoc khi do cat cheo")] public float routeMinSegment = 0.5f;
        [Tooltip("So mau tren moi rail Move")] public int railSamples = 16;

        [Header("(g) Luat theo level")]
        public List<LevelRule> levelRules = new List<LevelRule> { new LevelRule() };
        public LevelRule fallbackRule = new LevelRule
        {
            sceneNameContains = "",
            forbidHostage = false, forbidBarrel = false, forbidWeaponCrate = false, forbidHumanShield = false, forbidGrenadier = false,
            maxEnemiesPerWave = 8, maxEnemiesTotal = 80, maxConcurrentCap = 6
        };
        public List<string> hostagePrefixes = new List<string> { "HostageSpawn_" };
        public List<string> barrelPrefixes = new List<string> { "PropSlot_Barrel_", "Prop_Barrel_" };
        public List<string> weaponCratePrefixes = new List<string> { "PickupSpawn_" };
        public List<string> humanShieldPrefixes = new List<string> { "ShieldSpawn_" };
        public List<string> grenadierPrefixes = new List<string> { "GrenadierSpawn_" };

        [Header("(h) Quy uoc ten")]
        public List<MarkerNameRule> nameRules = new List<MarkerNameRule>
        {
            new MarkerNameRule{label="EnemySpawn", prefix="EnemySpawn_", regex=@"^EnemySpawn_P\d+_W\d+_\d+$"},
            new MarkerNameRule{label="HostageSpawn", prefix="HostageSpawn_", regex=@"^HostageSpawn_P\d+_W\d+_\d+$"},
            new MarkerNameRule{label="GrenadierSpawn", prefix="GrenadierSpawn_", regex=@"^GrenadierSpawn_P\d+_W\d+_\d+$"},
            new MarkerNameRule{label="ShieldSpawn", prefix="ShieldSpawn_", regex=@"^ShieldSpawn_P\d+_W\d+_\d+$"},
            new MarkerNameRule{label="PickupSpawn", prefix="PickupSpawn_", regex=@"^PickupSpawn_P\d+_W\d+_\d+$"},
            new MarkerNameRule{label="PropSlot", prefix="PropSlot_", regex=@"^PropSlot_(Barrel|Box|Glass)_P\d+_W\d+_\d+$"},
            new MarkerNameRule{label="Prop", prefix="Prop_", regex=@"^Prop_(Barrel|Box|Glass)_P\d+_W\d+_\d+$"},
            new MarkerNameRule{label="CamPoint", prefix="CamPoint_", regex=@"^CamPoint_P\d+_S\d+$"},
            new MarkerNameRule{label="RailHint", prefix="RailHint_", regex=@"^RailHint_P\d+_S\d+_\d+$"},
            new MarkerNameRule{label="Shot", prefix="Shot_", regex=@"^Shot_P\d+_S\d+_(Move|Combat)$"},
            new MarkerNameRule{label="Wave", prefix="Wave_", regex=@"^Wave_P\d+_W\d+$"},
            new MarkerNameRule{label="Rail", prefix="Rail_", regex=@"^Rail_P\d+_S\d+$"},
        };

        [Header("(j) PropSlot khop Prop")]
        public float propSlotTolerance = 0.05f;

        /// <summary>Level con cua mot wave trong scene chuoi: ten root Level_XX cua diem spawn; khong co thi theo tien to wave "L5_" -> Level_05; mac dinh Level_01.</summary>
        public static string LevelKeyOf(string waveName, string spawnRootName)
        {
            if (!string.IsNullOrEmpty(spawnRootName) && spawnRootName.StartsWith("Level_", StringComparison.Ordinal)) return spawnRootName;
            var m = System.Text.RegularExpressions.Regex.Match(waveName ?? "", @"^L(\d+)_");
            if (m.Success) return "Level_" + int.Parse(m.Groups[1].Value).ToString("00");
            return "Level_01";
        }

        /// <summary>Gioi han enemy cua level con trong chuoi: rule rieng cua level (neu co) hoac chain.maxEnemiesPerLevel.</summary>
        public int PerLevelLimit(LevelRule chain, string levelKey)
        {
            var r = RuleFor(levelKey);
            return r != null && r != fallbackRule && r != chain ? r.maxEnemiesTotal : chain.maxEnemiesPerLevel;
        }

        /// <summary>Tong enemy toi da cua scene chuoi: maxEnemiesPerLevel x so level.</summary>
        public static int ChainTotalLimit(LevelRule chain, int levelCount) => chain.maxEnemiesPerLevel * Mathf.Max(1, levelCount);

        public LevelRule RuleFor(string sceneName)
        {
            foreach (var r in levelRules)
                if (r != null && !string.IsNullOrEmpty(r.sceneNameContains) && sceneName.IndexOf(r.sceneNameContains, StringComparison.OrdinalIgnoreCase) >= 0) return r;
            return fallbackRule;
        }
    }
}
