#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Ads;

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

        [ContextMenu("Show Win")] public void ShowWin() { if (win != null) win.Show(score); }
        [ContextMenu("Show GameOver")] public void ShowGameOver() { if (gameOver != null) gameOver.Show(score); }
        [ContextMenu("Show Fake Ad")] public void ShowAd() { if (ad != null) ad.Show(() => Debug.Log("[UIDebug] Ad rewarded"), () => Debug.Log("[UIDebug] Ad failed")); }
        [ContextMenu("Flash")] public void Flash() { if (flash != null) flash.Flash(); }
    }
}

#endif
