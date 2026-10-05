using UnityEngine;
using UCamera = UnityEngine.Camera;

namespace ClaudeCop.Props
{
    /// <summary>Giu <see cref="PropPool"/> dung chung + tick moi frame. Mot cai trong scene; vat ban duoc tim qua <see cref="Instance"/>.</summary>
    [DisallowMultipleComponent]
    public sealed class PropSystem : MonoBehaviour
    {
        [SerializeField] PropConfig config;
        [Tooltip("Prefab tao san (tuy chon).")]
        [SerializeField] GameObject[] prewarmPrefabs;
        [SerializeField, Min(0)] int prewarmCount = 3;

        PropPool pool;
        UCamera cam;

        public static PropSystem Instance { get; private set; }
        public PropConfig Config => config != null ? config : PropConfig.Fallback;
        public PropPool Pool => pool ?? (pool = new PropPool(transform, Config.MaxActiveBodies, Config.DebrisFadeSeconds, Config.OffscreenMargin));

        void Awake()
        {
            Instance = this;
            var p = Pool;
            if (prewarmPrefabs != null)
                for (int i = 0; i < prewarmPrefabs.Length; i++) p.Prewarm(prewarmPrefabs[i], prewarmCount);
        }

        void OnEnable() { Instance = this; }
        void OnDisable() { if (Instance == this) Instance = null; pool?.ReleaseAll(); }
        void OnDestroy() { if (Instance == this) Instance = null; pool?.Destroy(); }

        void Update()
        {
            if (pool == null) return;
            if (cam == null || !cam.isActiveAndEnabled) cam = UCamera.main;
            pool.Tick(Time.time, cam);
        }

        /// <summary>Phat manh vo tu pool; lifetime lay tu PropConfig.</summary>
        public PropInstance SpawnDebris(GameObject prefab, Vector3 pos, Quaternion rot)
            => Pool.Spawn(prefab, pos, rot, Config.DebrisLifetime, Time.time);

        /// <summary>Chi cho test EditMode (Awake khong chay).</summary>
        public static void SetInstanceForTests(PropSystem s) { Instance = s; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Instance = null; }
    }
}
