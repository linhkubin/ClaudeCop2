using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Props
{
    /// <summary>Hop/thung carton/lon: nhan dan thi bay theo huong dan. Khong bi pha huy o M3. Rigidbody tu ngu khi nam yen.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody))]
    public sealed class PhysicsProp : MonoBehaviour, IShootable
    {
        [SerializeField] PropConfig config;
        Rigidbody body;

        PropConfig Cfg => config != null ? config : PropConfig.Fallback;

        void Awake() { body = GetComponent<Rigidbody>(); }

        public void OnShot(ShotInfo shot)
        {
            if (body == null) body = GetComponent<Rigidbody>();
            body.WakeUp();
            body.AddForceAtPosition(shot.Direction * (Cfg.BoxBaseForce * shot.ImpulseScale), shot.HitPoint, ForceMode.Impulse);
        }
    }
}
