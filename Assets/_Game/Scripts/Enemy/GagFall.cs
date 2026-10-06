using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy
{
    /// <summary>
    /// Canh hai H01 (Level 3): tay sung nhay tu gac thap, vuong lan can, chui mat nga xuong san va nam bat dong, sung vang ra.
    /// KHONG phai muc tieu (khong collider, khong dang ky TargetRegistry), khong tinh kill, khong ban. EncounterWave.Begin goi Play().
    /// Builder dat: body (pivot o chan, mat huong +Z local), gun, cac diem Hop (dinh lan can), Land (chan khi nam), GunLand.
    /// An (renderer tat) cho toi khi Play. Dung khi CombatPauseSignal.IsPaused.
    /// </summary>
    public sealed class GagFall : MonoBehaviour
    {
        [SerializeField] Transform body;
        [SerializeField] Transform gun;
        [SerializeField] Transform hop;
        [SerializeField] Transform land;
        [SerializeField] Transform gunLand;
        [SerializeField, Min(0f)] float delay = 0.6f;
        [SerializeField, Min(0.05f)] float hopTime = 0.35f;
        [SerializeField, Min(0.05f)] float fallTime = 0.5f;

        Vector3 bodyStart, gunStart;
        Quaternion bodyRot, gunRot;
        float t = -1f;
        bool done;

        public bool IsPlaying => t >= 0f && !done;
        public bool IsDone => done;

        void Awake()
        {
            if (body != null) { bodyStart = body.position; bodyRot = body.rotation; }
            if (gun != null) { gunStart = gun.position; gunRot = gun.rotation; }
            SetVisible(false);
        }

        /// <summary>Bat dau canh hai (mot lan).</summary>
        public void Play()
        {
            if (t >= 0f) return;
            t = 0f;
            SetVisible(true);
        }

        void SetVisible(bool on)
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = on;
        }

        void Update()
        {
            if (t < 0f || done || CombatPauseSignal.IsPaused) return;
            t += Time.deltaTime;
            Step(t);
        }

        /// <summary>Dat tu the tai thoi diem time (s) tinh tu Play (test goi truc tiep).</summary>
        public void Step(float time)
        {
            if (body == null) return;
            float a = time - delay;
            Vector3 hopPos = hop != null ? hop.position : bodyStart + Vector3.up * 0.6f;
            Vector3 landPos = land != null ? land.position : bodyStart;
            Quaternion lying = bodyRot * Quaternion.Euler(90f, 0f, 0f);   // chui mat ve phia truoc (+Z local)
            if (a <= 0f) { body.SetPositionAndRotation(bodyStart, bodyRot); return; }
            if (a < hopTime)
            {
                float k = a / hopTime;
                Vector3 p = Vector3.Lerp(bodyStart, hopPos, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.25f;
                body.SetPositionAndRotation(p, Quaternion.Slerp(bodyRot, bodyRot * Quaternion.Euler(25f, 0f, 0f), k));   // chan vuong lan can: nghieng ve truoc
                if (gun != null) gun.SetPositionAndRotation(gunStart + (p - bodyStart), gunRot);
                return;
            }
            float f = Mathf.Clamp01((a - hopTime) / fallTime);
            body.SetPositionAndRotation(Vector3.Lerp(hopPos, landPos, f * f),
                Quaternion.Slerp(bodyRot * Quaternion.Euler(25f, 0f, 0f), lying, Mathf.Sqrt(f)));
            if (gun != null && gunLand != null)
            {
                float g = Mathf.Clamp01((a - hopTime) / (fallTime * 1.3f));
                Vector3 from = gunStart + (hopPos - bodyStart);
                gun.SetPositionAndRotation(Vector3.Lerp(from, gunLand.position, g) + Vector3.up * Mathf.Sin(g * Mathf.PI) * 0.8f,
                    gunRot * Quaternion.Euler(0f, 0f, 540f * g));
            }
            if (f >= 1f && (gun == null || a - hopTime >= fallTime * 1.3f)) done = true;   // nam bat dong
        }
    }
}
