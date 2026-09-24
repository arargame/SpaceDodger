namespace SpaceDodger.Entities
{
    /// <summary>Common damage contract for pooled player-owned special projectiles.</summary>
    public interface IPlayerProjectile
    {
        int Damage { get; }
    }
}
