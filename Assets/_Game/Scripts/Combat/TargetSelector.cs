using System;
using System.Collections.Generic;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Combat
{
    public struct TargetHit
    {
        public ITapTarget Target;
        public bool Justice;
        /// <summary>Khoang cach man hinh (px) tu diem tap den AimPoint (hoac JusticePoint neu Justice).</summary>
        public float ScreenDistance;
    }

    /// <summary>
    /// Logic chon muc tieu thuan (khong phu thuoc Camera/Input) de test.
    /// Quy tac uu tien (theo plan: enemy/luu dan truoc, roi con tin, cuoi cung moi truong):
    ///  1. Nhom chinh = muc tieu IsTargetable la Enemy/Grenade co AimPoint trong ban kinh (Pickup KHONG nam trong nhom nay, F-107).
    ///     Enemy co Justice point ma tap trong justicePx cua JusticePoint la Justice.
    ///     Sap xep: Justice truoc, sau do gan nhat. Lay toi da maxHits.
    ///     HIT-BODY: enemy con duoc tinh trung neu tap nam trong bodyRect (bounds collider tren man hinh), ngoai ban kinh.
    ///  2. Nhom chinh rong -> xet Pickup (gan nhat, toi da 1): tap trung ca enemy va thung thi ban enemy.
///  3. Van rong -> xet Hostage (gan nhat trong ban kinh, toi da 1): tap trung ca enemy va con tin thi luon trung enemy.
    ///  4. Khong co gi: TapShooter raycast moi truong hoac Miss.
    /// </summary>
    public static class TargetSelector
    {
        /// <param name="project">World -> pixel; null neu diem o sau camera.</param>
        /// <returns>So hit trong results (results duoc Clear truoc).</returns>
        public static int Select(IReadOnlyList<ITapTarget> targets, Vector2 tap, float radiusPx, float justicePx,
            int maxHits, Func<Vector3, Vector2?> project, List<TargetHit> results, Func<ITapTarget, Rect?> bodyRect = null,
            Func<ITapTarget, float> depth = null)
        {
            results.Clear();
            if (targets == null || maxHits <= 0) return 0;

            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null || !t.IsTargetable || t.Kind == TargetKind.Hostage || t.Kind == TargetKind.Pickup) continue;

                bool justice = false;
                float dist = float.MaxValue;
                if (t.Kind == TargetKind.Enemy && t.HasJusticePoint)
                {
                    var jp = project(t.JusticePoint);
                    if (jp.HasValue)
                    {
                        float jd = Vector2.Distance(tap, jp.Value);
                        if (jd <= justicePx) { justice = true; dist = jd; }
                    }
                }
                if (!justice)
                {
                    var ap = project(t.AimPoint);
                    if (!ap.HasValue) continue;
                    dist = Vector2.Distance(tap, ap.Value);
                    if (dist > radiusPx)
                    {
                        // Trung than: tap nam trong hinh chu nhat man hinh cua collider enemy (HIT-BODY).
                        if (bodyRect == null || t.Kind != TargetKind.Enemy) continue;
                        var r = bodyRect(t);
                        if (!r.HasValue || !r.Value.Contains(tap)) continue;
                    }
                }
                Insert(results, new TargetHit { Target = t, Justice = justice, ScreenDistance = dist });
            }

            // Dan khong xuyen: vu khi 1 muc tieu -> chon muc tieu gan camera nhat (tru khi co Justice).
            if (depth != null && maxHits == 1 && results.Count > 1 && !results[0].Justice)
            {
                int best = 0;
                float bestDepth = depth(results[0].Target);
                for (int i = 1; i < results.Count; i++)
                {
                    float d = depth(results[i].Target);
                    if (d < bestDepth) { bestDepth = d; best = i; }
                }
                if (best != 0)
                {
                    var h = results[best];
                    results[best] = results[0];
                    results[0] = h;
                }
            }

            if (results.Count == 0) PickNearest(targets, TargetKind.Pickup, tap, radiusPx, project, results);
            if (results.Count == 0) PickNearest(targets, TargetKind.Hostage, tap, radiusPx, project, results);

            if (results.Count > maxHits) results.RemoveRange(maxHits, results.Count - maxHits);
            return results.Count;
        }

        static void PickNearest(IReadOnlyList<ITapTarget> targets, TargetKind kind, Vector2 tap, float radiusPx,
            Func<Vector3, Vector2?> project, List<TargetHit> results)
        {
            ITapTarget best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null || !t.IsTargetable || t.Kind != kind) continue;
                var ap = project(t.AimPoint);
                if (!ap.HasValue) continue;
                float d = Vector2.Distance(tap, ap.Value);
                if (d <= radiusPx && d < bestDist) { best = t; bestDist = d; }
            }
            if (best != null) results.Add(new TargetHit { Target = best, ScreenDistance = bestDist });
        }

        // Justice truoc, sau do khoang cach tang dan. Insertion sort (danh sach nho, khong cap phat them).
        static void Insert(List<TargetHit> list, TargetHit h)
        {
            int idx = list.Count;
            while (idx > 0 && Before(h, list[idx - 1])) idx--;
            list.Insert(idx, h);
        }

        static bool Before(TargetHit a, TargetHit b)
        {
            if (a.Justice != b.Justice) return a.Justice;
            return a.ScreenDistance < b.ScreenDistance;
        }
    }
}
