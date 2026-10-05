using System.Collections.Generic;

namespace ClaudeCop.Core
{
    /// <summary>Chu stage chi hien DUNG MOT LAN cho moi Phase (index), khong lap lai o doan Move/shot khac hay khi hoi sinh.</summary>
    public sealed class PhaseBannerGate
    {
        readonly HashSet<int> shown = new HashSet<int>();
        /// <summary>true neu day la lan dau xin hien banner cho Phase nay.</summary>
        public bool TryShow(int phaseIndex) => shown.Add(phaseIndex);
        public int ShownCount => shown.Count;
        public void Reset() => shown.Clear();
    }
}
