using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.UI
{
    /// <summary>GameEvents.ScoreAwarded -> FloatingScoreView.Spawn.</summary>
    public class FloatingScorePresenter : MonoBehaviour
    {
        [SerializeField] FloatingScoreView view;

        void OnEnable()
        {
            if (view == null) { Debug.LogError("[FloatingScorePresenter] Thieu view.", this); return; }
            GameEvents.ScoreAwarded += OnAwarded;
        }

        void OnDisable()
        {
            GameEvents.ScoreAwarded -= OnAwarded;
            if (view != null) view.ClearAll();
        }

        void OnAwarded(int points, Vector3 pos, bool isJustice, float multiplier) => view.Spawn(pos, points, isJustice, multiplier);
    }
}
