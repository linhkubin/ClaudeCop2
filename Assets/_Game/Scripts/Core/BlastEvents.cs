using System;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>
    /// C22. Bao cao mot vu no. Props phat; Game (diem), RankScore (thong ke), FX (hieu ung no), Camera (rung), UI (chu bay) nghe.
    /// </summary>
    public struct BlastReport
    {
        /// <summary>Tam vu no (world).</summary>
        public Vector3 Center;
        /// <summary>Ban kinh (m).</summary>
        public float Radius;
        /// <summary>So enemy bi ha.</summary>
        public int EnemiesKilled;
        /// <summary>So con tin bi trung.</summary>
        public int HostagesHit;
        /// <summary>Id on dinh cua vat no.</summary>
        public int SourceId;
    }

    /// <summary>
    /// C21. Su kien vu no. Chi module Props duoc <see cref="Raise"/> (main thread); Game, RankScore, FX, Camera, UI nghe.
    /// Tu reset khi vao Play mode.
    /// </summary>
    public static class BlastEvents
    {
        public static event Action<BlastReport> Blasted;

        public static void Raise(BlastReport report) => Blasted?.Invoke(report);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Blasted = null; }
    }
}
