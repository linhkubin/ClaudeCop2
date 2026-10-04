using System.Collections.Generic;

namespace ClaudeCop.Jev
{
    /// <summary>Loai cau hoi theo API TypeSafe System One.</summary>
    public enum JevQuestionType { Noul, Choice, Score }

    /// <summary>Nguon cua mot quyet dinh.</summary>
    public enum JevDecisionSource { Offline, Default }

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

        public override string ToString() =>
            $"shots={Shots} hits={Hits} miss={Misses} hostage={HostageHits} acc={Accuracy:0.00} react={AvgReactionTime:0.00}s dmg={DamageTaken}";
    }

    /// <summary>Mot cau hoi gui toi Jev. Criteria cua Choice la dictionary {ten: mo ta} (danh sach bi 422).</summary>
    public class JevQuestion
    {
        public string Id;
        public JevQuestionType Type;
        public string Instructions;
        public Dictionary<string, string> Criteria = new Dictionary<string, string>();
    }

    /// <summary>Yeu cau: tuong ung body {state, model, questions}.</summary>
    public class JevRequest
    {
        public string State;
        public string Model = "jev-latest";
        public List<JevQuestion> Questions = new List<JevQuestion>();
    }

    /// <summary>Tra loi mot cau hoi: {choice, probabilities, confidence} hoac {noul}.</summary>
    public class JevChoiceAnswer
    {
        public bool IsNoul;
        public string Choice;
        public Dictionary<string, float> Probabilities = new Dictionary<string, float>();
        public float Confidence;
    }

    /// <summary>Phan hoi: answers[id].</summary>
    public class JevResponse
    {
        public bool Success;
        public bool TimedOut;
        public string Error;
        public Dictionary<string, JevChoiceAnswer> Answers = new Dictionary<string, JevChoiceAnswer>();

        public static JevResponse Timeout() => new JevResponse { Success = false, TimedOut = true, Error = "timeout" };
        public static JevResponse Failure(string error) => new JevResponse { Success = false, Error = error };
    }

    /// <summary>Quyet dinh cuoi cung ap vao EncounterWave.</summary>
    public struct JevDecision
    {
        public string Question;
        /// <summary>Gia tri reticle_time (s) da chon.</summary>
        public float ReticleTime;
        /// <summary>Ten lua chon (vd. "short"/"normal"/"long").</summary>
        public string Choice;
        public Dictionary<string, float> Probabilities;
        public float Confidence;
        public JevDecisionSource Source;
        /// <summary>Time.realtimeSinceStartup luc quyet dinh.</summary>
        public float Timestamp;
        public PlayerStats Stats;
    }
}
