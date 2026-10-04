using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>
    /// C8. Danh sach muc tieu dang co the tap. Muc tieu Register khi lo ra, Unregister khi chet/an/disable.
    /// TapShooter va UI nghe. Tu reset khi vao Play mode.
    /// </summary>
    public static class TargetRegistry
    {
        static readonly List<ITapTarget> targets = new List<ITapTarget>(16);
        static readonly IReadOnlyList<ITapTarget> readOnly = targets.AsReadOnly();

        public static IReadOnlyList<ITapTarget> Targets => readOnly;
        public static int Count => targets.Count;
        public static event Action<ITapTarget> Registered;
        public static event Action<ITapTarget> Unregistered;

        public static void Register(ITapTarget target)
        {
            if (target == null || targets.Contains(target)) return;
            targets.Add(target);
            Registered?.Invoke(target);
        }

        public static void Unregister(ITapTarget target)
        {
            if (target == null) return;
            if (targets.Remove(target)) Unregistered?.Invoke(target);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            targets.Clear();
            Registered = null;
            Unregistered = null;
        }
    }
}
