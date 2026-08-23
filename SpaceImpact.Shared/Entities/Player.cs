using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Core;
using SpaceImpact.Graphics;
using SpaceImpact.Input;

namespace SpaceImpact.Entities
{
    /// <summary>
    /// The player ship. Reads only an <see cref="InputState"/>, so it behaves
    /// identically on desktop and Android.
    /// </summary>
    public sealed class Player : Entity, ICollidable
    {
        /// <summary>Raised when the ship should fire; the gameplay screen spawns bullets.</summary>
        public event Action<Player> Fired;

        /// <summary>Raised when the player takes a hit (lives already decremented).</summary>
        public event Action<Player> Damaged;

        /// <summary>Raised when the last life is lost.</summary>
        public event Action<Player> Died;

        public int Lives { get; private set; }
        public int WeaponLevel { get; private set; } = 1;

        /// <summary>Post-hit mercy invulnerability.</summary>
        public bool IsInvulnerable => _invulnTimer > 0f;

        /// <summary>Shield pickup active (also blocks damage, and is visible).</summary>
        public bool IsShielded => _shieldTimer > 0f;

        public bool IsRapidFiring => _rapidTimer > 0f;

        public float ShieldTimer => _shieldTimer;
        public float RapidTimer => _rapidTimer;

        private readonly Animation _animation;
        private AnimationPlayer _player;
        private Rectangle _world;

        private float _fireCooldown;
        private float _invulnTimer;
        private float _shieldTimer;
        private float _rapidTimer;

        public Player(Animation animation, Rectangle world)
        {
            _animation = animation;
            _world = world;
            _player.Play(animation);
        }

        public override Rectangle Bounds
        {
            get
            {
                // Generous inset: the classic games forgive wing clipping.
                var r = CenteredRect(_animation.FrameWidth, _animation.FrameHeight);
                r.Inflate(-5, -4);
                return r;
            }
        }

        public void Reset(Vector2 position, int lives)
        {
            OnObtain();
            Position = position;
            Lives = lives;
            WeaponLevel = 1;
            _fireCooldown = 0f;
            _invulnTimer = 0f;
            _shieldTimer = 0f;
            _rapidTimer = 0f;
        }

        public void Update(float dt, in InputState input)
        {
            Age += dt;
            _player.Update(dt);

            if (_invulnTimer > 0f) _invulnTimer -= dt;
            if (_shieldTimer > 0f) _shieldTimer -= dt;
            if (_rapidTimer > 0f) _rapidTimer -= dt;

            // Movement, clamped to the playfield.
            Position += input.Move * GameConfig.PlayerSpeed * dt;
            float halfW = _animation.FrameWidth / 2f;
            float halfH = _animation.FrameHeight / 2f;
            Position.X = MathHelper.Clamp(Position.X, _world.Left + halfW, _world.Right - halfW);
            Position.Y = MathHelper.Clamp(Position.Y, _world.Top + halfH, _world.Bottom - halfH);

            // Firing.
            if (_fireCooldown > 0f)
                _fireCooldown -= dt;

            if (input.Fire && _fireCooldown <= 0f)
            {
                _fireCooldown = GameConfig.PlayerFireCooldown *
                    (IsRapidFiring ? GameConfig.RapidFireMultiplier : 1f);
                Fired?.Invoke(this);
            }
        }

        public override void Update(float dt)
        {
            // The player is driven by the input-aware overload above.
            var idle = default(InputState);
            Update(dt, idle);
        }

        // --- pickups ------------------------------------------------------

        public void UpgradeWeapon() =>
            WeaponLevel = Math.Min(WeaponLevel + 1, GameConfig.MaxWeaponLevel);

        public void AddLife() => Lives++;

        public void GrantShield() => _shieldTimer = GameConfig.ShieldDuration;

        public void GrantRapidFire() => _rapidTimer = GameConfig.RapidFireDuration;

        // --- damage -------------------------------------------------------

        public void OnCollision(ICollidable other)
        {
            switch (other)
            {
                case Bullet bullet when bullet.Owner == BulletOwner.Enemy:
                    TakeHit();
                    break;
                case Enemy _:
                    TakeHit();
                    break;
            }
        }

        private void TakeHit()
        {
            if (!Active || IsInvulnerable)
                return;

            // A shield absorbs the hit entirely and is consumed.
            if (IsShielded)
            {
                _shieldTimer = 0f;
                _invulnTimer = GameConfig.PlayerInvulnTime * 0.5f;
                return;
            }

            Lives--;
            // Keep the current weapon tier after a hit.  Rebuilding firepower
            // after every mistake made later missions unnecessarily punishing.
            _invulnTimer = GameConfig.PlayerInvulnTime;

            Damaged?.Invoke(this);

            if (Lives <= 0)
            {
                Died?.Invoke(this);
                Deactivate();
            }
        }

        // --- drawing ------------------------------------------------------

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Blink while in post-hit mercy frames (skip every other 0.1s slice).
            if (IsInvulnerable && !IsShielded && (int)(_invulnTimer * 10f) % 2 == 0)
                return;

            var origin = new Vector2(_animation.FrameWidth / 2f, _animation.FrameHeight / 2f);
            spriteBatch.Draw(
                _animation.Texture, Position, _animation.FrameRect(_player.FrameIndex),
                Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
        }

        /// <summary>Front (right) edge of the ship, where bullets appear.</summary>
        public Vector2 MuzzlePosition =>
            new Vector2(Position.X + _animation.FrameWidth / 2f, Position.Y);
    }
}
