#if UNITY_EDITOR || DEVELOPMENT_BUILD
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy.Debugging
{
    /// <summary>
    /// Sandbox: nut IMGUI de sinh Grenadier / HumanShield tai spawnPoint (co the co con 'Peek'), bat/tat CombatPause.
    /// (Project dung Input System moi nen khong dung phim legacy.)
    /// </summary>
    public class SandboxEnemySpawner : MonoBehaviour
    {
        [SerializeField] EnemyConfig config;
        [SerializeField] EnemyActor grenadierPrefab;
        [SerializeField] HumanShieldEnemy humanShieldPrefab;
        [Tooltip("Diem sinh (tuy chon co con Peek). Trong thi dung transform nay.")]
        [SerializeField] Transform spawnPoint;

        bool paused;
        const string PauseReason = "SandboxEnemySpawner";

        void OnDisable() { if (paused) { paused = false; CombatPauseSignal.Pop(PauseReason); } }

        public EnemyActor Spawn(EnemyActor prefab)
        {
            if (prefab == null) return null;
            var sp = spawnPoint != null ? spawnPoint : transform;
            var peek = sp.Find("Peek");
            var e = Instantiate(prefab, sp.position, sp.rotation);
            e.Setup(config, sp.position, sp.rotation,
                peek != null ? peek.position : sp.position, peek != null ? peek.rotation : sp.rotation);
            e.Activate();
            return e;
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 220, 220));
            if (GUILayout.Button("Spawn Grenadier", GUILayout.Height(40))) Spawn(grenadierPrefab);
            if (GUILayout.Button("Spawn HumanShield", GUILayout.Height(40))) Spawn(humanShieldPrefab);
            bool want = GUILayout.Toggle(paused, " CombatPause", GUILayout.Height(30));
            if (want != paused)
            {
                paused = want;
                if (paused) CombatPauseSignal.Push(PauseReason); else CombatPauseSignal.Pop(PauseReason);
            }
            GUILayout.Label("Targets: " + TargetRegistry.Count);
            GUILayout.EndArea();
        }
    }
}
#endif
