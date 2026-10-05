#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using UnityEngine;
using ClaudeCop.Camera;

namespace ClaudeCop.Game.Debugging
{
    /// <summary>Do camera cuoi frame: toc do vi tri, toc do xoay, toc do FOV, gia toc xoay; thong ke max theo Shot. Chi gan runtime.</summary>
    public sealed class DebugCameraProbe : MonoBehaviour
    {
        public string logFile = "";
        public float fixedDt = 0f; // >0: dung dt co dinh (khi Step thu cong) thay vi realtime
        PhaseDirector pd; Unity.Cinemachine.CinemachineBrain brain;
        class St { public float maxPos, maxRot, maxFov, maxAcc, minFov = 999, maxFovSeen; }
        readonly System.Collections.Generic.Dictionary<string, St> stats = new System.Collections.Generic.Dictionary<string, St>();

        void Log(string m) { Debug.Log("[CAMPROBE] " + m); if (!string.IsNullOrEmpty(logFile)) File.AppendAllText(logFile, m + "\n"); }

        IEnumerator Start()
        {
            Vector3 lp = default; Quaternion lq = default; float lf = 0, lt = 0, lastRate = 0; bool have = false;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                var cam = UnityEngine.Camera.main; if (cam == null) { have = false; continue; }
                if (pd == null) pd = FindFirstObjectByType<PhaseDirector>();
                if (brain == null) brain = FindFirstObjectByType<Unity.Cinemachine.CinemachineBrain>();
                float t = lt + Time.deltaTime;
                Vector3 p = cam.transform.position; Quaternion q = cam.transform.rotation; float f = cam.fieldOfView;
                string sn = pd != null && pd.CurrentShot != null ? pd.CurrentShot.name : "-";
                bool bl = brain != null && brain.IsBlending;
                if (have && Time.timeScale > 0f)
                {
                    float dt = t - lt;
                    if (dt > 0.001f && dt < 0.1f)
                    {
                        float vp = Vector3.Distance(p, lp) / dt, vr = Quaternion.Angle(q, lq) / dt, vf = Mathf.Abs(f - lf) / dt, acc = Mathf.Abs(vr - lastRate) / dt;
                        string key = sn + (bl ? "|blend" : "");
                        if (!stats.TryGetValue(key, out var s)) stats[key] = s = new St();
                        s.maxPos = Mathf.Max(s.maxPos, vp); s.maxRot = Mathf.Max(s.maxRot, vr); s.maxFov = Mathf.Max(s.maxFov, vf); s.maxAcc = Mathf.Max(s.maxAcc, acc);
                        s.minFov = Mathf.Min(s.minFov, f); s.maxFovSeen = Mathf.Max(s.maxFovSeen, f);
                        if (vp > 8f || vr > 90f || vf > 40f) Log("SPIKE shot=" + key + " pos=" + vp.ToString("F1") + "m/s rot=" + vr.ToString("F0") + "deg/s fov=" + f.ToString("F1") + " dFov=" + vf.ToString("F0") + "/s dt=" + dt.ToString("F3") + " f=" + Time.frameCount);
                        lastRate = vr;
                    }
                }
                lp = p; lq = q; lf = f; lt = t; have = true;
            }
        }

        public void Dump()
        {
            foreach (var kv in stats)
                Log("STAT " + kv.Key + " maxPos=" + kv.Value.maxPos.ToString("F1") + " maxRot=" + kv.Value.maxRot.ToString("F0") + " maxFovRate=" + kv.Value.maxFov.ToString("F0") + " maxRotAcc=" + kv.Value.maxAcc.ToString("F0") + " fov=[" + kv.Value.minFov.ToString("F1") + "," + kv.Value.maxFovSeen.ToString("F1") + "]");
        }
        void OnDisable() { Dump(); }
    }
}
#endif
