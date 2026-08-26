using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Core;
using SpaceDodger.Entities;
using SpaceDodger.Graphics;
using SpaceDodger.Movement;
using SpaceDodger.Pooling;
using SpaceDodger.Weapons;

namespace SpaceDodger.Systems
{
    /// <summary>
    /// Factory + owner of every pooled entity (Factory pattern over Object Pools).
    /// Gameplay code asks for "a bullet here" and never sees `new`.
    /// </summary>
    public sealed class EntityFactory
    {
        private readonly AnimationLibrary _animations;
        private readonly TextureStore _textures;
        private readonly Rectangle _world;
        private readonly Random _random = new Random();

        public EntityPool<Bullet> PlayerBullets { get; }
        public EntityPool<Bullet> EnemyBullets { get; }
        public EntityPool<Enemy> Enemies { get; }
        public EntityPool<Explosion> Explosions { get; }
        public EntityPool<PowerUp> PowerUps { get; }
        public EntityPool<HomingBullet> HomingBullets { get; }

        public EntityFactory(AnimationLibrary animations, TextureStore textures, Rectangle world)
        {
            _animations = animations;
            _textures = textures;
            _world = world;

            PlayerBullets = new EntityPool<Bullet>(() => new Bullet(), 64);
            EnemyBullets = new EntityPool<Bullet>(() => new Bullet(), 96);
            Enemies = new EntityPool<Enemy>(() => new Enemy(), 48);
            Explosions = new EntityPool<Explosion>(() => new Explosion(), 32);
            PowerUps = new EntityPool<PowerUp>(() => new PowerUp(), 12);
            HomingBullets = new EntityPool<HomingBullet>(() => new HomingBullet(), 16);
        }

        /// <summary>Fire 8 scatter bullets in a radial pattern around the player.</summary>
        public void SpawnScatterShot(Vector2 position)
        {
            var animation = _animations.PlayerBullet;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * MathHelper.TwoPi / 8f;
                var velocity = new Vector2(
                    (float)Math.Cos(angle) * GameConfig.ScatterBulletSpeed,
                    (float)Math.Sin(angle) * GameConfig.ScatterBulletSpeed);
                var bullet = PlayerBullets.Obtain();
                bullet.Configure(animation, BulletOwner.Player, 1, position, velocity, _world);
            }
        }

        /// <summary>Spawn a homing missile that seeks the nearest enemy.</summary>
        public void SpawnHomingMissile(Vector2 position)
        {
            var animation = _animations.HomingBullet;
            var missile = HomingBullets.Obtain();
            missile.Configure(animation, position, 0f, GameConfig.HomingMissileSpeed, _world, Enemies.Items);
        }

        /// <summary>Fire the player's current weapon pattern from a muzzle point.</summary>
        public void SpawnPlayerShot(Vector2 muzzle, int weaponLevel)
        {
            var pattern = WeaponRegistry.ForLevel(weaponLevel);
            var animation = pattern.Plasma ? _animations.PlayerPlasma : _animations.PlayerBullet;

            for (int i = 0; i < pattern.ShotCount; i++)
            {
                var bullet = PlayerBullets.Obtain();
                bullet.Configure(
                    animation,
                    BulletOwner.Player,
                    pattern.Damage,
                    muzzle + pattern.OffsetFor(i),
                    pattern.VelocityFor(i, GameConfig.PlayerBulletSpeed, 1f),
                    _world);
            }
        }

        /// <summary>Fire an enemy's weapon, optionally aimed at the player.</summary>
        public void SpawnEnemyShot(Enemy enemy, Vector2 playerPosition)
        {
            var definition = enemy.Definition;
            var muzzle = enemy.MuzzlePosition;
            var animation = _animations.ForEnemyWeapon(definition.Weapon);

            bool heavy = definition.Weapon == EnemyWeapon.Heavy;
            float speed = heavy ? GameConfig.EnemyHeavyBulletSpeed : GameConfig.EnemyBulletSpeed;
            int damage = heavy ? 2 : 1;

            var forward = AimDirection(muzzle, playerPosition, definition.AimAtPlayer);

            if (definition.Weapon == EnemyWeapon.Spread)
            {
                // Three shots fanned around the aim direction.
                for (int i = -1; i <= 1; i++)
                    FireOne(animation, damage, muzzle, Rotate(forward, i * 0.28f) * speed);
            }
            else
            {
                FireOne(animation, damage, muzzle, forward * speed);
            }
        }

        private void FireOne(Animation animation, int damage, Vector2 muzzle, Vector2 velocity)
        {
            var bullet = EnemyBullets.Obtain();
            bullet.Configure(animation, BulletOwner.Enemy, damage, muzzle, velocity, _world);
        }

