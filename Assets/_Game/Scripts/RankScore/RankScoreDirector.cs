using System;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Enemy;

namespace ClaudeCop.RankScore
{
    /// <summary>
    /// Khi Shot di chuyen bat dau (RailEvents.MoveSegmentStarted) toi mot EncounterWave: hoi RankScore (Offline mac dinh)
    /// wave_preset + reticle_time + weapon_drop dua tren PlayerStatsTracker, ghi RankScoreDecisionLog, roi ap vao wave TRUOC khi wave Begin().
    /// Khong chan gameplay. Qua timeout / RankScore tat / wave da Begin => giu mac dinh cua wave (khong ap).
    /// Khi RailEvents.LevelCompleted: danh gia rank cuoi man (ScoreRankBoard).
    /// </summary>
    public class RankScoreDirector : MonoBehaviour
    {
        /// <summary>Tien to ten con cua wave danh dau diem spawn thung vu khi.</summary>
        public const string PickupSpawnPrefix = "PickupSpawn";

        [SerializeField] RankScoreConfig config;
        [SerializeField] PlayerStatsTracker tracker;
        [Header("wave_preset (calm / standard / intense / hostage_heavy)")]
        [SerializeField] EnemyPreset presetCalm;
        [SerializeField] EnemyPreset presetStandard;
        [SerializeField] EnemyPreset presetIntense;
        [SerializeField] EnemyPreset presetHostageHeavy;
        [Header("weapon_drop (thung vu khi)")]
        [SerializeField] GameObject shotgunPickupPrefab;
        [SerializeField] GameObject machineGunPickupPrefab;

        IRankScoreClient client;
        RankScoreCall pending;
        bool subscribed;
        int wavesAsked; // so dot da hoi trong luot nay: dot dau (Phase 1) luon dung mac dinh cho preset/weapon_drop

        /// <summary>Dot dau luot choi chua co du lieu nguoi choi: giu cau hinh lam san.</summary>
        public int WavesAsked => wavesAsked;
        /// <summary>Bat dau luot moi (Title/Win/GameOver hoac goi tay trong test).</summary>
        public void ResetRun() { wavesAsked = 0; }

        /// <summary>Seam cho test; mac dinh goi wave.ApplyReticleTime.</summary>
        public Action<EncounterWave, float> ReticleApplier = (w, t) => w.ApplyReticleTime(t);
        /// <summary>Seam cho test: ap wave_preset (ten lua chon). Mac dinh tra cuu EnemyPreset da gan roi wave.ApplyPreset.</summary>
        public Action<EncounterWave, string> PresetApplier;
        /// <summary>Seam cho test: ap weapon_drop (none/shotgun/machinegun).</summary>
        public Action<EncounterWave, string> WeaponDropApplier;
        /// <summary>Seam cho test: wave co diem PickupSpawn khong. Mac dinh quet ten con.</summary>
        public Func<EncounterWave, bool> HasPickupSpawn = DefaultHasPickupSpawn;

        public RankScoreConfig Config => config;

        public void Configure(RankScoreConfig cfg, PlayerStatsTracker statsTracker, IRankScoreClient rankScoreClient = null)
        {
            config = cfg; tracker = statsTracker; client = rankScoreClient;
        }

        void OnEnable() { Subscribe(); }
        void OnDisable() { Unsubscribe(); }

        public void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;
            RailEvents.MoveSegmentStarted += OnMoveSegmentStarted;
            RailEvents.LevelCompleted += OnLevelCompleted;
            GameEvents.GameStateChanged += OnGameState;
        }

