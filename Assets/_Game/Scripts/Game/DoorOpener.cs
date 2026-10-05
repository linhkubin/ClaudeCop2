using System.Collections;
using ClaudeCop.Core;
using UnityEngine;

namespace ClaudeCop.Game
{
    /// <summary>
    /// Mo cua bang xoay muot khi mot encounter Cleared (vd. cua chinh ngan hang sau wave cuoi cua Level 1).
    /// Moi canh la mot pivot (ban le) xoay quanh truc Y local them yawDelta (do). Dat trong scene gameplay, noi tham chieu.
    /// </summary>
    public sealed class DoorOpener : MonoBehaviour
    {
        [System.Serializable]
        public struct Leaf { public Transform pivot; public float yawDelta; [Tooltip("Truot: dich pivot (local) them vector nay khi mo (cua truot). Co the ket hop yawDelta.")] public Vector3 slide; }

        [SerializeField] EncounterBase trigger;
        [SerializeField] Leaf[] leaves;
        [SerializeField] float duration = 1.2f;
        [SerializeField] float delay = 0f;

        Quaternion[] start;
        Vector3[] startPos;
        bool opened;

        public bool Opened => opened;

        void OnEnable() { if (trigger != null) trigger.Cleared += OnCleared; }
        void OnDisable() { if (trigger != null) trigger.Cleared -= OnCleared; }

        void OnCleared(EncounterBase e, Vector3 pos) { Open(); }

        /// <summary>Mo cua (chi mot lan).</summary>
        public void Open()
        {
            if (opened) return;
            opened = true;
            StartCoroutine(Run());
        }

        /// <summary>Mo ngay lap tuc (bat dau o level sau cua chuoi: cua level truoc da mo san).</summary>
        public void OpenImmediate()
        {
            if (opened) return;
            opened = true;
            start = new Quaternion[leaves.Length]; startPos = new Vector3[leaves.Length];
            for (int i = 0; i < leaves.Length; i++) { start[i] = leaves[i].pivot != null ? leaves[i].pivot.localRotation : Quaternion.identity; startPos[i] = leaves[i].pivot != null ? leaves[i].pivot.localPosition : Vector3.zero; }
            Apply(1f);
        }

        IEnumerator Run()
        {
            start = new Quaternion[leaves.Length]; startPos = new Vector3[leaves.Length];
            for (int i = 0; i < leaves.Length; i++) { start[i] = leaves[i].pivot != null ? leaves[i].pivot.localRotation : Quaternion.identity; startPos[i] = leaves[i].pivot != null ? leaves[i].pivot.localPosition : Vector3.zero; }
            if (delay > 0f) yield return new WaitForSeconds(delay);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                Apply(Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration)));
                yield return null;
            }
            Apply(1f);
        }

        void Apply(float k)
        {
            for (int i = 0; i < leaves.Length; i++)
                if (leaves[i].pivot != null)
                {
                    leaves[i].pivot.localRotation = start[i] * Quaternion.AngleAxis(leaves[i].yawDelta * k, Vector3.up);
                    if (leaves[i].slide != Vector3.zero) leaves[i].pivot.localPosition = startPos[i] + leaves[i].slide * k;
                }
        }
    }
}
