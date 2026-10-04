using UnityEngine;

namespace ClaudeCop.Camera
{
    /// <summary>Trang thai runtime dung chung giua PhaseDirector va cac CameraFeelApplier (moi CinemachineCamera mot Applier).</summary>
    public static class CameraFeelState
    {
        public static bool Combat;
        public static float MoveSpeed;     // m/s hien tai
        public static float CombatWeight;  // 0 = Move, 1 = Combat (muot)
        public static float DollyProgress; // 0..1 trong Shot giao tranh
        public static float ShakeStart = -10f;
        public static float ShakeDuration = 0.2f;

        public static void TriggerShake(float duration) { ShakeStart = Time.time; ShakeDuration = Mathf.Max(0.01f, duration); }

        public static void ResetDolly() => DollyProgress = 0f;

        public static void Tick(float dt, CameraFeelProfile p)
        {
            float target = Combat ? 1f : 0f;
            CombatWeight = Mathf.MoveTowards(CombatWeight, target, dt / Mathf.Max(0.01f, p.modeFade));
            if (Combat && p.dollyInDuration > 0f) DollyProgress = Mathf.Clamp01(DollyProgress + dt / p.dollyInDuration);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Combat = false; MoveSpeed = 0f; CombatWeight = 0f; DollyProgress = 0f; ShakeStart = -10f; ShakeDuration = 0.2f;
        }
    }
}