        private static Vector2 AimDirection(Vector2 from, Vector2 target, bool aim)
        {
            if (!aim)
                return -Vector2.UnitX;

            var direction = target - from;
            if (direction.LengthSquared() < 0.001f)
                return -Vector2.UnitX;

            direction.Normalize();
            return direction;
        }

        private static Vector2 Rotate(Vector2 v, float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }

        /// <summary>Spawn an enemy of the given species at a position.</summary>
        public Enemy SpawnEnemy(
            EnemyDefinition definition, IMovementStrategy movement,
            Vector2 position, EnemyWorld world,
            float healthMultiplier, float speedMultiplier)
        {
            var enemy = Enemies.Obtain();
            enemy.Configure(
                definition, _animations.ForEnemy(definition), movement,
                position, world, healthMultiplier, speedMultiplier);
            return enemy;
        }

        public void SpawnExplosion(Vector2 position, float scale = 1f)
        {
            var explosion = Explosions.Obtain();
            explosion.Configure(_animations.Explosion, position, scale);
        }

        /// <summary>Small impact spark, reusing the explosion pool.</summary>
        public void SpawnSpark(Vector2 position)
        {
            var spark = Explosions.Obtain();
            spark.Configure(_animations.Spark, position);
        }

        /// <summary>
        /// Roll for a pickup drop at a destroyed enemy's position.
        /// Bosses always drop, and drop from the rarer end of the table.
        /// </summary>
        public void MaybeDropPowerUp(Vector2 position, bool guaranteed = false)
        {
            if (!guaranteed && _random.NextDouble() > GameConfig.PowerUpDropChance)
                return;

            var type = guaranteed ? RollBossDrop() : RollCommonDrop();
            var powerUp = PowerUps.Obtain();
            powerUp.Configure(_textures.Get("sprites/powerups"), type, position, _world);
        }

        private PowerUpType RollCommonDrop()
        {
            double r = _random.NextDouble();
            if (r < 0.28) return PowerUpType.Weapon;
            if (r < 0.44) return PowerUpType.Score;
            if (r < 0.58) return PowerUpType.Rapid;
            if (r < 0.70) return PowerUpType.Shield;
            if (r < 0.78) return PowerUpType.Bomb;
            if (r < 0.86) return PowerUpType.Scatter;
            if (r < 0.94) return PowerUpType.Homing;
            return PowerUpType.Health;
        }

        private PowerUpType RollBossDrop()
        {
            double r = _random.NextDouble();
            if (r < 0.30) return PowerUpType.Weapon;
            if (r < 0.50) return PowerUpType.Health;
            if (r < 0.65) return PowerUpType.Shield;
            if (r < 0.78) return PowerUpType.Scatter;
            if (r < 0.90) return PowerUpType.Homing;
            return PowerUpType.Bomb;
        }

        /// <summary>Smart bomb: kill every active enemy, awarding score for each.</summary>
        public void DetonateBomb(Action<Enemy> onKilled)
        {
            var items = Enemies.Items;
            for (int i = 0; i < items.Count; i++)
            {
                var enemy = items[i];
                if (!enemy.Active)
                    continue;

                // Bosses are heavily damaged rather than instantly killed.
                if (enemy.IsBoss)
                {
                    enemy.TakeDamage(Math.Max(1, enemy.MaxHealth / 4));
                    continue;
                }

                onKilled?.Invoke(enemy);
                enemy.TakeDamage(enemy.Health);
            }

            // Enemy bullets are cleared too — that is the point of a smart bomb.
            EnemyBullets.ReleaseAll();
        }

        public void UpdateAll(float dt)
        {
            PlayerBullets.Update(dt);
            HomingBullets.Update(dt);
            EnemyBullets.Update(dt);
            Enemies.Update(dt);
            Explosions.Update(dt);
            PowerUps.Update(dt);
        }

        public void DrawAll(SpriteBatch spriteBatch)
        {
            PowerUps.Draw(spriteBatch);
            Enemies.Draw(spriteBatch);
            PlayerBullets.Draw(spriteBatch);
            HomingBullets.Draw(spriteBatch);
            EnemyBullets.Draw(spriteBatch);
            Explosions.Draw(spriteBatch);
        }

        public void ReleaseAll()
        {
            PlayerBullets.ReleaseAll();
            HomingBullets.ReleaseAll();
            EnemyBullets.ReleaseAll();
            Enemies.ReleaseAll();
            Explosions.ReleaseAll();
            PowerUps.ReleaseAll();
        }
    }
}
