using System;
using UnityEngine;
using TMPro;

namespace ClaudeCop.Ads
{
    /// <summary>
    /// Quang cao gia: overlay "Quang cao" dem nguoc bang unscaled time roi tra thuong.
    /// Dat prefab FakeAdOverlay vao scene (mac dinh an).
    /// </summary>
    public class FakeRewardedAd : MonoBehaviour, IRewardedAd
    {
        [SerializeField] GameObject content;
        [SerializeField] TMP_Text countdownText;
        [SerializeField] TMP_Text titleText;
        [Tooltip("Thoi gian quang cao gia (giay) - nguon duy nhat (TASK_BOARD: 3 s). Khong co nut dong som.")]
        [SerializeField] float durationSeconds = 3f;

        Action onRewarded, onFailed;
        float remaining;
        bool showing;

        public bool IsReady => !showing;
        public bool IsShowing => showing;

        void Awake()
        {
            if (content != null) content.SetActive(false);
        }

        public void Show(Action onRewarded, Action onFailed)
        {
            if (showing) { onFailed?.Invoke(); return; }
            this.onRewarded = onRewarded; this.onFailed = onFailed;
            remaining = Mathf.Max(0.1f, durationSeconds);
            showing = true;
            if (titleText != null) titleText.text = "QUẢNG CÁO";
            if (content != null) content.SetActive(true);
            UpdateText();
        }

        void Update()
        {
            if (!showing) return;
            remaining -= Time.unscaledDeltaTime;
            UpdateText();
            if (remaining <= 0f) Finish(true);
        }

        void UpdateText() { if (countdownText != null) countdownText.text = Mathf.CeilToInt(Mathf.Max(0f, remaining)).ToString(); }

        void Finish(bool rewarded)
        {
            showing = false;
            if (content != null) content.SetActive(false);
            var cb = rewarded ? onRewarded : onFailed;
            onRewarded = onFailed = null;
            cb?.Invoke();
        }
    }
}
