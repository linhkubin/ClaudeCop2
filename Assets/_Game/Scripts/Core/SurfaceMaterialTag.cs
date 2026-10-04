using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>C2. Gan chat lieu cho collider moi truong. Level-designer gan, FX doc. Thieu tag thi coi la Concrete.</summary>
    [DisallowMultipleComponent]
    public sealed class SurfaceMaterialTag : MonoBehaviour
    {
        [SerializeField] SurfaceMaterial material = SurfaceMaterial.Concrete;
        public SurfaceMaterial Material => material;

        /// <summary>Tim tag tren collider hoac cha cua no; khong co thi Concrete.</summary>
        public static SurfaceMaterial Resolve(Component hit)
        {
            if (hit == null) return SurfaceMaterial.Concrete;
            var tag = hit.GetComponentInParent<SurfaceMaterialTag>();
            return tag != null ? tag.material : SurfaceMaterial.Concrete;
        }
    }
}
