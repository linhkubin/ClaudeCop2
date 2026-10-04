#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Combat;

namespace ClaudeCop.Game.Debugging
{
    /// <summary>
    /// Chi de test ghep scene: tu "tap" qua TapShooter (TryFire that, co chan pause/PointerBlocker) vao vi tri man hinh cua AimPoint
    /// muc tieu dau tien co ExposedTime &gt;= delay. Khong luu trong scene; them luc runtime khi can.
    /// </summary>
    public sealed class DebugAutoTapper : MonoBehaviour
    {
        public float delay = 0.6f;
        public float interval = 0.3f;
        public bool active = true;
        public int shots;

        TapShooter shooter;
        MethodInfo tryFire;
        float next;
        readonly List<ITapTarget> buf = new List<ITapTarget>();

        void Awake()
        {
            shooter = FindFirstObjectByType<TapShooter>();
            tryFire = typeof(TapShooter).GetMethod("TryFire", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        void Update()
        {
            if (!active || shooter == null || tryFire == null || Time.time < next) return;
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;
            buf.Clear();
            for (int i = 0; i < TargetRegistry.Targets.Count; i++) buf.Add(TargetRegistry.Targets[i]);
            foreach (var t in buf)
            {
                if (t == null || !t.IsTargetable || t.Kind != TargetKind.Enemy || t.ExposedTime < delay) continue;
                var sp = cam.WorldToScreenPoint(t.AimPoint);
                if (sp.z <= 0f) continue;
                tryFire.Invoke(shooter, new object[] { new Vector2(sp.x, sp.y) });
                shots++;
                next = Time.time + interval;
                return;
            }
        }
    }
}
#endif
