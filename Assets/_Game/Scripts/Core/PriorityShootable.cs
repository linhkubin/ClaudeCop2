using System.Collections.Generic;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>
    /// Vat bi tap duoc uu tien hon enemy (tru Justice), vd. thung no. Props cai dat + dang ky vao <see cref="PriorityShootables"/>.
    /// </summary>
    public interface IPriorityShootable : IShootable
    {
        /// <summary>Con nguyen ven (chua no / chua bi huy).</summary>
        bool IsPriorityLive { get; }
        /// <summary>Bounds world cua cac collider dang bat (size 0 neu khong co).</summary>
        Bounds PriorityBounds { get; }
    }

    public static class PriorityShootables
    {
        static readonly List<IPriorityShootable> items = new List<IPriorityShootable>(16);
        public static IReadOnlyList<IPriorityShootable> Items => items;

        public static void Register(IPriorityShootable s) { if (s != null && !items.Contains(s)) items.Add(s); }
        public static void Unregister(IPriorityShootable s) { items.Remove(s); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { items.Clear(); }
    }
}
