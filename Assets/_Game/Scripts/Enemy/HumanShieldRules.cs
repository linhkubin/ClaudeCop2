using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy
{
    public enum ShieldTap { Justice, Head, Body }

    /// <summary>Logic thuan phan loai tap len human shield (de test).</summary>
    public static class HumanShieldRules
    {
        /// <summary>Quy doi ban kinh tu px chuan (chieu rong tham chieu) sang px man hinh that.</summary>
        public static float ScaleRadius(float radiusPx, float screenWidth, float referenceWidth)
        {
            return referenceWidth > 0f ? radiusPx * screenWidth / referenceWidth : radiusPx;
        }

        /// <summary>
        /// Justice (TargetSelector da xac dinh) > dau (tap trong ban kinh quanh dau tren man hinh) > than con tin.
        /// headScreen null (khong chieu duoc, vd. khong co camera) -> coi la trung dau.
        /// </summary>
        public static ShieldTap Classify(bool isJustice, Vector2 tap, Vector2? headScreen, float headRadiusScreenPx)
        {
            if (isJustice) return ShieldTap.Justice;
            if (!headScreen.HasValue) return ShieldTap.Head;
            return Vector2.Distance(tap, headScreen.Value) <= headRadiusScreenPx ? ShieldTap.Head : ShieldTap.Body;
        }

        public static TapOutcome ToOutcome(ShieldTap t)
        {
            switch (t)
            {
                case ShieldTap.Justice: return TapOutcome.JusticeKill;
                case ShieldTap.Head: return TapOutcome.Kill;
                default: return TapOutcome.HostageHit;
            }
        }
    }
}
