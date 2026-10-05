using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>Logic tinh diem thuan (khong MonoBehaviour) de test EditMode.</summary>
    public static class ScoreCalculator
    {
        /// <summary>W7: diem vu no = EnemiesKilled x ExplosionKillPoints x combo hien tai (combo khong bi doi).</summary>
        public static int ComputeBlast(GameConfig cfg, BlastReport b, float comboMultiplier, out float multiplier)
        {
            multiplier = cfg.ApplyComboMultiplier ? Mathf.Max(1f, comboMultiplier) : 1f;
            if (b.EnemiesKilled <= 0) return 0;
            return Mathf.RoundToInt(b.EnemiesKilled * cfg.ExplosionKillPoints * multiplier);
        }

        /// <summary>Diem cho mot ShotResult. Tra 0 neu khong phai Kill/JusticeKill. isJustice cho UI.</summary>
        public static int Compute(GameConfig cfg, ShotResult r, out bool isJustice, out float multiplier)
        {
            isJustice = false;
            multiplier = 1f;
            if (r.Outcome != TapOutcome.Kill && r.Outcome != TapOutcome.JusticeKill) return 0;

            // W7: ban roi luu dan = diem co dinh (khong thuong som), van nhan combo.
            if (r.Outcome == TapOutcome.Kill && r.TargetKind == TargetKind.Grenade)
            {
                if (cfg.ApplyComboMultiplier) multiplier = Mathf.Max(1f, r.ComboMultiplier);
                return Mathf.RoundToInt(cfg.GrenadeShotPoints * multiplier);
            }

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
