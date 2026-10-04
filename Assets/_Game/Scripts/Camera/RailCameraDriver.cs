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

        public bool Active { get; private set; }
        public bool Running { get; private set; }
        public bool Finished { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float Length => length;

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
            time = 0f; dist = 0f; CurrentSpeed = 0f;
            Running = false; Finished = false; Active = true;

            dolly.Spline = sc;
            dolly.PositionUnits = PathIndexUnit.Distance;
            dolly.CameraPosition = 0f;

            Vector3 e = initialRotation.eulerAngles;
            yaw = e.y; pitch = Mathf.DeltaAngle(0f, e.x); roll = 0f;
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
            float l = dist + p.lookAhead;
            Vector3 look = l <= length ? PositionAt(l) : PositionAt(length) + TangentAt(length) * (l - length);
            Vector3 dir = look - camPos;
            if (dir.sqrMagnitude < 1e-6f) dir = TangentAt(dist);
            float wantYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float wantPitch = Mathf.Clamp(-Mathf.Asin(Mathf.Clamp(dir.normalized.y, -1f, 1f)) * Mathf.Rad2Deg, -p.maxPitch, p.maxPitch);

            float oldYaw = yaw;
            float smoothed = Mathf.SmoothDampAngle(yaw, wantYaw, ref yawVel, p.lookDamping, p.maxYawRate, dt);
            // SmoothDamp co the vuot maxSpeed nhe -> ep cung theo gioi han do/giay
            yaw += Mathf.Clamp(Mathf.DeltaAngle(yaw, smoothed), -p.maxYawRate * dt, p.maxYawRate * dt);
            pitch = Mathf.SmoothDamp(pitch, wantPitch, ref pitchVel, p.lookDamping, p.maxYawRate, dt);
            float yawRate = Mathf.DeltaAngle(oldYaw, yaw) / dt;
            float wantRoll = UserSettings.ReduceMotion ? 0f : -Mathf.Clamp(yawRate / p.maxYawRate, -1f, 1f) * p.maxRoll;
            roll = Mathf.SmoothDamp(roll, wantRoll, ref rollVel, 0.3f, Mathf.Infinity, dt);
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
