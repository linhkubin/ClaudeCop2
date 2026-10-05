using UnityEngine;
using TMPro;

namespace ClaudeCop.UI
{
    /// <summary>Mot chu diem bay (phan tu cua pool trong FloatingScoreView). Khong tu Update: View dieu khien.</summary>
    public class FloatingScoreItem : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        public TMP_Text Label => label;
        [System.NonSerialized] public Vector3 World;
        [System.NonSerialized] public float Age, Life, Rise, OffsetY;
        [System.NonSerialized] public bool Active;
        public string Text => label != null ? label.text : string.Empty;
    }
}
