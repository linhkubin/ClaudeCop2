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

        public static void RequestReload() => ReloadRequested?.Invoke();
        public static void RequestStartGame() => StartGameRequested?.Invoke();
        public static void RequestRestart() => RestartRequested?.Invoke();
        public static void RequestRevive() => ReviveRequested?.Invoke();
        public static void DeclineRevive() => ReviveDeclined?.Invoke();
        public static void RequestDebugOverlayToggle() => DebugOverlayToggleRequested?.Invoke();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            ReloadRequested = null; StartGameRequested = null; RestartRequested = null;
            ReviveRequested = null; ReviveDeclined = null; DebugOverlayToggleRequested = null;
        }
    }
}
