using System.Collections.Generic;
using UnityEngine;

namespace ClaudeCop.FX
{
    /// <summary>Pool GameObject FX theo prefab. Khong Instantiate/Destroy moi phat; day pool thi tai dung ban cu nhat.</summary>
    public sealed class FxPool
    {
        sealed class Slot
        {
            public GameObject go;
            public ParticleSystem[] ps;
            public int[] baseMax;
            public int cost;
            public float releaseAt;
            public bool active;
        }

        readonly Transform root;
        readonly Dictionary<GameObject, List<Slot>> pools = new Dictionary<GameObject, List<Slot>>();
        readonly Dictionary<GameObject, int> baseCost = new Dictionary<GameObject, int>();
        readonly List<Slot> activeList = new List<Slot>(64);

        public int ActiveCount { get; private set; }
        /// <summary>Tong so hat toi da cua cac instance dang chay.</summary>
        public int ActiveCost { get; private set; }
        public int TotalCreated { get; private set; }

        public FxPool(Transform root) { this.root = root; }

        public int InstanceCount(GameObject prefab) => prefab != null && pools.TryGetValue(prefab, out var l) ? l.Count : 0;

        int CostOf(GameObject prefab)
        {
            if (baseCost.TryGetValue(prefab, out int c)) return c;
            c = 0;
            foreach (var p in prefab.GetComponentsInChildren<ParticleSystem>(true)) c += p.main.maxParticles;
            baseCost[prefab] = c;
            return c;
        }

        /// <summary>Tao san count instance cua prefab (tat), de Spawn khong Instantiate luc choi.</summary>
        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null) return;
            if (!pools.TryGetValue(prefab, out var list)) { list = new List<Slot>(Mathf.Max(count, 4)); pools[prefab] = list; }
            while (list.Count < count) list.Add(Create(prefab));
        }

        /// <summary>Phat mot FX. Tra ve instance, hoac null neu bi tu choi (maxInstances=0 / vuot ngan sach hat).</summary>
        public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot, float lifetime, int maxInstances,
                                float particleScale, int particleBudget, float now)
        {
            if (prefab == null || maxInstances <= 0) return null;
            int cost = Mathf.CeilToInt(CostOf(prefab) * particleScale);
            if (cost > 0 && ActiveCost + cost > particleBudget) return null;

            if (!pools.TryGetValue(prefab, out var list)) { list = new List<Slot>(maxInstances); pools[prefab] = list; }

            Slot slot = null;
            for (int i = 0; i < list.Count; i++) if (!list[i].active) { slot = list[i]; break; }
            if (slot == null && list.Count < maxInstances) { slot = Create(prefab); list.Add(slot); }
            if (slot == null)
            {
                // Day pool: tai dung instance sap het han nhat.
                for (int i = 0; i < list.Count; i++) if (slot == null || list[i].releaseAt < slot.releaseAt) slot = list[i];
                Release(slot);
                if (cost > 0 && ActiveCost + cost > particleBudget) return null;
            }

            slot.cost = cost;
            slot.releaseAt = now + lifetime;
            slot.active = true;
            ActiveCount++; ActiveCost += cost;
            activeList.Add(slot);

            slot.go.transform.SetPositionAndRotation(pos, rot);
            slot.go.SetActive(true);
            for (int i = 0; i < slot.ps.Length; i++)
            {
                var main = slot.ps[i].main;
                main.maxParticles = Mathf.Max(1, Mathf.CeilToInt(slot.baseMax[i] * particleScale));
                slot.ps[i].Clear(true);
                slot.ps[i].Play(false);
            }
            return slot.go;
        }

        Slot Create(GameObject prefab)
        {
            var go = Object.Instantiate(prefab, root);
            go.name = prefab.name;
            go.SetActive(false);
            var s = new Slot { go = go, ps = go.GetComponentsInChildren<ParticleSystem>(true) };
            s.baseMax = new int[s.ps.Length];
            for (int i = 0; i < s.ps.Length; i++) s.baseMax[i] = s.ps[i].main.maxParticles;
            TotalCreated++;
            return s;
        }

        void Release(Slot s)
        {
            if (s == null || !s.active) return;
            s.active = false;
            ActiveCount--; ActiveCost -= s.cost; s.cost = 0;
            activeList.Remove(s);
            if (s.go != null) s.go.SetActive(false);
        }

        /// <summary>Goi moi frame: tra ve pool cac instance het han.</summary>
        public void Tick(float now)
        {
            for (int i = activeList.Count - 1; i >= 0; i--)
                if (activeList[i].releaseAt <= now) Release(activeList[i]);
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
                    if (s.go != null) Object.DestroyImmediate(s.go);
            pools.Clear();
        }
    }
}
