using System;
using UnityEngine;

namespace ClaudeCop.Viewmodel
{
    /// <summary>Thong so chuyen dong thu cap cua sung (giat, nghieng theo diem tap). Dung chung cho ViewmodelConfig va test.</summary>
    [Serializable]
    public sealed class ViewmodelMotionSettings
    {
        [Header("Giat (spring)")]
        [Tooltip("Do cung lo xo giat (cang cao cang nhanh ve cho).")] [Min(1f)] public float recoilStiffness = 260f;
        [Tooltip("Can lo xo (~2*sqrt(stiffness) = tat dan toi han).")] [Min(0f)] public float recoilDamping = 26f;
        [Tooltip("Van toc cong vao lo xo moi phat (don vi giat/giay).")] [Min(0f)] public float recoilImpulse = 9f;
        [Tooltip("Gioi han do giat (1 = muc giat chuan). Chong cong don vo han khi ban lien thanh.")] [Min(0.1f)] public float recoilMax = 1.6f;
        [Tooltip("Do giat lui (m, local) o muc 1.")] public Vector3 recoilPosition = new Vector3(0f, 0.012f, -0.05f);
        [Tooltip("Do hat mui sung (do) o muc 1; x am = ngang len.")] public Vector3 recoilEuler = new Vector3(-6f, 0f, 1.5f);

        [Header("Nghieng theo diem tap")]
        [Tooltip("Goc xoay toi da quanh Y (do) khi tap sat mep trai/phai.")] [Min(0f)] public float tiltMaxYaw = 7f;
        [Tooltip("Goc xoay toi da quanh X (do) khi tap sat mep tren/duoi.")] [Min(0f)] public float tiltMaxPitch = 5f;
        [Tooltip("Goc lac (roll) toi da (do).")] [Min(0f)] public float tiltMaxRoll = 3f;
        [Tooltip("Thoi gian lam muot (giay) toi muc tieu nghieng.")] [Min(0.01f)] public float tiltSmoothTime = 0.08f;
        [Tooltip("Sau bao lau khong ban thi sung tu ve giua (giay).")] [Min(0f)] public float tiltHoldTime = 0.6f;
        [Tooltip("Thoi gian lam muot khi ve giua (giay).")] [Min(0.01f)] public float tiltReturnTime = 0.25f;

        [Header("Huong theo di chuyen (CAM-VC2)")]
        [Tooltip("Mui sung lech nguoc huong re: do lech yaw (do) tren moi do/giay toc do xoay cua camera (quan tinh).")] [Min(0f)] public float moveYawGain = 0.10f;
        [Tooltip("Do lech yaw toi da do re (do).")] [Min(0f)] public float moveMaxYaw = 3.5f;
        [Tooltip("Nghieng (roll) vao trong khi re: do tren moi do/giay toc do xoay.")] [Min(0f)] public float moveRollGain = 0.05f;
        [Tooltip("Roll toi da do re (do).")] [Min(0f)] public float moveMaxRoll = 2.5f;
        [Tooltip("Hat mui sung xuong theo toc do tien (do) o toc do moveSpeedRef.")] public float moveSpeedPitch = 1.2f;
        [Tooltip("Toc do tien (m/s) ung voi moveSpeedPitch.")] [Min(0.1f)] public float moveSpeedRef = 4f;
        [Tooltip("Gioi han toc do xoay dau vao (do/giay) - chong spike khi Cut/blend.")] [Min(1f)] public float moveMaxYawRateInput = 90f;
        [Tooltip("Thoi gian lam muot (giay).")] [Min(0.01f)] public float moveSmoothTime = 0.18f;

        [Header("Giam chuyen dong")]
        [Tooltip("He so nhan bien do giat/nghieng khi UserSettings.ReduceMotion.")] [Range(0f, 1f)] public float reduceMotionScale = 0.35f;
    }

    /// <summary>
    /// Chuyen dong thu cap cua viewmodel: lo xo giat cong don + nghieng theo diem tap. Lop thuan (khong phu thuoc scene) de test EditMode.
    /// </summary>
    public sealed class ViewmodelMotion
    {
        readonly ViewmodelMotionSettings s;
        float recoil;
        float recoilVel;
        Vector2 tiltTarget;
        Vector2 tilt;
        Vector2 tiltVel;
        float sinceShot = 999f;
        Vector3 moveEuler, moveEulerVel; // (pitch x, yaw y, roll z) do di chuyen

        public ViewmodelMotion(ViewmodelMotionSettings settings) { s = settings ?? new ViewmodelMotionSettings(); }

        public float Recoil => recoil;
        public Vector2 TiltNormalized => tilt;

        public void Reset() { recoil = recoilVel = 0f; tilt = tiltTarget = tiltVel = Vector2.zero; sinceShot = 999f; moveEuler = moveEulerVel = Vector3.zero; }

        /// <summary>Chuan hoa diem tap ve -1..1 (tam man hinh = 0), kep trong khoang.</summary>
        public static Vector2 NormalizeScreen(Vector2 screenPos, float width, float height)
        {
            if (width <= 1f || height <= 1f) return Vector2.zero;
            float nx = Mathf.Clamp(screenPos.x / width * 2f - 1f, -1f, 1f);
            float ny = Mathf.Clamp(screenPos.y / height * 2f - 1f, -1f, 1f);
            return new Vector2(nx, ny);
        }

        /// <summary>Goi khi co phat ban. impulseScale: 1 = chuan.</summary>
        public void OnShot(Vector2 screenPos, float width, float height, float impulseScale = 1f)
        {
            recoilVel += s.recoilImpulse * Mathf.Max(0f, impulseScale);
            tiltTarget = NormalizeScreen(screenPos, width, height);
            sinceShot = 0f;
        }

