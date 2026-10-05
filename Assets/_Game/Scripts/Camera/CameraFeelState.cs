using UnityEngine;

namespace ClaudeCop.Camera
{
    /// <summary>Trang thai runtime dung chung giua PhaseDirector va cac CameraFeelApplier (moi CinemachineCamera mot Applier).</summary>
    public static class CameraFeelState
    {
        public static bool Combat;
        public static float MoveSpeed;     // m/s hien tai
        public static float CombatWeight;  // 0 = Move, 1 = Combat (muot) - chi de debug/UI; Applier dung trang thai rieng moi camera
        public static float ShakeStart = -10f;
        public static float ShakeDuration = 0.2f;

        /// <summary>He so bien do shake hien tai (1 = shake trung dan; no dung explosionShakeScale).</summary>
        public static float ShakeScale = 1f;

        /// <summary>Phan rung (hit/no) da ap len pose camera frame nay (local theo huong camera). CameraPoseSmoother tach ra de khong loc mat rung.</summary>
        public static Vector3 ShakePos;
        public static Quaternion ShakeRot = Quaternion.identity;

        /// <summary>CAM-LIVELY: muc tieu reaction hien tai (x = yaw do, y = pitch do (len = duong), z = punch FOV do (giam FOV)). PhaseDirector ghi, CameraPoseSmoother ap sau luoi gioi han.</summary>
        public static Vector3 ReactTarget;
        /// <summary>CAM-LIVELY: do giam FOV (do) theo combo, da lam muot. PhaseDirector ghi, CameraFeelApplier doc.</summary>
        public static float ComboDolly;

        /// <summary>CAM-VC2: giat khi ban. x = pitch len (do), y = giam FOV (do). PhaseDirector ghi, CameraPoseSmoother ap qua kenh rieng sau luoi gioi han.</summary>
        public static Vector2 KickTarget;

        public static void TriggerShake(float duration, float scale = 1f) { ShakeStart = Time.time; ShakeDuration = Mathf.Max(0.01f, duration); ShakeScale = Mathf.Max(0f, scale); }

        public static void Tick(float dt, CameraFeelProfile p)
        {
            float target = Combat ? 1f : 0f;
            CombatWeight = Mathf.MoveTowards(CombatWeight, target, dt / Mathf.Max(0.01f, p.modeFade));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Combat = false; MoveSpeed = 0f; CombatWeight = 0f; ShakeStart = -10f; ShakeDuration = 0.2f; ShakeScale = 1f; ShakePos = Vector3.zero; ShakeRot = Quaternion.identity; ReactTarget = Vector3.zero; ComboDolly = 0f; KickTarget = Vector2.zero;
        }
    }
}
