using UnityEngine;

namespace ClaudeCop.Game
{
    /// <summary>
    /// Co tinh qua cac lan nap scene (Title -> Level_01, Restart). Tu reset khi vao Play mode.
    /// PendingStart: scene gameplay vua nap phai bat dau chay ngay (Start tu Title hoac Restart).
    /// TitleVisited: da qua man Title trong phien nay; false = chay thang scene gameplay (Editor/sandbox) -> tu bat dau.
    /// </summary>
    public static class GameSession
    {
        public static bool PendingStart { get; private set; }
        public static bool TitleVisited { get; private set; }

        public static void MarkTitleVisited() => TitleVisited = true;
        public static void RequestStartOnLoad() => PendingStart = true;

        /// <summary>Doc va xoa co PendingStart.</summary>
        public static bool ConsumePendingStart()
        {
            bool p = PendingStart;
            PendingStart = false;
            return p;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { PendingStart = false; TitleVisited = false; }
    }
}
