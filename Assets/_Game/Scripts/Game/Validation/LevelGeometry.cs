using System.Collections.Generic;
using UnityEngine;

namespace ClaudeCop.Game.Validation
{
    /// <summary>Logic thuan (khong phu thuoc scene) cho LevelValidator: goc, khoang cach, cat cheo doan thang 2D, yaw tich luy.</summary>
    public static class LevelGeometry
    {
        /// <summary>Lech ngang (do, co dau, + = ben phai) cua target so voi huong nhin phang cua camera.</summary>
        public static float HorizontalOffset(Vector3 camPos, Vector3 camForward, Vector3 target)
        {
            Vector3 flat = Vector3.ProjectOnPlane(camForward, Vector3.up);
            if (flat.sqrMagnitude < 1e-6f) return 0f;
            flat.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, flat);
            Vector3 d = target - camPos;
            return Mathf.Atan2(Vector3.Dot(d, right), Vector3.Dot(d, flat)) * Mathf.Rad2Deg;
        }

        /// <summary>Do rong cum (max - min) va lech tuyet doi lon nhat (do) cua cac target so voi huong nhin.</summary>
        public static void ClusterSpread(Vector3 camPos, Vector3 camForward, IList<Vector3> targets, out float width, out float maxAbs)
        {
            float mn = float.MaxValue, mx = float.MinValue; maxAbs = 0f;
            for (int i = 0; i < targets.Count; i++)
            {
                float h = HorizontalOffset(camPos, camForward, targets[i]);
                mn = Mathf.Min(mn, h); mx = Mathf.Max(mx, h); maxAbs = Mathf.Max(maxAbs, Mathf.Abs(h));
            }
            width = targets.Count == 0 ? 0f : mx - mn;
        }

        /// <summary>FOV doc nho nhat (do) de moi target vao khung o ti le aspect (rong/cao) voi le margin; 180 neu co diem sau camera. Cung cong thuc CameraShot.RequiredFov.</summary>
        public static float RequiredVerticalFov(Vector3 pos, Quaternion rot, IList<Vector3> targets, float aspect, float margin)
        {
            var inv = Quaternion.Inverse(rot);
            float hMax = 0f, vMax = 0f;
            for (int i = 0; i < targets.Count; i++)
            {
                Vector3 d = inv * (targets[i] - pos);
                if (d.z < 0.1f) return 180f;
                hMax = Mathf.Max(hMax, Mathf.Abs(Mathf.Atan2(d.x, d.z)) * Mathf.Rad2Deg);
                vMax = Mathf.Max(vMax, Mathf.Abs(Mathf.Atan2(d.y, d.z)) * Mathf.Rad2Deg);
            }
            float halfH = Mathf.Min(89f, hMax + margin) * Mathf.Deg2Rad;
            float fromH = 2f * Mathf.Atan(Mathf.Tan(halfH) / aspect) * Mathf.Rad2Deg;
            float fromV = 2f * Mathf.Min(89f, vMax + margin);
            return Mathf.Max(fromH, fromV);
        }

        /// <summary>Goc 3D (do) giua hai huong.</summary>
        public static float AngleBetween(Vector3 a, Vector3 b) => Vector3.Angle(a, b);

        /// <summary>Yaw (do, 0 = +Z, tang theo chieu kim dong ho nhin tu tren) cua huong v.</summary>
        public static float Yaw(Vector3 v) => Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;

        /// <summary>Thoi gian (s) mot doan Move, cung cong thuc RailCameraDriver (tang toc toi da maxRailSpeed de giu maxMoveSeconds) + dwell.</summary>
        public static float MoveSeconds(float length, float railSpeed, float speedOverride, float easeIn, float easeOut, float maxMoveSeconds, float maxRailSpeed, float dwell)
        {
            length = Mathf.Max(0.01f, length);
            float v = speedOverride > 0f ? speedOverride : railSpeed;
            float ease = (easeIn + easeOut) * 0.5f;
            if (speedOverride <= 0f && maxMoveSeconds > 0f)
            {
                float denom = maxMoveSeconds - ease;
                if (denom > 0.5f && ease + length / v > maxMoveSeconds)
                    v = Mathf.Max(v, Mathf.Min(length / denom, Mathf.Max(v, maxRailSpeed)));
            }
            return ease + length / Mathf.Max(0.01f, v) + Mathf.Max(0f, dwell);
        }

