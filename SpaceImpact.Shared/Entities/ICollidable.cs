using Microsoft.Xna.Framework;

namespace SpaceImpact.Entities
{
    /// <summary>Anything that participates in AABB collision checks.</summary>
    public interface ICollidable
    {
        bool Active { get; }
        Rectangle Bounds { get; }

        /// <summary>Called by the CollisionSystem when overlapping another collidable.</summary>
        void OnCollision(ICollidable other);
    }
}
