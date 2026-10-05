namespace ClaudeCop.Game
{
    /// <summary>Logic mang + bat tu thuan, test EditMode. Thoi gian truyen vao (giay).</summary>
    public sealed class LifeTracker
    {
        readonly float invulnerableSeconds;
        float invulnerableUntil = float.NegativeInfinity;

        public int Lives { get; private set; }
        public int MaxLives { get; }
        /// <summary>Giap: moi diem do 1 phat (khong mat mang, van bat tu nhu trung dan).</summary>
        public int Armor { get; private set; }

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

        public void AddArmor(int n) { if (n > 0) Armor += n; }
        /// <summary>Nap giap toi it nhat n (ao chong dan moi Stage).</summary>
        public void TopUpArmor(int n) { if (Armor < n) Armor = n; }
        /// <summary>true neu phat vua roi bi giap do (TryDamage tra false).</summary>
        public bool LastAbsorbed { get; private set; }

        /// <summary>Tru 1 mang. Tra false neu dang bat tu / da chet / giap do (khong tru mang).</summary>
        public bool TryDamage(float now)
        {
            LastAbsorbed = false;
            if (IsDead || IsInvulnerable(now)) return false;
            if (Armor > 0)
            {
                Armor--;
                LastAbsorbed = true;
                invulnerableUntil = now + invulnerableSeconds;
                return false;
            }
            Lives--;
            invulnerableUntil = now + invulnerableSeconds;
            return true;
        }
    }
}
