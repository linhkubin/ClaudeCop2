using System.Collections.Generic;
using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Vong target: nghe TargetRegistry, pool TargetReticleView duoi HudView.ReticleRoot,
    /// moi LateUpdate doi AimPoint -> screen bang Camera.main (doc truc tiep camera hien tai nen dung ca khi blend/zoom).
    /// Khong alloc moi frame.
    /// </summary>
    public class TargetReticlePresenter : MonoBehaviour
    {
        struct Entry { public ITapTarget Target; public TargetReticleView View; }

        [SerializeField] HudView hud;
        [SerializeField] TargetReticleView reticlePrefab;

        readonly List<Entry> active = new List<Entry>(16);
        readonly Stack<TargetReticleView> pool = new Stack<TargetReticleView>(16);
        Camera cam;

        public int ActiveCount => active.Count;

        void OnEnable()
        {
            TargetRegistry.Registered += OnRegistered;
            TargetRegistry.Unregistered += OnUnregistered;
            var list = TargetRegistry.Targets;
            for (int i = 0; i < list.Count; i++) OnRegistered(list[i]);
        }

        void OnDisable()
        {
            TargetRegistry.Registered -= OnRegistered;
            TargetRegistry.Unregistered -= OnUnregistered;
            for (int i = active.Count - 1; i >= 0; i--) Release(active[i].View);
            active.Clear();
        }

        void OnRegistered(ITapTarget t)
        {
            if (t == null || hud == null || reticlePrefab == null) return;
            for (int i = 0; i < active.Count; i++) if (active[i].Target == t) return;
            TargetReticleView v = null;
            while (v == null && pool.Count > 0) v = pool.Pop();
            if (v == null) v = Instantiate(reticlePrefab, hud.ReticleRoot, false);
            v.Show(false);
            active.Add(new Entry { Target = t, View = v });
        }

        void OnUnregistered(ITapTarget t)
        {
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].Target != t) continue;
                Release(active[i].View);
                active.RemoveAt(i);
                return;
            }
        }

        void Release(TargetReticleView v)
        {
            if (v == null) return;
            v.Show(false);
            pool.Push(v);
        }

        void LateUpdate()
        {
            if (active.Count == 0) return;
            if (cam == null || !cam.isActiveAndEnabled) cam = Camera.main;
            for (int i = 0; i < active.Count; i++)
            {
                var e = active[i];
                if (e.View == null) continue;
                var t = e.Target;
                if (cam == null || !t.IsTargetable || !t.ShowsReticle) { e.View.Show(false); continue; }
                Vector3 sp = cam.WorldToScreenPoint(t.AimPoint);
                if (sp.z <= 0f) { e.View.Show(false); continue; }
                e.View.Show(true);
                e.View.SetScreenPosition(new Vector2(sp.x, sp.y));
                e.View.SetProgress(t.ReticleProgress);
            }
        }
    }
}
