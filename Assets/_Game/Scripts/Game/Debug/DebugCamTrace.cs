#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using ClaudeCop.Camera;
using ClaudeCop.Core;

namespace ClaudeCop.Game.Debugging
{
    /// <summary>CAM-SMOOTH: ghi moi frame (cuoi frame) pose camera that: t, shot, blending, vcam active, pos, quat, fov. CSV o outPath khi LevelCompleted/OnDisable.</summary>
    public sealed class DebugCamTrace : MonoBehaviour
    {
        public string outPath = "";
        readonly StringBuilder sb = new StringBuilder(1 << 20);
        PhaseDirector pd; Unity.Cinemachine.CinemachineBrain brain;
        System.Action hLevel;
        static string F(float v) => v.ToString("0.#######", CultureInfo.InvariantCulture);

        void OnEnable()
        {
            sb.Clear();
            sb.AppendLine("t,dt,shot,phase,blend,vcam,px,py,pz,qx,qy,qz,qw,fov,ts,rx,ry,rz,cd");
            hLevel = () => Flush();
            RailEvents.LevelCompleted += hLevel;
            StartCoroutine(Loop());
        }
        void OnDisable() { RailEvents.LevelCompleted -= hLevel; Flush(); }

        public void Flush() { if (!string.IsNullOrEmpty(outPath)) File.WriteAllText(outPath, sb.ToString()); }

        IEnumerator Loop()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                var cam = UnityEngine.Camera.main; if (cam == null) continue;
                if (pd == null) pd = FindFirstObjectByType<PhaseDirector>();
                if (brain == null) brain = FindFirstObjectByType<Unity.Cinemachine.CinemachineBrain>();
                var p = cam.transform.position; var q = cam.transform.rotation;
                string sn = pd != null && pd.CurrentShot != null ? pd.CurrentShot.name : "-";
                string vc = brain != null && brain.ActiveVirtualCamera != null ? ((Component)brain.ActiveVirtualCamera).name : "-";
                sb.Append(F(Time.time)).Append(',').Append(F(Time.deltaTime)).Append(',').Append(sn).Append(',').Append(pd != null ? pd.CurrentPhaseIndex : -1).Append(',')
                  .Append(brain != null && brain.IsBlending ? 1 : 0).Append(',').Append(vc).Append(',')
                  .Append(F(p.x)).Append(',').Append(F(p.y)).Append(',').Append(F(p.z)).Append(',')
                  .Append(F(q.x)).Append(',').Append(F(q.y)).Append(',').Append(F(q.z)).Append(',').Append(F(q.w)).Append(',')
                  .Append(F(cam.fieldOfView)).Append(',').Append(F(Time.timeScale)).Append(',')
                  .Append(F(CameraFeelState.ReactTarget.x)).Append(',').Append(F(CameraFeelState.ReactTarget.y)).Append(',').Append(F(CameraFeelState.ReactTarget.z)).Append(',').Append(F(CameraFeelState.ComboDolly)).AppendLine();
            }
        }
    }
}
#endif
