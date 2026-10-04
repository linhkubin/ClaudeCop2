using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>
    /// C11. Tin hieu tam dung chien dau, co dem theo ly do. Camera (blend/chuyen Phase) va Game
    /// (Revive/Win/GameOver/an han) Push/Pop; Enemy va TapShooter doc IsPaused/Changed.
    /// Pop mot ly do chua Push duoc bo qua (kem canh bao).
    /// </summary>
    public static class CombatPauseSignal
    {
        static readonly Dictionary<string, int> reasons = new Dictionary<string, int>();
        static int total;

        public static bool IsPaused => total > 0;
        public static int Count => total;
        public static event Action<bool> Changed;

        public static void Push(string reason)
        {
            reason ??= string.Empty;
            reasons.TryGetValue(reason, out var n);
            reasons[reason] = n + 1;
            total++;
            if (total == 1) Changed?.Invoke(true);
        }

        public static void Pop(string reason)
        {
            reason ??= string.Empty;
            if (!reasons.TryGetValue(reason, out var n) || n <= 0)
            {
                Debug.LogWarning("[CombatPauseSignal] Pop('" + reason + "') khong khop Push.");
                return;
            }
            if (n == 1) reasons.Remove(reason); else reasons[reason] = n - 1;
            total--;
            if (total == 0) Changed?.Invoke(false);
        }

        public static bool HasReason(string reason) => reasons.ContainsKey(reason ?? string.Empty);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            reasons.Clear();
            total = 0;
            Changed = null;
        }
    }
}
