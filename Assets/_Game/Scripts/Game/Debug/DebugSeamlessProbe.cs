#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Camera;

namespace ClaudeCop.Game.Debugging
{
    /// <summary>
    /// L1-SEAMLESS: do tinh lien mach cua camera. Moi frame (cuoi frame, thoi gian that) ghi pose camera that, do:
    /// nhay vi tri (m/frame, m/s), toc do xoay (do/s), gia toc xoay, alpha lop den PhaseFade (CanvasGroup "Black"), enemy targetable ngoai man hinh.
    /// Ghi tom tat + cac frame vuot nguong vao outPath (.txt) va CSV day du vao csvPath. Chi gan luc runtime (khong luu trong scene).
    /// </summary>
    public sealed class DebugSeamlessProbe : MonoBehaviour
    {
        public string outPath = "", csvPath = "", shotDir = "";
        public float jumpMeters = 0.6f;          // > ngay frame = nhay vi tri
        public float jumpDegPerSec = 150f;       // xoay > nguong = nhay huong
        public float shotDelay = 0.9f;           // chup tieu de Phase sau khi banner hien

        readonly StringBuilder csv = new StringBuilder(1 << 20);
        readonly StringBuilder log = new StringBuilder();
        CanvasGroup black;
        PhaseDirector pd;
        Vector3 lastPos; Quaternion lastRot; float lastT, lastRate; bool have;
        float maxBlack, maxPosStep, maxRate, maxRateMove, maxRateAny, maxSpeed, maxAccelRot;
        int frames, posJumps, rotJumps, offscreenFrames, blackFrames;
        string maxRateShot = "", maxStepShot = "";
        System.Action<string> hBanner; System.Action hLevel;
        bool flushed;

        static string F(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);

        void OnEnable()
        {
            csv.AppendLine("t,dt,state,phase,shot,px,py,pz,yaw,pitch,rateDeg,step,blackA,offscreen");
            hBanner = title => StartCoroutine(BannerShot(title));
            hLevel = () => Flush("LEVEL_COMPLETED");
            RailEvents.PhaseBanner += hBanner; RailEvents.LevelCompleted += hLevel;
            StartCoroutine(Loop());
        }

        void OnDisable() { RailEvents.PhaseBanner -= hBanner; RailEvents.LevelCompleted -= hLevel; Flush("DISABLE"); }

        IEnumerator BannerShot(string title)
        {
            log.AppendLine("BANNER '" + title + "' t=" + F(Time.time));
            yield return new WaitForSecondsRealtime(shotDelay);
            if (!string.IsNullOrEmpty(shotDir))
            {
                string n = shotDir + "/banner_" + title.Replace(' ', '_') + ".png";
                ScreenCapture.CaptureScreenshot(n);
                log.AppendLine("SCREENSHOT " + n + " camPos=" + UnityEngine.Camera.main.transform.position + " shot=" + (pd != null && pd.CurrentShot != null ? pd.CurrentShot.name : "-"));
            }
        }

