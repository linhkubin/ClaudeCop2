using System;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Enemy;

namespace ClaudeCop.Jev
{
    /// <summary>
    /// Khi Shot di chuyen bat dau (RailEvents.MoveSegmentStarted) toi mot EncounterWave: hoi Jev (Offline mac dinh)
    /// reticle_time dua tren PlayerStatsTracker, ghi JevDecisionLog, roi ap vao wave TRUOC khi wave Begin().
    /// Khong chan gameplay. Qua timeout / Jev tat / wave da Begin => giu mac dinh cua wave (khong ap).
    /// </summary>
    public class JevDirector : MonoBehaviour
    {
        [SerializeField] JevConfig config;
        [SerializeField] PlayerStatsTracker tracker;

        IJevClient client;
        JevCall pending;
        bool subscribed;

        /// <summary>Seam cho test; mac dinh goi wave.ApplyReticleTime.</summary>
        public Action<EncounterWave, float> ReticleApplier = (w, t) => w.ApplyReticleTime(t);

        public JevConfig Config => config;

        public void Configure(JevConfig cfg, PlayerStatsTracker statsTracker, IJevClient jevClient = null)
        {
            config = cfg; tracker = statsTracker; client = jevClient;
        }

        void OnEnable() { Subscribe(); }
        void OnDisable() { Unsubscribe(); }

        public void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;
            RailEvents.MoveSegmentStarted += OnMoveSegmentStarted;
        }

        public void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            RailEvents.MoveSegmentStarted -= OnMoveSegmentStarted;
            CancelPending();
        }

        void CancelPending()
        {
            if (pending != null) { pending.Cancel(); pending = null; }
        }

        IJevClient Client
        {
            get
            {
                if (client == null)
                    client = new OfflineJevClient(config, () => tracker != null ? tracker.Snapshot() : default);
                return client;
            }
        }

        /// <summary>Public de test goi truc tiep.</summary>
        public void OnMoveSegmentStarted(EncounterBase upcoming)
        {
            CancelPending();
            var wave = upcoming as EncounterWave;
            if (wave == null || wave.IsActive || wave.IsCleared) return;
            if (config == null || !config.enabled) return;

            PlayerStats stats = tracker != null ? tracker.Snapshot() : default;
            var request = OfflineJevClient.BuildReticleRequest(stats);
            JevCall call = null;
            call = Client.Ask(request, config.timeoutSeconds, response =>
            {
                // Bo qua phan hoi cu (da bi huy / thay bang cuoc goi moi).
                if (call != null && call.IsCancelled) return;
                OnResponse(wave, response, stats);
            });
            pending = call != null && !call.IsDone ? call : null;
        }

        void OnResponse(EncounterWave wave, JevResponse response, PlayerStats stats)
        {
            pending = null;
            var decision = JevPolicy.Decide(response, stats, config, Time.realtimeSinceStartup);
            JevDecisionLog.Record(decision);
            if (wave == null || wave.IsActive || wave.IsCleared) return; // tre: giu mac dinh
            if (decision.Source == JevDecisionSource.Default) return;    // khong co y kien => de wave dung cau hinh cua no
            ReticleApplier?.Invoke(wave, decision.ReticleTime);
        }
    }
}
