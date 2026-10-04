#if UNITY_EDITOR || DEVELOPMENT_BUILD
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy.Debugging
{
    /// <summary>Receiver gia cho Sandbox: log khi bi ban.</summary>
    public class FakePlayerDamageReceiver : MonoBehaviour, IPlayerDamageReceiver
    {
        public int hits;
        void OnEnable() => PlayerDamageService.Register(this);
        void OnDisable() => PlayerDamageService.Unregister(this);
        public void Damage(DamageSource source, Vector3 worldPosition)
        {
            hits++;
            Debug.Log("[FakeReceiver] Bi ban " + source + " tai " + worldPosition + " (tong " + hits + ")");
        }
    }
}
#endif
