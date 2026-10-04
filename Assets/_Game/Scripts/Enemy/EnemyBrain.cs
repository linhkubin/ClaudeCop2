namespace ClaudeCop.Enemy
{
    public enum EnemyState { Hidden, Peeking, Aiming, Retreating, Dead }

    /// <summary>Logic thuan (khong Unity) cua state machine enemy, de test. Enemy goi Tick moi frame.</summary>
    public class EnemyBrain
    {
        readonly float peekDuration, reticleTime, retreatDuration, hideTime;
        float timer;
        bool activated;

        public EnemyState State { get; private set; } = EnemyState.Hidden;
        /// <summary>0 = o cho nap, 1 = o vi tri Peek.</summary>
        public float PeekT { get; private set; }
        public float ReticleProgress { get; private set; }
        public float ExposedTime { get; private set; }
        public bool IsTargetable => State == EnemyState.Aiming;
        public bool ShowsReticle => State == EnemyState.Aiming;
        public bool IsActivated => activated;
        public int Volleys { get; private set; }

        /// <summary>Goi khi vua bat dau Aiming (de dang ky target).</summary>
        public System.Action AimStarted;
        /// <summary>Goi khi ban (vong het).</summary>
        public System.Action Fired;
        /// <summary>Goi khi roi Aiming (an/chet) de huy dang ky.</summary>
        public System.Action AimEnded;

        public EnemyBrain(float peekDuration, float reticleTime, float retreatDuration, float hideTime)
        {
            this.peekDuration = peekDuration > 0.001f ? peekDuration : 0.001f;
            this.reticleTime = reticleTime > 0.001f ? reticleTime : 0.001f;
            this.retreatDuration = retreatDuration > 0.001f ? retreatDuration : 0.001f;
            this.hideTime = hideTime;
        }

        /// <summary>Bat dau lo ra (tu Hidden).</summary>
        public void Activate()
        {
            if (State != EnemyState.Hidden || activated) return;
            activated = true;
            StartPeek();
        }

        public bool Kill()
        {
            if (State != EnemyState.Aiming) return false;
            AimEnded?.Invoke();
            State = EnemyState.Dead;
            return true;
        }

        public void Tick(float dt)
        {
            switch (State)
            {
                case EnemyState.Hidden:
                    if (!activated) break;
                    timer += dt;
                    if (timer >= hideTime) StartPeek();
                    break;
                case EnemyState.Peeking:
                    timer += dt;
                    PeekT = timer >= peekDuration ? 1f : timer / peekDuration;
                    if (PeekT >= 1f)
                    {
                        State = EnemyState.Aiming;
                        ReticleProgress = 0f; ExposedTime = 0f; timer = 0f;
                        AimStarted?.Invoke();
                    }
                    break;
                case EnemyState.Aiming:
                    ExposedTime += dt;
                    timer += dt;
                    ReticleProgress = timer >= reticleTime ? 1f : timer / reticleTime;
                    if (ReticleProgress >= 1f)
                    {
                        AimEnded?.Invoke();
                        Volleys++;
                        State = EnemyState.Retreating; timer = 0f;
                        Fired?.Invoke();
                    }
                    break;
                case EnemyState.Retreating:
                    timer += dt;
                    PeekT = timer >= retreatDuration ? 0f : 1f - timer / retreatDuration;
                    if (PeekT <= 0f) { State = EnemyState.Hidden; timer = 0f; ReticleProgress = 0f; }
                    break;
            }
        }

        void StartPeek() { State = EnemyState.Peeking; timer = 0f; PeekT = 0f; ReticleProgress = 0f; }
    }
}