        public void Tick(float dt)
        {
            if (dt <= 0f) return;
            dt = Mathf.Min(dt, 0.05f); // chong no khi hitch
            recoilVel += (-s.recoilStiffness * recoil - s.recoilDamping * recoilVel) * dt;
            recoil += recoilVel * dt;
            if (recoil > s.recoilMax) { recoil = s.recoilMax; if (recoilVel > 0f) recoilVel = 0f; }
            float lo = -0.3f * s.recoilMax;
            if (recoil < lo) { recoil = lo; if (recoilVel < 0f) recoilVel = 0f; }

            sinceShot += dt;
            bool holding = sinceShot <= s.tiltHoldTime;
            Vector2 target = holding ? tiltTarget : Vector2.zero;
            float t = holding ? s.tiltSmoothTime : s.tiltReturnTime;
            tilt = Vector2.SmoothDamp(tilt, target, ref tiltVel, t, Mathf.Infinity, dt);
            tilt.x = Mathf.Clamp(tilt.x, -1f, 1f);
            tilt.y = Mathf.Clamp(tilt.y, -1f, 1f);
        }

        /// <summary>Euler (do) huong theo di chuyen hien tai (pitch, yaw, roll) - chua nhan amplitude.</summary>
        public Vector3 MoveEuler => moveEuler;

        /// <summary>
        /// Muc tieu nghieng theo di chuyen (thuan): yawRate = toc do xoay yaw cua camera (do/giay, phai +), speed = toc do tien (m/s).
        /// Vao cua: mui sung tre nguoc huong re (quan tinh, "lech ra ngoai") va nghieng nhe vao trong; tien nhanh: hat mui xuong chut.
        /// </summary>
        public static Vector3 MoveTarget(ViewmodelMotionSettings s, float yawRate, float speed)
        {
            float r = Mathf.Clamp(yawRate, -s.moveMaxYawRateInput, s.moveMaxYawRateInput);
            float yaw = Mathf.Clamp(-r * s.moveYawGain, -s.moveMaxYaw, s.moveMaxYaw);
            float roll = Mathf.Clamp(-r * s.moveRollGain, -s.moveMaxRoll, s.moveMaxRoll);
            float pitch = s.moveSpeedPitch * Mathf.Clamp01(speed / Mathf.Max(0.1f, s.moveSpeedRef));
            return new Vector3(pitch, yaw, roll);
        }

        /// <summary>Tien nghieng theo di chuyen (goi moi frame voi toc do xoay/tien cua camera).</summary>
        public void TickMove(float dt, float yawRate, float speed)
        {
            if (dt <= 0f) return;
            dt = Mathf.Min(dt, 0.05f);
            moveEuler = Vector3.SmoothDamp(moveEuler, MoveTarget(s, yawRate, speed), ref moveEulerVel, s.moveSmoothTime, Mathf.Infinity, dt);
        }

        /// <summary>Do dich vi tri local do giat. amplitude = 1 binh thuong, reduceMotionScale khi giam chuyen dong.</summary>
        public Vector3 PositionOffset(float amplitude = 1f) { return s.recoilPosition * (recoil * amplitude); }

        /// <summary>Goc xoay local (euler, do): giat + nghieng theo diem tap. Tap ben phai -> mui sung quay sang phai.</summary>
        public Vector3 EulerOffset(float amplitude = 1f)
        {
            Vector3 e = s.recoilEuler * recoil;
            e.y += tilt.x * s.tiltMaxYaw;
            e.x += -tilt.y * s.tiltMaxPitch;
            e.z += -tilt.x * s.tiltMaxRoll;
            e += moveEuler;
            return e * amplitude;
        }

        /// <summary>Vi tri local de dat vat o toa do viewport (0..1) o do sau depth truoc camera co fov doc (do) va aspect (w/h).</summary>
        public static Vector3 AnchorLocalPosition(Vector2 viewport, float depth, float verticalFovDeg, float aspect)
        {
            float halfH = depth * Mathf.Tan(verticalFovDeg * 0.5f * Mathf.Deg2Rad);
            float halfW = halfH * aspect;
            return new Vector3((viewport.x * 2f - 1f) * halfW, (viewport.y * 2f - 1f) * halfH, depth);
        }
    }

    /// <summary>
    /// CAM-VC2: trang thai ngam cua sung (thuan). Mot phat dat delta xoay (so voi tu the nghi) -> giu aimHold giay -> tra ve nghi trong aimReturn giay (smoothstep).
    /// Snap vao tu the ngam la tuc thoi (de vet dan va nong khop dung frame ban).
    /// </summary>
    public sealed class ViewmodelAimState
    {
        Quaternion delta = Quaternion.identity;
        float age = 999f;

        public Quaternion Delta => delta;
        public float Age => age;

        public void Set(Quaternion d) { delta = d; age = 0f; }
        public void Clear() { delta = Quaternion.identity; age = 999f; }
        public void Tick(float dt) { if (dt > 0f) age = Mathf.Min(age + dt, 999f); }

        public static float Weight(float age, float hold, float ret)
        {
            if (age <= hold) return 1f;
            float k = Mathf.Clamp01((age - hold) / Mathf.Max(0.01f, ret));
            return 1f - k * k * (3f - 2f * k);
        }

        public Quaternion Current(float hold, float ret) { return Quaternion.Slerp(Quaternion.identity, delta, Weight(age, hold, ret)); }

        /// <summary>Kep goc xoay toi da maxDeg (giu truc).</summary>
        public static Quaternion ClampAngle(Quaternion q, float maxDeg)
        {
            float ang = Quaternion.Angle(Quaternion.identity, q);
            return ang <= maxDeg || ang < 1e-4f ? q : Quaternion.RotateTowards(Quaternion.identity, q, maxDeg);
        }
    }
}
