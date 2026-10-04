#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Camera.DebugTools
{
    /// <summary>EncounterBase gia cho Sandbox: tu Cleared sau clearAfter giay ke tu Begin().</summary>
    public class DebugTimedEncounter : EncounterBase
    {
        public float clearAfter = 3f;

        bool active, cleared;
        float timer;

        public override bool IsActive => active;
        public override bool IsCleared => cleared;
        public override string Description => "Debug encounter (" + clearAfter + "s)";

        public override void Begin()
        {
            active = true; cleared = false; timer = 0f;
        }

        void Update()
        {
            if (!active) return;
            timer += Time.deltaTime;
            if (timer >= clearAfter)
            {
                active = false; cleared = true;
                RaiseCleared(transform.position);
            }
        }
    }
}
#endif
