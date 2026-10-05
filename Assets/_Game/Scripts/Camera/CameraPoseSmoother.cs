using UnityEngine;
using Unity.Cinemachine;
using ClaudeCop.Core;

namespace ClaudeCop.Camera
{
    /// <summary>
    /// CAM-SMOOTH: luoi an toan CUOI pipeline. Chay trong CinemachineCore.CameraUpdatedEvent (ngay sau khi Brain ghi pose vao
    /// transform/FOV cua Main Camera, truoc moi LateUpdate khac) nen reticle/tap thay dung pose da lam muot.
    /// Camera that di theo pose "tho" cua Brain nhung KHONG bao gio vuot gioi han van toc + gia toc (xoay, vi tri, FOV),
    /// va tu phanh truoc khi toi dich (khong vot lo). Khi pose tho thay doi trong gioi han thi chi tre vai do (v^2 / 2a) - khong thay.
    /// Rung trung dan/no (CameraFeelState.Shake*) duoc tach ra truoc va cong lai sau de khong bi loc mat.
    /// </summary>
    public sealed class CameraPoseSmoother
    {
        readonly CameraFeelProfile p;
        readonly CinemachineBrain brain;
        readonly UnityEngine.Camera outCam;

        struct S { public Vector3 pos, vel, angVel; public Quaternion rot; public float fov, fovVel; public Vector3 rA, rAV; public float rF, rFV; public float kP, kPV, kF, kFV; } // rA = reaction (yaw, pitch), rF = reaction FOV

        bool has, snap;
        ICinemachineCamera snapExpect;
        S committed, cand;        // committed = trang thai cuoi frame truoc; cand = ket qua frame nay
        int candFrame = -1;
        float lastAngSpeed, lastSpeed;

        // Do kiem (debug / bao cao): gia tri lon nhat cua pose DA lam muot va do tre toi da so voi pose tho.
        public float MaxAngVel, MaxAngAcc, MaxVel, MaxAcc, MaxFovRate, MaxLagAngle, MaxLagPos;
        public float MaxKickPitch, MaxKickFov, MaxKickAngVel;
        public float MaxReactAngVel, MaxReactAngAcc, MaxReactFovRate, MaxReactYaw; public int Calls, MaxCallsPerFrame; int callFrame, callsThisFrame;

        public CameraPoseSmoother(CinemachineBrain brain, CameraFeelProfile profile)
        {
            this.brain = brain; p = profile;
            outCam = brain != null ? brain.GetComponent<UnityEngine.Camera>() : null;
        }