        /// <summary>Hai doan thang 2D (p1-p2) va (p3-p4) cat nhau that su (khong tinh cham dau mut).</summary>
        public static bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float d1 = Cross(p4 - p3, p1 - p3), d2 = Cross(p4 - p3, p2 - p3);
            float d3 = Cross(p2 - p1, p3 - p1), d4 = Cross(p2 - p1, p4 - p1);
            return ((d1 > 1e-6f && d2 < -1e-6f) || (d1 < -1e-6f && d2 > 1e-6f))
                && ((d3 > 1e-6f && d4 < -1e-6f) || (d3 < -1e-6f && d4 > 1e-6f));
        }
        static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        /// <summary>Bo cac diem lien tiep gan nhau hon minSeg (tranh doan thang suy bien lam cat cheo gia).</summary>
        public static List<Vector2> Simplify(IList<Vector2> pts, float minSeg)
        {
            var r = new List<Vector2>();
            for (int i = 0; i < pts.Count; i++)
                if (r.Count == 0 || (pts[i] - r[r.Count - 1]).magnitude >= minSeg) r.Add(pts[i]);
            return r;
        }

        /// <summary>Tim cap doan thang khong ke nhau cat nhau trong duong gap khuc. True neu tu cat cheo (quay vong lai).</summary>
        public static bool FindSelfIntersection(IList<Vector2> poly, out int segA, out int segB)
        {
            segA = segB = -1;
            for (int i = 0; i < poly.Count - 1; i++)
                for (int j = i + 2; j < poly.Count - 1; j++)
                    if (SegmentsIntersect(poly[i], poly[i + 1], poly[j], poly[j + 1])) { segA = i; segB = j; return true; }
            return false;
        }

        /// <summary>Yaw rong tich luy (tong co dau cac chenh lech yaw, do) cua day yaw.</summary>
        public static float NetYaw(IList<float> yaws)
        {
            float s = 0f;
            for (int i = 1; i < yaws.Count; i++) s += Mathf.DeltaAngle(yaws[i - 1], yaws[i]);
            return s;
        }

        /// <summary>Tong tuyet doi cac chenh lech yaw (do).</summary>
        public static float AbsYaw(IList<float> yaws)
        {
            float s = 0f;
            for (int i = 1; i < yaws.Count; i++) s += Mathf.Abs(Mathf.DeltaAngle(yaws[i - 1], yaws[i]));
            return s;
        }

        /// <summary>So lan dao chieu re: gom chenh lech cung dau thanh "doan re", chi tinh doan >= minRunDeg, dem so lan doi dau giua cac doan.</summary>
        public static int CountTurnReversals(IList<float> yaws, float minRunDeg)
        {
            var runs = new List<float>();
            float run = 0f; int runSign = 0;
            for (int i = 1; i < yaws.Count; i++)
            {
                float d = Mathf.DeltaAngle(yaws[i - 1], yaws[i]);
                int sg = Mathf.Abs(d) < 0.5f ? 0 : (d > 0f ? 1 : -1);
                if (sg == 0) continue;
                if (runSign == 0 || sg == runSign) { runSign = sg; run += d; }
                else { runs.Add(run); runSign = sg; run = d; }
            }
            if (runSign != 0) runs.Add(run);
            int rev = 0, last = 0;
            foreach (var r in runs)
            {
                if (Mathf.Abs(r) < minRunDeg) continue;
                int s = r > 0f ? 1 : -1;
                if (last != 0 && s != last) rev++;
                last = s;
            }
            return rev;
        }
    }
}
