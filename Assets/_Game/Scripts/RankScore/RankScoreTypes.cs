using System.Collections.Generic;

namespace ClaudeCop.RankScore
{
    /// <summary>Loai cau hoi theo API TypeSafe System One.</summary>
    public enum RankScoreQuestionType { Noul, Choice, Score }

    /// <summary>Nguon cua mot quyet dinh.</summary>
    public enum RankScoreDecisionSource { Offline, Default }

    /// <summary>Anh chup so lieu nguoi choi trong Phase hien tai.</summary>
    public struct PlayerStats
    {
        public int Shots;
        public int Hits;
        public int Misses;
        public int HostageHits;
        /// <summary>Trung / ban (0..1). 0 neu chua ban.</summary>
        public float Accuracy;
        /// <summary>Thoi gian phan ung trung binh (s). 0 neu chua co mau.</summary>
        public float AvgReactionTime;
        public int ReactionSamples;
        /// <summary>So mang mat trong Phase.</summary>
        public int DamageTaken;
        /// <summary>So lan ban vat the co IShootable (TapOutcome.Environment): KHONG tinh la ban truot.</summary>
        public int EnvironmentShots;
        /// <summary>So enemy ha bang no (BlastEvents): khong cong vao Hits/Accuracy.</summary>
        public int BlastKills;
        /// <summary>So con tin trung no (da co trong HostageHits).</summary>
        public int BlastHostageHits;
        /// <summary>So lan Revive da dung.</summary>
        public int RevivesUsed;
        /// <summary>Chi so Phase hien tai (tinh tu 0).</summary>
        public int PhaseIndex;

        public override string ToString() =>
            $"shots={Shots} hits={Hits} miss={Misses} hostage={HostageHits} acc={Accuracy:0.00} react={AvgReactionTime:0.00}s dmg={DamageTaken} env={EnvironmentShots} blast={BlastKills} revive={RevivesUsed}";
    }

    /// <summary>Mot cau hoi gui toi RankScore. Criteria cua Choice la dictionary {ten: mo ta} (danh sach bi 422).</summary>
    public class RankScoreQuestion
    {
        public string Id;
        public RankScoreQuestionType Type;
        public string Instructions;
        public Dictionary<string, string> Criteria = new Dictionary<string, string>();
    }

    /// <summary>Yeu cau: tuong ung body {state, model, questions}.</summary>
    public class RankScoreRequest
    {
        public string State;
        public string Model = "rankScore-latest";
        public List<RankScoreQuestion> Questions = new List<RankScoreQuestion>();
    }

    /// <summary>Tra loi mot cau hoi: {choice, probabilities, confidence} hoac {noul}.</summary>
    public class RankScoreChoiceAnswer
    {
        public bool IsNoul;
        public string Choice;
        public Dictionary<string, float> Probabilities = new Dictionary<string, float>();
        public float Confidence;
    }

    /// <summary>Phan hoi: answers[id].</summary>
    public class RankScoreResponse
    {
        public bool Success;
        public bool TimedOut;
        public string Error;
        public Dictionary<string, RankScoreChoiceAnswer> Answers = new Dictionary<string, RankScoreChoiceAnswer>();

        public static RankScoreResponse Timeout() => new RankScoreResponse { Success = false, TimedOut = true, Error = "timeout" };
        public static RankScoreResponse Failure(string error) => new RankScoreResponse { Success = false, Error = error };
    }

    /// <summary>Quyet dinh cuoi cung ap vao EncounterWave.</summary>
    public struct RankScoreDecision
    {
        public string Question;
        /// <summary>Gia tri reticle_time (s) da chon.</summary>
        public float ReticleTime;
        /// <summary>Ten lua chon (vd. "short"/"normal"/"long").</summary>
        public string Choice;
        public Dictionary<string, float> Probabilities;
        public float Confidence;
        public RankScoreDecisionSource Source;
        /// <summary>Time.realtimeSinceStartup luc quyet dinh.</summary>
        public float Timestamp;
        public PlayerStats Stats;
    }
}
