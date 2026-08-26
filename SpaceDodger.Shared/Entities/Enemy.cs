using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Graphics;
using SpaceDodger.Movement;

namespace SpaceDodger.Entities
{
    /// <summary>
    /// Pooled enemy. Behaviour comes entirely from data
    /// (<see cref="EnemyDefinition"/>) plus an injected
    /// <see cref="IMovementStrategy"/>, so new enemy types need no new classes.
    /// </summary>
    public sealed class Enemy : Entity, ICollidable
    {
        /// <summary>Raised when health reaches zero. The spawner subscribes to
        /// award score, spawn explosions and roll powerup drops.</summary>
        public event Action<Enemy> Destroyed;

        /// <summary>Raised when the fire timer elapses; the spawner creates the bullet.</summary>
        public event Action<Enemy> WantsToFire;

        /// <summary>Raised when damaged but still alive, for impact feedback.</summary>
        public event Action<Enemy> Hit;

        public EnemyDefinition Definition { get; private set; }

        // Named MovementStrategy (not Movement) so it never shadows the
        // SpaceDodger.Movement namespace inside this file.
        public IMovementStrategy MovementStrategy { get; private set; }

        public int Health { get; private set; }

        /// <summary>Health this enemy spawned with (definition value * wave multiplier).</summary>
        public int MaxHealth { get; private set; }

        /// <summary>Shared world snapshot (player position, playfield bounds).</summary>
        public EnemyWorld WorldInfo { get; private set; }

        /// <summary>Y coordinate this enemy spawned at (used by wave patterns).</summary>
        public float SpawnY { get; private set; }

        /// <summary>Per-wave speed scaling applied on top of the definition speed.</summary>
        public float SpeedMultiplier { get; private set; } = 1f;

        public float EffectiveSpeed => Definition.Speed * SpeedMultiplier;

        public bool IsBoss => Definition.IsBoss;

        private Animation _animation;
        private AnimationPlayer _player;
        private Rectangle _world;
        private float _fireTimer;
        private float _hitFlash;

        public override Rectangle Bounds
        {
            get
            {
                // Slightly forgiving hitbox (2px inset) — feels better to play.
                var r = CenteredRect(_animation.FrameWidth, _animation.FrameHeight);
                r.Inflate(-2, -2);
                return r;
            }
        }

        public void Configure(
            EnemyDefinition definition, Animation animation, IMovementStrategy movement,
            Vector2 position, EnemyWorld world, float healthMultiplier = 1f, float speedMultiplier = 1f)
        {
            Definition = definition;
            _animation = animation;
            _player.Play(animation);
            MovementStrategy = movement;
            Position = position;
            SpawnY = position.Y;
            WorldInfo = world;
            _world = world.Bounds;
            SpeedMultiplier = speedMultiplier;
            MaxHealth = Math.Max(1, (int)Math.Round(definition.MaxHealth * healthMultiplier));
            Health = MaxHealth;

            // Stagger first shots so a wave does not fire in unison.
            _fireTimer = definition.Shoots ? definition.FireInterval * 0.5f : 0f;
            _hitFlash = 0f;
        }

        public override void OnRelease()
        {
            base.OnRelease();
            // Pooled objects must not keep listeners alive between lives.
            Destroyed = null;
            WantsToFire = null;
            Hit = null;
        }

        public override void Update(float dt)
        {
            Age += dt;
            _player.Update(dt);
            MovementStrategy.Move(this, WorldInfo, dt);

            if (_hitFlash > 0f)
                _hitFlash -= dt;

            if (Definition.Shoots && Position.X < _world.Right)
            {
                _fireTimer -= dt;
                if (_fireTimer <= 0f)
                {
                    _fireTimer = Definition.FireInterval;
                    WantsToFire?.Invoke(this);
                }
            }

            // Despawn once fully past the left edge (no score, no explosion).
            if (Position.X < _world.Left - _animation.FrameWidth)
                Deactivate();
        }

        public void TakeDamage(int amount)
        {
            if (!Active)
                return;

            Health -= amount;
            _hitFlash = 0.08f;

            if (Health <= 0)
            {
                Destroyed?.Invoke(this);
                Deactivate();
            }
            else if (amount > 0)
            {
                Hit?.Invoke(this);
            }
        }

        public void OnCollision(ICollidable other)
        {
            if (other is Bullet bullet && bullet.Owner == BulletOwner.Player)
                TakeDamage(bullet.Damage);
            else if (other is Player)
                TakeDamage(IsBoss ? 0 : Health); // ramming kills normal enemies only
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Tint red briefly when damaged so hits read clearly at this resolution.
            var color = _hitFlash > 0f ? new Color(255, 120, 120) : Color.White;
            var origin = new Vector2(_animation.FrameWidth / 2f, _animation.FrameHeight / 2f);

            spriteBatch.Draw(
                _animation.Texture, Position, _animation.FrameRect(_player.FrameIndex),
                color, 0f, origin, 1f, SpriteEffects.None, 0f);
        }

        /// <summary>Muzzle point in world space (front/left edge of the sprite).</summary>
        public Vector2 MuzzlePosition =>
            new Vector2(Position.X - _animation.FrameWidth / 2f, Position.Y);

        public float HealthFraction => MaxHealth <= 0
            ? 0f
            : MathHelper.Clamp(Health / (float)MaxHealth, 0f, 1f);
    }
}
