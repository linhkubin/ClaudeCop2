using UnityEngine;
using UnityEngine.UI;

namespace ClaudeCop.UI
{
    /// <summary>Mot vong target: co lai + doi mau theo progress (xanh &lt; 0.4 &lt;= vang &lt; 0.75 &lt;= do). Pool-friendly.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class TargetReticleView : MonoBehaviour
    {
        [SerializeField] UIConfig config;
        [SerializeField] Image ring;

        RectTransform rt;
        RectTransform parentRect;
        UIConfig Cfg => config != null ? config : UIConfig.Fallback;

        void Awake() { rt = (RectTransform)transform; }

        /// <summary>Dat progress 0..1: scale + mau.</summary>
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            var c = Cfg;
            float s = Mathf.Lerp(c.reticleStartScale, c.reticleEndScale, progress);
            if (rt == null) rt = (RectTransform)transform;
            rt.localScale = new Vector3(s, s, 1f);
            if (ring != null) ring.color = c.ReticleColor(progress);
        }

        /// <summary>Dat vi tri theo toa do man hinh (pixel). Canvas Screen Space Overlay.</summary>
        public void SetScreenPosition(Vector2 screenPos)
        {
            if (rt == null) rt = (RectTransform)transform;
            if (parentRect == null || rt.parent != parentRect) parentRect = rt.parent as RectTransform;
            if (parentRect == null) { rt.position = screenPos; return; }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, null, out var local))
                rt.localPosition = local;
        }

        public void Show(bool visible) { if (gameObject.activeSelf != visible) gameObject.SetActive(visible); }
    }
}
