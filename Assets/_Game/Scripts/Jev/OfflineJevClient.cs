using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeCop.Jev
{
    /// <summary>
    /// Jev gia lap bang luat viet san, tra ve cung kieu voi online. Deterministic, dong bo.
    /// Stats lay qua statsProvider (thuong la PlayerStatsTracker.Snapshot).
    /// Cau hoi "reticle_time" kieu Choice voi 3 lua chon short/normal/long.
    /// </summary>
    public class OfflineJevClient : IJevClient
    {
        public const string QuestionReticleTime = "reticle_time";

        readonly JevConfig config;
        readonly Func<PlayerStats> statsProvider;

        public OfflineJevClient(JevConfig config, Func<PlayerStats> statsProvider)
        {
            this.config = config;
            this.statsProvider = statsProvider;
        }

        /// <summary>Tao request chuan cho cau hoi reticle_time (cung hinh dang gui online).</summary>
        public static JevRequest BuildReticleRequest(PlayerStats stats)
        {
            var q = new JevQuestion
            {
                Id = QuestionReticleTime,
                Type = JevQuestionType.Choice,
                Instructions = "Chon thoi gian vong target cho dot enemy tiep theo sao cho vua suc nguoi choi.",
                Criteria = new Dictionary<string, string>
                {
                    { JevConfig.ChoiceShort, "Nguoi choi gioi: ban chinh xac, phan ung nhanh, it mat mang. Rut ngan de co thu thach." },
                    { JevConfig.ChoiceNormal, "Nguoi choi trung binh. Giu thoi gian mac dinh." },
                    { JevConfig.ChoiceLong, "Nguoi choi dang kho: ban hut, phan ung cham hoac mat nhieu mang. Keo dai de giup." }
                }
            };
            return new JevRequest { State = stats.ToString(), Questions = new List<JevQuestion> { q } };
        }

        public JevCall Ask(JevRequest request, float timeoutSeconds, Action<JevResponse> onDone)
        {
            var call = new JevCall();
            var resp = new JevResponse { Success = true };
            PlayerStats stats = statsProvider != null ? statsProvider() : default;
            if (request != null && request.Questions != null)
            {
                foreach (var q in request.Questions)
                {
                    if (q.Id == QuestionReticleTime && q.Type == JevQuestionType.Choice)
                        resp.Answers[q.Id] = AnswerReticle(stats, config);
                    else
                        resp.Answers[q.Id] = new JevChoiceAnswer { IsNoul = true };
                }
            }
            call.Complete();
            onDone?.Invoke(resp);
            return call;
        }

        /// <summary>Do kha nang nguoi choi 0..1 (1 = gioi).</summary>
        public static float Skill(PlayerStats s)
        {
            float acc = Mathf.Clamp01(s.Accuracy);
            // 0.8 s => 1, 2.0 s => 0. Chua co mau => trung tinh 0.5.
            float react = s.ReactionSamples > 0 ? Mathf.Clamp01(1f - (s.AvgReactionTime - 0.8f) / 1.2f) : 0.5f;
            float safety = 1f - Mathf.Clamp01(s.DamageTaken / 2f);
            float hostagePenalty = Mathf.Clamp01(s.HostageHits * 0.15f);
            return Mathf.Clamp01(0.45f * acc + 0.3f * react + 0.25f * safety - hostagePenalty);
        }

        public static JevChoiceAnswer AnswerReticle(PlayerStats s, JevConfig cfg)
        {
            float skill = (s.Shots > 0 || s.DamageTaken > 0) ? Skill(s) : 0.5f;
            float[] centers = { 0.85f, 0.5f, 0.15f }; // short, normal, long
            var w = new float[3];
            float sum = 0f;
            for (int i = 0; i < 3; i++)
            {
                w[i] = Mathf.Max(0.05f, 1f - Mathf.Abs(skill - centers[i]) / 0.6f);
                sum += w[i];
            }
            var probs = new Dictionary<string, float>();
            int best = 0; float second = 0f;
            for (int i = 0; i < 3; i++)
            {
                w[i] /= sum;
                probs[JevConfig.ChoiceNames[i]] = w[i];
                if (w[i] > w[best]) best = i;
            }
            for (int i = 0; i < 3; i++) if (i != best && w[i] > second) second = w[i];

            int minShots = cfg != null ? Mathf.Max(1, cfg.minShotsForConfidence) : 5;
            float sample = Mathf.Clamp01((float)s.Shots / minShots);
            float conf = Mathf.Clamp01((0.45f + (w[best] - second) * 1.5f) * sample);
            return new JevChoiceAnswer
            {
                Choice = JevConfig.ChoiceNames[best],
                Probabilities = probs,
                Confidence = conf
            };
        }
    }
}
