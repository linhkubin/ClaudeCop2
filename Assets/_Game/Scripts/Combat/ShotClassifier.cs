using ClaudeCop.Core;

namespace ClaudeCop.Combat
{
    /// <summary>
    /// Logic thuan phan loai phat ban khong trung muc tieu tap (C9): Environment CHI khi trung vat IShootable
    /// (giu combo); tuong/san tro hoac khong trung gi -> Miss (reset combo).
    /// </summary>
    public static class ShotClassifier
    {
        public static TapOutcome ClassifyEnvironment(bool rayHit, bool hitShootable)
            => rayHit && hitShootable ? TapOutcome.Environment : TapOutcome.Miss;

        /// <summary>Environment luon giu combo (vi chi phat khi trung IShootable).</summary>
        public static bool KeepsCombo(TapOutcome outcome) => outcome == TapOutcome.Environment;
    }
}
