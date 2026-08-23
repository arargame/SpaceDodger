using Microsoft.Xna.Framework;

namespace SpaceImpact.Weapons
{
    /// <summary>
    /// Strategy describing the bullet spread produced by one shot.
    /// Returns offsets/velocities so the caller owns the actual pooling.
    /// </summary>
    public interface IWeaponPattern
    {
        /// <summary>Number of bullets this shot emits.</summary>
        int ShotCount { get; }

        /// <summary>Spawn offset relative to the muzzle for shot i.</summary>
        Vector2 OffsetFor(int index);

        /// <summary>Velocity for shot i, given the base speed and facing direction.</summary>
        Vector2 VelocityFor(int index, float speed, float directionX);

        int Damage { get; }

        /// <summary>True for upper tiers, which use the fatter plasma bullet sprite.</summary>
        bool Plasma { get; }
    }
}
