using System;
using UnityEngine;

namespace ClaudeCop.RankScore
{
    /// <summary>Hang cuoi man (chi khi thang).</summary>
    public enum ScoreRank { S, A, B, C }

    /// <summary>Diem yeu nhat cua nguoi choi trong man.</summary>
    public enum ScoreWeakness { None, Accuracy, Reaction, Damage, Hostage }

    /// <summary>Ket qua danh gia cuoi man, UI bind vao (T-711).</summary>
    public struct ScoreRankResult
    {
        public ScoreRank Rank;
        public ScoreWeakness Weakness;
        /// <summary>Cau nhan xet tieng Viet co dau.</summary>
        public string WeaknessText;
        public float Accuracy;
        public float AvgReactionTime;
        public int DamageTaken;
        public int HostageHits;
        public int RevivesUsed;
        public int BlastKills;
        /// <summary>Diem ky nang 0..1 dung de xep hang.</summary>
        public float Skill;

        public override string ToString() => $"rank={Rank} weak={Weakness} skill={Skill:0.00}";
    }

    /// <summary>Diem cong bo tinh ket qua rank. Tu reset khi vao Play mode va khi bat dau luot moi (PlayerStatsTracker goi Clear).</summary>
    public static class ScoreRankBoard
    {
        static ScoreRankResult last;
        static bool hasResult;

        public static bool HasResult => hasResult;
        public static ScoreRankResult Last => last;

        public static event Action<ScoreRankResult> RankEvaluated;

        public static void Publish(ScoreRankResult r)
        {
            last = r; hasResult = true;
            RankEvaluated?.Invoke(r);
        }

        public static void Clear() { last = default; hasResult = false; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Clear(); RankEvaluated = null; }
    }

    /// <summary>Ham thuan: thong ke ca man => rank + diem yeu.</summary>
    public static class ScoreRankEvaluator
    {
        public const string TextAccuracy = "Bắn trượt nhiều — ngắm kỹ hơn";
        public const string TextReaction = "Phản xạ chậm — bắn sớm khi vòng còn xanh";
        public const string TextDamage = "Mất nhiều mạng — ưu tiên kẻ địch vòng đỏ";
        public const string TextHostage = "Bắn trúng con tin — quan sát trước khi bắn";
        public const string TextNone = "Hoàn hảo!";

        public static ScoreRankResult Evaluate(PlayerStats s, RankScoreConfig cfg)
        {
            if (cfg == null) cfg = RankScoreConfig.Fallback;
            float skill = OfflineRankScoreClient.Skill(s, cfg);

            ScoreRank rank;
            if (skill >= cfg.rankSMinSkill && s.DamageTaken == 0 && s.RevivesUsed == 0) rank = ScoreRank.S;
            else if (skill >= cfg.rankAMinSkill) rank = ScoreRank.A;
            else if (skill >= cfg.rankBMinSkill) rank = ScoreRank.B;
            else rank = ScoreRank.C;
            // Da revive => toi da B (B hoac C; S/A bi ha xuong B).
            if (s.RevivesUsed > 0 && rank < ScoreRank.B) rank = ScoreRank.B;

            var weak = PickWeakness(s, cfg);
            return new ScoreRankResult
            {
                Rank = rank,
                Weakness = weak,
                WeaknessText = TextFor(weak),
                Accuracy = s.Accuracy,
                AvgReactionTime = s.AvgReactionTime,
                DamageTaken = s.DamageTaken,
                HostageHits = s.HostageHits,
                RevivesUsed = s.RevivesUsed,
                BlastKills = s.BlastKills,
                Skill = skill
            };
        }

        /// <summary>Thanh phan thap nhat; neu van &gt;= nguong thi None. Hoa: uu tien Accuracy, Reaction, Damage, Hostage.</summary>
        public static ScoreWeakness PickWeakness(PlayerStats s, RankScoreConfig cfg)
        {
            if (cfg == null) cfg = RankScoreConfig.Fallback;
            float acc = s.Shots > 0 ? Mathf.Clamp01(s.Accuracy) : 1f;
            float react = s.ReactionSamples > 0 ? OfflineRankScoreClient.ReactionScore(s, cfg) : 1f;
            float safety = OfflineRankScoreClient.SafetyScore(s, cfg);
            float hostage = 1f - Mathf.Clamp01(s.HostageHits * cfg.hostageWeaknessPerHit);

            ScoreWeakness w = ScoreWeakness.Accuracy; float min = acc;
            if (react < min) { min = react; w = ScoreWeakness.Reaction; }
            if (safety < min) { min = safety; w = ScoreWeakness.Damage; }
            if (hostage < min) { min = hostage; w = ScoreWeakness.Hostage; }
            return min >= cfg.weaknessOkThreshold ? ScoreWeakness.None : w;
        }

        public static string TextFor(ScoreWeakness w)
        {
            switch (w)
            {
                case ScoreWeakness.Accuracy: return TextAccuracy;
                case ScoreWeakness.Reaction: return TextReaction;
                case ScoreWeakness.Damage: return TextDamage;
                case ScoreWeakness.Hostage: return TextHostage;
                default: return TextNone;
            }
        }
    }
}
