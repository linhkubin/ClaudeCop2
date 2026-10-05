#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Ads;
using ClaudeCop.RankScore;

namespace ClaudeCop.UI
{
    /// <summary>Script debug cho Sandbox: tu doi gia tri cac View de nhin bang mat. Khong dung trong gameplay.</summary>
    public class UIDebugDriver : MonoBehaviour
    {
        [SerializeField] HudView hud;
        [SerializeField] TargetReticleView[] reticles;
        [SerializeField] DamageFlashView flash;
        [SerializeField] WinView win;
        [SerializeField] GameOverView gameOver;
        [SerializeField] FakeRewardedAd ad;
        [SerializeField] float reticleCycleSeconds = 2.5f;

        float timer;
        int score, lives = 3, ammo = 6;
        float nextEvent;

        void Start()
        {
            if (hud != null) { hud.SetScore(0); hud.SetLives(3, 3); hud.SetAmmo(6, 6); hud.SetWeapon(WeaponKind.Pistol); hud.SetReloading(false); }
        }

        void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (fakeGrenade != null && fakeGrenade.ExposedTime > fakeGrenade.Duration) fakeGrenade.Start = Time.time;
            if (reticles != null)
            {
                for (int i = 0; i < reticles.Length; i++)
                {
                    if (reticles[i] == null) continue;
                    float p = Mathf.Repeat(timer / reticleCycleSeconds + i * 0.33f, 1f);
                    reticles[i].SetProgress(p);
                    reticles[i].SetScreenPosition(new Vector2(Screen.width * (0.3f + 0.2f * i), Screen.height * (0.55f - 0.08f * i)));
                }
            }
            if (timer >= nextEvent) { nextEvent = timer + 1.5f; Tick(); }
        }

        void Tick()
        {
            score += 100; ammo--; if (ammo < 0) ammo = 6;
            if (hud == null) return;
            hud.SetScore(score);
            hud.SetAmmo(ammo, 6);
            hud.SetReloading(ammo == 0);
            hud.SetWeapon((WeaponKind)((score / 300) % 3));
            if (score % 500 == 0) { lives--; if (lives < 0) lives = 3; hud.SetLives(lives, 3); if (flash != null) flash.Flash(); }
        }

        // ---- Grenade gia (T-611): thu vong cam + banner khong can T-602 ----
        sealed class FakeGrenade : ITapTarget
        {
            public Vector3 Pos; public float Start, Duration;
            public int Id => 900001;
            public TargetKind Kind => TargetKind.Grenade;
            public bool IsTargetable => true;
            public Vector3 AimPoint => Pos;
            public bool HasJusticePoint => false;
            public Vector3 JusticePoint => Pos;
            public bool ShowsReticle => true;
            public float ReticleProgress => Mathf.Clamp01(ExposedTime / Duration);
            public float ExposedTime => Time.time - Start;
            public TapOutcome OnTapHit(ShotInfo shot, bool isJustice) => TapOutcome.Kill;
        }

        FakeGrenade fakeGrenade;

        [ContextMenu("Toggle Fake Grenade")]
        public void ToggleFakeGrenade()
        {
            if (fakeGrenade != null) { TargetRegistry.Unregister(fakeGrenade); fakeGrenade = null; return; }
            var cam = UnityEngine.Camera.main;
            Vector3 p = cam != null ? cam.transform.position + cam.transform.forward * 8f + cam.transform.up * 0.5f : new Vector3(0, 1, 8);
            fakeGrenade = new FakeGrenade { Pos = p, Start = Time.time, Duration = 3f };
            TargetRegistry.Register(fakeGrenade);
        }

        void OnDisable() { if (fakeGrenade != null) { TargetRegistry.Unregister(fakeGrenade); fakeGrenade = null; } }

        void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 220, 70), fakeGrenade == null ? "Spawn fake grenade" : "Remove fake grenade")) ToggleFakeGrenade();
            if (GUI.Button(new Rect(10, 90, 220, 70), "ReduceMotion: " + UserSettings.ReduceMotion)) UserSettings.ReduceMotion = !UserSettings.ReduceMotion;
            if (GUI.Button(new Rect(10, 170, 100, 70), "Win S")) ShowWinRank(ScoreRank.S, ScoreWeakness.None);
            if (GUI.Button(new Rect(120, 170, 100, 70), "Win A")) ShowWinRank(ScoreRank.A, ScoreWeakness.Reaction);
            if (GUI.Button(new Rect(10, 250, 100, 70), "Win B")) ShowWinRank(ScoreRank.B, ScoreWeakness.Accuracy);
            if (GUI.Button(new Rect(120, 250, 100, 70), "Win C")) ShowWinRank(ScoreRank.C, ScoreWeakness.Hostage);
            if (GUI.Button(new Rect(10, 330, 220, 70), "Fake blast")) FakeBlast();
        }

        [ContextMenu("Show Win")] public void ShowWin() { if (win != null) win.Show(score); }
        /// <summary>Gia lap man Win voi rank + diem yeu (de chup anh).</summary>
        public void ShowWinRank(ScoreRank rank, ScoreWeakness weak)
        {
            if (win == null) return;
            var r = new ScoreRankResult
            {
                Rank = rank, Weakness = weak, WeaknessText = ScoreRankEvaluator.TextFor(weak),
                Accuracy = rank == ScoreRank.S ? 0.96f : rank == ScoreRank.A ? 0.84f : rank == ScoreRank.B ? 0.68f : 0.41f,
                AvgReactionTime = 1.35f, DamageTaken = (int)rank, HostageHits = weak == ScoreWeakness.Hostage ? 3 : 0,
                RevivesUsed = rank >= ScoreRank.B ? 1 : 0, BlastKills = 4
            };
            win.Show(123450 + 1000 * (int)rank);
            win.ShowRank(r);
        }

        public void FakeBlast()
        {
            var cam = UnityEngine.Camera.main;
            Vector3 p = cam != null ? cam.transform.position + cam.transform.forward * 8f : new Vector3(0, 1, 8);
            BlastEvents.Raise(new BlastReport { Center = p, Radius = 3f, EnemiesKilled = 3, HostagesHit = 0, SourceId = 1 });
        }

        [ContextMenu("Show GameOver")] public void ShowGameOver() { if (gameOver != null) gameOver.Show(score); }
        [ContextMenu("Show Fake Ad")] public void ShowAd() { if (ad != null) ad.Show(() => Debug.Log("[UIDebug] Ad rewarded"), () => Debug.Log("[UIDebug] Ad failed")); }
        [ContextMenu("Flash")] public void Flash() { if (flash != null) flash.Flash(); }
    }
}

#endif
