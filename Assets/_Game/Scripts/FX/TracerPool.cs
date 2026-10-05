using UnityEngine;

namespace ClaudeCop.FX
{
    /// <summary>Pool LineRenderer cho vet dan. Tao san luc khoi tao, khong cap phat khi ban.</summary>
    public sealed class TracerPool
    {
        sealed class Tracer
        {
            public LineRenderer lr;
            public Vector3 a, b;
            public float t0, travel, tailLag, life, width;
            public Color color;
            public bool active;
        }

        readonly Tracer[] items;
        readonly Material material;
        readonly GameObject root;
        int next;

        public int Capacity => items.Length;

        public TracerPool(Transform parent, Material mat, int capacity)
        {
            root = new GameObject("Tracers");
            root.transform.SetParent(parent, false);
            material = mat;
            items = new Tracer[Mathf.Max(1, capacity)];
            for (int i = 0; i < items.Length; i++)
            {
                var go = new GameObject("Tracer");
                go.transform.SetParent(root.transform, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.useWorldSpace = true;
                lr.sharedMaterial = material;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.numCapVertices = 2;
                lr.alignment = LineAlignment.View;
                lr.enabled = false;
                items[i] = new Tracer { lr = lr };
            }
        }

        public void Spawn(Vector3 a, Vector3 b, Color color, float width, float travel, float tailLag, float now)
        {
            var t = items[next];
            next = (next + 1) % items.Length; // ghi de vet cu nhat khi day
            t.a = a; t.b = b; t.t0 = now; t.travel = travel; t.tailLag = tailLag;
            t.life = travel + tailLag; t.width = width; t.color = color; t.active = true;
            t.lr.startWidth = t.lr.endWidth = width;
            t.lr.enabled = true;
            Apply(t, 0f);
        }

        public void Tick(float now)
        {
            for (int i = 0; i < items.Length; i++)
            {
                var t = items[i];
                if (!t.active) continue;
                float el = now - t.t0;
                if (el >= t.life) { t.active = false; t.lr.enabled = false; continue; }
                Apply(t, el);
            }
        }

        static void Apply(Tracer t, float el)
        {
            TracerMath.HeadTail(el, t.travel, t.tailLag, out float h, out float tl);
            var head = Vector3.Lerp(t.a, t.b, h);
            var tail = Vector3.Lerp(t.a, t.b, tl);
            t.lr.SetPosition(0, head);
            t.lr.SetPosition(1, tail);
            // Dau sang (a=1), duoi mo dan; ca vet mo dan o cuoi doi.
            float fade = 1f - Mathf.Clamp01((el - t.travel) / Mathf.Max(1e-4f, t.tailLag));
            var c = t.color;
            var head_c = new Color(Mathf.Min(1f, c.r + 0.35f), Mathf.Min(1f, c.g + 0.35f), Mathf.Min(1f, c.b + 0.35f), c.a * fade);
            var tail_c = new Color(c.r, c.g, c.b, 0f);
            t.lr.startColor = head_c;
            t.lr.endColor = tail_c;
        }

        public void ReleaseAll()
        {
            for (int i = 0; i < items.Length; i++) { items[i].active = false; items[i].lr.enabled = false; }
        }

        public void Destroy()
        {
            if (root != null) Object.DestroyImmediate(root);
        }
    }
}
