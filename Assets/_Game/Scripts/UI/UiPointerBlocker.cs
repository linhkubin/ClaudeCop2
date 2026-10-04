using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClaudeCop.UI
{
    /// <summary>
    /// Chan tap xuyen UI. Khong dung IsPointerOverGameObject(pointerId): id trong Input System UI module
    /// khong trung fingerId/-1 mot cach dang tin cay. Thay vao do raycast truc tiep EventSystem tai screenPos
    /// cua phat ban: bat ky Graphic raycastTarget nao trung (nut, panel Win/GameOver...) deu chan.
    /// Khong alloc moi lan goi. Graphic trang tri (chu, tim, vong target, nhay do) phai tat raycastTarget.
    /// </summary>
    public static class UiPointerBlocker
    {
        static PointerEventData data;
        static EventSystem dataOwner;
        static readonly List<RaycastResult> results = new List<RaycastResult>(8);

        /// <summary>Delegate tinh de gan/go vao TapShooter.PointerBlocker.</summary>
        public static readonly Func<Vector2, bool> Delegate = IsBlocked;

        public static bool IsBlocked(Vector2 screenPos)
        {
            var es = EventSystem.current;
            if (es == null) return false;
            if (data == null || dataOwner != es) { data = new PointerEventData(es); dataOwner = es; }
            data.position = screenPos;
            results.Clear();
            es.RaycastAll(data, results);
            return results.Count > 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { data = null; dataOwner = null; results.Clear(); }
    }
}
