using System.Collections.Generic;
using UnityEngine;
using UCamera = UnityEngine.Camera;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Chu diem bay: pool FloatingScoreItem, world -> screen moi frame bang Camera.main (bam diem trung khi camera chay/blend),
    /// bay len + mo dan. Justice mau/co khac, hien he so. Het pool thi tai dung cai cu nhat. Khong alloc moi frame.
    /// </summary>
    public class FloatingScoreView : MonoBehaviour
    {
        [SerializeField] UIConfig config;
        [Tooltip("Node cha cua item (Canvas Overlay, stretch toan man hinh).")]
        [SerializeField] RectTransform layer;
        [Tooltip("Item mau (con cua layer, bi tat). Nhan ban khi can.")]
        [SerializeField] FloatingScoreItem template;
        [SerializeField] int poolSize = 12;

        readonly List<FloatingScoreItem> pool = new List<FloatingScoreItem>(16);
        UCamera cam;

        UIConfig Cfg => config != null ? config : UIConfig.Fallback;
        public int PoolCount => pool.Count;
        public int ActiveCount { get { int n = 0; for (int i = 0; i < pool.Count; i++) if (pool[i].Active) n++; return n; } }
        public FloatingScoreItem LastSpawned { get; private set; }

        FloatingScoreItem Acquire()
        {
            FloatingScoreItem oldest = null;
            for (int i = 0; i < pool.Count; i++)
            {
                var it = pool[i];
                if (!it.Active) return it;
                if (oldest == null || it.Age > oldest.Age) oldest = it;
            }
            if (pool.Count < Mathf.Max(1, poolSize) && template != null && layer != null)
            {
                var it = Instantiate(template, layer);
                it.name = "FloatingScore" + pool.Count;
                pool.Add(it);
                return it;
            }
            return oldest;
        }

        public void Spawn(Vector3 world, int points, bool justice, float multiplier)
        {
            var it = Acquire();
            if (it == null) return;
            var c = Cfg;
            var label = it.Label;
            if (label != null)
            {
                bool mult = multiplier > 1.001f;
                if (justice) { if (mult) label.SetText("JUSTICE!\n+{0}  x{1:1}", points, multiplier); else label.SetText("JUSTICE!\n+{0}", points); }
                else { if (mult) label.SetText("+{0}  x{1:1}", points, multiplier); else label.SetText("+{0}", points); }
                label.fontSize = justice ? c.floatingJusticeSize : c.floatingNormalSize;
                label.color = justice ? c.floatingJusticeColor : c.floatingNormalColor;
            }
            it.World = world; it.Age = 0f; it.Life = Mathf.Max(0.05f, c.floatingLife); it.Rise = c.floatingRise; it.OffsetY = 0f;
            it.Active = true;
            it.gameObject.SetActive(true);
            LastSpawned = it;
            Place(it);
        }

        /// <summary>Chu "NO! xK" tai tam vu no (K = so enemy bi ha; khong doc diem nen khong lech voi Game).</summary>
        public void SpawnBlast(Vector3 world, int enemiesKilled)
        {
            var it = Acquire();
            if (it == null) return;
            var c = Cfg;
            var label = it.Label;
            if (label != null)
            {
                if (enemiesKilled > 0) label.SetText("NỔ! x{0}", enemiesKilled); else label.SetText("NỔ!");
                label.fontSize = c.blastTextSize;
                label.color = c.blastTextColor;
            }
            it.World = world; it.Age = 0f; it.Life = Mathf.Max(0.05f, c.floatingLife); it.Rise = c.floatingRise; it.OffsetY = c.blastTextOffsetY;
            it.Active = true;
            it.gameObject.SetActive(true);
            LastSpawned = it;
            Place(it);
        }

        public void ClearAll()
        {
            for (int i = 0; i < pool.Count; i++) Release(pool[i]);
        }

        static void Release(FloatingScoreItem it)
        {
            it.Active = false;
            it.gameObject.SetActive(false);
        }

        void Update() { Tick(Time.unscaledDeltaTime); }

        public void Tick(float dt)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var it = pool[i];
                if (!it.Active) continue;
                it.Age += dt;
                if (it.Age >= it.Life) { Release(it); continue; }
                Place(it);
            }
        }

        void Place(FloatingScoreItem it)
        {
            if (cam == null || !cam.isActiveAndEnabled) cam = UCamera.main;
            var label = it.Label;
            float k = it.Age / it.Life;
            var rt = (RectTransform)it.transform;
            if (cam != null && layer != null)
            {
                Vector3 sp = cam.WorldToScreenPoint(it.World);
                bool visible = sp.z > 0f;
                if (label != null) label.alpha = visible ? (k < Cfg.floatingFadeStart ? 1f : 1f - (k - Cfg.floatingFadeStart) / (1f - Cfg.floatingFadeStart)) : 0f;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, new Vector2(sp.x, sp.y), null, out var local);
                float ease = 1f - (1f - k) * (1f - k);
                rt.anchoredPosition = local + new Vector2(0f, it.Rise * ease + it.OffsetY);
            }
            float pf = Mathf.Max(0.01f, Cfg.floatingPopFraction);
            float pop = k < pf ? Mathf.Lerp(Cfg.floatingPopScale, 1f, k / pf) : 1f;
            rt.localScale = new Vector3(pop, pop, 1f);
        }
    }
}
