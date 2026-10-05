namespace ClaudeCop.Enemy
{
    public enum EnemyState { Hidden, Peeking, Aiming, Retreating, Dead }

    /// <summary>Logic thuan (khong Unity) cua state machine enemy, de test. Enemy goi Tick moi frame.</summary>
    public class EnemyBrain
    {
        readonly float peekDuration, reticleTime, retreatDuration, hideTime, threshold;
        float timer;
        bool activated, targetable;

        public EnemyState State { get; private set; } = EnemyState.Hidden;
        /// <summary>0 = o cho nap, 1 = o vi tri Peek.</summary>
        public float PeekT { get; private set; }
        public float ReticleProgress { get; private set; }
        public float ExposedTime { get; private set; }
        /// <summary>Ban duoc khi da lo du thay: Peeking (PeekT &gt;= nguong), Aiming, Retreating (PeekT &gt;= nguong). Dead/Hidden: khong.</summary>
        public bool IsTargetable => targetable;
        public bool ShowsReticle => State == EnemyState.Aiming;
        public bool IsActivated => activated;
        public int Volleys { get; private set; }
        /// <summary>Enemy dung san (SceneStanding): ActivateStanding bo qua pha lo, vao thang Aiming; het vong thi ban roi ngam lai (khong rut xuong).</summary>
        public bool StandsGround { get; set; }

        /// <summary>Goi khi enemy vua tro nen ban duoc (de dang ky target). Luon di cap voi AimEnded.</summary>
        public System.Action AimStarted;
        /// <summary>Goi khi ban (vong het).</summary>
        public System.Action Fired;
        /// <summary>Goi khi enemy het ban duoc (chet/rut xuong duoi nguong) de huy dang ky.</summary>
        public System.Action AimEnded;

        public EnemyBrain(float peekDuration, float reticleTime, float retreatDuration, float hideTime, float targetableThreshold = 0.35f)
        {
            threshold = targetableThreshold < 0f ? 0f : (targetableThreshold > 1f ? 1f : targetableThreshold);
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

        /// <summary>Kich hoat enemy dung san: vao thang Aiming (PeekT = 1), vong target bat dau ngay. Chi tu Hidden, chua kich hoat.</summary>
        public void ActivateStanding()
        {
            if (State != EnemyState.Hidden || activated) return;
            activated = true;
            State = EnemyState.Aiming;
            PeekT = 1f; timer = 0f; ReticleProgress = 0f; ExposedTime = 0f;
            BeginTargetable();
        }

        public bool Kill()
        {
            if (!targetable) return false;
            State = EnemyState.Dead;
            EndTargetable();
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
                    if (!targetable && PeekT >= threshold) BeginTargetable();
                    if (targetable) ExposedTime = timer - threshold * peekDuration > 0f ? timer - threshold * peekDuration : 0f;
                    if (PeekT >= 1f)
                    {
                        State = EnemyState.Aiming;
                        ReticleProgress = 0f; timer = 0f;
                    }
                    break;
                case EnemyState.Aiming:
                    ExposedTime += dt;
                    timer += dt;
                    ReticleProgress = timer >= reticleTime ? 1f : timer / reticleTime;
                    if (ReticleProgress >= 1f)
                    {
                        Volleys++;
                        timer = 0f;
                        if (StandsGround) { ReticleProgress = 0f; Fired?.Invoke(); break; }
                        State = EnemyState.Retreating;
                        Fired?.Invoke();
                    }
                    break;
                case EnemyState.Retreating:
                    timer += dt;
                    PeekT = timer >= retreatDuration ? 0f : 1f - timer / retreatDuration;
                    if (targetable) { ExposedTime += dt; if (PeekT < threshold || PeekT <= 0f) EndTargetable(); }
                    if (PeekT <= 0f) { State = EnemyState.Hidden; timer = 0f; ReticleProgress = 0f; }
                    break;
            }
        }

        void BeginTargetable()
        {
            if (targetable) return;
            targetable = true; ExposedTime = 0f;
            AimStarted?.Invoke();
        }

        void EndTargetable()
        {
            if (!targetable) return;
            targetable = false;
            AimEnded?.Invoke();
        }

        void StartPeek() { State = EnemyState.Peeking; timer = 0f; PeekT = 0f; ReticleProgress = 0f; ExposedTime = 0f; }
    }
}
