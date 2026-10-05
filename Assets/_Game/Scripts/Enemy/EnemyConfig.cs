using UnityEngine;

namespace ClaudeCop.Enemy
{
    /// <summary>Moi so can bang cua Enemy va EncounterWave (khong hard-code).</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Enemy Config", fileName = "EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Enemy")]
        [Min(0.01f), Tooltip("Thoi gian lo ra tu cho nap (s)")] public float peekDuration = 0.3f;
        [Range(0f, 1f), Tooltip("Enemy ban duoc khi PeekT >= nguong nay (luc Peeking va Retreating), khong can cho vong target. Mac dinh ~0.35")] public float targetableThreshold = 0.35f;
        [Min(0.1f), Tooltip("Thoi gian vong target thu nho 0->1 (s). Preset: 2.0 / 2.5 / 3.0")] public float reticleTime = 2.5f;
        [Min(0.01f), Tooltip("Thoi gian thut vao sau khi ban (s)")] public float retreatDuration = 0.3f;
        [Min(0f), Tooltip("Nap bao lau truoc khi lo lai voi vong moi (s)")] public float hideTime = 0.8f;
        [Min(0.1f), Tooltip("Cao diem ngam so voi chan (m)")] public float aimHeight = 1.5f;
        [Min(0f), Tooltip("Thoi gian sau khi chet truoc khi tat (s)")] public float deathLinger = 1f;
        [Min(0.01f), Tooltip("Thoi gian enemy nga xuong khi bi ban (s)")] public float fallDuration = 0.25f;
        [Range(0f, 180f), Tooltip("Goc nga quanh chan (do)")] public float fallAngle = 90f;

        [Header("Justice point")]
        [Tooltip("Cong tac tong cho EncounterWave: bat thi wave gan Justice point cho khoang justiceFraction so enemy. (Bat tay: EnemyActor.justiceEnabled)")]
        public bool justiceEnabled = true;
        [Range(0f, 1f), Tooltip("Ty le enemy trong dot co Justice point (~1/3)")] public float justiceFraction = 0.34f;
        [Tooltip("Vi tri Justice point (local cua enemy): tay cam sung")]
        public Vector3 justiceOffset = new Vector3(0.45f, 1.15f, 0.35f);
        [Min(0.1f), Tooltip("Enemy dau hang (gio tay) bao lau truoc khi bien mat (s)")] public float surrenderTime = 1f;

        [Header("Hostage")]
        [Min(0.5f), Tooltip("Con tin lo ra bao lau truoc khi tu roi di (s)")] public float hostageExposeTime = 4f;
        [Min(0.1f), Tooltip("Cao diem ngam con tin so voi chan (m)")] public float hostageAimHeight = 1.3f;

        [Header("Grenadier (luu dan)")]
        [Min(0.1f), Tooltip("Thoi gian luu dan bay tu tay enemy toi diem dap (s)")] public float grenadeFlightTime = 1.5f;
        [Min(0f), Tooltip("Do cao cung bay (m) so voi duong thang")] public float grenadeArcHeight = 1.5f;
        [Min(0.1f), Tooltip("Diem dap cach camera (m) ve phia truoc")] public float grenadeLandDistance = 1.5f;
        [Tooltip("Do cao diem dap so voi camera (m, am = thap hon)")] public float grenadeLandHeightOffset = -0.4f;
        [Min(0.1f), Tooltip("Khi khong co camera: diem dap cach enemy (m) theo huong forward")] public float grenadeFallbackDistance = 8f;
        [Min(0f), Tooltip("Do cao tay nem so voi chan enemy (m) khi khong co con ThrowOrigin")] public float grenadeThrowHeight = 1.4f;
        [Min(0.01f), Tooltip("Thoi gian hieu ung no (s)")] public float grenadeExplosionTime = 0.35f;
        [Min(0.1f), Tooltip("Kich thuoc hieu ung no luc cuoi (m)")] public float grenadeExplosionSize = 1.5f;

        [Header("Human shield")]
        [Min(1f), Tooltip("Ban kinh trung dau (px, chuan chieu rong tham chieu)")] public float shieldHeadRadiusPx = 45f;
        [Min(1f), Tooltip("Chieu rong man hinh chuan de quy doi px")] public float uiReferenceWidth = 1080f;
        [Min(0.1f), Tooltip("Cao diem ngam (dau) so voi chan khi khong co con Head (m)")] public float shieldHeadHeight = 1.75f;
        [Min(0f), Tooltip("Toc do con tin chay khi duoc tha (m/s)")] public float hostageRunSpeed = 3f;
        [Min(0.1f), Tooltip("Con tin chay bao lau truoc khi bien mat (s)")] public float hostageRunTime = 2f;

        [Header("Encounter")]
        [Min(0f), Tooltip("Khoang cach kich hoat giua cac enemy: tu (s)")] public float staggerMin = 0.4f;
        [Min(0f), Tooltip("... den (s)")] public float staggerMax = 1.0f;

        static EnemyConfig fallback;
        /// <summary>Gia tri mac dinh (tu field initializer) dung khi khong gan asset EnemyConfig; khong hard-code o noi khac.</summary>
        public static EnemyConfig Fallback
        {
            get
            {
                if (fallback == null)
                {
                    fallback = CreateInstance<EnemyConfig>();
                    fallback.hideFlags = HideFlags.HideAndDontSave;
                }
                return fallback;
            }
        }

        /// <summary>Preset thoi gian vong cho RankScore (reticle_time).</summary>
        public static readonly float[] ReticlePresets = { 2.0f, 2.5f, 3.0f };
    }
}
