using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Core;
using SpaceDodger.Graphics;
using SpaceDodger.Input;

namespace SpaceDodger.Entities
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
        public float WeaponTimer { get; private set; }

        /// <summary>Post-hit mercy invulnerability.</summary>
        public bool IsInvulnerable => _invulnTimer > 0f;

        /// <summary>Shield pickup active (also blocks damage, and is visible).</summary>
        public bool IsShielded => _shieldTimer > 0f;

        public bool IsRapidFiring => _rapidTimer > 0f;
        public float ShieldTimer => _shieldTimer;
        public float RapidTimer => _rapidTimer;
        public float SpiralAngle => _spiralAngle;
        public SpecialFireType SpecialFire { get; private set; }
        public int SpecialCharges { get; private set; }
        public int OrbitCount { get; private set; }
        public float OrbitTimer { get; private set; }

        public string SpecialFireLabel => SpecialFire switch
        {
            SpecialFireType.Scatter => "SCATTER",
            SpecialFireType.Spiral => "SPIRAL",
            SpecialFireType.Homing => "HOMING",
            SpecialFireType.Ricochet => "RICO",
            SpecialFireType.Wave => "WAVE",
            SpecialFireType.SweepLaser => "LASER",
            _ => ""
        };

        private readonly Animation _animation;
        private AnimationPlayer _player;
        private Rectangle _world;

        private float _fireCooldown;
        private float _invulnTimer;
        private float _shieldTimer;
        private float _rapidTimer;
        private float _spiralAngle;
        private float _specialCooldown;

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
            WeaponTimer = 0f;
            _fireCooldown = 0f;
            _invulnTimer = 0f;
            _shieldTimer = 0f;
            _rapidTimer = 0f;
            _spiralAngle = 0f;
            _specialCooldown = 0f;
            SpecialFire = SpecialFireType.None;
            SpecialCharges = 0;
            OrbitCount = 0;
            OrbitTimer = 0f;
        }

        public void RestoreProgress(int lives, int weaponLevel, float shieldTime, float rapidTime,
            int specialFire, int specialCharges, int orbitCount, float orbitTime, float weaponTime)
        {
            Lives = Math.Max(1, lives);
            WeaponLevel = MathHelper.Clamp(weaponLevel, 1, GameConfig.MaxWeaponLevel);
            WeaponTimer = WeaponLevel == 1 ? 0f : Math.Max(0f, weaponTime);
            if (WeaponTimer <= 0f)
                WeaponLevel = 1;
            _shieldTimer = Math.Max(0f, shieldTime);
            _rapidTimer = Math.Max(0f, rapidTime);
            SpecialFire = System.Enum.IsDefined(typeof(SpecialFireType), specialFire)
                ? (SpecialFireType)specialFire
                : SpecialFireType.None;
            SpecialCharges = SpecialFire == SpecialFireType.None ? 0 : Math.Max(0, specialCharges);
            if (SpecialCharges == 0)
                SpecialFire = SpecialFireType.None;
            OrbitCount = MathHelper.Clamp(orbitCount, 0, GameConfig.OrbitMaximumShots);
            OrbitTimer = Math.Max(0f, orbitTime);
            if (OrbitTimer <= 0f)
                OrbitCount = 0;
        }

        public void Update(float dt, in InputState input)
        {
            Age += dt;
            _player.Update(dt);

            if (_invulnTimer > 0f) _invulnTimer -= dt;
            if (_shieldTimer > 0f) _shieldTimer -= dt;
            if (_rapidTimer > 0f) _rapidTimer -= dt;
            if (WeaponTimer > 0f)
            {
                WeaponTimer -= dt;
                if (WeaponTimer <= 0f)
                {
                    WeaponTimer = 0f;
                    WeaponLevel = 1;
                }
            }
            if (OrbitTimer > 0f)
            {
                OrbitTimer -= dt;
                if (OrbitTimer <= 0f)
                    OrbitCount = 0;
            }
            if (_specialCooldown > 0f) _specialCooldown -= dt;
            _spiralAngle += dt * 8.5f;

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

        /// <summary>Loads a timed main-weapon tier. A fresh pickup refreshes rather than stacks the timer.</summary>
        public void GrantWeaponUpgrade(int level, float duration)
        {
            WeaponLevel = MathHelper.Clamp(level, 1, GameConfig.MaxWeaponLevel);
            WeaponTimer = WeaponLevel == 1 ? 0f : Math.Max(0f, duration);
        }

        public void AddLife() => Lives++;

        public void GrantShield(float duration) =>
            _shieldTimer = MathHelper.Clamp(duration, GameConfig.TimedEffectMinimumDuration, GameConfig.TimedEffectMaximumDuration);

        public void GrantRapidFire(float duration) =>
            _rapidTimer = MathHelper.Clamp(duration, GameConfig.TimedEffectMinimumDuration, GameConfig.TimedEffectMaximumDuration);

        /// <summary>Loads one finite special cartridge, replacing the previous one to prevent stacking screen clears.</summary>
        public void EquipSpecial(SpecialFireType type, int charges)
        {
            SpecialFire = type;
            SpecialCharges = Math.Max(0, charges);
            _specialCooldown = 0f;
        }

        /// <summary>Refreshes one bounded orbit guard; orbit pickups never stack into a permanent wall.</summary>
        public void GrantOrbitGuard(int count, float duration)
        {
            OrbitCount = MathHelper.Clamp(count, GameConfig.OrbitMinimumShots, GameConfig.OrbitMaximumShots);
            OrbitTimer = MathHelper.Clamp(duration, GameConfig.OrbitMinimumDuration, GameConfig.OrbitMaximumDuration);
        }

        /// <summary>Consumes at most one special effect per controlled cadence window.</summary>
        public bool TryConsumeSpecial(out SpecialFireType type)
        {
            type = SpecialFire;
            if (type == SpecialFireType.None || SpecialCharges <= 0 || _specialCooldown > 0f)
                return false;

            SpecialCharges--;
            _specialCooldown = type == SpecialFireType.SweepLaser ? .85f
                : type == SpecialFireType.Wave ? .48f
                : .30f;

            if (SpecialCharges == 0)
                SpecialFire = SpecialFireType.None;

            return true;
        }

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
            // Losing a life costs one weapon tier (softens death spirals).
            WeaponLevel = Math.Max(1, WeaponLevel - 1);
            if (WeaponLevel == 1)
                WeaponTimer = 0f;
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
