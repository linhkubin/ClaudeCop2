using System.Collections.Generic;
using UnityEngine;
using UCamera = UnityEngine.Camera;

namespace ClaudeCop.Props
{
    /// <summary>Mot instance trong pool (manh vo / vat bay).</summary>
    public sealed class PropInstance
    {
        public GameObject Go;
        public Rigidbody[] Bodies;
        internal Transform[] bodyTransforms;
        internal Vector3[] basePos;
        internal Quaternion[] baseRot;
        internal Vector3[] baseScale;
        internal GameObject prefab;
        internal float expireAt;
        internal float fadeStart;
        internal Vector3 scaleMul = Vector3.one;
        internal bool active;
        public bool Active => active;
    }

    /// <summary>
    /// Pool tong quat theo prefab cho vat co Rigidbody. Tong Rigidbody dang hoat dong &lt;= maxBodies: vuot thi tai che
    /// instance cu nhat. Het han thi tra ve pool (thu nho o cuoi doi). Vat ngoai khung hinh cho ngu. Khong cap phat khi Tick.
    /// </summary>
    public sealed class PropPool
    {
        readonly Transform root;
        readonly int maxBodies;
        readonly float fadeSeconds, offscreenMargin;
        readonly Dictionary<GameObject, List<PropInstance>> pools = new Dictionary<GameObject, List<PropInstance>>();
        readonly Dictionary<GameObject, int> bodyCost = new Dictionary<GameObject, int>();
        readonly List<PropInstance> activeList = new List<PropInstance>(64); // thu tu sinh ra: cu nhat o dau

        public int ActiveBodies { get; private set; }
        public int ActiveCount => activeList.Count;
        public int TotalCreated { get; private set; }
        public int MaxBodies => maxBodies;
        /// <summary>Dinh cao nhat cua ActiveBodies (de kiem tra gioi han).</summary>
        public int PeakActiveBodies { get; private set; }

        public PropPool(Transform root, int maxBodies = 40, float fadeSeconds = 0.5f, float offscreenMargin = 0.15f)
        {
            this.root = root; this.maxBodies = Mathf.Max(1, maxBodies);
            this.fadeSeconds = Mathf.Max(0f, fadeSeconds); this.offscreenMargin = offscreenMargin;
        }

        public int InstanceCount(GameObject prefab) => prefab != null && pools.TryGetValue(prefab, out var l) ? l.Count : 0;

        int CostOf(GameObject prefab)
        {
            if (bodyCost.TryGetValue(prefab, out int c)) return c;
            c = prefab.GetComponentsInChildren<Rigidbody>(true).Length;
            bodyCost[prefab] = c;
            return c;
        }

        PropInstance Create(GameObject prefab)
        {
            var go = Object.Instantiate(prefab, root);
            go.name = prefab.name;
            go.SetActive(false);
            var inst = new PropInstance { Go = go, prefab = prefab, Bodies = go.GetComponentsInChildren<Rigidbody>(true) };
            int n = inst.Bodies.Length;
            inst.bodyTransforms = new Transform[n]; inst.basePos = new Vector3[n]; inst.baseRot = new Quaternion[n]; inst.baseScale = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                var t = inst.Bodies[i].transform;
                inst.bodyTransforms[i] = t; inst.basePos[i] = t.localPosition; inst.baseRot[i] = t.localRotation; inst.baseScale[i] = t.localScale;
            }
            TotalCreated++;
            return inst;
        }

