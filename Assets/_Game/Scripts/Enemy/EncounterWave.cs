using System.Collections.Generic;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Enemy
{
    /// <summary>
    /// Mot dot giao tranh: enemy + con tin + thung vu khi. Moi loai lay theo 2 cach (dung ca hai duoc):
    /// 1) dat san trong scene (sceneEnemies / sceneHostages / scenePickups; vi tri nap = vi tri hien tai);
    /// 2) spawn (lam con cua wave) tai cac diem EnemySpawn_P*_W*_NN / HostageSpawn_* / PickupSpawn_* (co con Peek voi enemy/con tin).
    /// Enemy va con tin kich hoat so le staggerMin..staggerMax, XEN KE (con tin chen deu giua cac enemy); dung khi CombatPauseSignal.IsPaused.
    /// Con tin KHONG tinh vao dieu kien Cleared; khi dot Cleared con tin con lai bi huy bo va thung chua nhat bi tat.
    /// Thung vu khi la GameObject co WeaponPickup (Enemy khong tham chieu Combat): wave chi bat/tat GameObject, WeaponPickup tu dang ky target.
    /// Cleared luon phat o LateUpdate (sau ShotResolved cua phat ban cuoi), khong phat dong bo trong OnTapHit/Begin.
    /// Cau hinh truoc Begin(): Configure(EnemyConfig), ApplyPreset(EnemyPreset), ApplyReticleTime(float) (RankScore).
    /// </summary>
    public class EncounterWave : EncounterBase
    {
        [SerializeField] EnemyConfig config;
        [Tooltip("Bo cau hinh lam san ap o Awake (T-403). RankScore reticle_time (neu co) ghi de sau, truoc Begin.")]
        [SerializeField] EnemyPreset preset;
        [Header("Enemy")]
        [SerializeField] List<EnemyActor> sceneEnemies = new List<EnemyActor>();
        [SerializeField] List<Transform> spawnPoints = new List<Transform>();
        [SerializeField] EnemyActor enemyPrefab;
        [Header("Grenadier (EnemyActor co ConfigureGrenadier / prefab Enemy_Grenadier)")]
        [SerializeField] List<Transform> grenadierSpawnPoints = new List<Transform>();
        [SerializeField] EnemyActor grenadierPrefab;
        [Tooltip("So grenadier toi da moi dot (< 0 = khong gioi han)")]
        [SerializeField] int maxGrenadiersPerWave = 2;
        [Header("Human shield (prefab Enemy_HumanShield)")]
        [SerializeField] List<Transform> humanShieldSpawnPoints = new List<Transform>();
        [SerializeField] HumanShieldEnemy humanShieldPrefab;
        [Tooltip("So human shield toi da moi dot (< 0 = khong gioi han)")]
        [SerializeField] int maxHumanShieldsPerWave = 2;
        [Header("Hostage")]
        [SerializeField] List<HostageActor> sceneHostages = new List<HostageActor>();
        [SerializeField] List<Transform> hostageSpawnPoints = new List<Transform>();
        [SerializeField] HostageActor hostagePrefab;
        [Tooltip("Mac dinh false: con tin tu cui xuong sau hostageExposeTime. Bat = dung im tai cho den het dot (con tin co dinh); het dot thi dung im, khong ban duoc.")]
        [SerializeField] bool hostagesStandStill = false;
        [Header("Pickup (GameObject co WeaponPickup)")]
        [SerializeField] List<GameObject> scenePickups = new List<GameObject>();
        [SerializeField] List<Transform> pickupSpawnPoints = new List<Transform>();
        [SerializeField] GameObject pickupPrefab;
        [Tooltip("So enemy toi da song cung luc trong dot (<= 0 = khong gioi han). Enemy ke tiep cho den khi co cho trong.")]
        [SerializeField] int maxConcurrent;
        [Header("Canh hai (GagFall) - khong phai muc tieu, khong tinh kill")]
        [SerializeField] List<GagFall> gags = new List<GagFall>();
        [SerializeField, TextArea] string description;

        struct Slot { public EnemyActor enemy; public HostageActor hostage; }

        readonly List<EnemyActor> queue = new List<EnemyActor>();
        readonly List<HostageActor> hostages = new List<HostageActor>();
        readonly List<GameObject> pickups = new List<GameObject>();
        readonly List<Slot> sequence = new List<Slot>();
        // Pre-spawn (tat san): Begin chi bat, khong Instantiate (tranh hitch ~100 ms).
        readonly List<EnemyActor> spawnedEnemies = new List<EnemyActor>();
        readonly List<HostageActor> spawnedHostages = new List<HostageActor>();
        readonly List<GameObject> spawnedPickups = new List<GameObject>();
        readonly List<Grenade> grenades = new List<Grenade>();
        bool preSpawned;
        int nextIndex;
        float countdown;
        bool active, cleared;
        Vector3 lastKillPos;

        // Ghi de theo dot (preset / RankScore); < 0 hoac null = dung EnemyConfig.
        float reticleTimeOverride = -1f, hideTimeOverride = -1f, staggerMinOverride = -1f, staggerMaxOverride = -1f;
        bool? justiceOverride;
        float justiceFractionOverride = -1f;
        bool useHostages = true;

        public override bool IsActive => active;
        public override bool IsCleared => cleared;
        /// <summary>So enemy con song (khong tinh enemy da bi Destroy).</summary>
        public int AliveCount
        {
            get
            {
                int n = 0;
                foreach (var e in queue) if (e != null && !e.IsDead) n++;
                return n;
            }
        }
        /// <summary>So con tin da kich hoat va chua roi di / bi ban.</summary>
        public int ActiveHostageCount
        {
            get
            {
                int n = 0;
                foreach (var h in hostages) if (h != null && h.IsActivated && !h.IsFinished) n++;
                return n;
            }
        }
        /// <summary>So luu dan da nem nhung chua duoc giai quyet (chua bi ban, chua no). Dot chua Cleared khi con &gt; 0.</summary>
        public int PendingGrenadeCount
        {
            get
            {
                int n = 0;
                foreach (var g in grenades) if (g != null && !g.IsResolved) n++;
                return n;
            }
        }
        public IReadOnlyList<EnemyActor> Enemies => queue;
        public IReadOnlyList<HostageActor> Hostages => hostages;
        public override string Description =>
            string.IsNullOrEmpty(description)
                ? "Wave '" + name + "': " + (sceneEnemies.Count + spawnPoints.Count + grenadierSpawnPoints.Count + humanShieldSpawnPoints.Count) + " enemy, "
                  + (sceneHostages.Count + hostageSpawnPoints.Count) + " hostage"
                : description;

        // ---------- Cau hinh (goi truoc Begin) ----------

        public void Configure(EnemyConfig cfg) { config = cfg; }

        /// <summary>Doi thoi gian vong target cua dot (RankScore reticle_time). Enemy chua kich hoat se dung gia tri moi.</summary>
        public void ApplyReticleTime(float seconds)
        {
            if (seconds <= 0f) return;
            reticleTimeOverride = seconds;
            foreach (var e in queue) if (e != null) e.SetReticleTime(seconds);
        }

        /// <summary>Ap bo cau hinh lam san (calm/standard/intense/hostage_heavy).</summary>
        public void ApplyPreset(EnemyPreset p)
        {
            if (p == null) return;
            ApplyReticleTime(p.reticleTime);
            hideTimeOverride = p.hideTime;
            staggerMinOverride = p.staggerMin;
            staggerMaxOverride = p.staggerMax;
            justiceOverride = p.justiceEnabled;
            justiceFractionOverride = p.justiceFraction;
            useHostages = p.useHostages;
            foreach (var e in queue) if (e != null) e.SetHideTime(p.hideTime);
        }

        /// <summary>Cau hinh grenadier cho dot: prefab (Enemy_Grenadier), cac diem spawn, so toi da. Goi truoc PreSpawn (Start); goi sau thi spawn bu ngay (tat san).</summary>
        public void ConfigureGrenadiers(EnemyActor prefab, IEnumerable<Transform> points, int max)
        {
            grenadierPrefab = prefab;
            grenadierSpawnPoints = points != null ? new List<Transform>(points) : new List<Transform>();
            maxGrenadiersPerWave = max;
            if (preSpawned && !active) SpawnEnemies(grenadierPrefab, grenadierSpawnPoints, maxGrenadiersPerWave);
        }

        /// <summary>Cau hinh human shield cho dot: prefab (Enemy_HumanShield), cac diem spawn, so toi da.</summary>
        public void ConfigureHumanShields(HumanShieldEnemy prefab, IEnumerable<Transform> points, int max)
        {
            humanShieldPrefab = prefab;
            humanShieldSpawnPoints = points != null ? new List<Transform>(points) : new List<Transform>();
            maxHumanShieldsPerWave = max;
            if (preSpawned && !active) SpawnEnemies(humanShieldPrefab, humanShieldSpawnPoints, maxHumanShieldsPerWave);
        }

        /// <summary>Doi prefab thung vu khi cho dot (RankScore weapon_drop). null = khong co thung spawn.</summary>
        public void SetPickupPrefab(GameObject prefab)
        {
            if (prefab == pickupPrefab) return;
            pickupPrefab = prefab;
            // Thung da pre-spawn voi prefab cu: huy de spawn lai dung prefab moi.
            if (!active)
            {
                foreach (var g in spawnedPickups) if (g != null) Destroy(g);
                spawnedPickups.Clear();
                if (preSpawned) SpawnPickups();
            }
        }

        EnemyConfig Cfg => config != null ? config : EnemyConfig.Fallback;
        float StaggerMin => staggerMinOverride >= 0f ? staggerMinOverride : (Cfg.staggerMin);
        float StaggerMax => staggerMaxOverride >= 0f ? staggerMaxOverride : (Cfg.staggerMax);
        bool JusticeActive => justiceOverride.HasValue ? justiceOverride.Value : Cfg.justiceEnabled;
        float JusticeFraction => justiceFractionOverride >= 0f ? justiceFractionOverride : (Cfg.justiceFraction);

        void Awake()
        {
            if (preset != null) ApplyPreset(preset);
            // Thung dat san chi hien khi dot bat dau.
            foreach (var p in scenePickups) if (p != null) p.SetActive(false);
        }

        void Start() { PreSpawn(); }

        /// <summary>Instantiate san enemy/con tin/thung tai cac diem spawn va tat di; Begin() chi bat len. Idempotent. Start goi; Begin goi du phong.</summary>
        public void PreSpawn()
        {
            if (preSpawned) return;
            preSpawned = true;
            SpawnEnemies(enemyPrefab, spawnPoints, -1);
            SpawnEnemies(grenadierPrefab, grenadierSpawnPoints, maxGrenadiersPerWave);
            SpawnEnemies(humanShieldPrefab, humanShieldSpawnPoints, maxHumanShieldsPerWave);
            if (hostagePrefab != null)
            {
                foreach (var sp in hostageSpawnPoints)
                {
                    if (sp == null) continue;
                    GetPeek(sp, out Vector3 pp, out Quaternion pr);
                    var h = Instantiate(hostagePrefab, sp.position, sp.rotation, transform);
                    h.Setup(config, sp.position, sp.rotation, pp, pr);
                    h.gameObject.SetActive(false);
                    spawnedHostages.Add(h);
                }
            }
            SpawnPickups();
        }

        void SpawnEnemies(EnemyActor prefab, List<Transform> points, int max)
        {
            if (prefab == null) return;
            int n = 0;
            foreach (var sp in points)
            {
                if (sp == null) continue;
                if (max >= 0 && n >= max) break;
                n++;
                GetPeek(sp, out Vector3 pp, out Quaternion pr);
                var e = Instantiate(prefab, sp.position, sp.rotation, transform);
                e.Setup(config, sp.position, sp.rotation, pp, pr);
                var entry = sp.GetComponent<SpawnPointEntry>();
                if (entry != null) e.SetEntryStyle(entry.Style, entry.DropHeight);
                e.gameObject.SetActive(false);
                spawnedEnemies.Add(e);
            }
        }

        void SpawnPickups()
        {
            if (pickupPrefab == null) return;
            foreach (var sp in pickupSpawnPoints)
            {
                if (sp == null) continue;
                var g = Instantiate(pickupPrefab, sp.position, sp.rotation, transform);
                g.SetActive(false);
                spawnedPickups.Add(g);
            }
        }

        // ---------- Dot ----------

        public override void Begin()
        {
            if (active || cleared) return;
            PreSpawn();
            BuildQueue();
            active = true;
            lastKillPos = transform.position;
            nextIndex = 0;
            foreach (var e in queue) if (e != null && e.SceneStanding) e.Activate();
            foreach (var g in gags) if (g != null) g.Play();
            ActivateNext();   // phan tu dau lo ngay (dot khong enemy: LateUpdate se phat Cleared)
        }

        void BuildQueue()
        {
            foreach (var g in grenades) if (g != null) g.Resolved -= OnGrenadeResolved;
            grenades.Clear();
            queue.Clear(); hostages.Clear(); pickups.Clear(); sequence.Clear();

            foreach (var e in sceneEnemies)
                if (e != null) { queue.Add(e); if (config != null) e.ApplyConfig(config); }
            foreach (var e in spawnedEnemies)
            {
                if (e == null) continue;
                if (config != null) e.ApplyConfig(config);
                e.gameObject.SetActive(true);
                queue.Add(e);
            }

            // Ghi de theo dot + Justice point cho ~justiceFraction enemy.
            bool justice = JusticeActive;
            float frac = JusticeFraction, acc = 0.5f;
            foreach (var e in queue)
            {
                if (reticleTimeOverride > 0f) e.SetReticleTime(reticleTimeOverride);
                if (hideTimeOverride >= 0f) e.SetHideTime(hideTimeOverride);
                if (justice)
                {
                    acc += frac;
                    bool on = acc >= 1f;
                    if (on) acc -= 1f;
                    e.SetJustice(on);
                }
                e.Died += OnEnemyDied;
                e.GrenadeThrown += OnGrenadeThrown;
            }

            if (useHostages)
            {
                foreach (var h in sceneHostages)
                    if (h != null) { hostages.Add(h); if (config != null) h.ApplyConfig(config); h.SetStandStill(hostagesStandStill); }
                foreach (var h in spawnedHostages)
                {
                    if (h == null) continue;
                    if (config != null) h.ApplyConfig(config);
                    h.gameObject.SetActive(true);
                    h.SetStandStill(hostagesStandStill);
                    hostages.Add(h);
                }
            }

            // Thu tu kich hoat: enemy theo thu tu, con tin chen deu giua cac enemy.
            int ec = 0, hc = hostages.Count;
            foreach (var q in queue) if (!q.SceneStanding) ec++;
            // Enemy dung san: ban duoc ngay khi dot bat dau (Begin kich hoat het), khong xep hang so le.
            for (int i = 0; i < queue.Count; i++) if (!queue[i].SceneStanding) sequence.Add(new Slot { enemy = queue[i] });
            for (int k = 0; k < hc; k++)
            {
                int at = Mathf.Clamp(Mathf.RoundToInt((k + 1) * (float)ec / (hc + 1)) + k, 0, sequence.Count);
                sequence.Insert(at, new Slot { hostage = hostages[k] });
            }

            // Thung vu khi: hien ngay khi dot bat dau.
            foreach (var p in scenePickups) if (p != null) { pickups.Add(p); p.SetActive(true); }
            foreach (var g in spawnedPickups) if (g != null) { pickups.Add(g); g.SetActive(true); }
        }

        static void GetPeek(Transform sp, out Vector3 pos, out Quaternion rot)
        {
            var peek = sp.Find("Peek");
            pos = peek != null ? peek.position : sp.position;
            rot = peek != null ? peek.rotation : sp.rotation;
        }

        void ActivateNext()
        {
            while (nextIndex < sequence.Count && SlotGone(sequence[nextIndex])) nextIndex++;
            if (nextIndex >= sequence.Count) return;
            var s = sequence[nextIndex++];
            if (s.enemy != null) s.enemy.Activate();
            else if (s.hostage != null) s.hostage.Activate();
            float min = StaggerMin, max = StaggerMax;
            countdown = Random.Range(min, Mathf.Max(min, max));
        }

        static bool SlotGone(Slot s) { return s.enemy == null && s.hostage == null; }

        void Update()
        {
            if (CombatPauseSignal.IsPaused) return;
            Tick(Time.deltaTime);
        }

        /// <summary>Tien bo dem so le (Update goi; test goi truc tiep).</summary>
        public void Tick(float dt)
        {
            if (!active || cleared || nextIndex >= sequence.Count) return;
            countdown -= dt;
            if (countdown > 0f) return;
            if (maxConcurrent > 0 && ActivatedAliveCount() >= maxConcurrent) return; // cho co cho trong, giu countdown <= 0
            ActivateNext();
        }

        void LateUpdate() { FlushClear(); }

        int ActivatedAliveCount()
        {
            int n = 0;
            foreach (var q in queue) if (q != null && q.SceneStanding && !q.IsDead) n++;
            for (int i = 0; i < nextIndex && i < sequence.Count; i++)
            {
                var e = sequence[i].enemy;
                if (e != null && !e.IsDead) n++;
            }
            return n;
        }

        bool HasEnemyToActivate()
        {
            for (int i = nextIndex; i < sequence.Count; i++) if (sequence[i].enemy != null) return true;
            return false;
        }

        /// <summary>Phat Cleared neu da kich hoat het enemy va khong con enemy song (con tin khong tinh). LateUpdate goi; test goi truc tiep.</summary>
        public void FlushClear()
        {
            if (!active || cleared) return;
            if (HasEnemyToActivate()) return;
            if (AliveCount > 0) return;
            if (PendingGrenadeCount > 0) return;
            active = false; cleared = true;
            DismissExtras();
            RaiseCleared(lastKillPos);
        }

        void DismissExtras()
        {
            foreach (var h in hostages) if (h != null && h.gameObject.activeSelf) h.Freeze();
            if (pickups.Count > 0) StartCoroutine(SinkPickups(new List<GameObject>(pickups)));
        }

        /// <summary>Thung vu khi chim xuong roi tat sau khi dot xong (khong bien mat dot ngot).</summary>
        System.Collections.IEnumerator SinkPickups(List<GameObject> items)
        {
            var from = new List<Vector3>();
            foreach (var g in items) from.Add(g != null ? g.transform.position : Vector3.zero);
            for (float t = 0f; t < 0.7f; t += Time.deltaTime)
            {
                float k = t / 0.7f;
                for (int i = 0; i < items.Count; i++) if (items[i] != null) items[i].transform.position = from[i] + Vector3.down * 1.5f * k;
                yield return null;
            }
            for (int i = 0; i < items.Count; i++) if (items[i] != null) { items[i].SetActive(false); items[i].transform.position = from[i]; }
        }

        void OnGrenadeThrown(EnemyActor e, Grenade g)
        {
            if (g == null) return;
            grenades.Add(g);
            g.Resolved += OnGrenadeResolved;
        }

        void OnGrenadeResolved(Grenade g, bool shotDown)
        {
            g.Resolved -= OnGrenadeResolved;
            lastKillPos = g.transform.position;
        }

        void OnEnemyDied(EnemyActor e, Vector3 pos)
        {
            e.Died -= OnEnemyDied;
            lastKillPos = pos;
        }

        void OnDestroy()
        {
            foreach (var e in queue) if (e != null) { e.Died -= OnEnemyDied; e.GrenadeThrown -= OnGrenadeThrown; }
            foreach (var g in grenades) if (g != null) g.Resolved -= OnGrenadeResolved;
        }
    }
}
