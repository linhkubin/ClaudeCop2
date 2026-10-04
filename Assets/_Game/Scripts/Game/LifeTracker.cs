namespace ClaudeCop.Game
{
    /// <summary>Logic mang + bat tu thuan, test EditMode. Thoi gian truyen vao (giay).</summary>
    public sealed class LifeTracker
    {
        readonly float invulnerableSeconds;
        float invulnerableUntil = float.NegativeInfinity;

        public int Lives { get; private set; }
        public int MaxLives { get; }

        public LifeTracker(int maxLives, float invulnerableSeconds)
        {
            MaxLives = maxLives;
            this.invulnerableSeconds = invulnerableSeconds;
            Lives = maxLives;
        }

        public bool IsInvulnerable(float now) => now < invulnerableUntil;
        public bool IsDead => Lives <= 0;

        public void Reset(int lives = -1)
        {
            Lives = lives < 0 ? MaxLives : System.Math.Min(lives, MaxLives);
            invulnerableUntil = float.NegativeInfinity;
        }

        /// <summary>Bat tu tu now den now + seconds (vd. an han sau revive).</summary>
        public void GrantInvulnerability(float now, float seconds) => invulnerableUntil = now + seconds;

        /// <summary>Tru 1 mang. Tra false neu dang bat tu / da chet (khong tru).</summary>
        public bool TryDamage(float now)
        {
            if (IsDead || IsInvulnerable(now)) return false;
            Lives--;
            invulnerableUntil = now + invulnerableSeconds;
            return true;
        }
    }
}
