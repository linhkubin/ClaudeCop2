using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>
    /// Scene chuoi level (Level_Chain): bat dau o level sau (GameCommands.SelectedLevel &gt;= 1) thi cua cuoi cac level truoc mo san.
    /// </summary>
    public sealed class ChainStartSetup : MonoBehaviour
    {
        [SerializeField] DoorOpener doorToOpen;
        [Tooltip("Cua cuoi level 2, 3, ... (phan tu k mo san khi SelectedLevel >= k + 2). Level khong co cua cuoi: de trong.")]
        [SerializeField] DoorOpener[] laterDoors = new DoorOpener[0];

        void Start()
        {
            if (GameCommands.SelectedLevel >= 1 && doorToOpen != null) doorToOpen.OpenImmediate();
            for (int k = 0; k < laterDoors.Length; k++)
                if (GameCommands.SelectedLevel >= k + 2 && laterDoors[k] != null) laterDoors[k].OpenImmediate();
        }
    }
}
