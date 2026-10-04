using UnityEngine;

namespace ClaudeCop.Enemy
{
    /// <summary>Moi so can bang cua Enemy va EncounterWave (khong hard-code).</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Enemy Config", fileName = "EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Enemy")]
        [Min(0.01f), Tooltip("Thoi gian lo ra tu cho nap (s)")] public float peekDuration = 0.3f;
        [Min(0.1f), Tooltip("Thoi gian vong target thu nho 0->1 (s). Preset: 2.0 / 2.5 / 3.0")] public float reticleTime = 2.5f;
        [Min(0.01f), Tooltip("Thoi gian thut vao sau khi ban (s)")] public float retreatDuration = 0.3f;
        [Min(0f), Tooltip("Nap bao lau truoc khi lo lai voi vong moi (s)")] public float hideTime = 0.8f;
        [Min(0.1f), Tooltip("Cao diem ngam so voi chan (m)")] public float aimHeight = 1.5f;
        [Min(0f), Tooltip("Thoi gian sau khi chet truoc khi tat (s)")] public float deathLinger = 1f;

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

        [Header("Encounter")]
        [Min(0f), Tooltip("Khoang cach kich hoat giua cac enemy: tu (s)")] public float staggerMin = 0.4f;
        [Min(0f), Tooltip("... den (s)")] public float staggerMax = 1.0f;

        /// <summary>Preset thoi gian vong cho Jev (reticle_time).</summary>
        public static readonly float[] ReticlePresets = { 2.0f, 2.5f, 3.0f };
    }
}
