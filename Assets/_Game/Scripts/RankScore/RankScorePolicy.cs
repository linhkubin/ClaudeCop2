using System;
using System.Collections.Generic;

namespace ClaudeCop.RankScore
{
    /// <summary>Ham thuan: bien stats/phan hoi thanh RankScoreDecision. RankScoreDirector goi. Khong ghi log.</summary>
    public static class RankScorePolicy
    {
        /// <summary>Quyet dinh dong bo bang luat Offline (khong qua client). Tat RankScore => mac dinh.</summary>
        public static RankScoreDecision Decide(PlayerStats stats, RankScoreConfig config, float now = 0f)
        {
            if (config == null || !config.enabled)
                return Default(stats, config, now);
            var ans = OfflineRankScoreClient.AnswerReticle(stats, config);
            return FromAnswer(ans, stats, config, now);
        }

        /// <summary>Ap dung phan hoi cua IRankScoreClient. Null/timeout/loi/noul/confidence thap => mac dinh.</summary>
        public static RankScoreDecision Decide(RankScoreResponse response, PlayerStats stats, RankScoreConfig config, float now = 0f)
        {
            if (!TryGetAnswer(response, OfflineRankScoreClient.QuestionReticleTime, config, out var ans))
                return Default(stats, config, now);
            return FromAnswer(ans, stats, config, now);
        }

        static bool TryGetAnswer(RankScoreResponse response, string question, RankScoreConfig config, out RankScoreChoiceAnswer ans)
        {
            ans = null;
            if (config == null || !config.enabled || response == null || !response.Success || response.TimedOut
                || response.Answers == null || !response.Answers.TryGetValue(question, out ans)
                || ans == null || ans.IsNoul)
                return false;
            return true;
        }

        static float Threshold(RankScoreConfig config) => (config ?? RankScoreConfig.Fallback).confidenceThreshold;

        static RankScoreDecision FromAnswer(RankScoreChoiceAnswer ans, PlayerStats stats, RankScoreConfig config, float now)
        {
            if (ans.Confidence < Threshold(config) || string.IsNullOrEmpty(ans.Choice))
            {
                var d = Default(stats, config, now);
                d.Probabilities = ans.Probabilities ?? d.Probabilities; // giu de debug
                d.Confidence = ans.Confidence;
                return d;
            }
            return new RankScoreDecision
            {
                Question = OfflineRankScoreClient.QuestionReticleTime,
                Choice = ans.Choice,
                ReticleTime = (config ?? RankScoreConfig.Fallback).TimeFor(ans.Choice),
                Probabilities = ans.Probabilities,
                Confidence = ans.Confidence,
                Source = RankScoreDecisionSource.Offline,
                Timestamp = now,
                Stats = stats
            };
        }

        public static RankScoreDecision Default(PlayerStats stats, RankScoreConfig config, float now = 0f)
        {
            return new RankScoreDecision
            {
                Question = OfflineRankScoreClient.QuestionReticleTime,
                Choice = RankScoreConfig.ChoiceNormal,
                ReticleTime = (config ?? RankScoreConfig.Fallback).defaultReticleTime,
                Probabilities = new Dictionary<string, float>(),
                Confidence = 0f,
                Source = RankScoreDecisionSource.Default,
                Timestamp = now,
                Stats = stats
            };
        }

        // ---------- wave_preset / weapon_drop ----------

        /// <summary>wave_preset bang luat Offline tren thong ke ca man. Tat RankScore/thieu du lieu/confidence thap => Source=Default, Choice="default".</summary>
        public static RankScoreDecision DecideWavePreset(PlayerStats levelStats, RankScoreConfig config, float now = 0f)
        {
            if (config == null || !config.enabled) return DefaultChoice(OfflineRankScoreClient.QuestionWavePreset, levelStats, now);
            return FromChoice(OfflineRankScoreClient.QuestionWavePreset, RankScoreConfig.PresetNames,
                OfflineRankScoreClient.AnswerWavePreset(levelStats, config), levelStats, config, now);
        }

        public static RankScoreDecision DecideWavePreset(RankScoreResponse response, PlayerStats levelStats, RankScoreConfig config, float now = 0f)
        {
            if (!TryGetAnswer(response, OfflineRankScoreClient.QuestionWavePreset, config, out var ans))
                return DefaultChoice(OfflineRankScoreClient.QuestionWavePreset, levelStats, now);
            return FromChoice(OfflineRankScoreClient.QuestionWavePreset, RankScoreConfig.PresetNames, ans, levelStats, config, now);
        }

        public static RankScoreDecision DecideWeaponDrop(PlayerStats levelStats, RankScoreConfig config, float now = 0f)
        {
            if (config == null || !config.enabled) return DefaultChoice(OfflineRankScoreClient.QuestionWeaponDrop, levelStats, now);
            return FromChoice(OfflineRankScoreClient.QuestionWeaponDrop, RankScoreConfig.DropNames,
                OfflineRankScoreClient.AnswerWeaponDrop(levelStats, config), levelStats, config, now);
        }

        public static RankScoreDecision DecideWeaponDrop(RankScoreResponse response, PlayerStats levelStats, RankScoreConfig config, float now = 0f)
        {
            if (!TryGetAnswer(response, OfflineRankScoreClient.QuestionWeaponDrop, config, out var ans))
                return DefaultChoice(OfflineRankScoreClient.QuestionWeaponDrop, levelStats, now);
            return FromChoice(OfflineRankScoreClient.QuestionWeaponDrop, RankScoreConfig.DropNames, ans, levelStats, config, now);
        }

        static RankScoreDecision FromChoice(string question, string[] valid, RankScoreChoiceAnswer ans, PlayerStats stats, RankScoreConfig config, float now)
        {
            if (ans.IsNoul || ans.Confidence < Threshold(config) || Array.IndexOf(valid, ans.Choice) < 0)
            {
                var d = DefaultChoice(question, stats, now);
                if (ans.Probabilities != null) d.Probabilities = ans.Probabilities;
                d.Confidence = ans.Confidence;
                return d;
            }
            return new RankScoreDecision
            {
                Question = question,
                Choice = ans.Choice,
                Probabilities = ans.Probabilities,
                Confidence = ans.Confidence,
                Source = RankScoreDecisionSource.Offline,
                Timestamp = now,
                Stats = stats
            };
        }

        /// <summary>Khong y kien: Choice = "default" (dot giu cau hinh cua no).</summary>
        public static RankScoreDecision DefaultChoice(string question, PlayerStats stats, float now = 0f)
        {
            return new RankScoreDecision
            {
                Question = question,
                Choice = RankScoreConfig.ChoiceDefault,
                Probabilities = new Dictionary<string, float>(),
                Confidence = 0f,
                Source = RankScoreDecisionSource.Default,
                Timestamp = now,
                Stats = stats
            };
        }

        /// <summary>Ban ghi "rank" cho RankScoreDecisionLog (bang debug).</summary>
        public static RankScoreDecision RankDecision(ScoreRankResult r, PlayerStats levelStats, float now = 0f)
        {
            return new RankScoreDecision
            {
                Question = OfflineRankScoreClient.QuestionRank,
                Choice = r.Rank + "/" + r.Weakness,
                Probabilities = new Dictionary<string, float>(),
                Confidence = 1f,
                Source = RankScoreDecisionSource.Offline,
                Timestamp = now,
                Stats = levelStats
            };
        }
    }
}
