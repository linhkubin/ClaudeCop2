using System.Text;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Jev;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Bind JevDecisionLog.DecisionMade + GameCommands.DebugOverlayToggleRequested vao JevDebugPanelView.
    /// Mac dinh an. Nut JEV chi hien khi UNITY_EDITOR || DEVELOPMENT_BUILD hoac UIConfig.showJevDebugButton.
    /// Lich su: vong dem co dinh; chuoi chi dung khi co quyet dinh moi (khong alloc moi frame).
    /// </summary>
    public class JevDebugPanelPresenter : MonoBehaviour
    {
        public const int HistoryCapacity = 5;

        [SerializeField] JevDebugPanelView view;
        [SerializeField] UIConfig config;
        [SerializeField] JevConfig jevConfig;

        readonly JevDecision[] ring = new JevDecision[HistoryCapacity];
        int ringStart, ringCount;
        readonly StringBuilder sb = new StringBuilder(256);
        bool visible, revivePrompt;

        UIConfig Cfg => config != null ? config : UIConfig.Fallback;
        float Threshold => jevConfig != null ? jevConfig.confidenceThreshold : 0.6f;

        public bool IsVisible => visible;
        public int HistoryCount => ringCount;

        public static bool DebugAllowedByBuild
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        public bool Allowed => DebugAllowedByBuild || Cfg.showJevDebugButton;

        void OnEnable()
        {
            if (view == null) { Debug.LogError("[JevDebugPanelPresenter] Thieu view.", this); return; }
            JevDecisionLog.DecisionMade += OnDecision;
            GameCommands.DebugOverlayToggleRequested += Toggle;
            view.ToggleClicked += OnButton;
            GameEvents.GameStateChanged += OnState;
            ringStart = ringCount = 0;
            revivePrompt = GameEvents.Current.State == GameState.RevivePrompt;
            view.SetToggleButtonVisible(Allowed && !revivePrompt);
            view.SetEmpty();
            if (JevDecisionLog.HasLast) Push(JevDecisionLog.Last);
            SetVisible(false);
        }

        void OnDisable()
        {
            JevDecisionLog.DecisionMade -= OnDecision;
            GameCommands.DebugOverlayToggleRequested -= Toggle;
            GameEvents.GameStateChanged -= OnState;
            if (view != null) view.ToggleClicked -= OnButton;
        }

        // Khi RevivePrompt: an bang + nut JEV de khong chong len RevivePopup.
        void OnState(GameState s)
        {
            revivePrompt = s == GameState.RevivePrompt;
            if (view == null) return;
            if (revivePrompt) SetVisible(false);
            view.SetToggleButtonVisible(Allowed && !revivePrompt);
        }

        void OnButton() => GameCommands.RequestDebugOverlayToggle();

        public void Toggle()
        {
            if (!Allowed || revivePrompt) return;
            SetVisible(!visible);
        }

        void SetVisible(bool v)
        {
            visible = v;
            if (view != null) view.SetVisible(v);
        }

        void OnDecision(JevDecision d) => Push(d);

        void Push(JevDecision d)
        {
            if (ringCount < HistoryCapacity) { ring[(ringStart + ringCount) % HistoryCapacity] = d; ringCount++; }
            else { ring[ringStart] = d; ringStart = (ringStart + 1) % HistoryCapacity; }
            Render(d);
        }

        void Render(JevDecision d)
        {
            if (view == null) return;
            view.SetQuestion("Câu hỏi: " + d.Question);
            view.SetChoice($"Chọn: {d.Choice}  (reticle {d.ReticleTime:0.0#}s)");
            bool offline = d.Source == JevDecisionSource.Offline;
            view.SetSource("Nguồn: " + (offline ? "Offline" : "Mặc định"), offline ? Cfg.jevSourceOffline : Cfg.jevSourceDefault);
            float th = Threshold;
            view.SetConfidence($"Confidence {d.Confidence:0.00} / ngưỡng {th:0.00}" + (d.Confidence >= th ? "  đạt" : "  chưa đạt"), d.Confidence, th);
            view.SetTime($"Thời điểm: {d.Timestamp:0.0}s");
            var s = d.Stats;
            view.SetStats($"Accuracy {s.Accuracy * 100f:0}%  ({s.Hits}/{s.Shots})  Miss {s.Misses}\nReaction {s.AvgReactionTime:0.00}s (n={s.ReactionSamples})\nHostage hit {s.HostageHits}  Mất mạng {s.DamageTaken}");

            var names = JevConfig.ChoiceNames;
            for (int i = 0; i < JevDebugPanelView.BarCount; i++)
            {
                if (i >= names.Length) { view.SetBar(i, "", 0f, Color.clear); continue; }
                float p = 0f;
                if (d.Probabilities != null) d.Probabilities.TryGetValue(names[i], out p);
                bool chosen = names[i] == d.Choice;
                view.SetBar(i, $"{names[i]} {p * 100f:0}%", p, chosen ? Cfg.jevBarChosenColor : Cfg.jevBarColor);
            }
            RenderHistory();
        }

        void RenderHistory()
        {
            sb.Clear();
            sb.Append("Lịch sử:");
            for (int k = ringCount - 1; k >= 0; k--)
            {
                var h = ring[(ringStart + k) % HistoryCapacity];
                sb.Append('\n').Append(h.Timestamp.ToString("0.0")).Append("s  ").Append(h.Choice)
                  .Append(' ').Append(h.ReticleTime.ToString("0.0#")).Append("s  c=").Append(h.Confidence.ToString("0.00"))
                  .Append(h.Source == JevDecisionSource.Offline ? "  Offline" : "  Mặc định");
            }
            view.SetHistory(sb.ToString());
        }
    }
}
