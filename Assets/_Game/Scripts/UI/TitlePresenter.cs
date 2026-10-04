using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.UI
{
    /// <summary>Start -> GameCommands.RequestStartGame(); cong tac Giam chuyen dong <-> UserSettings.ReduceMotion (Core luu PlayerPrefs).</summary>
    public class TitlePresenter : MonoBehaviour
    {
        [SerializeField] TitleView view;

        void OnEnable()
        {
            if (view == null) { Debug.LogError("[TitlePresenter] Thieu view.", this); return; }
            view.StartClicked += OnStart;
            view.ReduceMotionToggled += OnToggle;
            UserSettings.Changed += SyncToggle;
            SyncToggle();
        }

        void OnDisable()
        {
            UserSettings.Changed -= SyncToggle;
            if (view != null) { view.StartClicked -= OnStart; view.ReduceMotionToggled -= OnToggle; }
        }

        void SyncToggle() => view.SetReduceMotion(UserSettings.ReduceMotion);
        void OnStart() => GameCommands.RequestStartGame();
        void OnToggle(bool on) => UserSettings.ReduceMotion = on;
    }
}
