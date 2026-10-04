using UnityEngine;
using Unity.Cinemachine;
using ClaudeCop.Core;

namespace ClaudeCop.Camera
{
    /// <summary>
    /// Mo rong Cinemachine: rung cam tay (Combat), nhun (Move), dolly-in FOV (Combat), shake trung dan.
    /// Dat tren moi CinemachineCamera; ap o stage Noise nen blend giua cac camera van muot.
    /// Giam chuyen dong (UserSettings.ReduceMotion): tat cam tay/nhun/dolly/zoom punch/kill zoom, shake con hitShakeReduceScale.
    /// </summary>
    public class CameraFeelApplier : CinemachineExtension
    {
        public CameraFeelProfile profile;

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Noise || profile == null || !Application.isPlaying) return;

            state.Lens.FieldOfView = FitFov(state.Lens.FieldOfView, state.Lens.Aspect, profile);

            bool reduce = UserSettings.ReduceMotion;
            float t = Time.time;
            Vector3 pos = Vector3.zero;
            Vector3 rot = Vector3.zero;
            float cw = CameraFeelState.CombatWeight;

            if (!reduce)
            {
                if (cw > 0f && profile.handheldAmplitude > 0f)
                {
                    float x = t * profile.handheldFrequency * profile.handheldTimeScale;
                    float a = profile.handheldAmplitude * cw;
                    rot.x += (Mathf.PerlinNoise(x, 11.3f) - 0.5f) * 2f * a;
                    rot.y += (Mathf.PerlinNoise(x, 57.1f) - 0.5f) * 2f * a;
                    rot.z += (Mathf.PerlinNoise(x, 93.7f) - 0.5f) * 2f * a * 0.5f;
                }
                float speed01 = profile.railSpeed > 0f ? Mathf.Clamp01(CameraFeelState.MoveSpeed / profile.railSpeed) : 0f;
                if (speed01 > 0f && profile.bobAmplitude > 0f)
                {
                    float ph = t * profile.bobStepsPerSecond * Mathf.PI * 2f;
                    pos.y += Mathf.Sin(ph) * profile.bobAmplitude * speed01;
                    pos.x += Mathf.Sin(ph * 0.5f) * profile.bobAmplitude * 0.4f * speed01;
                }
                if (cw > 0f && profile.dollyInFov > 0f)
                {
                    float ease = CameraFeelState.DollyProgress; ease = ease * ease * (3f - 2f * ease);
                    state.Lens.FieldOfView -= Mathf.Min(10f, profile.dollyInFov) * ease * cw;
                }
            }

            float el = t - CameraFeelState.ShakeStart;
            if (el >= 0f && el < CameraFeelState.ShakeDuration)
            {
                float k = 1f - el / CameraFeelState.ShakeDuration;
                k *= k;
                if (reduce) k *= profile.hitShakeReduceScale;
                float x = t * 40f;
                Vector3 n = new Vector3(Mathf.PerlinNoise(x, 1.7f) - 0.5f, Mathf.PerlinNoise(x, 8.2f) - 0.5f, Mathf.PerlinNoise(x, 15.9f) - 0.5f) * 2f;
                pos += new Vector3(n.x, n.y, 0f) * profile.hitShakePosition * k;
                rot += new Vector3(n.y, n.x, n.z) * profile.hitShakeAngle * k;
            }

            state.PositionCorrection += state.GetFinalOrientation() * pos;
            state.OrientationCorrection = state.OrientationCorrection * Quaternion.Euler(rot);

            if (!reduce)
            {
                float fovCut = SlowZoom.PunchOffset(t);
                float kw = SlowZoom.KillWeight(vcam, t);
                if (kw > 0f)
                {
                    fovCut += SlowZoom.KillFovOffset * kw;
                    // Xoay nhe ve vi tri kill cuoi (gioi han goc de khong vuot gioi han xoay chong chong mat)
                    Quaternion cur = state.GetFinalOrientation();
                    Vector3 dir = SlowZoom.KillPosition - state.GetFinalPosition();
                    if (dir.sqrMagnitude > 1e-4f)
                    {
                        Quaternion want = Quaternion.LookRotation(dir, Vector3.up);
                        float ang = Quaternion.Angle(cur, want);
                        float turn = Mathf.Min(ang * SlowZoom.KillAimBlend, SlowZoom.KillMaxTurn) * kw;
                        Quaternion target = Quaternion.RotateTowards(cur, want, turn);
                        state.OrientationCorrection = state.OrientationCorrection * (Quaternion.Inverse(cur) * target);
                    }
                }
                if (fovCut > 0f) state.Lens.FieldOfView = Mathf.Max(20f, state.Lens.FieldOfView - fovCut);
            }
        }

        /// <summary>
        /// Quy doi FOV doc (dat o designAspect) khi man hep hon (man doc): noi rong FOV doc de giu goc nhin ngang
        /// theo keepHorizontalFov, toi da maxVerticalFov. Man rong hon designAspect: giu nguyen.
        /// </summary>
        public static float FitFov(float verticalFov, float aspect, CameraFeelProfile p)
        {
            if (p == null || aspect <= 0f || aspect >= p.designAspect || p.keepHorizontalFov <= 0f) return verticalFov;
            float halfV = verticalFov * 0.5f * Mathf.Deg2Rad;
            float fullV = 2f * Mathf.Atan(Mathf.Tan(halfV) * p.designAspect / aspect) * Mathf.Rad2Deg;
            float v = Mathf.Lerp(verticalFov, fullV, p.keepHorizontalFov);
            return Mathf.Max(verticalFov, Mathf.Min(v, p.maxVerticalFov));
        }
    }
}
