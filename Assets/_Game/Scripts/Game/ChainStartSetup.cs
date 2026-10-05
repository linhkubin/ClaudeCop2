using UnityEngine;
using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>
    /// Scene chuoi level (Level_Chain): bat dau o level sau (GameCommands.SelectedLevel &gt;= 1) thi cua cuoi level 1 mo san.
    /// </summary>
    public sealed class ChainStartSetup : MonoBehaviour
    {
        [SerializeField] DoorOpener doorToOpen;

        void Start()
        {
            if (GameCommands.SelectedLevel >= 1 && doorToOpen != null) doorToOpen.OpenImmediate();
        }
    }
}
