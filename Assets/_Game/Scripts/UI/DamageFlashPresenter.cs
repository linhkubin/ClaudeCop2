using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.UI
{
    /// <summary>GameEvents.PlayerDamaged -> DamageFlashView.Flash().</summary>
    public class DamageFlashPresenter : MonoBehaviour
    {
        [SerializeField] DamageFlashView view;

        void OnEnable() { GameEvents.PlayerDamaged += OnDamaged; }
        void OnDisable() { GameEvents.PlayerDamaged -= OnDamaged; }

        void OnDamaged(DamageSource source, Vector3 pos, int livesLeft) { if (view != null) view.Flash(); }
    }
}
