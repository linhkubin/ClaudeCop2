#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Camera.DebugTools
{
    /// <summary>Do toc do xoay/roll/pitch cua Main Camera moi frame; log tong ket khi LevelCompleted. Chi de kiem Sandbox.</summary>
    public class DebugCameraProbe : MonoBehaviour
    {
        public float maxYawRateMove, maxYawRateAll, maxRoll, maxPitch, maxSpeed;
        public int pauseTransitions;
        PhaseDirector dir; string worst = "";
        Transform cam; float lastYaw; Vector3 lastPos; bool first = true;
        readonly System.Text.StringBuilder log = new System.Text.StringBuilder();

        void Awake() { Application.runInBackground = true; }

        System.Action<int, string> hPhase;
        System.Action<EncounterBase> hMove, hStart, hClear;
        System.Action hLevel;
        System.Action<bool> hPause;

        void OnEnable()
        {
            hPhase = (i, t) => Log("PhaseStarted " + i + " " + t);
            hMove = e => Log("MoveSegmentStarted -> " + (e ? e.Id : "null"));
            hStart = e => Log("EncounterStarted " + e.Id);
            hClear = e => Log("EncounterCleared " + e.Id);
            hLevel = () => { Log("LevelCompleted"); Debug.Log("[Probe] " + log + Summary()); };
            hPause = p => { Log("Pause=" + p); if (p) pauseTransitions++; };
            RailEvents.PhaseStarted += hPhase;
            RailEvents.MoveSegmentStarted += hMove;
            RailEvents.EncounterStarted += hStart;
            RailEvents.EncounterCleared += hClear;
            RailEvents.LevelCompleted += hLevel;
            CombatPauseSignal.Changed += hPause;
        }

        void OnDisable()
        {
            RailEvents.PhaseStarted -= hPhase;
            RailEvents.MoveSegmentStarted -= hMove;
            RailEvents.EncounterStarted -= hStart;
            RailEvents.EncounterCleared -= hClear;
            RailEvents.LevelCompleted -= hLevel;
            CombatPauseSignal.Changed -= hPause;
        }

        string Summary() => "worstMove[" + worst + "] maxYawRateMove=" + maxYawRateMove.ToString("F1") +" maxYawRateAll=" +maxYawRateAll.ToString("F1") + " maxRollDeg=" + maxRoll.ToString("F2") + " maxPitchDeg=" + maxPitch.ToString("F2") + " maxSpeed=" + maxSpeed.ToString("F2") + " pauses=" + pauseTransitions;
        void Log(string s) => log.AppendLine(Time.time.ToString("F2") + " " + s);

        void LateUpdate()
        {
            if (cam == null) { var c = UnityEngine.Camera.main; if (c == null) return; cam = c.transform; }
            float yaw = cam.eulerAngles.y;
            if (!first && Time.deltaTime > 0f)
            {
                float rate = Mathf.Abs(Mathf.DeltaAngle(lastYaw, yaw)) / Time.deltaTime;
                if (rate > maxYawRateAll && rate < 400f) maxYawRateAll = rate; // bo qua Cut
                if (dir == null) dir = FindFirstObjectByType<PhaseDirector>();
                if (dir != null && dir.CurrentShot != null && dir.CurrentShot.kind == ShotKind.Move && !CombatPauseSignal.IsPaused)
                {
                    if (rate > maxYawRateMove) worst = "t=" + Time.time.ToString("F2") + " dt=" + Time.deltaTime.ToString("F4") + " rate=" + rate.ToString("F1") + " shot=" + dir.CurrentShot.name + " yaw=" + yaw.ToString("F1");
                    maxYawRateMove = Mathf.Max(maxYawRateMove, rate);
                }
                maxSpeed = Mathf.Max(maxSpeed, (cam.position - lastPos).magnitude / Time.deltaTime < 50f ? (cam.position - lastPos).magnitude / Time.deltaTime : 0f);
            }
            maxRoll = Mathf.Max(maxRoll, Mathf.Abs(Mathf.DeltaAngle(0, cam.eulerAngles.z)));
            maxPitch = Mathf.Max(maxPitch, Mathf.Abs(Mathf.DeltaAngle(0, cam.eulerAngles.x)));
            lastYaw = yaw; lastPos = cam.position; first = false;
        }
    }
}
#endif
