using System.Text;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.RankScore;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Bind RankScoreDecisionLog.DecisionMade + GameCommands.DebugOverlayToggleRequested vao RankScoreDebugPanelView.
    /// Mac dinh an. Nut RANKSCORE chi hien khi UNITY_EDITOR || DEVELOPMENT_BUILD hoac UIConfig.showRankScoreDebugButton.
    /// Lich su: vong dem co dinh; chuoi chi dung khi co quyet dinh moi (khong alloc moi frame).
    /// </summary>
    public class RankScoreDebugPanelPresenter : MonoBehaviour
    {
        public const int HistoryCapacity = 5;

        [SerializeField] RankScoreDebugPanelView view;
        [SerializeField] UIConfig config;
        [SerializeField] RankScoreConfig rankScoreConfig;

        readonly RankScoreDecision[] ring = new RankScoreDecision[HistoryCapacity];
        int ringStart, ringCount;
        readonly StringBuilder sb = new StringBuilder(256);
        bool visible, revivePrompt;
        string lastPreset = "—", lastDrop = "—", lastRank = "—";

        UIConfig Cfg => config != null ? config : UIConfig.Fallback;
        float Threshold => rankScoreConfig != null ? rankScoreConfig.confidenceThreshold : 0.6f;

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

        public bool Allowed => DebugAllowedByBuild || Cfg.showRankScoreDebugButton;

        void OnEnable()
        {
            if (view == null) { Debug.LogError("[RankScoreDebugPanelPresenter] Thieu view.", this); return; }
            RankScoreDecisionLog.DecisionMade += OnDecision;
            GameCommands.DebugOverlayToggleRequested += Toggle;
            view.ToggleClicked += OnButton;
            GameEvents.GameStateChanged += OnState;
            ringStart = ringCount = 0;
            lastPreset = lastDrop = lastRank = "—";
            revivePrompt = GameEvents.Current.State == GameState.RevivePrompt;
            view.SetToggleButtonVisible(Allowed && !revivePrompt);
            view.SetEmpty();
            if (RankScoreDecisionLog.HasLast) Push(RankScoreDecisionLog.Last);
            SetVisible(false);
        }

        void OnDisable()
        {
            RankScoreDecisionLog.DecisionMade -= OnDecision;
            GameCommands.DebugOverlayToggleRequested -= Toggle;
            GameEvents.GameStateChanged -= OnState;
            if (view != null) view.ToggleClicked -= OnButton;
        }

        // Khi RevivePrompt: an bang + nut RANKSCORE de khong chong len RevivePopup.
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

        void OnDecision(RankScoreDecision d) => Push(d);

        void Push(RankScoreDecision d)
        {
            if (ringCount < HistoryCapacity) { ring[(ringStart + ringCount) % HistoryCapacity] = d; ringCount++; }
            else { ring[ringStart] = d; ringStart = (ringStart + 1) % HistoryCapacity; }
            Render(d);
        }

        void Render(RankScoreDecision d)
        {
            if (view == null) return;
            view.SetQuestion("Câu hỏi: " + d.Question);
            if (d.Question == "wave_preset") lastPreset = d.Choice;
            else if (d.Question == "weapon_drop") lastDrop = d.Choice;
            else if (d.Question == "rank") lastRank = d.Choice;
            bool isReticle = d.Question != "wave_preset" && d.Question != "weapon_drop" && d.Question != "rank";
            view.SetChoice(isReticle ? $"Chọn: {d.Choice}  (reticle {d.ReticleTime:0.0#}s)" : $"Chọn: {d.Choice}");
            bool offline = d.Source == RankScoreDecisionSource.Offline;
            view.SetSource("Nguồn: " + (offline ? "Offline" : "Mặc định"), offline ? Cfg.rankScoreSourceOffline : Cfg.rankScoreSourceDefault);
            float th = Threshold;
            view.SetConfidence($"Confidence {d.Confidence:0.00} / ngưỡng {th:0.00}" + (d.Confidence >= th ? "  đạt" : "  chưa đạt"), d.Confidence, th);
            view.SetTime($"Thời điểm: {d.Timestamp:0.0}s");
            var s = d.Stats;
            view.SetStats($"Accuracy {s.Accuracy * 100f:0}%  ({s.Hits}/{s.Shots})  Miss {s.Misses}\nReaction {s.AvgReactionTime:0.00}s (n={s.ReactionSamples})\nHostage hit {s.HostageHits}  Mất mạng {s.DamageTaken}");

            var names = RankScoreConfig.ChoiceNames;
            for (int i = 0; i < RankScoreDebugPanelView.BarCount; i++)
            {
                if (i >= names.Length) { view.SetBar(i, "", 0f, Color.clear); continue; }
                float p = 0f;
                if (d.Probabilities != null) d.Probabilities.TryGetValue(names[i], out p);
                bool chosen = names[i] == d.Choice;
                view.SetBar(i, $"{names[i]} {p * 100f:0}%", p, chosen ? Cfg.rankScoreBarChosenColor : Cfg.rankScoreBarColor);
            }
            RenderHistory();
        }

        static bool IsReticle(RankScoreDecision d) => d.Question != "wave_preset" && d.Question != "weapon_drop" && d.Question != "rank";

        void RenderHistory()
        {
            sb.Clear();
            sb.Append("preset: ").Append(lastPreset).Append("  drop: ").Append(lastDrop).Append("  rank: ").Append(lastRank).Append('\n');
            sb.Append("Lịch sử:");
            for (int k = ringCount - 1; k >= 0; k--)
            {
                var h = ring[(ringStart + k) % HistoryCapacity];
                sb.Append('\n').Append(h.Timestamp.ToString("0.0")).Append("s  ").Append(IsReticle(h) ? "" : h.Question + "=").Append(h.Choice)
                  .Append(IsReticle(h) ? " " + h.ReticleTime.ToString("0.0#") + "s" : "").Append("  c=").Append(h.Confidence.ToString("0.00"))
                  .Append(h.Source == RankScoreDecisionSource.Offline ? "  Offline" : "  Mặc định");
            }
            view.SetHistory(sb.ToString());
        }
    }
}
