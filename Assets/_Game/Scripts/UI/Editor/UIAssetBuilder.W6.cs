using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

namespace ClaudeCop.UI.Editor
{
    /// <summary>
    /// W6 (T-611): vá tại chỗ (không dựng lại) HUD.prefab thêm banner "LỰU ĐẠN!", gán config cho PhaseFade,
    /// và thêm TargetReticlePresenter vào sandbox ui-coder. Idempotent.
    /// </summary>
    public static partial class UIAssetBuilder
    {
        [MenuItem("ClaudeCop/UI/W6 - Patch Grenade Warning")]
        public static void PatchW6()
        {
            font = GetUiFont();
            var cfg = AssetDatabase.LoadAssetAtPath<UIConfig>(Root + "/UIConfig.asset");

            var hudPath = PrefabDir + "/HUD.prefab";
            var hud = PrefabUtility.LoadPrefabContents(hudPath);
            try
            {
                var safe = hud.transform.Find("SafeArea");
                var existing = safe.Find("GrenadeWarning");
                if (existing == null)
                {
                    var warn = NewText("GrenadeWarning", safe, "LỰU ĐẠN!", 64, TextAlignmentOptions.Center, cfg.grenadeWarnColor);
                    warn.fontStyle = FontStyles.Bold;
                    Anchor(warn.rectTransform, new Vector2(0.5f, 1), new Vector2(700, 90), new Vector2(0, -190));
                    warn.gameObject.SetActive(false);
                    existing = warn.transform;
                }
                Set(hud.GetComponent<HudView>(), "grenadeWarning", existing.GetComponent<TMP_Text>());
                PrefabUtility.SaveAsPrefabAsset(hud, hudPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(hud); }

            var fadePath = PrefabDir + "/PhaseFade.prefab";
            var fade = PrefabUtility.LoadPrefabContents(fadePath);
            try
            {
                var v = fade.GetComponentInChildren<PhaseTransitionView>(true);
                if (v != null) { Set(v, "config", cfg); PrefabUtility.SaveAsPrefabAsset(fade, fadePath); }
            }
            finally { PrefabUtility.UnloadPrefabContents(fade); }

            AssetDatabase.SaveAssets();
            Debug.Log("[UIAssetBuilder] W6 patch xong.");
        }

        [MenuItem("ClaudeCop/UI/W6 - Add Reticle Presenter To Sandbox")]
        public static void PatchSandboxW6()
        {
            var scene = EditorSceneManager.OpenScene(SandboxScene, OpenSceneMode.Single);
            if (Object.FindFirstObjectByType<TargetReticlePresenter>() == null)
            {
                var hud = Object.FindFirstObjectByType<HudView>();
                var rp = new GameObject("TargetReticlePresenter").AddComponent<TargetReticlePresenter>();
                Set(rp, "hud", hud);
                Set(rp, "reticlePrefab", AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/TargetReticle.prefab").GetComponent<TargetReticleView>());
            }
            EditorSceneManager.SaveScene(scene, SandboxScene);
        }
    }
}
