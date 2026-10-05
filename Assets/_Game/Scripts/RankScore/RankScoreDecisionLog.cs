using System;
using UnityEngine;

namespace ClaudeCop.RankScore
{
    /// <summary>Nhat ky quyet dinh RankScore cho bang debug (T-411). Tu reset khi vao Play mode.</summary>
    public static class RankScoreDecisionLog
    {
        static RankScoreDecision last;
        static bool hasLast;
        static int count;

        public static bool HasLast => hasLast;
        public static RankScoreDecision Last => last;
        public static int Count => count;

        public static event Action<RankScoreDecision> DecisionMade;

        public static void Record(RankScoreDecision d)
        {
            last = d; hasLast = true; count++;
            DecisionMade?.Invoke(d);
        }

        public static void Clear() { last = default; hasLast = false; count = 0; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Clear(); DecisionMade = null; }
    }
}
