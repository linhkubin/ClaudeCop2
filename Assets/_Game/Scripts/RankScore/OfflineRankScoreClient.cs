using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeCop.RankScore
{
    /// <summary>
    /// RankScore gia lap bang luat viet san (khong mang). Deterministic, dong bo.
    /// Cau hoi: "reticle_time" (short/normal/long, theo thong ke Phase), "wave_preset" va "weapon_drop" (theo thong ke CA MAN).
    /// </summary>
    public class OfflineRankScoreClient : IRankScoreClient
    {
        public const string QuestionReticleTime = "reticle_time";
        public const string QuestionWavePreset = "wave_preset";
        public const string QuestionWeaponDrop = "weapon_drop";
        public const string QuestionRank = "rank";

        readonly RankScoreConfig config;
        readonly Func<PlayerStats> statsProvider;
        readonly Func<PlayerStats> levelStatsProvider;

        /// <param name="statsProvider">Thong ke Phase (reticle_time).</param>
        /// <param name="levelStatsProvider">Thong ke ca man (wave_preset, weapon_drop). Null => dung statsProvider.</param>
        public OfflineRankScoreClient(RankScoreConfig config, Func<PlayerStats> statsProvider, Func<PlayerStats> levelStatsProvider = null)
        {
            this.config = config;
            this.statsProvider = statsProvider;
            this.levelStatsProvider = levelStatsProvider;
        }

        /// <summary>Tao request chuan cho cau hoi reticle_time (cung hinh dang gui online).</summary>
        public static RankScoreRequest BuildReticleRequest(PlayerStats stats)
        {
            return new RankScoreRequest { State = stats.ToString(), Questions = new List<RankScoreQuestion> { ReticleQuestion() } };
        }

        /// <summary>Request day du 3 cau hoi: reticle_time, wave_preset, weapon_drop.</summary>
        public static RankScoreRequest BuildWaveRequest(PlayerStats phaseStats, PlayerStats levelStats)
        {
            return new RankScoreRequest
            {
                State = "phase: " + phaseStats + " | level: " + levelStats,
                Questions = new List<RankScoreQuestion> { ReticleQuestion(), WavePresetQuestion(), WeaponDropQuestion() }
            };
        }

        public static RankScoreRequest BuildWavePresetRequest(PlayerStats levelStats) =>
            new RankScoreRequest { State = levelStats.ToString(), Questions = new List<RankScoreQuestion> { WavePresetQuestion() } };

        public static RankScoreRequest BuildWeaponDropRequest(PlayerStats levelStats) =>
            new RankScoreRequest { State = levelStats.ToString(), Questions = new List<RankScoreQuestion> { WeaponDropQuestion() } };

        static RankScoreQuestion ReticleQuestion() => new RankScoreQuestion
        {
            Id = QuestionReticleTime,
            Type = RankScoreQuestionType.Choice,
            Instructions = "Chon thoi gian vong target cho dot enemy tiep theo sao cho vua suc nguoi choi.",
            Criteria = new Dictionary<string, string>
            {
                { RankScoreConfig.ChoiceShort, "Nguoi choi gioi: ban chinh xac, phan ung nhanh, it mat mang. Rut ngan de co thu thach." },
                { RankScoreConfig.ChoiceNormal, "Nguoi choi trung binh. Giu thoi gian mac dinh." },
                { RankScoreConfig.ChoiceLong, "Nguoi choi dang kho: ban hut, phan ung cham hoac mat nhieu mang. Keo dai de giup." }
            }
        };

        static RankScoreQuestion WavePresetQuestion() => new RankScoreQuestion
        {
            Id = QuestionWavePreset,
            Type = RankScoreQuestionType.Choice,
            Instructions = "Chon bo cau hinh cho dot enemy tiep theo.",
            Criteria = new Dictionary<string, string>
            {
                { RankScoreConfig.PresetCalm, "Nguoi choi dang kho: dot nhe nhang." },
                { RankScoreConfig.PresetStandard, "Nguoi choi trung binh." },
                { RankScoreConfig.PresetIntense, "Nguoi choi gioi: dot dong va nhanh." },
                { RankScoreConfig.PresetHostageHeavy, "Nguoi choi chinh xac, chua trung con tin: nhieu con tin hon." }
            }
        };

        static RankScoreQuestion WeaponDropQuestion() => new RankScoreQuestion
        {
            Id = QuestionWeaponDrop,
            Type = RankScoreQuestionType.Choice,
            Instructions = "Chon vu khi trong thung o dot tiep theo.",
            Criteria = new Dictionary<string, string>
            {
                { RankScoreConfig.DropNone, "Nguoi choi gioi: khong can ho tro." },
                { RankScoreConfig.DropShotgun, "Nguoi choi trung binh: shotgun." },
                { RankScoreConfig.DropMachineGun, "Nguoi choi dang kho: machinegun de ho tro." }
            }
        };

        public RankScoreCall Ask(RankScoreRequest request, float timeoutSeconds, Action<RankScoreResponse> onDone)
        {
            var call = new RankScoreCall();
            var resp = new RankScoreResponse { Success = true };
            PlayerStats stats = statsProvider != null ? statsProvider() : default;
            PlayerStats level = levelStatsProvider != null ? levelStatsProvider() : stats;
            if (request != null && request.Questions != null)
            {
                foreach (var q in request.Questions)
                {
                    if (q.Type != RankScoreQuestionType.Choice) resp.Answers[q.Id] = new RankScoreChoiceAnswer { IsNoul = true };
                    else if (q.Id == QuestionReticleTime) resp.Answers[q.Id] = AnswerReticle(stats, config);
                    else if (q.Id == QuestionWavePreset) resp.Answers[q.Id] = AnswerWavePreset(level, config);
                    else if (q.Id == QuestionWeaponDrop) resp.Answers[q.Id] = AnswerWeaponDrop(level, config);
                    else resp.Answers[q.Id] = new RankScoreChoiceAnswer { IsNoul = true };
                }
            }
            call.Complete();
            onDone?.Invoke(resp);
            return call;
        }

        // ---------- Cong thuc ky nang ----------

        /// <summary>Do kha nang nguoi choi 0..1 (1 = gioi).</summary>
        public static float Skill(PlayerStats s) => Skill(s, null);

        public static float Skill(PlayerStats s, RankScoreConfig cfg)
        {
            if (cfg == null) cfg = RankScoreConfig.Fallback;
            float acc = Mathf.Clamp01(s.Accuracy);
            float react = s.ReactionSamples > 0 ? ReactionScore(s, cfg) : cfg.neutralScore;
            float safety = SafetyScore(s, cfg);
            float hostagePenalty = Mathf.Clamp01(s.HostageHits * cfg.hostagePenaltyPerHit);
            return Mathf.Clamp01(cfg.weightAccuracy * acc + cfg.weightReaction * react + cfg.weightSafety * safety - hostagePenalty);
        }

        /// <summary>Phan ung 0..1: reactionBestSeconds => 1, +reactionSpanSeconds => 0.</summary>
        public static float ReactionScore(PlayerStats s, RankScoreConfig cfg)
        {
            if (cfg == null) cfg = RankScoreConfig.Fallback;
            return Mathf.Clamp01(1f - (s.AvgReactionTime - cfg.reactionBestSeconds) / cfg.reactionSpanSeconds);
        }

        /// <summary>An toan 0..1: 1 neu khong mat mang, 0 khi mat livesLostForZeroSafety mang.</summary>
        public static float SafetyScore(PlayerStats s, RankScoreConfig cfg)
        {
            if (cfg == null) cfg = RankScoreConfig.Fallback;
            return 1f - Mathf.Clamp01(s.DamageTaken / (float)Mathf.Max(1, cfg.livesLostForZeroSafety));
        }

        // ---------- reticle_time ----------

        public static RankScoreChoiceAnswer AnswerReticle(PlayerStats s, RankScoreConfig cfg)
        {
            var c = cfg != null ? cfg : RankScoreConfig.Fallback;
            float skill = (s.Shots > 0 || s.DamageTaken > 0) ? Skill(s, c) : c.neutralScore;
            float[] centers = c.reticleCenters != null && c.reticleCenters.Length >= 3 ? c.reticleCenters : RankScoreConfig.Fallback.reticleCenters;
            var w = new float[3];
            float sum = 0f;
            for (int i = 0; i < 3; i++)
            {
                w[i] = Mathf.Max(c.reticleMinWeight, 1f - Mathf.Abs(skill - centers[i]) / c.reticleFalloff);
                sum += w[i];
            }
            var probs = new Dictionary<string, float>();
            int best = 0; float second = 0f;
            for (int i = 0; i < 3; i++)
            {
                w[i] /= sum;
                probs[RankScoreConfig.ChoiceNames[i]] = w[i];
                if (w[i] > w[best]) best = i;
            }
            for (int i = 0; i < 3; i++) if (i != best && w[i] > second) second = w[i];

            int minShots = Mathf.Max(1, c.minShotsForConfidence);
            float sample = Mathf.Clamp01((float)s.Shots / minShots);
            float conf = Mathf.Clamp01((c.reticleConfidenceBase + (w[best] - second) * c.reticleConfidenceMarginGain) * sample);
            return new RankScoreChoiceAnswer
            {
                Choice = RankScoreConfig.ChoiceNames[best],
                Probabilities = probs,
                Confidence = conf
            };
        }

        // ---------- wave_preset / weapon_drop ----------

        /// <summary>Luat chon preset theo ky nang CA MAN. Chua du minShots => Noul.</summary>
        public static RankScoreChoiceAnswer AnswerWavePreset(PlayerStats level, RankScoreConfig cfg)
        {
            var c = cfg != null ? cfg : RankScoreConfig.Fallback;
            if (level.Shots < Mathf.Max(1, c.minShotsForConfidence)) return new RankScoreChoiceAnswer { IsNoul = true };
            float skill = Skill(level, c);
            string choice;
            float margin;
            if (skill >= c.presetIntenseMinSkill) { choice = RankScoreConfig.PresetIntense; margin = skill - c.presetIntenseMinSkill; }
            else if (skill <= c.presetCalmMaxSkill) { choice = RankScoreConfig.PresetCalm; margin = c.presetCalmMaxSkill - skill; }
            else if (level.HostageHits == 0 && skill >= c.presetHostageHeavyMinSkill && level.PhaseIndex >= c.presetHostageHeavyMinPhaseIndex)
            { choice = RankScoreConfig.PresetHostageHeavy; margin = Mathf.Min(skill - c.presetHostageHeavyMinSkill, c.presetIntenseMinSkill - skill); }
            else
            {
                choice = RankScoreConfig.PresetStandard;
                margin = Mathf.Min(skill - c.presetCalmMaxSkill, c.presetIntenseMinSkill - skill);
            }
            return RuleAnswer(choice, RankScoreConfig.PresetNames, margin, c);
        }

        /// <summary>Luat chon vu khi trong thung theo ky nang CA MAN. Chua du minShots => Noul.</summary>
        public static RankScoreChoiceAnswer AnswerWeaponDrop(PlayerStats level, RankScoreConfig cfg)
        {
            var c = cfg != null ? cfg : RankScoreConfig.Fallback;
            if (level.Shots < Mathf.Max(1, c.minShotsForConfidence)) return new RankScoreChoiceAnswer { IsNoul = true };
            float skill = Skill(level, c);
            string choice; float margin;
            if (skill <= c.dropMachineGunMaxSkill) { choice = RankScoreConfig.DropMachineGun; margin = c.dropMachineGunMaxSkill - skill; }
            else if (skill <= c.dropShotgunMaxSkill)
            {
                choice = RankScoreConfig.DropShotgun;
                margin = Mathf.Min(skill - c.dropMachineGunMaxSkill, c.dropShotgunMaxSkill - skill);
            }
            else { choice = RankScoreConfig.DropNone; margin = skill - c.dropShotgunMaxSkill; }
            return RuleAnswer(choice, RankScoreConfig.DropNames, margin, c);
        }

        static RankScoreChoiceAnswer RuleAnswer(string choice, string[] names, float margin, RankScoreConfig c)
        {
            float conf = Mathf.Clamp01(c.ruleConfidenceBase + Mathf.Max(0f, margin) * c.ruleConfidenceGain);
            var probs = new Dictionary<string, float>();
            float rest = names.Length > 1 ? (1f - conf) / (names.Length - 1) : 0f;
            foreach (var n in names) probs[n] = n == choice ? conf : rest;
            return new RankScoreChoiceAnswer { Choice = choice, Probabilities = probs, Confidence = conf };
        }
    }
}