        public void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            RailEvents.MoveSegmentStarted -= OnMoveSegmentStarted;
            RailEvents.LevelCompleted -= OnLevelCompleted;
            GameEvents.GameStateChanged -= OnGameState;
            CancelPending();
        }

        void OnGameState(GameState s)
        {
            if (s == GameState.Title || s == GameState.Win || s == GameState.GameOver) wavesAsked = 0;
        }

        void CancelPending()
        {
            if (pending != null) { pending.Cancel(); pending = null; }
        }

        IRankScoreClient Client
        {
            get
            {
                if (client == null)
                    client = new OfflineRankScoreClient(config,
                        () => tracker != null ? tracker.Snapshot() : default,
                        () => tracker != null ? tracker.SnapshotLevel() : default);
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

            bool firstWave = wavesAsked == 0;
            wavesAsked++;
            PlayerStats stats = tracker != null ? tracker.Snapshot() : default;
            PlayerStats level = tracker != null ? tracker.SnapshotLevel() : default;
            var request = OfflineRankScoreClient.BuildWaveRequest(stats, level);
            RankScoreCall call = null;
            call = Client.Ask(request, config.timeoutSeconds, response =>
            {
                // Bo qua phan hoi cu (da bi huy / thay bang cuoc goi moi).
                if (call != null && call.IsCancelled) return;
                OnResponse(wave, response, stats, level, firstWave);
            });
            pending = call != null && !call.IsDone ? call : null;
        }

        void OnResponse(EncounterWave wave, RankScoreResponse response, PlayerStats stats, PlayerStats level, bool firstWave = false)
        {
            pending = null;
            float now = Time.realtimeSinceStartup;
            bool applicable = wave != null && !wave.IsActive && !wave.IsCleared; // tre: giu mac dinh

            // wave_preset truoc (preset co reticleTime rieng), reticle_time sau de RankScore ghi de len.
            if (HasAnswer(response, OfflineRankScoreClient.QuestionWavePreset))
            {
                var d = RankScorePolicy.DecideWavePreset(response, level, config, now);
                RankScoreDecisionLog.Record(d);
                if (applicable && !firstWave && d.Source != RankScoreDecisionSource.Default) (PresetApplier ?? ApplyPresetByName)(wave, d.Choice);
            }

            var decision = RankScorePolicy.Decide(response, stats, config, now);
            RankScoreDecisionLog.Record(decision);
            if (applicable && decision.Source != RankScoreDecisionSource.Default) ReticleApplier?.Invoke(wave, decision.ReticleTime);

            if (HasAnswer(response, OfflineRankScoreClient.QuestionWeaponDrop))
            {
                var d = RankScorePolicy.DecideWeaponDrop(response, level, config, now);
                RankScoreDecisionLog.Record(d);
                if (applicable && !firstWave && d.Source != RankScoreDecisionSource.Default && (HasPickupSpawn == null || HasPickupSpawn(wave)))
                    (WeaponDropApplier ?? ApplyWeaponDropByName)(wave, d.Choice);
            }
        }

        static bool HasAnswer(RankScoreResponse r, string q) =>
            r != null && r.Answers != null && r.Answers.ContainsKey(q);

        // ---------- Rank cuoi man ----------

        /// <summary>Danh gia rank khi thang (LevelCompleted). Public de test.</summary>
        public void OnLevelCompleted()
        {
            if (tracker == null) return;
            var level = tracker.SnapshotLevel();
            var result = ScoreRankEvaluator.Evaluate(level, config);
            RankScoreDecisionLog.Record(RankScorePolicy.RankDecision(result, level, Time.realtimeSinceStartup));
            ScoreRankBoard.Publish(result);
        }

        // ---------- Ap mac dinh (chi dung API EncounterWave co san) ----------

        void ApplyPresetByName(EncounterWave wave, string choice)
        {
            EnemyPreset p = null;
            switch (choice)
            {
                case RankScoreConfig.PresetCalm: p = presetCalm; break;
                case RankScoreConfig.PresetStandard: p = presetStandard; break;
                case RankScoreConfig.PresetIntense: p = presetIntense; break;
                case RankScoreConfig.PresetHostageHeavy: p = presetHostageHeavy; break;
            }
            if (p != null) wave.ApplyPreset(p);
        }

        void ApplyWeaponDropByName(EncounterWave wave, string choice)
        {
            switch (choice)
            {
                case RankScoreConfig.DropNone: wave.SetPickupPrefab(null); break;
                case RankScoreConfig.DropShotgun: if (shotgunPickupPrefab != null) wave.SetPickupPrefab(shotgunPickupPrefab); break;
                case RankScoreConfig.DropMachineGun: if (machineGunPickupPrefab != null) wave.SetPickupPrefab(machineGunPickupPrefab); break;
            }
        }

        static bool DefaultHasPickupSpawn(EncounterWave wave)
        {
            if (wave == null) return false;
            foreach (var t in wave.GetComponentsInChildren<Transform>(true))
                if (t != wave.transform && t.name.StartsWith(PickupSpawnPrefix, StringComparison.Ordinal)) return true;
            return false;
        }
    }
}
