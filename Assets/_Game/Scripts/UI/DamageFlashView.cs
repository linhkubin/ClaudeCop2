using UnityEngine;

namespace ClaudeCop.UI
{
    /// <summary>Nhay do toan man hinh khi trung dan. Dung unscaled time.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DamageFlashView : MonoBehaviour
    {
        [SerializeField] UIConfig config;
        [SerializeField] CanvasGroup group;

        float t = -1f;
        UIConfig Cfg => config != null ? config : UIConfig.Fallback;

        void Awake()
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            group.alpha = 0f; group.blocksRaycasts = false; group.interactable = false;
        }

        public void Flash() { t = 0f; }

        void Update()
        {
            if (t < 0f) return;
            t += Time.unscaledDeltaTime;
            float k = t / Mathf.Max(0.01f, Cfg.flashDuration);
            if (k >= 1f) { group.alpha = 0f; t = -1f; return; }
            group.alpha = Cfg.flashPeakAlpha * (k < 0.15f ? k / 0.15f : 1f - (k - 0.15f) / 0.85f);
        }
    }
}
