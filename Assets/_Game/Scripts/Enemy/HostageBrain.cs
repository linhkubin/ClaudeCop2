namespace ClaudeCop.Enemy
{
    public enum HostageState { Hidden, Peeking, Exposed, Retreating, Left, Shot }

    /// <summary>
    /// State machine thuan cua con tin: Hidden -> Peeking -> Exposed (tap duoc, KHONG co vong target) -> Retreating -> Left.
    /// Bi ban (Shoot) khi Exposed -> Shot (bien mat). Khong bao gio lo lai sau khi roi di.
    /// </summary>
    public class HostageBrain
    {
        readonly float peekDuration, exposeTime, retreatDuration;
        float timer;
        bool activated;

        public HostageState State { get; private set; } = HostageState.Hidden;
        /// <summary>0 = o cho nap, 1 = o vi tri lo ra.</summary>
        public float PeekT { get; private set; }
        public float ExposedTime { get; private set; }
        public bool IsTargetable => State == HostageState.Exposed;
        public bool IsActivated => activated;
        /// <summary>Con tin dung im: Exposed khong tu het gio thut vao (cho toi khi dot xong / bi ban).</summary>
        public bool StaysPut { get; set; }
        public bool IsFinished => State == HostageState.Left || State == HostageState.Shot;

        /// <summary>Vua vao Exposed (dang ky target).</summary>
        public System.Action ExposeStarted;
        /// <summary>Roi Exposed (bi ban hoac bat dau thut vao) de huy dang ky.</summary>
        public System.Action ExposeEnded;

        public HostageBrain(float peekDuration, float exposeTime, float retreatDuration)
        {
            this.peekDuration = peekDuration > 0.001f ? peekDuration : 0.001f;
            this.exposeTime = exposeTime > 0.001f ? exposeTime : 0.001f;
            this.retreatDuration = retreatDuration > 0.001f ? retreatDuration : 0.001f;
        }

        public void Activate()
        {
            if (activated || State != HostageState.Hidden) return;
            activated = true;
            State = HostageState.Peeking; timer = 0f; PeekT = 0f;
        }

        /// <summary>Bi ban trung. Tra true neu dang Exposed.</summary>
        public bool Shoot()
        {
            if (State != HostageState.Exposed) return false;
            ExposeEnded?.Invoke();
            State = HostageState.Shot;
            return true;
        }

        /// <summary>Huy bo (dot da xong): roi di ngay, khong hoat canh.</summary>
        public void Dismiss()
        {
            if (IsFinished) return;
            if (State == HostageState.Exposed) ExposeEnded?.Invoke();
            State = HostageState.Left;
            PeekT = 0f;
        }

        public void Tick(float dt)
        {
            switch (State)
            {
                case HostageState.Peeking:
                    timer += dt;
                    PeekT = timer >= peekDuration ? 1f : timer / peekDuration;
                    if (PeekT >= 1f)
                    {
                        State = HostageState.Exposed; timer = 0f; ExposedTime = 0f;
                        ExposeStarted?.Invoke();
                    }
                    break;
                case HostageState.Exposed:
                    timer += dt; ExposedTime += dt;
                    if (!StaysPut && timer >= exposeTime)
                    {
                        ExposeEnded?.Invoke();
                        State = HostageState.Retreating; timer = 0f;
                    }
                    break;
                case HostageState.Retreating:
                    timer += dt;
                    PeekT = timer >= retreatDuration ? 0f : 1f - timer / retreatDuration;
                    if (PeekT <= 0f) State = HostageState.Left;
                    break;
            }
        }
    }
}
