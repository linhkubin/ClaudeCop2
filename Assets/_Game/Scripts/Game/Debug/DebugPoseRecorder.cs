#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Camera;

namespace ClaudeCop.Game.Debugging
{
    /// <summary>T-702: ghi tu the camera THAT luc bat dau giao tranh va luc enemy lo ra + thong tin thung no. Ghi JSON o outPath.</summary>
    public sealed class DebugPoseRecorder : MonoBehaviour
    {
        public string outPath = "";
        readonly List<string> entries = new List<string>();
        System.Action<EncounterBase> hStart;
        static string F(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);
        static string V(Vector3 v) => "[" + F(v.x) + "," + F(v.y) + "," + F(v.z) + "]";

        void OnEnable() { hStart = e => StartCoroutine(Record(e)); RailEvents.EncounterStarted += hStart; }
        void OnDisable() { RailEvents.EncounterStarted -= hStart; }

        IEnumerator Record(EncounterBase e)
        {
            var pd = FindFirstObjectByType<PhaseDirector>();
            string shot = pd != null && pd.CurrentShot != null ? pd.CurrentShot.name : "?";
            Snap(e, shot, "start");
            yield return new WaitForSeconds(1.2f);
            Snap(e, shot, "settled");
        }

        void Snap(EncounterBase e, string shot, string tag)
        {
            var cam = UnityEngine.Camera.main; if (cam == null) return;
            var sb = new StringBuilder();
            sb.Append("{\"wave\":\"" + e.Id + "\",\"shot\":\"" + shot + "\",\"when\":\"" + tag + "\",\"t\":" + F(Time.time));
            sb.Append(",\"camPos\":" + V(cam.transform.position) + ",\"camEuler\":" + V(cam.transform.eulerAngles));
            sb.Append(",\"vFov\":" + F(cam.fieldOfView) + ",\"aspect\":" + F(cam.aspect) + ",\"screen\":\"" + Screen.width + "x" + Screen.height + "\"");
            // enemy dang targetable
            var tl = new List<string>();
            for (int i = 0; i < TargetRegistry.Targets.Count; i++)
            {
                var t = TargetRegistry.Targets[i]; if (t == null || !t.IsTargetable) continue;
                tl.Add("{\"kind\":\"" + t.Kind + "\",\"aim\":" + V(t.AimPoint) + "}");
            }
            sb.Append(",\"targets\":[" + string.Join(",", tl) + "]");
            // thung no
            var bl = new List<string>();
            foreach (var m in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (m.GetType().Name != "ExplosiveBarrel") continue;
                Bounds b = new Bounds(m.transform.position, Vector3.zero); bool has = false;
                foreach (var r in m.GetComponentsInChildren<Renderer>()) { if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds); }
                float best = 1e9f; Vector3 bp = Vector3.zero; string bk = "";
                for (int i = 0; i < TargetRegistry.Targets.Count; i++)
                {
                    var t = TargetRegistry.Targets[i]; if (t == null || !t.IsTargetable) continue;
                    float d = (t.AimPoint - b.center).magnitude; if (d < best) { best = d; bp = t.AimPoint; bk = t.Kind.ToString(); }
                }
                var sp = cam.WorldToScreenPoint(b.center);
                bl.Add("{\"name\":\"" + m.name + "\",\"center\":" + V(b.center) + ",\"size\":" + V(b.size) + ",\"nearestKind\":\"" + bk + "\",\"nearestAim\":" + V(bp) + ",\"nearestDist\":" + F(best < 1e8f ? best : -1f)
                    + ",\"screenPx\":[" + F(sp.x) + "," + F(sp.y) + "],\"inFront\":" + (sp.z > 0 ? "true" : "false") + ",\"camDist\":" + F(sp.z) + "}");
            }
            sb.Append(",\"barrels\":[" + string.Join(",", bl) + "]}");
            entries.Add(sb.ToString());
            if (!string.IsNullOrEmpty(outPath)) File.WriteAllText(outPath, "[\n" + string.Join(",\n", entries) + "\n]");
        }
    }
}
#endif
