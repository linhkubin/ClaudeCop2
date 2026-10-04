using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.UI
{
    /// <summary>
    /// RailEvents.PhaseTransition -> fade den + tieu de. RailEvents.PhaseStarted (Phase dau khong co transition) -> banner chu.
    /// PhaseDirector phat PhaseStarted truoc PhaseTransition cung frame nen banner duoc hoan toi Update va huy neu transition toi.
    /// </summary>
    public class PhaseTransitionPresenter : MonoBehaviour
    {
        [SerializeField] PhaseTransitionView view;
        [SerializeField] UIConfig config;

        bool pendingBanner;
        string pendingTitle;

        UIConfig Cfg => config != null ? config : UIConfig.Fallback;

        void OnEnable()
        {
            if (view == null) { Debug.LogError("[PhaseTransitionPresenter] Thieu view.", this); return; }
            RailEvents.PhaseTransition += OnTransition;
            RailEvents.PhaseStarted += OnStarted;
        }

        void OnDisable()
        {
            RailEvents.PhaseTransition -= OnTransition;
            RailEvents.PhaseStarted -= OnStarted;
            pendingBanner = false;
        }

        void OnTransition(string title, float fadeOut, float hold, float fadeIn)
        {
            pendingBanner = false;
            view.Play(title, fadeOut, hold, fadeIn);
        }

        void OnStarted(int index, string title)
        {
            pendingBanner = true;
            pendingTitle = string.IsNullOrEmpty(title) ? "STAGE " + (index + 1) : title;
        }

        void Update() { FlushPending(); }

        /// <summary>Public de test: day banner dang cho (neu khong co transition dang chay).</summary>
        public void FlushPending()
        {
            if (!pendingBanner) return;
            pendingBanner = false;
            if (!view.IsPlaying) view.Banner(pendingTitle, Cfg.bannerFadeIn, Cfg.bannerHold, Cfg.bannerFadeOut);
        }
    }
}
