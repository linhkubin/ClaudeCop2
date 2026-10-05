#if UNITY_EDITOR || DEVELOPMENT_BUILD
using ClaudeCop.Combat;
using UnityEngine;

namespace ClaudeCop.Viewmodel.Debugging
{
    /// <summary>
    /// Chi dung trong Sandbox: nut IMGUI Ban / Reload / doi 3 sung / xa lien thanh. Khong dung trong game that.
    /// Nut chan tap thuc (qua TapShooter.PointerBlocker) de bam nut khong ton dan.
    /// </summary>
    public sealed class ViewmodelSandboxDriver : MonoBehaviour
    {
        [SerializeField] TapShooter shooter;
        [SerializeField] WeaponData[] weapons;
        [SerializeField] Vector2 fireScreenFraction = new Vector2(0.5f, 0.55f);

        bool autoFire;
        Rect panel;

        void OnEnable() { TapShooter.PointerBlocker = IsOverPanel; }
        void OnDisable() { if (TapShooter.PointerBlocker == (System.Func<Vector2, bool>)IsOverPanel) TapShooter.PointerBlocker = null; }

        bool IsOverPanel(Vector2 screenPos)
        {
            var gui = new Vector2(screenPos.x, Screen.height - screenPos.y);
            return panel.Contains(gui);
        }

        void Update()
        {
            if (autoFire && shooter != null)
                shooter.FireAt(new Vector2(Screen.width * fireScreenFraction.x, Screen.height * fireScreenFraction.y));
        }

        public void SetAutoFire(bool on) { autoFire = on; }

        /// <summary>
        /// Chup anh co dinh kich thuoc (de kiem vi tri sung theo aspect khi Game view khong chay). Tien Animator + viewmodel thu cong
        /// <paramref name="steps"/> buoc dt, roi render Main Camera (kem stack) vao RenderTexture w x h va luu PNG.
        /// </summary>
        public static string Capture(string path, int w, int h, int steps = 0, float dt = 1f / 60f)
        {
            var ctrl = FindFirstObjectByType<ViewmodelController>();
            var cam = Camera.main;
            if (ctrl == null || cam == null) return "no controller/camera";
            var animators = ctrl.GetComponentsInChildren<Animator>(false);
            for (int i = 0; i < steps; i++)
            {
                for (int a = 0; a < animators.Length; a++) animators[a].Update(dt);
                ctrl.Advance(dt);
            }
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            cam.targetTexture = rt;
            ctrl.Advance(0f);
            cam.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            cam.targetTexture = prevTarget;
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Destroy(tex);
            rt.Release();
            Destroy(rt);
            return "saved " + path;
        }

        void OnGUI()
        {
            float s = Mathf.Max(1f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            panel = new Rect(4, 4, 150, 270);
            GUI.Box(panel, "Viewmodel");
            if (shooter == null) return;
            float y = 26;
            if (GUI.Button(new Rect(10, y, 138, 28), "Fire")) shooter.FireAt(new Vector2(Screen.width * fireScreenFraction.x, Screen.height * fireScreenFraction.y)); y += 32;
            if (GUI.Button(new Rect(10, y, 138, 28), "Reload")) shooter.StartReload(); y += 32;
            autoFire = GUI.Toggle(new Rect(10, y, 138, 24), autoFire, "Hold-fire (auto)"); y += 28;
            if (weapons != null)
                for (int i = 0; i < weapons.Length; i++)
                {
                    if (weapons[i] == null) continue;
                    if (GUI.Button(new Rect(10, y, 138, 28), "Equip " + weapons[i].Kind)) shooter.Equip(weapons[i], true);
                    y += 32;
                }
            GUI.Label(new Rect(10, y, 138, 24), "Ammo " + shooter.Ammo + (shooter.IsReloading ? " (R)" : ""));
            panel = new Rect(panel.x * s, panel.y * s, panel.width * s, panel.height * s);
        }
    }
}
#endif
