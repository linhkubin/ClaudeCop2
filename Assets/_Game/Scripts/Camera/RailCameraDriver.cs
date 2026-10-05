using UnityEngine;
using UnityEngine.Splines;
using Unity.Cinemachine;
using Unity.Mathematics;
using ClaudeCop.Core;

namespace ClaudeCop.Camera
{
    /// <summary>
    /// Dieu khien CinemachineCamera "Rail" (co CinemachineSplineDolly): vi tri = dolly theo khoang cach (ease-in/out hinh thang),
    /// huong = nhin truoc doc ray, gioi han xoay ngang/pitch, nghieng theo toc do xoay (tat khi Giam chuyen dong).
    /// Ghi chu: gia dinh SplineContainer co scale 1 (khoang cach dolly tinh theo do dai local cua spline).
    /// </summary>
    public class RailCameraDriver
    {
        readonly CinemachineCamera cam;
        readonly CinemachineSplineDolly dolly;
        readonly CameraFeelProfile p;

        SplineContainer sc;
        Spline spline;
        float length, v, ti, to, total, da, db, time;
        float yaw, pitch, roll, yawVel, pitchVel, rollVel;
        float dist;
        RailSpeedCurve curve;           // CAM-VC2: duong cong toc do (null = hinh thang cu)
        bool hasNext; float nextYaw, nextPitch; // CAM-VC2: nhin truoc huong shot ke
        float startYaw, startPitch;              // huong luc vao ray (SEAMLESS)
        System.Collections.Generic.List<LookKey> keys; // SEAMLESS: huong nhin theo moc (null = nhin theo tiep tuyen)
        const float KeyEnd = 0.92f;              // toi huong shot ke truoc cuoi ray de kip on dinh khi giam toc

