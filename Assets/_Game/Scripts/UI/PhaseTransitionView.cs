using UnityEngine;
using TMPro;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Fade den + tieu de Phase (hoac banner khong fade den). Dung unscaled time. Khong chan raycast.
    /// Play(): fadeOut -> hold -> fadeIn. Banner(): chu hien/giu/mo.
    /// </summary>
    public class PhaseTransitionView : MonoBehaviour
    {
        [SerializeField] GameObject content;
        [SerializeField] CanvasGroup blackGroup;
        [SerializeField] CanvasGroup titleGroup;
        [SerializeField] TMP_Text titleText;

        bool playing, useBlack;
        float elapsed, d0, d1, d2;

        public bool IsPlaying => playing;
        public float BlackAlpha => blackGroup != null ? blackGroup.alpha : 0f;
        public float TitleAlpha => titleGroup != null ? titleGroup.alpha : 0f;
        public string TitleString => titleText != null ? titleText.text : string.Empty;

        void OnDisable() { Stop(); }

        public void Play(string title, float fadeOut, float hold, float fadeIn)
        {
            Begin(title, true, Mathf.Max(0.01f, fadeOut), Mathf.Max(0f, hold), Mathf.Max(0.01f, fadeIn));
        }

        public void Banner(string title, float fadeIn, float hold, float fadeOut)
        {
            Begin(title, false, Mathf.Max(0.01f, fadeIn), Mathf.Max(0f, hold), Mathf.Max(0.01f, fadeOut));
        }

        void Begin(string title, bool black, float a, float b, float c)
        {
            if (titleText != null) titleText.text = title ?? string.Empty;
            useBlack = black; d0 = a; d1 = b; d2 = c; elapsed = 0f; playing = true;
            if (content != null) content.SetActive(true);
            Apply();
        }

        public void Stop()
        {
            playing = false; elapsed = 0f;
            if (blackGroup != null) blackGroup.alpha = 0f;
            if (titleGroup != null) titleGroup.alpha = 0f;
            if (content != null) content.SetActive(false);
        }

        void Update() { if (playing) Tick(Time.unscaledDeltaTime); }

        public void Tick(float dt)
        {
            if (!playing) return;
            elapsed += dt;
            if (elapsed >= d0 + d1 + d2) { Stop(); return; }
            Apply();
        }

        void Apply()
        {
            float black, title;
            if (useBlack)
            {
                // fadeOut: den dan; hold: den + chu hien; fadeIn: sang dan, chu mo theo.
                if (elapsed < d0) { black = elapsed / d0; title = 0f; }
                else if (elapsed < d0 + d1) { black = 1f; title = Mathf.Clamp01((elapsed - d0) / Mathf.Min(0.2f, Mathf.Max(0.01f, d1))); }
                else { float k = (elapsed - d0 - d1) / d2; black = 1f - k; title = 1f - k; }
            }
            else
            {
                black = 0f;
                if (elapsed < d0) title = elapsed / d0;
                else if (elapsed < d0 + d1) title = 1f;
                else title = 1f - (elapsed - d0 - d1) / d2;
            }
            if (blackGroup != null) blackGroup.alpha = Mathf.Clamp01(black);
            if (titleGroup != null) titleGroup.alpha = Mathf.Clamp01(title);
        }
    }
}
