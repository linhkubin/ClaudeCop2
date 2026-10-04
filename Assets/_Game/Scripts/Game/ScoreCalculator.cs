using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>Logic tinh diem thuan (khong MonoBehaviour) de test EditMode.</summary>
    public static class ScoreCalculator
    {
        /// <summary>Diem cho mot ShotResult. Tra 0 neu khong phai Kill/JusticeKill. isJustice cho UI.</summary>
        public static int Compute(GameConfig cfg, ShotResult r, out bool isJustice, out float multiplier)
        {
            isJustice = false;
            multiplier = 1f;
            if (r.Outcome != TapOutcome.Kill && r.Outcome != TapOutcome.JusticeKill) return 0;

            float progress = Mathf.Clamp01(r.ReticleProgress);
            float basePoints;
            if (r.Outcome == TapOutcome.JusticeKill)
            {
                isJustice = true;
                basePoints = cfg.JusticePoints;
                if (progress < cfg.GreenThreshold) basePoints *= cfg.JusticeGreenMultiplier;
            }
            else
            {
                basePoints = cfg.KillPoints + cfg.EarlyBonusMax * (1f - progress);
            }

            if (cfg.ApplyComboMultiplier) multiplier = Mathf.Max(1f, r.ComboMultiplier);
            // Moi muc tieu cua Shotgun sinh mot ShotResult rieng => khong nhan them TargetsHit (tranh n^2).
            return Mathf.RoundToInt(basePoints * multiplier);
        }
    }
}
