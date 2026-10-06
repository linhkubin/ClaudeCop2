using UnityEngine;

namespace ClaudeCop.Enemy
{
    /// <summary>Gan len spawn point de enemy spawn tai do dung kieu xuat hien rieng (mac dinh Auto neu khong co).</summary>
    public class SpawnPointEntry : MonoBehaviour
    {
        [SerializeField] EnemyActor.EntryStyle style = EnemyActor.EntryStyle.Auto;
        public EnemyActor.EntryStyle Style => style;
        [Tooltip("Drop: do cao nhay xuong (m) tinh tu Peek, vd. mep gac thap. <= 0 = tu tinh theo mep tren man hinh.")]
        [SerializeField, Min(0f)] float dropHeight;
        public float DropHeight => dropHeight;
    }
}
