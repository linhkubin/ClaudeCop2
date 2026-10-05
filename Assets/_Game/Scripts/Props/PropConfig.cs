using UnityEngine;

namespace ClaudeCop.Props
{
    /// <summary>So lieu chung cua module Props (no, luc day, pool). Mot nguon duy nhat cho cac con so nay.</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Props/Prop Config", fileName = "PropConfig")]
    public sealed class PropConfig : ScriptableObject
    {
        [Header("Vu no")]
        [Tooltip("Ban kinh no (m).")]
        [SerializeField, Min(0.1f)] float blastRadius = 3f;
        [Tooltip("Tre no day chuyen (s).")]
        [SerializeField, Min(0f)] float chainDelay = 0.15f;
        [Tooltip("Luc AddExplosionForce len vat ly trong ban kinh.")]
        [SerializeField, Min(0f)] float explosionForce = 800f;
        [SerializeField] float explosionUpwardsModifier = 1f;
        [Tooltip("ImpulseScale cua ShotInfo dung khi no ha enemy/con tin.")]
        [SerializeField, Min(0f)] float blastImpulseScale = 2f;
        [SerializeField] LayerMask blastMask = ~0;

        [Header("Vat ly")]
        [Tooltip("Luc co ban len hop (nhan ShotInfo.ImpulseScale), ForceMode.Impulse.")]
        [SerializeField, Min(0f)] float boxBaseForce = 4f;

        [Header("Pool")]
        [Tooltip("Tong so Rigidbody dang hoat dong toi da (pool).")]
        [SerializeField, Min(1)] int maxActiveBodies = 40;
        [Tooltip("Manh vo song (s).")]
        [SerializeField, Min(0.1f)] float debrisLifetime = 4f;
        [Tooltip("Thoi gian thu nho cuoi doi (s).")]
        [SerializeField, Min(0f)] float debrisFadeSeconds = 0.5f;
        [Tooltip("Vat ngoai khung hinh (viewport, ngoai [-m, 1+m]) cho ngu.")]
        [SerializeField, Min(0f)] float offscreenMargin = 0.15f;

        [Header("Kinh")]
        [Tooltip("Luc day manh kinh theo huong dan (ForceMode.Impulse, nhan ImpulseScale).")]
        [SerializeField, Min(0f)] float glassShardForce = 3f;
        [Tooltip("Do lech ngau nhien huong manh kinh (0..1).")]
        [SerializeField, Range(0f, 1f)] float glassShardSpread = 0.35f;

        [Tooltip("Bui kinh song (s).")]
        [SerializeField, Min(0.1f)] float glassDustLifetime = 1.5f;

        public float GlassDustLifetime => glassDustLifetime;
        public float BlastRadius => blastRadius;
        public float ChainDelay => chainDelay;
        public float ExplosionForce => explosionForce;
        public float ExplosionUpwardsModifier => explosionUpwardsModifier;
        public float BlastImpulseScale => blastImpulseScale;
        public LayerMask BlastMask => blastMask;
        public float BoxBaseForce => boxBaseForce;
        public int MaxActiveBodies => maxActiveBodies;
        public float DebrisLifetime => debrisLifetime;
        public float DebrisFadeSeconds => debrisFadeSeconds;
        public float OffscreenMargin => offscreenMargin;
        public float GlassShardForce => glassShardForce;
        public float GlassShardSpread => glassShardSpread;

        static PropConfig fallback;
        /// <summary>Gia tri mac dinh (field initializer) khi component chua gan asset.</summary>
        public static PropConfig Fallback
        {
            get
            {
                if (fallback == null) { fallback = CreateInstance<PropConfig>(); fallback.hideFlags = HideFlags.HideAndDontSave; }
                return fallback;
            }
        }
    }
}
