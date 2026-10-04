using UnityEngine;

namespace ClaudeCop.Core
{
    /// <summary>C13. Nguoi nhan sat thuong cua nguoi choi. PlayerHealth (Game) cai dat.</summary>
    public interface IPlayerDamageReceiver
    {
        void Damage(DamageSource source, Vector3 worldPosition);
    }

    /// <summary>C13. Dich vu tinh: Enemy/TapShooter goi Damage; PlayerHealth Register. Khong co receiver thi canh bao va bo qua.</summary>
    public static class PlayerDamageService
    {
        static IPlayerDamageReceiver receiver;

        public static bool HasReceiver => receiver != null;

        public static void Register(IPlayerDamageReceiver r) { receiver = r; }

        public static void Unregister(IPlayerDamageReceiver r)
        {
            if (ReferenceEquals(receiver, r)) receiver = null;
        }

        public static void Damage(DamageSource source, Vector3 worldPosition)
        {
            if (receiver == null)
            {
                Debug.LogWarning("[PlayerDamageService] Khong co receiver, bo qua sat thuong (" + source + ").");
                return;
            }
            receiver.Damage(source, worldPosition);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { receiver = null; }
    }
}
