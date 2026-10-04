using System;
using UnityEngine;

namespace ClaudeCop.Jev
{
    /// <summary>Nhat ky quyet dinh Jev cho bang debug (T-411). Tu reset khi vao Play mode.</summary>
    public static class JevDecisionLog
    {
        static JevDecision last;
        static bool hasLast;
        static int count;

        public static bool HasLast => hasLast;
        public static JevDecision Last => last;
        public static int Count => count;

        public static event Action<JevDecision> DecisionMade;

        public static void Record(JevDecision d)
        {
            last = d; hasLast = true; count++;
            DecisionMade?.Invoke(d);
        }

        public static void Clear() { last = default; hasLast = false; count = 0; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Clear(); DecisionMade = null; }
    }
}