        public void Enable() { CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated); CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated); }
        public void Disable() { CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated); }

        /// <summary>Lan cap pose ke tiep (khi camera expectActive da live) copy thang pose tho: chi dung cho Cut duoc phep (vao level, luc man den giua Phase).</summary>
        public void RequestSnap(ICinemachineCamera expectActive = null) { snap = true; snapExpect = expectActive; }

        public void ResetStats() { MaxAngVel = MaxAngAcc = MaxVel = MaxAcc = MaxFovRate = MaxLagAngle = MaxLagPos = MaxReactAngVel = MaxReactAngAcc = MaxReactFovRate = MaxReactYaw = 0f; MaxKickPitch = MaxKickFov = MaxKickAngVel = 0f; }

        /// <summary>
        /// Brain co the phat CameraUpdatedEvent nhieu lan/frame (do duoc 2 lan): moi lan tinh lai tu trang thai CUOI FRAME TRUOC (committed)
        /// nen thoi gian chi tien 1 lan/frame. Pose tho lay tu Brain.State (khong doc transform vi co the dang chua pose da lam muot).
        /// </summary>
        void OnCameraUpdated(CinemachineBrain b)
        {
            if (b != brain || p == null || !p.smootherEnabled || !Application.isPlaying) return;
            Calls++; if (callFrame != Time.frameCount) { callFrame = Time.frameCount; callsThisFrame = 0; } callsThisFrame++; MaxCallsPerFrame = Mathf.Max(MaxCallsPerFrame, callsThisFrame);

            var st = b.State;
            Quaternion shakeQ = CameraFeelState.ShakeRot;
            Vector3 shakeP = CameraFeelState.ShakePos;
            Quaternion rawRot = st.GetFinalOrientation() * Quaternion.Inverse(shakeQ);
            Vector3 rawPos = st.GetFinalPosition() - rawRot * shakeP;
            float rawFov = st.Lens.FieldOfView;

            if (candFrame != Time.frameCount) { if (has) committed = cand; candFrame = Time.frameCount; }
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            bool doSnap = snap && (snapExpect == null || ReferenceEquals(b.ActiveVirtualCamera, snapExpect));
            if (!has || doSnap)
            {
                has = true; snap = false; snapExpect = null;
                committed = new S { pos = rawPos, rot = rawRot, fov = rawFov };
                cand = committed;
                lastAngSpeed = lastSpeed = 0f;
            }
            else if (dt > 1e-5f && Time.timeScale > 0f)
            {
                cand = committed;
                float sc = UserSettings.ReduceMotion ? p.smoothReduceMotionScale : 1f;

                // Vi tri
                cand.pos += Step(rawPos - cand.pos, ref cand.vel, p.smoothMaxVel * sc, p.smoothMaxAcc * sc, dt);

                // Xoay: sai so la vector xoay (do) trong khong gian world
                Quaternion err = rawRot * Quaternion.Inverse(cand.rot);
                err.ToAngleAxis(out float ang, out Vector3 axis);
                if (ang > 180f) ang -= 360f;
                if (float.IsNaN(axis.x) || axis.sqrMagnitude < 1e-8f) { axis = Vector3.up; ang = 0f; }
                Vector3 dr = Step(axis * ang, ref cand.angVel, p.smoothMaxAngVel * sc, p.smoothMaxAngAcc * sc, dt);
                float da = dr.magnitude;
                if (da > 1e-6f) cand.rot = Quaternion.AngleAxis(da, dr / da) * cand.rot;
                cand.rot = Quaternion.Normalize(cand.rot);

                // FOV
                Vector3 fv = new Vector3(cand.fovVel, 0f, 0f);
                Vector3 df = Step(new Vector3(rawFov - cand.fov, 0f, 0f), ref fv, p.smoothMaxFovRate * sc, p.smoothMaxFovAcc * sc, dt);
                cand.fov += df.x; cand.fovVel = fv.x;

                // CAM-LIVELY: kenh reaction (giat minh quay sang) co gioi han RIENG cao hon, ap SAU luoi chinh nen khong bi loc
                Vector3 rt = CameraFeelState.ReactTarget;
                Vector3 rd = Step(new Vector3(rt.x, rt.y, 0f) - cand.rA, ref cand.rAV, p.reactMaxAngVel, p.reactMaxAngAcc, dt);
                cand.rA += rd;
                Vector3 rfv = new Vector3(cand.rFV, 0f, 0f);
                Vector3 rdf = Step(new Vector3(rt.z - cand.rF, 0f, 0f), ref rfv, p.reactMaxFovRate, p.reactMaxFovAcc, dt);
                cand.rF += rdf.x; cand.rFV = rfv.x;
                // CAM-VC2: kenh giat khi ban (pitch len + punch FOV) co gioi han RIENG, ap sau luoi chinh
                Vector2 kt = CameraFeelState.KickTarget;
                Vector3 kpv = new Vector3(cand.kPV, 0f, 0f);
                Vector3 kd = Step(new Vector3(kt.x - cand.kP, 0f, 0f), ref kpv, p.kickMaxAngVel, p.kickMaxAngAcc, dt);
                cand.kP += kd.x; cand.kPV = kpv.x;
                Vector3 kfv = new Vector3(cand.kFV, 0f, 0f);
                Vector3 kdf = Step(new Vector3(kt.y - cand.kF, 0f, 0f), ref kfv, p.kickMaxFovRate, p.kickMaxFovAcc, dt);
                cand.kF += kdf.x; cand.kFV = kfv.x;
                MaxKickPitch = Mathf.Max(MaxKickPitch, cand.kP); MaxKickFov = Mathf.Max(MaxKickFov, cand.kF); MaxKickAngVel = Mathf.Max(MaxKickAngVel, Mathf.Abs(cand.kPV));
                MaxReactAngVel = Mathf.Max(MaxReactAngVel, cand.rAV.magnitude); MaxReactAngAcc = Mathf.Max(MaxReactAngAcc, (cand.rAV - committed.rAV).magnitude / dt);
                MaxReactFovRate = Mathf.Max(MaxReactFovRate, Mathf.Abs(cand.rFV)); MaxReactYaw = Mathf.Max(MaxReactYaw, Mathf.Abs(cand.rA.x));

                // Do kiem (chi frame dau cua moi frame: tinh theo cand)
                float av = cand.angVel.magnitude, sp = cand.vel.magnitude;
                MaxAngVel = Mathf.Max(MaxAngVel, av); MaxVel = Mathf.Max(MaxVel, sp);
                MaxAngAcc = Mathf.Max(MaxAngAcc, Mathf.Abs(av - committed.angVel.magnitude) / dt); MaxAcc = Mathf.Max(MaxAcc, Mathf.Abs(sp - committed.vel.magnitude) / dt);
                MaxFovRate = Mathf.Max(MaxFovRate, Mathf.Abs(cand.fovVel));
                MaxLagAngle = Mathf.Max(MaxLagAngle, Quaternion.Angle(cand.rot, rawRot)); MaxLagPos = Mathf.Max(MaxLagPos, (rawPos - cand.pos).magnitude);
            }
            else cand = committed;

            // Reaction: yaw quanh truc dung world, pitch quanh truc phai cuc bo (len = duong); tap/reticle/sung (con cua Main Camera) di theo pose cuoi
            Quaternion finalRot = Quaternion.AngleAxis(cand.rA.x, Vector3.up) * cand.rot * Quaternion.Euler(-(cand.rA.y + cand.kP), 0f, 0f);
            b.transform.SetPositionAndRotation(cand.pos + finalRot * shakeP, finalRot * shakeQ);
            if (outCam != null) outCam.fieldOfView = Mathf.Max(15f, cand.fov - cand.rF - cand.kF);
        }

        /// <summary>Mot buoc dieu khien: toc do mong muon = min(vMax, can bac hai(2*a*0.9*khoang cach)) (de phanh kip), tien gan toc do do voi gia toc toi da.</summary>
        static Vector3 Step(Vector3 err, ref Vector3 v, float vMax, float aMax, float dt)
        {
            float d = err.magnitude;
            Vector3 dir = d > 1e-7f ? err / d : Vector3.zero;
            float want = Mathf.Min(vMax, Mathf.Sqrt(2f * aMax * 0.9f * d));
            want = Mathf.Min(want, d / dt); // toi dich ngay frame nay neu du gan
            Vector3 dv = dir * want - v;
            float maxDv = aMax * dt;
            if (dv.magnitude > maxDv) dv = dv.normalized * maxDv;
            v += dv;
            Vector3 step = v * dt;
            if (step.sqrMagnitude >= err.sqrMagnitude && Vector3.Dot(step, err) > 0f)
            {
                // se vuot qua dich: dung o dich, giu toc do (khong lam bat ngo)
                step = err;
                v = step / dt;
                if (v.magnitude > vMax) v = v.normalized * vMax;
            }
            return step;
        }
    }
}
