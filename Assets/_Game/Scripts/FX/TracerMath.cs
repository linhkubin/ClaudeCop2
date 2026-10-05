using UnityEngine;

namespace ClaudeCop.FX
{
    /// <summary>Logic thuan cua tracer (de test EditMode).</summary>
    public static class TracerMath
    {
        /// <summary>Diem dich: co trung thi dung hitPoint, khong thi diem xa theo huong tia.</summary>
        public static Vector3 ComputeEnd(Vector3 rayOrigin, Vector3 rayDir, bool hasHit, Vector3 hitPoint, float missDistance)
        {
            if (hasHit) return hitPoint;
            if (rayDir.sqrMagnitude < 1e-8f) rayDir = Vector3.forward;
            return rayOrigin + rayDir.normalized * missDistance;
        }

        /// <summary>Lech huong trong non goc spreadDeg. u1,u2 in [0,1).</summary>
        public static Vector3 SpreadDirection(Vector3 dir, float spreadDeg, float u1, float u2)
        {
            if (dir.sqrMagnitude < 1e-8f) return Vector3.forward;
            dir.Normalize();
            if (spreadDeg <= 0f) return dir;
            float ang = spreadDeg * Mathf.Sqrt(Mathf.Clamp01(u1));
            float roll = u2 * 360f;
            var perp = Vector3.Cross(dir, Mathf.Abs(dir.y) > 0.99f ? Vector3.right : Vector3.up).normalized;
            perp = Quaternion.AngleAxis(roll, dir) * perp;
            return (Quaternion.AngleAxis(ang, perp) * dir).normalized;
        }

        /// <summary>Diem dich cua vet tan: lech tu diem dich goc theo huong tu origin, giu nguyen khoang cach.</summary>
        public static Vector3 SpreadEnd(Vector3 origin, Vector3 end, float spreadDeg, float u1, float u2)
        {
            var d = end - origin;
            float len = d.magnitude;
            if (len < 1e-4f) return end;
            return origin + SpreadDirection(d / len, spreadDeg, u1, u2) * len;
        }

        /// <summary>Do day theo khoang cach toi camera: xa day hon de van thay. Gioi han maxWidth.</summary>
        public static float WidthAtDistance(float baseWidth, float perMeter, float distance, float maxWidth)
        {
            float w = baseWidth + perMeter * Mathf.Max(0f, distance);
            return Mathf.Min(w, Mathf.Max(baseWidth, maxWidth));
        }

        /// <summary>Dau/duoi theo thoi gian t (s) ke tu luc ban: dau bay tu 0 den 1 trong travelTime, duoi theo sau tailLag giay.</summary>
        public static void HeadTail(float t, float travelTime, float tailLag, out float head, out float tail)
        {
            travelTime = Mathf.Max(1e-4f, travelTime);
            head = Mathf.Clamp01(t / travelTime);
            tail = Mathf.Clamp01((t - tailLag) / travelTime);
        }

        /// <summary>Thoi gian bay = quang duong / toc do (toi thieu minTravel).</summary>
        public static float TravelTime(float distance, float speed, float minTravel)
        {
            return Mathf.Max(minTravel, distance / Mathf.Max(0.01f, speed));
        }
    }
}
