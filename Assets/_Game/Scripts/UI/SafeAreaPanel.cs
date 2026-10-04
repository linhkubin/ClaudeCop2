using UnityEngine;

namespace ClaudeCop.UI
{
    /// <summary>Co RectTransform theo Screen.safeArea (landscape, notch).</summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPanel : MonoBehaviour
    {
        Rect last;
        Vector2Int lastRes;

        void OnEnable() => Apply();
        void Update() { if (Screen.safeArea != last || lastRes.x != Screen.width || lastRes.y != Screen.height) Apply(); }

        void Apply()
        {
            var r = Screen.safeArea;
            last = r; lastRes = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(r.xMin / Screen.width, r.yMin / Screen.height);
            rt.anchorMax = new Vector2(r.xMax / Screen.width, r.yMax / Screen.height);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
