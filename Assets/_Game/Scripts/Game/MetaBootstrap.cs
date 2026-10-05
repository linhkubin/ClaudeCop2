using UnityEngine;
using UnityEngine.SceneManagement;
using ClaudeCop.Combat;
using ClaudeCop.Enemy;
using ClaudeCop.Meta;

namespace ClaudeCop.Game
{
    /// <summary>
    /// Tu gan he thong man Home vao moi scene gameplay (co GameManager) luc nap, khong can sua scene:
    /// LoadoutApplier (ap sung/nang cap/trang bi) + LevelProgressTracker (luu thang/thua, xu, huy hieu).
    /// </summary>
    public static class MetaBootstrap
    {
        const string RuntimeName = "[MetaRuntime]";
        static bool hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (!hooked) { hooked = true; SceneManager.sceneLoaded += (s, m) => Attach(s); }
            Attach(SceneManager.GetActiveScene());
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { hooked = false; }

        static void Attach(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;
            GameManager gm = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == RuntimeName) return; // da gan
                if (gm == null) gm = root.GetComponentInChildren<GameManager>(true);
            }
            if (gm == null) return;
            var go = new GameObject(RuntimeName);
            SceneManager.MoveGameObjectToScene(go, scene);
            var tracker = go.AddComponent<LevelProgressTracker>();
            var applier = go.AddComponent<LoadoutApplier>();
            applier.Apply(tracker);
        }
    }

    /// <summary>Ap trang bi tu ho so vao tran: sung (mua/thue + ong ngam), giam thanh, ao, mu, gang tay, kinh, bo dam, giap quang cao.</summary>
    public sealed class LoadoutApplier : MonoBehaviour
    {
        public Loadout Current { get; private set; }

        public void Apply(LevelProgressTracker tracker)
        {
            var p = PlayerProfile.Data;
            var l = LoadoutBuilder.Build(p, PlayerProfile.Catalog);
            Current = l;

            var shooter = FindFirstObjectByType<TapShooter>(FindObjectsInactive.Include);
            if (shooter != null)
            {
                var w = l.CreateWeapon();
                if (w != null) shooter.SetStartingWeapon(w);
                shooter.SetJusticeRadiusScale(l.JusticeRadiusScale);
            }
            var combo = FindFirstObjectByType<ComboSystem>(FindObjectsInactive.Include);
            if (combo != null) combo.SetMissForgivenessPerStage(l.MissForgiveness);
            var health = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
            if (health != null) health.ConfigureLoadout(l.ExtraLives, l.ArmorPerStage, l.StartArmor);
            EnemyActor.GlobalReticleScale = l.EnemyReticleScale;
            if (tracker != null) tracker.CoinBonus = l.CoinBonus;

            if (LoadoutBuilder.Consume(p)) PlayerProfile.Save();
        }

        void OnDestroy() { EnemyActor.GlobalReticleScale = 1f; }
    }
}
