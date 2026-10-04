using System;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>C19. Tuy chon nguoi choi. UI ghi ReduceMotion (luu PlayerPrefs), Camera doc.</summary>
    public static class UserSettings
    {
        public const string ReduceMotionKey = "cc_reduce_motion";

        static bool loaded;
        static bool reduceMotion;

        public static event Action Changed;

        public static bool ReduceMotion
        {
            get
            {
                if (!loaded) { reduceMotion = PlayerPrefs.GetInt(ReduceMotionKey, 0) == 1; loaded = true; }
                return reduceMotion;
            }
            set
            {
                if (ReduceMotion == value) return;
                reduceMotion = value;
                PlayerPrefs.SetInt(ReduceMotionKey, value ? 1 : 0);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { loaded = false; reduceMotion = false; Changed = null; }
    }
}
