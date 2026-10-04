#if UNITY_EDITOR || DEVELOPMENT_BUILD
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy.Debugging
{
    /// <summary>Sandbox: tu ban mot muc tieu trong TargetRegistry moi 'killAfter' s (de thu Kill). killAfter &lt; 0 de tat.</summary>
    public class SandboxAutoShooter : MonoBehaviour
    {
        public float killAfter = -1f;
        float t;
        void Update()
        {
            if (killAfter < 0f) return;
            t += Time.deltaTime;
            if (t < killAfter || TargetRegistry.Count == 0) return;
            t = 0f;
            var target = TargetRegistry.Targets[0];
            var r = target.OnTapHit(new ShotInfo { Direction = Vector3.forward, HitPoint = target.AimPoint }, false);
            Debug.Log("[AutoShooter] Tap -> " + r);
        }
    }
}
#endif