        public bool Active { get; private set; }
        public bool Running { get; private set; }
        public bool Finished { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float Length => length;
        public RailSpeedCurve Curve => curve;
        /// <summary>Tong thoi gian doan Move du kien (s).</summary>
        public float PlannedTotal => total;
        /// <summary>Huong nhin hien tai (yaw, pitch do) - de do kiem.</summary>
        public float Yaw => yaw;
        public float Pitch => pitch;
        public float Progress => length > 0f ? dist / length : 0f;

        /// <summary>CAM-VC2: huong nhin cua shot ke (yaw/pitch do). Ray se nghieng dan toi do o lookNextStart..1 va khop dung khi toi. Goi sau Prepare, truoc StartMoving.</summary>
        public void SetNextLook(float yawDeg, float pitchDeg) { hasNext = true; nextYaw = yawDeg; nextPitch = Mathf.Clamp(pitchDeg, -p.maxPitch, p.maxPitch); }
        public void ClearNextLook() { hasNext = false; }

        /// <summary>SEAMLESS: dung moc huong nhin thay cho nhin theo tiep tuyen (rong/null = tat). Goi sau Prepare + SetNextLook, truoc StartMoving.</summary>
        float lookDampingOverride, yawRateCapOverride, lookAheadOverride;
        /// <summary>Doan Move noi Phase: camera bam tiep tuyen sat hon (damping nho, toc do xoay toi da cao). 0 = dung CameraFeelProfile. Goi sau Prepare, truoc StartMoving.</summary>
        public void SetLookTuning(float damping, float yawRateCap, float lookAhead = 0f) { lookDampingOverride = damping; yawRateCapOverride = yawRateCap; lookAheadOverride = lookAhead; }

        public void SetLookKeys(System.Collections.Generic.List<LookKey> k) { keys = k != null && k.Count > 0 ? k : null; }

        static float Smooth(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

        float KeyYaw(float prog)
        {
            float py = startYaw, pp = 0f;
            for (int i = 0; i < keys.Count; i++)
            {
                var k = keys[i];
                if (prog <= k.progress) return py + Mathf.DeltaAngle(py, k.yaw) * Smooth((prog - pp) / Mathf.Max(1e-4f, k.progress - pp));
                py = py + Mathf.DeltaAngle(py, k.yaw); pp = k.progress;
            }
            float end = hasNext ? py + Mathf.DeltaAngle(py, nextYaw) : py;
            float e = Mathf.Max(pp + 1e-3f, KeyEnd);
            return Mathf.Lerp(py, end, Smooth((prog - pp) / (e - pp)));
        }

        public RailCameraDriver(CinemachineCamera cam, CinemachineSplineDolly dolly, CameraFeelProfile profile)
        {
            this.cam = cam; this.dolly = dolly; p = profile;
        }

        /// <summary>Dat camera ray ve diem dau spline voi huong ban dau (de blend khong bi nhay). Chua di chuyen.</summary>
        public void Prepare(SplineContainer container, float speedOverride, Quaternion initialRotation)
        {
            sc = container;
            spline = sc.Splines[0];
            length = Mathf.Max(0.01f, spline.GetLength());
            v = speedOverride > 0f ? speedOverride : p.railSpeed;
            ti = p.easeInTime; to = p.easeOutTime;
            // Nhip: doan Move khong qua maxMoveSeconds (plan <= 6 s). Chi tang toc khi khong co speedOverride, tran maxRailSpeed.
            if (speedOverride <= 0f && p.maxMoveSeconds > 0f)
            {
                float denom = p.maxMoveSeconds - (ti + to) * 0.5f;
                if (denom > 0.5f && (ti + to) * 0.5f + length / v > p.maxMoveSeconds)
                    v = Mathf.Max(v, Mathf.Min(length / denom, Mathf.Max(v, p.maxRailSpeed)));
            }
            da = v * ti * 0.5f; db = v * to * 0.5f;
            if (da + db > length)
            {
                float k = length / (da + db);
                ti *= k; to *= k; da *= k; db *= k;
            }
            total = ti + to + (length - da - db) / v;
            curve = null; hasNext = false; keys = null; lookDampingOverride = 0f; yawRateCapOverride = 0f; lookAheadOverride = 0f;
            if (p.railCurveEnabled)
            {
                // Giu tong thoi gian bang hinh thang cu; duong cong chi doi phan bo toc do (nhanh ra dau, giam mem o cuoi).
                curve = new RailSpeedCurve(length, total, p.railAccelTime, p.railDecelFraction, Mathf.Max(v, p.railCurveMaxSpeed));
                total = curve.Total;
            }
            time = 0f; dist = 0f; CurrentSpeed = 0f;
            Running = false; Finished = false; Active = true;

            dolly.Spline = sc;
            dolly.PositionUnits = PathIndexUnit.Distance;
            dolly.CameraPosition = 0f;

            Vector3 e = initialRotation.eulerAngles;
            yaw = e.y; pitch = Mathf.DeltaAngle(0f, e.x); roll = 0f;
            startYaw = yaw; startPitch = pitch;
            yawVel = pitchVel = rollVel = 0f;
            cam.transform.SetPositionAndRotation(PositionAt(0f), Quaternion.Euler(pitch, yaw, 0f));
            var lens = cam.Lens; lens.FieldOfView = p.baseFov; cam.Lens = lens;
        }

        public Vector3 StartTangent() => TangentAt(0f);

        public void StartMoving() { Running = true; }
        public void Deactivate() { Active = false; Running = false; CurrentSpeed = 0f; }

        public void Tick(float dt)
        {
            if (!Active || sc == null || dt <= 0f) return;
            if (Running)
            {
                time += dt;
                if (time >= total) { time = total; dist = length; CurrentSpeed = 0f; Running = false; Finished = true; }
                else if (curve != null) { curve.Evaluate(time, out dist, out float cs); CurrentSpeed = cs; }
                else
                {
                    if (time < ti) { dist = v * time * time / (2f * ti); CurrentSpeed = v * time / ti; }
                    else if (time < total - to) { dist = da + v * (time - ti); CurrentSpeed = v; }
                    else { float s = total - time; dist = length - v * s * s / (2f * to); CurrentSpeed = v * s / to; }
                    dist = Mathf.Clamp(dist, 0f, length);
                }
                dolly.CameraPosition = dist;
            }

            // T-403: truoc khi xuat phat (dang blend vao Shot Move) giu nguyen huong ban dau, tranh cong don toc do xoay cua blend
            // Cinemachine voi viec xoay ve tiep tuyen (dinh ~90 do/s > gioi han 60). Sau khi Running moi xoay theo ray (<= maxYawRate).
            if (!Running && !Finished)
            {
                cam.transform.SetPositionAndRotation(PositionAt(dist), Quaternion.Euler(pitch, yaw, roll));
                return;
            }

            // Huong nhin: nhin truoc doc ray (qua cuoi spline thi keo dai theo tiep tuyen cuoi)
            Vector3 camPos = PositionAt(dist);
            float l = dist + (lookAheadOverride > 0f ? lookAheadOverride : p.lookAhead);
            Vector3 look = l <= length ? PositionAt(l) : PositionAt(length) + TangentAt(length) * (l - length);
            Vector3 dir = look - camPos;
            if (dir.sqrMagnitude < 1e-6f) dir = TangentAt(dist);
            float wantYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float wantPitch = Mathf.Clamp(-Mathf.Asin(Mathf.Clamp(dir.normalized.y, -1f, 1f)) * Mathf.Rad2Deg, -p.maxPitch, p.maxPitch);

            // CAM-VC2: 30% cuoi doan Move nghieng dan ve huong shot ke (smoothstep), khop dung khi toi; khong xoay muon roi giat lai.
            float damping = lookDampingOverride > 0f ? lookDampingOverride : p.lookDamping, rateCap = yawRateCapOverride > 0f ? yawRateCapOverride : p.maxYawRate;
            if (keys != null)
            {
                float prog = length > 0.01f ? dist / length : 1f;
                wantYaw = KeyYaw(prog);
                wantPitch = Mathf.Lerp(startPitch, hasNext ? nextPitch : startPitch, Smooth(prog / KeyEnd));
                damping = Mathf.Min(p.lookDamping, p.lookNextDamping);
                rateCap = Mathf.Max(1f, p.lookNextMaxYawRate);
            }
            else if (hasNext && p.lookNextWeight > 0f && length > 0.01f)
            {
                float w = Mathf.Clamp01((dist / length - p.lookNextStart) / Mathf.Max(0.01f, 1f - p.lookNextStart));
                w = w * w * (3f - 2f * w) * p.lookNextWeight;
                wantYaw = wantYaw + Mathf.DeltaAngle(wantYaw, nextYaw) * w;
                wantPitch = Mathf.Lerp(wantPitch, nextPitch, w);
                damping = Mathf.Lerp(p.lookDamping, Mathf.Min(p.lookDamping, p.lookNextDamping), w);
                rateCap = Mathf.Lerp(p.maxYawRate, Mathf.Max(p.maxYawRate, p.lookNextMaxYawRate), w);
            }

            float oldYaw = yaw;
            float smoothed = Mathf.SmoothDampAngle(yaw, wantYaw, ref yawVel, damping, rateCap, dt);
            // SmoothDamp co the vuot maxSpeed nhe -> ep cung theo gioi han do/giay
            yaw += Mathf.Clamp(Mathf.DeltaAngle(yaw, smoothed), -rateCap * dt, rateCap * dt);
            pitch = Mathf.SmoothDamp(pitch, wantPitch, ref pitchVel, damping, rateCap, dt);
            float yawRate = Mathf.DeltaAngle(oldYaw, yaw) / dt;
            float wantRoll = UserSettings.ReduceMotion ? 0f : -Mathf.Clamp(yawRate / Mathf.Max(1f, p.maxYawRate), -1f, 1f) * p.maxRoll;
            roll = Mathf.SmoothDamp(roll, wantRoll, ref rollVel, p.rollSmoothTime, Mathf.Infinity, dt);
            roll = Mathf.Clamp(roll, -p.maxRoll, p.maxRoll);

            cam.transform.SetPositionAndRotation(camPos, Quaternion.Euler(pitch, yaw, roll));
        }

        Vector3 PositionAt(float d)
        {
            float t = SplineUtility.GetNormalizedInterpolation(spline, d, PathIndexUnit.Distance);
            sc.Evaluate(0, t, out float3 pos, out float3 _, out float3 _);
            return pos;
        }

        Vector3 TangentAt(float d)
        {
            float t = SplineUtility.GetNormalizedInterpolation(spline, d, PathIndexUnit.Distance);
            sc.Evaluate(0, t, out float3 _, out float3 tan, out float3 _);
            Vector3 r = tan;
            return r.sqrMagnitude > 1e-8f ? r.normalized : Vector3.forward;
        }
    }
}
