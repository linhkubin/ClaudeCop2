using System;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>
    /// C20. Lenh tu UI. UI goi Request*; Combat nghe ReloadRequested, Game nghe cac lenh con lai.
    /// </summary>
    public static class GameCommands
    {
        public static event Action ReloadRequested;
        public static event Action StartGameRequested;
        public static event Action RestartRequested;
        public static event Action ReviveRequested;
        public static event Action ReviveDeclined;
        public static event Action DebugOverlayToggleRequested;
        /// <summary>Bang ket qua giua chuoi level: Continue = choi tiep man ke (khong ngat quang), Home = ve man hinh chinh (logic them sau).</summary>
        public static event Action ContinueRequested;
        public static event Action HomeRequested;
        /// <summary>Chon level tu man Title (0-based: 0 = Level 1, 1 = Level 2 ...). TitleLauncher nap scene gameplay; PhaseDirector bat dau tu Phase dau cua level do.</summary>
        public static event Action<int> LevelSelectRequested;
        /// <summary>Level dang duoc chon (giu qua nap scene / Restart).</summary>
        public static int SelectedLevel;

        public static void RequestReload() => ReloadRequested?.Invoke();
        public static void RequestStartGame() => StartGameRequested?.Invoke();
        public static void RequestRestart() => RestartRequested?.Invoke();
        public static void RequestRevive() => ReviveRequested?.Invoke();
        public static void DeclineRevive() => ReviveDeclined?.Invoke();
        public static void RequestContinue() => ContinueRequested?.Invoke();
        public static void RequestHome() => HomeRequested?.Invoke();
        public static void RequestSelectLevel(int level) { SelectedLevel = level < 0 ? 0 : level; LevelSelectRequested?.Invoke(SelectedLevel); }
        public static void RequestDebugOverlayToggle() => DebugOverlayToggleRequested?.Invoke();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            ReloadRequested = null; StartGameRequested = null; RestartRequested = null;
            ReviveRequested = null; ReviveDeclined = null; DebugOverlayToggleRequested = null; ContinueRequested = null; HomeRequested = null; LevelSelectRequested = null; SelectedLevel = 0;
        }
    }
}
