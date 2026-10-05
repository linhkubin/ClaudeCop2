#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using ClaudeCop.Camera;
using ClaudeCop.Core;

namespace ClaudeCop.Game.Debugging
{
    /// <summary>
    /// L1-WAVES: do khi bot chay. (1) moi target Enemy targetable: vung nhin thay tren man hinh (viewport) va so lan nam ngoai man;
    /// (2) so enemy targetable cung luc toi da; (3) camera cham collider (OverlapSphere 0.25) ben ngoai cac shot Combat. Ghi file khi LevelCompleted/OnDisable.
    /// </summary>
    public sealed class DebugL1Probe : MonoBehaviour
    {
        public string outPath = "";
        readonly Dictionary<int, string> seen = new Dictionary<int, string>();
        readonly Dictionary<int, Vector2> vmin = new Dictionary<int, Vector2>(), vmax = new Dictionary<int, Vector2>();
        readonly HashSet<string> camHits = new HashSet<string>();
        readonly HashSet<string> concurrent = new HashSet<string>();
        int maxConc; int offscreen;
        PhaseDirector pd; System.Action hLevel;
        readonly Collider[] buf = new Collider[16];

        void OnEnable() { hLevel = Flush; RailEvents.LevelCompleted += hLevel; }
        void OnDisable() { RailEvents.LevelCompleted -= hLevel; Flush(); }

        void LateUpdate()
        {
            var cam = UnityEngine.Camera.main; if (cam == null) return;
            if (pd == null) pd = FindFirstObjectByType<PhaseDirector>();
            string sn = pd != null && pd.CurrentShot != null ? pd.CurrentShot.name : "-";
            int conc = 0;
            for (int i = 0; i < TargetRegistry.Targets.Count; i++)
            {
                var t = TargetRegistry.Targets[i];
                if (t == null || t.Kind != TargetKind.Enemy || !t.IsTargetable) continue;
                conc++;
                var c = t as Component; int id = t.GetHashCode();
                var v = cam.WorldToViewportPoint(t.AimPoint);
                var p = new Vector2(v.x, v.y);
                if (!seen.ContainsKey(id)) { seen[id] = (c != null ? c.transform.parent.name + "/" + c.name : "?") + " @" + sn; vmin[id] = p; vmax[id] = p; }
                vmin[id] = Vector2.Min(vmin[id], p); vmax[id] = Vector2.Max(vmax[id], p);
                if (v.z <= 0f || v.x < 0.03f || v.x > 0.97f || v.y < 0.03f || v.y > 0.97f) offscreen++;
            }
            if (conc > maxConc) maxConc = conc;
            if (conc >= 2) concurrent.Add(sn + " x" + conc);
            int n = Physics.OverlapSphereNonAlloc(cam.transform.position, 0.25f, buf, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++) camHits.Add(sn + " -> " + buf[i].name);
        }

        public void Flush()
        {
            if (string.IsNullOrEmpty(outPath)) return;
            var sb = new StringBuilder();
            sb.AppendLine("enemies_seen=" + seen.Count + " maxConcurrentTargetable=" + maxConc + " offscreenFrames=" + offscreen);
            foreach (var kv in seen) sb.AppendLine("ENEMY " + kv.Value + " vp x " + vmin[kv.Key].x.ToString("F2") + ".." + vmax[kv.Key].x.ToString("F2") + " y " + vmin[kv.Key].y.ToString("F2") + ".." + vmax[kv.Key].y.ToString("F2"));
            foreach (var s in concurrent) sb.AppendLine("CONCURRENT " + s);
            foreach (var s in camHits) sb.AppendLine("CAMHIT " + s);
            File.WriteAllText(outPath, sb.ToString());
        }
    }
}
#endif
