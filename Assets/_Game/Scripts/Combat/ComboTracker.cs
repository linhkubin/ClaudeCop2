using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Combat
{
    /// <summary>
    /// Logic combo thuan (khong Unity runtime) de test. Streak tang 1 moi phat trung (Kill/JusticeKill);
    /// Miss/HostageHit/Environment thuong reset. Multiplier = clamp(1 + (streak-1)/hitsPerStep, 1, max): phat 1 = x1, phat 2 = x2...
    /// Pickup/Blocked trung tinh. Environment trung vat IShootable (envKeepsCombo) giu combo (plan muc 14).
    /// </summary>
    public sealed class ComboTracker
    {
        readonly int maxMultiplier, hitsPerStep;
        public int Streak { get; private set; }
        /// <summary>So lan truot/ban moi truong duoc bo qua khong reset combo (gang tay). Tru dan khi dung.</summary>
        public int ForgiveCharges { get; set; }

        public ComboTracker(int maxMultiplier = CombatConfig.DefaultComboMaxMultiplier, int hitsPerStep = CombatConfig.DefaultComboHitsPerStep)
        {
            this.maxMultiplier = Mathf.Max(1, maxMultiplier);
            this.hitsPerStep = Mathf.Max(1, hitsPerStep);
        }

        public float Multiplier => Streak <= 0 ? 1f : Mathf.Clamp(1 + (Streak - 1) / hitsPerStep, 1, maxMultiplier);

        /// <returns>true neu streak thay doi.</returns>
        public bool Apply(TapOutcome outcome, bool envKeepsCombo = false)
        {
            int before = Streak;
            switch (outcome)
            {
                case TapOutcome.Kill:
                case TapOutcome.JusticeKill:
                    Streak++; break;
                case TapOutcome.PickupCollected:
                case TapOutcome.Blocked:
                    break;
                case TapOutcome.Environment:
                    if (!envKeepsCombo && !Forgive()) Streak = 0;
                    break;
                case TapOutcome.Miss:
                    if (!Forgive()) Streak = 0;
                    break;
                default: // HostageHit: khong bao gio duoc bo qua
                    Streak = 0; break;
            }
            return Streak != before;
        }

        bool Forgive()
        {
            if (Streak <= 0 || ForgiveCharges <= 0) return false;
            ForgiveCharges--;
            return true;
        }

        public bool Reset()
        {
            bool changed = Streak != 0;
            Streak = 0;
            return changed;
        }
    }
}
