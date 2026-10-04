namespace ClaudeCop.Core
{
    /// <summary>C1. Chat lieu be mat dung cho FX/Props. Concrete la mac dinh.</summary>
    public enum SurfaceMaterial { Concrete = 0, Wood, Metal, Glass, Foliage, Flesh }

    /// <summary>C3. Loai vu khi. Dung boi Combat, Enemy, Jev, UI.</summary>
    public enum WeaponKind { Pistol = 0, Shotgun, MachineGun }

    /// <summary>C6. Loai muc tieu co the tap. Grenade du tru.</summary>
    public enum TargetKind { Enemy = 0, Hostage, Pickup, Grenade }

    /// <summary>C9. Ket qua cua mot lan tap len muc tieu.</summary>
    public enum TapOutcome { Miss = 0, Kill, JusticeKill, HostageHit, PickupCollected, Environment, Blocked }

    /// <summary>C12. Nguon sat thuong len nguoi choi. Explosion du tru.</summary>
    public enum DamageSource { EnemyShot = 0, HostageHit, Explosion }

    /// <summary>C14. Trang thai tong cua game. Game phat qua GameEvents.</summary>
    public enum GameState { Title = 0, Playing, RevivePrompt, Win, GameOver }
}
