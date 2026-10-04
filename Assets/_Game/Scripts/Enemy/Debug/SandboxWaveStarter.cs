#if UNITY_EDITOR || DEVELOPMENT_BUILD
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy.Debugging
{
    /// <summary>Sandbox: goi Begin() cua encounter khi Start va log Cleared.</summary>
    public class SandboxWaveStarter : MonoBehaviour
    {
        [SerializeField] EncounterBase encounter;
        void Start()
        {
            if (encounter == null) return;
            encounter.Cleared += (e, p) => Debug.Log("[Sandbox] Cleared " + e.Id + " lastKill=" + p);
            encounter.Begin();
        }
    }
}
#endif
