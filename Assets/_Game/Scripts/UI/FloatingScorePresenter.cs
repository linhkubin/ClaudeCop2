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
            BlastEvents.Blasted += OnBlast;
        }

        void OnDisable()
        {
            GameEvents.ScoreAwarded -= OnAwarded;
            BlastEvents.Blasted -= OnBlast;
            if (view != null) view.ClearAll();
        }

        void OnBlast(BlastReport r) => view.SpawnBlast(r.Center, r.EnemiesKilled);

        void OnAwarded(int points, Vector3 pos, bool isJustice, float multiplier) => view.Spawn(pos, points, isJustice, multiplier);
    }
}
