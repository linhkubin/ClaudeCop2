using System;
using System.Collections.Generic;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Combat
{
    /// <summary>
    /// Logic thuan: thung no (IPriorityShootable) uu tien hon enemy/pickup/con tin, tru khi tap trung Justice.
    /// </summary>
    public static class BarrelPriority
    {
        /// <summary>Chi dung uu tien thung khi bat, vu khi 1 muc tieu, va ket qua chon dau tien khong phai Justice.</summary>
        public static bool Applies(bool enabled, int maxHits, int hitCount, bool firstIsJustice)
            => enabled && maxHits == 1 && !(hitCount > 0 && firstIsJustice);

        /// <summary>
        /// Chon thung bi tap trung: tap nam trong rect man hinh (da them padding) va khong bi che. Nhieu thung -> thung gan camera nhat.
        /// </summary>
        public static IPriorityShootable Pick(IReadOnlyList<IPriorityShootable> items, Vector2 tap,
            Func<IPriorityShootable, Rect?> screenRect, Func<IPriorityShootable, bool> occluded, Func<IPriorityShootable, float> depth)
        {
            IPriorityShootable best = null;
            float bestDepth = float.MaxValue;
            for (int i = 0; i < items.Count; i++)
            {
                var b = items[i];
                if (b == null || !b.IsPriorityLive) continue;
                var r = screenRect(b);
                if (!r.HasValue || !r.Value.Contains(tap)) continue;
                if (occluded != null && occluded(b)) continue;
                float d = depth != null ? depth(b) : 0f;
                if (best == null || d < bestDepth) { best = b; bestDepth = d; }
            }
            return best;
        }
    }
}
