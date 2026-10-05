using System;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>
    /// C18. Su kien ray/Phase. Chi Camera (PhaseDirector) duoc Raise; Game (Win), UI (fade/tieu de), RankScore nghe.
    /// </summary>
    public static class RailEvents
    {
        /// <summary>(phaseIndex, title)</summary>
        public static event Action<int, string> PhaseStarted;
        /// <summary>(title, fadeOut, hold, fadeIn) - giay.</summary>
        public static event Action<string, float, float, float> PhaseTransition;
        /// <summary>Doi Phase LIEN MACH (seamlessPhaseTransitions): tieu de Phase hien chu chong len canh dang chay, khong man den.</summary>
        public static event Action<string> PhaseBanner;
        /// <summary>Encounter sap toi, hoac null neu doan di chuyen khong dan toi giao tranh.</summary>
        public static event Action<EncounterBase> MoveSegmentStarted;
        public static event Action<EncounterBase> EncounterStarted;
        public static event Action<EncounterBase> EncounterCleared;
        public static event Action LevelCompleted;
        /// <summary>Het mot man giua chuoi level (RailPhase.showResultsAfter): (phaseIndex, title). Camera dung cho den GameCommands.ContinueRequested.</summary>
        public static event Action<int, string> StageCompleted;

        public static void RaisePhaseStarted(int index, string title) => PhaseStarted?.Invoke(index, title);
        public static void RaisePhaseTransition(string title, float fadeOut, float hold, float fadeIn) => PhaseTransition?.Invoke(title, fadeOut, hold, fadeIn);
        public static void RaisePhaseBanner(string title) => PhaseBanner?.Invoke(title);
        public static void RaiseMoveSegmentStarted(EncounterBase upcoming) => MoveSegmentStarted?.Invoke(upcoming);
        public static void RaiseEncounterStarted(EncounterBase e) => EncounterStarted?.Invoke(e);
        public static void RaiseEncounterCleared(EncounterBase e) => EncounterCleared?.Invoke(e);
        public static void RaiseLevelCompleted() => LevelCompleted?.Invoke();
        public static void RaiseStageCompleted(int phaseIndex, string title) => StageCompleted?.Invoke(phaseIndex, title);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            PhaseStarted = null; PhaseTransition = null; PhaseBanner = null; MoveSegmentStarted = null;
            EncounterStarted = null; EncounterCleared = null; LevelCompleted = null; StageCompleted = null;
        }
    }
}