        /// <summary>Tao san count instance (tranh Instantiate luc choi).</summary>
        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null) return;
            if (!pools.TryGetValue(prefab, out var list)) { list = new List<PropInstance>(count); pools[prefab] = list; }
            CostOf(prefab);
            while (list.Count < count) list.Add(Create(prefab));
        }

        /// <summary>Phat mot instance. lifetime &lt;= 0: khong tu het han. Tra null neu prefab null hoac vuot tran mot minh.</summary>
        public PropInstance Spawn(GameObject prefab, Vector3 pos, Quaternion rot, float lifetime, float now)
            => Spawn(prefab, pos, rot, lifetime, now, Vector3.one);

        /// <summary>Nhu tren; scaleMul nhan vao vi tri/scale cuc bo cua tung Rigidbody con (kinh co gian theo kich thuoc o).</summary>
        public PropInstance Spawn(GameObject prefab, Vector3 pos, Quaternion rot, float lifetime, float now, Vector3 scaleMul)
        {
            if (prefab == null) return null;
            int cost = CostOf(prefab);
            if (cost > maxBodies) return null;
            while (ActiveBodies + cost > maxBodies && activeList.Count > 0) Release(activeList[0]);

            if (!pools.TryGetValue(prefab, out var list)) { list = new List<PropInstance>(8); pools[prefab] = list; }
            PropInstance inst = null;
            for (int i = 0; i < list.Count; i++) if (!list[i].active) { inst = list[i]; break; }
            if (inst == null) { inst = Create(prefab); list.Add(inst); }

            inst.active = true;
            inst.scaleMul = scaleMul;
            inst.expireAt = lifetime > 0f ? now + lifetime : float.PositiveInfinity;
            inst.fadeStart = inst.expireAt - fadeSeconds;
            activeList.Add(inst);
            ActiveBodies += inst.Bodies.Length;
            if (ActiveBodies > PeakActiveBodies) PeakActiveBodies = ActiveBodies;

            for (int i = 0; i < inst.bodyTransforms.Length; i++)
            {
                var t = inst.bodyTransforms[i];
                t.localPosition = Vector3.Scale(inst.basePos[i], scaleMul); t.localRotation = inst.baseRot[i]; t.localScale = Vector3.Scale(inst.baseScale[i], scaleMul);
            }
            inst.Go.transform.SetPositionAndRotation(pos, rot);
            inst.Go.SetActive(true);
            for (int i = 0; i < inst.Bodies.Length; i++)
            {
                var rb = inst.Bodies[i];
                if (rb.isKinematic) continue;
                rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; rb.WakeUp();
            }
            return inst;
        }

        public void Release(PropInstance inst)
        {
            if (inst == null || !inst.active) return;
            inst.active = false;
            ActiveBodies -= inst.Bodies.Length;
            activeList.Remove(inst);
            if (inst.Go != null) inst.Go.SetActive(false);
        }

        /// <summary>Goi moi frame: het han thi tra ve pool, thu nho cuoi doi, ngu vat ngoai khung (cam co the null).</summary>
        public void Tick(float now, UCamera cam)
        {
            for (int i = activeList.Count - 1; i >= 0; i--)
            {
                var inst = activeList[i];
                if (now >= inst.expireAt) { Release(inst); continue; }
                if (now > inst.fadeStart && fadeSeconds > 0f)
                {
                    float k = Mathf.Clamp01((inst.expireAt - now) / fadeSeconds);
                    for (int b = 0; b < inst.bodyTransforms.Length; b++) inst.bodyTransforms[b].localScale = Vector3.Scale(inst.baseScale[b], inst.scaleMul) * k;
                }
                if (cam != null) SleepIfOffscreen(inst, cam);
            }
        }

        void SleepIfOffscreen(PropInstance inst, UCamera cam)
        {
            for (int b = 0; b < inst.Bodies.Length; b++)
            {
                var rb = inst.Bodies[b];
                if (rb.isKinematic || rb.IsSleeping()) continue;
                Vector3 v = cam.WorldToViewportPoint(rb.position);
                float lo = -offscreenMargin, hi = 1f + offscreenMargin;
                if (v.z < 0f || v.x < lo || v.x > hi || v.y < lo || v.y > hi) rb.Sleep();
            }
        }

        public void ReleaseAll()
        {
            for (int i = activeList.Count - 1; i >= 0; i--) Release(activeList[i]);
        }

        public void Destroy()
        {
            ReleaseAll();
            foreach (var kv in pools)
                foreach (var s in kv.Value)
                    if (s.Go != null) Object.DestroyImmediate(s.Go);
            pools.Clear();
        }
    }
}
