using Microsoft.Xna.Framework;

namespace SpaceDodger.Entities
{
    /// <summary>
    /// Shared per-frame world info that enemies and movement strategies read
    /// (player position for aiming/chasing, playfield bounds).
    /// One instance owned by the gameplay screen, updated every frame.
    /// </summary>
    public sealed class EnemyWorld
    {
        public Vector2 PlayerPosition;
        public Rectangle Bounds;
    }
}
