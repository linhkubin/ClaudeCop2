using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>Viewmodel dang ky: xoay sung de nong chi ve targetWorld (diem trung that), tra ve diem xuat phat vet dan (the gioi). false = khong ngam duoc.</summary>
    public delegate bool MuzzleAimHandler(Vector3 targetWorld, out Vector3 originWorld);

    /// <summary>
    /// Hop dong dau nong sung. Viewmodel goi Set(socket) khi gan, Clear() khi go.
    /// FX (tracer, muzzle) doc TryGet; khong co thi tu fallback.
    /// CAM-VC2: FX goi TryAimAt(diemTrung) truoc khi sinh vet dan -> viewmodel xoay sung TRONG CUNG LUC GOI (khong phu thuoc thu tu
    /// ShotFired/ShotResolved) va tra diem xuat phat; vet dan di tu nong toi dung diem trung, nong chi dung vet.
    /// </summary>
    public static class MuzzleAnchor
    {
        static Transform anchor;
        static MuzzleAimHandler aimHandler;

        public static void Set(Transform t) { anchor = t; }

        /// <summary>Chi Clear neu anchor hien tai la t (tranh xoa nham); t=null xoa vo dieu kien.</summary>
        public static void Clear(Transform t = null)
        {
            if (t == null || anchor == t) anchor = null;
        }

        public static bool TryGet(out Vector3 worldPos)
        {
            if (anchor != null) { worldPos = anchor.position; return true; }
            worldPos = default;
            return false;
        }

        public static void SetAimHandler(MuzzleAimHandler h) { aimHandler = h; }
        public static void ClearAimHandler(MuzzleAimHandler h) { if (aimHandler == h) aimHandler = null; }

        /// <summary>Ngam nong ve targetWorld (neu co viewmodel), tra diem xuat phat vet dan. Khong co handler: false (FX tu fallback).</summary>
        public static bool TryAimAt(Vector3 targetWorld, out Vector3 originWorld)
        {
            if (aimHandler != null && aimHandler(targetWorld, out originWorld)) return true;
            originWorld = default;
            return false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { anchor = null; aimHandler = null; }
    }
}
