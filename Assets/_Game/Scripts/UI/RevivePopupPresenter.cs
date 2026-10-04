using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Ads;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Nghe GameStateChanged(RevivePrompt)/ReviveAvailabilityChanged. Dem nguoc (unscaled), Xem quang cao -> IRewardedAd -> RequestRevive,
    /// Bo qua / het gio -> DeclineRevive. Dung game bang Time.timeScale = 0 trong luc popup mo (tuy chon), tra lai khi dong.
    /// Gan IRewardedAd qua field "adBehaviour" (FakeRewardedAd) hoac SetAd(); thieu thi tu tim FakeRewardedAd trong scene.
    /// </summary>
    public class RevivePopupPresenter : MonoBehaviour
    {
        [SerializeField] RevivePopupView view;
        [SerializeField] UIConfig config;
        [Tooltip("Component implement IRewardedAd (vd FakeRewardedAd). De trong = tu tim.")]
        [SerializeField] MonoBehaviour adBehaviour;
        [SerializeField] bool freezeTimeScale = true;

        IRewardedAd ad;
        float remaining;
        bool open, adRunning, froze;
        float prevTimeScale = 1f;

        UIConfig Cfg => config != null ? config : UIConfig.Fallback;
        public bool IsOpen => open;
        public bool IsAdRunning => adRunning;
        public float Remaining => remaining;

        public void SetAd(IRewardedAd rewardedAd) { ad = rewardedAd; }

        void OnEnable()
        {
            if (view == null) { Debug.LogError("[RevivePopupPresenter] Thieu view.", this); return; }
            GameEvents.GameStateChanged += OnState;
            GameEvents.ReviveAvailabilityChanged += OnRevives;
            view.WatchClicked += OnWatch;
            view.SkipClicked += OnSkip;
            OnState(GameEvents.Current.State);
        }

        void OnDisable()
        {
            GameEvents.GameStateChanged -= OnState;
            GameEvents.ReviveAvailabilityChanged -= OnRevives;
            if (view != null) { view.WatchClicked -= OnWatch; view.SkipClicked -= OnSkip; }
            if (open) Close();
        }

        void Update() { if (open) Tick(Time.unscaledDeltaTime); }

        void OnState(GameState s)
        {
            if (s == GameState.RevivePrompt) Open();
            else if (open) Close();
            else if (view != null) view.Hide();
        }

        void OnRevives(int _) { if (open) RefreshButtons(); }

        void ResolveAd()
        {
            if (ad != null) return;
            if (adBehaviour is IRewardedAd a) ad = a;
            else ad = FindFirstObjectByType<FakeRewardedAd>(FindObjectsInactive.Include);
        }

        void Open()
        {
            if (open) return;
            open = true; adRunning = false;
            remaining = Cfg.reviveCountdownSeconds;
            if (freezeTimeScale && Time.timeScale > 0f) { prevTimeScale = Time.timeScale; Time.timeScale = 0f; froze = true; }
            ResolveAd();
            view.Show();
            view.SetCountdown(remaining);
            RefreshButtons();
        }

        void Close()
        {
            open = false; adRunning = false;
            if (froze) { if (Mathf.Approximately(Time.timeScale, 0f)) Time.timeScale = prevTimeScale; froze = false; }
            if (view != null) view.Hide();
        }

        void RefreshButtons()
        {
            ResolveAd();
            bool canWatch = !adRunning && GameEvents.Current.RevivesRemaining > 0 && ad != null && ad.IsReady;
            view.SetWatchInteractable(canWatch);
            view.SetSkipInteractable(!adRunning);
        }

        /// <summary>Tien thoi gian dem nguoc (giay thuc). Tam dung khi dang xem quang cao.</summary>
        public void Tick(float dt)
        {
            if (!open || adRunning) return;
            remaining -= dt;
            if (remaining <= 0f) { remaining = 0f; view.SetCountdown(0f); Decline(); return; }
            view.SetCountdown(remaining);
        }

        void OnWatch()
        {
            if (!open || adRunning) return;
            ResolveAd();
            if (ad == null || !ad.IsReady || GameEvents.Current.RevivesRemaining <= 0) { RefreshButtons(); return; }
            adRunning = true;
            RefreshButtons();
            ad.Show(OnRewarded, OnAdFailed);
        }

        void OnRewarded()
        {
            if (!open) return;
            Close();
            GameCommands.RequestRevive();
        }

        void OnAdFailed()
        {
            if (!open) return;
            adRunning = false;   // giu popup, tiep tuc dem nguoc
            RefreshButtons();
        }

        void OnSkip() { if (open && !adRunning) Decline(); }

        void Decline()
        {
            Close();
            GameCommands.DeclineRevive();
        }
    }
}
