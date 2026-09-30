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
        /// <summary>Tier 2 weapon: Double shot.</summary>
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
        /// <summary>Finite bolt that rebounds from playfield edges.</summary>
        Ricochet = 9,
        /// <summary>Short-lived orbital guard around the player.</summary>
        Orbit = 10,
        /// <summary>Finite growing energy ring travelling forward.</summary>
        Wave = 11,
        /// <summary>Finite full-height energy scan travelling left to right.</summary>
        SweepLaser = 12,
        /// <summary>Finite chain lightning that arcs and rebounds between visible enemies.</summary>
        ChainLightning = 13,
        /// <summary>Tier 3 weapon: 3-way spread shot.</summary>
        Weapon3 = 14,
        /// <summary>Tier 4 weapon: Heavy plasma spread.</summary>
        Weapon4 = 15,
        /// <summary>Tier 5 weapon: 5-way plasma storm.</summary>
        Weapon5 = 16,
    }

    /// <summary>
    /// Pooled pickup that drifts left with a gentle bob and magnetizes toward
    /// the player when within capture range (adapted from Blocked's supply magnet physics).
    /// </summary>
    public sealed class PowerUp : Entity, ICollidable
    {
        private const int Size = 10;

        public PowerUpType Type { get; private set; }
        public bool IsBeingPulled { get; private set; }

        private Texture2D _texture;
        private float _baseY;
        private Rectangle _world;
        private EnemyWorld _worldInfo;

        public override Rectangle Bounds => CenteredRect(Size, Size);

        public void Configure(Texture2D texture, PowerUpType type, Vector2 position, Rectangle world, EnemyWorld worldInfo = null)
        {
            _texture = texture;
            Type = type;
            Position = position;
            _baseY = position.Y;
            _world = world;
            _worldInfo = worldInfo;
            Velocity = new Vector2(-GameConfig.PowerUpSpeed, 0f);
            IsBeingPulled = false;
        }

        public override void Update(float dt)
        {
            Age += dt;

            bool inField = false;
            if (_worldInfo != null && _worldInfo.PlayerActive)
            {
                Vector2 toPlayer = _worldInfo.PlayerPosition - Position;
                float dist = toPlayer.Length();

                if (dist > 1f && dist < GameConfig.SupplyMagnetRadius)
                {
                    Vector2 dir = toPlayer / dist;
                    Velocity += dir * (GameConfig.SupplyMagnetAccel * dt);

                    float speed = Velocity.Length();
                    if (speed > GameConfig.SupplyMagnetMaxSpeed)
                    {
                        Velocity = (Velocity / speed) * GameConfig.SupplyMagnetMaxSpeed;
                    }

                    inField = true;
                }
            }

            IsBeingPulled = inField;

            if (inField)
            {
                // Magnet has direct kinetic influence: move along velocity vector
                Position += Velocity * dt;
                _baseY = Position.Y;
            }
            else
            {
                // Smoothly relax back to natural leftward drift velocity
                Vector2 natural = new Vector2(-GameConfig.PowerUpSpeed, 0f);
                float returnFactor = 1f - (float)Math.Exp(-GameConfig.SupplyMagnetDamping * dt);
                Velocity += (natural - Velocity) * returnFactor;

                Position.X += Velocity.X * dt;
                _baseY += Velocity.Y * dt;
                _baseY = MathHelper.Clamp(_baseY, _world.Top + Size, _world.Bottom - Size);
                Position.Y = _baseY + 4f * (float)Math.Sin(Age * 4f);
            }

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

            if (IsBeingPulled)
            {
                // Subtle bright cyan shimmer when locked into the ship's magnetic capture beam
                float pulse = (float)Math.Sin(Age * 18f) * 0.25f + 0.75f;
                color = Color.Lerp(color, Color.Cyan, 0.40f) * (1f + 0.15f * pulse);
            }

            spriteBatch.Draw(
                _texture,
                Position - new Vector2(Size / 2f, Size / 2f),
                src, color);
        }
    }
}
