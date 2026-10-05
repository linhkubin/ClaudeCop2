using System;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>C19. Tuy chon nguoi choi. UI ghi ReduceMotion (luu PlayerPrefs), Camera doc.</summary>
    public static class UserSettings
    {
        public const string ReduceMotionKey = "cc_reduce_motion";
        /// <summary>Gia tri mac dinh khi chua co PlayerPrefs (nguoi dung chon bat tu dau).</summary>
        public const bool DefaultReduceMotion = true;

        static bool loaded;
        static bool reduceMotion;

        public static event Action Changed;

        public static bool ReduceMotion
        {
            get
            {
                if (!loaded) { reduceMotion = PlayerPrefs.GetInt(ReduceMotionKey, DefaultReduceMotion ? 1 : 0) == 1; loaded = true; }
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

        /// <summary>Chi cho debug/bot do: doi gia tri trong bo nho, KHONG ghi PlayerPrefs, khong phat Changed.</summary>
        public static void SetRuntimeOnly(bool value) { reduceMotion = value; loaded = true; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { loaded = false; reduceMotion = false; Changed = null; }
    }
}
