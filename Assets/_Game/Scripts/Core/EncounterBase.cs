using System;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>
    /// C17. Lop co so cua mot dot giao tranh. EncounterWave (Enemy) ke thua; CameraShot/PhaseDirector va RankScore dung.
    /// Lop con goi <see cref="RaiseCleared"/> khi het enemy.
    /// </summary>
    public abstract class EncounterBase : MonoBehaviour
    {
        /// <summary>(encounter, vi tri world cua cu ha guc cuoi)</summary>
        public event Action<EncounterBase, Vector3> Cleared;

        public abstract void Begin();
        public abstract bool IsActive { get; }
        public abstract bool IsCleared { get; }

        /// <summary>Id on dinh, mac dinh la ten GameObject.</summary>
        public virtual string Id => name;
        /// <summary>Mo ta ngan (cho RankScore).</summary>
        public virtual string Description => string.Empty;

        protected void RaiseCleared(Vector3 lastKillWorldPos) => Cleared?.Invoke(this, lastKillWorldPos);
    }
}
