using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Core;

namespace SpaceDodger.Entities
{
    /// <summary>
    /// Pickup kinds. The order matches the frame order in sprites/powerups.png,
    /// so the enum value doubles as the sprite index.
    /// </summary>
    public enum PowerUpType
    {
        /// <summary>+1 life.</summary>
        Health = 0,
        /// <summary>+1 weapon tier.</summary>
        Weapon = 1,
        /// <summary>Temporary invulnerability bubble.</summary>
        Shield = 2,
        /// <summary>Smart bomb: destroys every enemy on screen.</summary>
        Bomb = 3,
        /// <summary>Temporary fire-rate boost.</summary>
        Rapid = 4,
        /// <summary>Instant bonus points.</summary>
        Score = 5,
        /// <summary>Temporary radial scatter fire around the player.</summary>
        Scatter = 6,
        /// <summary>Grants homing missiles that seek the nearest enemy.</summary>
        Homing = 7,
        /// <summary>Temporary dual spiral vortex fire.</summary>
        Spiral = 8,
    }

    /// <summary>Pooled pickup that drifts left with a gentle bob.</summary>
    public sealed class PowerUp : Entity, ICollidable
    {
        private const int Size = 10;

        public PowerUpType Type { get; private set; }

        private Texture2D _texture;
        private float _baseY;
        private Rectangle _world;

        public override Rectangle Bounds => CenteredRect(Size, Size);

        public void Configure(Texture2D texture, PowerUpType type, Vector2 position, Rectangle world)
        {
            _texture = texture;
            Type = type;
            Position = position;
            _baseY = position.Y;
            _world = world;
            Velocity = new Vector2(-GameConfig.PowerUpSpeed, 0f);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Position.Y = _baseY + 4f * (float)Math.Sin(Age * 4f);

            if (Position.X < _world.Left - Size)
                Deactivate();
        }

        public void OnCollision(ICollidable other)
        {
            // Only the player collides with pickups; the effect is applied there.
            Deactivate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var src = new Rectangle((int)Type * Size, 0, Size, Size);

            // Blink for the last second before drifting off screen.
            var color = Color.White;
            if (Position.X < _world.Left + 24 && (int)(Age * 12f) % 2 == 0)
                color = Color.White * 0.45f;

            spriteBatch.Draw(
                _texture,
                Position - new Vector2(Size / 2f, Size / 2f),
                src, color);
        }
    }
}