        IEnumerator Loop()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                var cam = UnityEngine.Camera.main; if (cam == null) { have = false; continue; }
                if (pd == null) pd = FindFirstObjectByType<PhaseDirector>();
                if (black == null)
                    foreach (var g in FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                        if (g.name == "Black") { black = g; break; }
                float t = Time.realtimeSinceStartup;
                var p = cam.transform.position; var q = cam.transform.rotation;
                float dt = t - lastT;
                float step = 0f, rate = 0f;
                if (have && dt > 0.0005f && Time.timeScale > 0f && GameEvents.Current.State == GameState.Playing)
                {
                    step = Vector3.Distance(p, lastPos);
                    rate = Quaternion.Angle(lastRot, q) / dt;
                    float rr = Mathf.Min(rate, 1000f);
                    string sn = pd != null && pd.CurrentShot != null ? pd.CurrentShot.name : "-";
                    bool move = pd != null && pd.CurrentShot != null && pd.CurrentShot.kind == ShotKind.Move;
                    if (step > maxPosStep) { maxPosStep = step; maxStepShot = sn; }
                    if (step / dt > maxSpeed && step < jumpMeters) maxSpeed = step / dt;
                    if (rate > maxRateAny && rate < 1000f) { maxRateAny = rate; maxRateShot = sn; }
                    if (move && rate > maxRateMove && rate < 1000f) maxRateMove = rate;
                    maxAccelRot = Mathf.Max(maxAccelRot, Mathf.Abs(rate - lastRate) / dt);
                    if (step > jumpMeters) { posJumps++; log.AppendLine("POS_JUMP " + F(step) + " m t=" + F(Time.time) + " shot=" + sn + " at " + p); }
                    if (rate > jumpDegPerSec) { rotJumps++; log.AppendLine("ROT_JUMP " + F(rate) + " deg/s t=" + F(Time.time) + " shot=" + sn); }
                }
                lastPos = p; lastRot = q; lastT = t; lastRate = rate; have = true;
                float ba = black != null && black.gameObject.activeInHierarchy ? black.alpha : 0f;
                if (ba > maxBlack) maxBlack = ba;
                if (ba > 0.001f) blackFrames++;
                int off = 0;
                if (GameEvents.Current.State == GameState.Playing)
                    for (int i = 0; i < TargetRegistry.Targets.Count; i++)
                    {
                        var tg = TargetRegistry.Targets[i];
                        if (tg == null || !tg.IsTargetable || tg.Kind != TargetKind.Enemy) continue;
                        var sp = cam.WorldToViewportPoint(tg.AimPoint);
                        if (sp.z <= 0f || sp.x < 0f || sp.x > 1f || sp.y < 0f || sp.y > 1f) off++;
                    }
                if (off > 0) offscreenFrames++;
                frames++;
                var e = q.eulerAngles;
                csv.Append(F(Time.time)).Append(',').Append(F(dt)).Append(',').Append((int)GameEvents.Current.State).Append(',').Append(pd != null ? pd.CurrentPhaseIndex : -1).Append(',')
                   .Append(pd != null && pd.CurrentShot != null ? pd.CurrentShot.name : "-").Append(',').Append(F(p.x)).Append(',').Append(F(p.y)).Append(',').Append(F(p.z)).Append(',')
                   .Append(F(e.y)).Append(',').Append(F(e.x)).Append(',').Append(F(rate)).Append(',').Append(F(step)).Append(',').Append(F(ba)).Append(',').Append(off).AppendLine();
            }
        }

        public void Flush(string why)
        {
            if (flushed && why == "DISABLE") return;
            flushed = why != "LEVEL_COMPLETED" ? flushed : true;
            if (!string.IsNullOrEmpty(csvPath)) File.WriteAllText(csvPath, csv.ToString());
            if (string.IsNullOrEmpty(outPath)) return;
            var s = new StringBuilder();
            s.AppendLine("SEAMLESS_PROBE (" + why + ") frames=" + frames);
            s.AppendLine("maxPosStepPerFrame=" + F(maxPosStep) + " m (shot " + maxStepShot + ")  posJumps(>" + jumpMeters + " m)=" + posJumps);
            s.AppendLine("maxRotRate=" + F(maxRateAny) + " deg/s (shot " + maxRateShot + ")  maxRotRateDuringMove=" + F(maxRateMove) + "  rotJumps(>" + jumpDegPerSec + ")=" + rotJumps);
            s.AppendLine("maxMoveSpeed=" + F(maxSpeed) + " m/s  maxRotAccel=" + F(maxAccelRot) + " deg/s^2");
            s.AppendLine("maxBlackAlpha=" + F(maxBlack) + " blackFrames=" + blackFrames + " (black=" + (black != null ? black.name : "none-found") + ")");
            s.AppendLine("framesWithTargetableEnemyOffscreen=" + offscreenFrames);
            s.Append(log);
            File.WriteAllText(outPath, s.ToString());
        }
    }
}
#endif
