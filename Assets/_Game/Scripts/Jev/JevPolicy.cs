using System.Collections.Generic;

namespace ClaudeCop.Jev
{
    /// <summary>Ham thuan: bien stats/phan hoi thanh JevDecision. T-402 (JevDirector) goi. Khong ghi log.</summary>
    public static class JevPolicy
    {
        /// <summary>Quyet dinh dong bo bang luat Offline (khong qua client). Tat Jev => mac dinh.</summary>
        public static JevDecision Decide(PlayerStats stats, JevConfig config, float now = 0f)
        {
            if (config == null || !config.enabled)
                return Default(stats, config, now);
            var ans = OfflineJevClient.AnswerReticle(stats, config);
            return FromAnswer(ans, stats, config, now);
        }

        /// <summary>Ap dung phan hoi cua IJevClient. Null/timeout/loi/noul/confidence thap => mac dinh.</summary>
        public static JevDecision Decide(JevResponse response, PlayerStats stats, JevConfig config, float now = 0f)
        {
            if (config == null || !config.enabled || response == null || !response.Success || response.TimedOut
                || response.Answers == null
                || !response.Answers.TryGetValue(OfflineJevClient.QuestionReticleTime, out var ans)
                || ans == null || ans.IsNoul)
                return Default(stats, config, now);
            return FromAnswer(ans, stats, config, now);
        }

        static JevDecision FromAnswer(JevChoiceAnswer ans, PlayerStats stats, JevConfig config, float now)
        {
            float threshold = config != null ? config.confidenceThreshold : 0.6f;
            if (ans.Confidence < threshold || string.IsNullOrEmpty(ans.Choice))
            {
                var d = Default(stats, config, now);
                d.Probabilities = ans.Probabilities ?? d.Probabilities; // giu de debug
                d.Confidence = ans.Confidence;
                return d;
            }
            return new JevDecision
            {
                Question = OfflineJevClient.QuestionReticleTime,
                Choice = ans.Choice,
                ReticleTime = config != null ? config.TimeFor(ans.Choice) : 2.5f,
                Probabilities = ans.Probabilities,
                Confidence = ans.Confidence,
                Source = JevDecisionSource.Offline,
                Timestamp = now,
                Stats = stats
            };
        }

        public static JevDecision Default(PlayerStats stats, JevConfig config, float now = 0f)
        {
            return new JevDecision
            {
                Question = OfflineJevClient.QuestionReticleTime,
                Choice = JevConfig.ChoiceNormal,
                ReticleTime = config != null ? config.defaultReticleTime : 2.5f,
                Probabilities = new Dictionary<string, float>(),
                Confidence = 0f,
                Source = JevDecisionSource.Default,
                Timestamp = now,
                Stats = stats
            };
        }
    }
}
