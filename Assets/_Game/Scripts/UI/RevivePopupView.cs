using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClaudeCop.UI
{
    /// <summary>View popup Revive: tieu de, dem nguoc, 2 nut. Chi hien thi + phat su kien nut; logic nam o RevivePopupPresenter.</summary>
    public class RevivePopupView : MonoBehaviour
    {
        [SerializeField] GameObject content;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text countdownText;
        [SerializeField] Button watchButton;
        [SerializeField] Button skipButton;

        public event Action WatchClicked;
        public event Action SkipClicked;

        int shownSeconds = -1;

        public bool IsVisible => content != null && content.activeSelf;
        public int ShownSeconds => shownSeconds;
        public bool WatchInteractable => watchButton != null && watchButton.interactable;
        public bool SkipInteractable => skipButton != null && skipButton.interactable;
        public Button WatchButton => watchButton;
        public Button SkipButton => skipButton;

        void Awake()
        {
            if (watchButton != null) watchButton.onClick.AddListener(() => WatchClicked?.Invoke());
            if (skipButton != null) skipButton.onClick.AddListener(() => SkipClicked?.Invoke());
        }

        public void Show() { shownSeconds = -1; if (content != null) content.SetActive(true); }
        public void Hide() { if (content != null) content.SetActive(false); }

        /// <summary>Lam tron len; chi cap nhat chu khi doi so nguyen (khong alloc moi frame).</summary>
        public void SetCountdown(float seconds)
        {
            int n = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            if (n == shownSeconds) return;
            shownSeconds = n;
            if (countdownText != null) countdownText.SetText("{0}", n);
        }

        public void SetWatchInteractable(bool on) { if (watchButton != null) watchButton.interactable = on; }
        public void SetSkipInteractable(bool on) { if (skipButton != null) skipButton.interactable = on; }
    }
}
