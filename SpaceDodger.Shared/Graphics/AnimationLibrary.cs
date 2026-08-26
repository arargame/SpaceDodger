using System;
using System.Collections.Generic;
using SpaceDodger.Entities;

namespace SpaceDodger.Graphics
{
    /// <summary>
    /// Builds and caches every <see cref="Animation"/> the game uses.
    /// Animations are immutable and shared by all entities (Flyweight).
    /// </summary>
    public sealed class AnimationLibrary
    {
        private readonly TextureStore _textures;
        private readonly Dictionary<string, Animation> _cache =
            new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);

        public AnimationLibrary(TextureStore textures)
        {
            _textures = textures;

            Player = Add("player", "sprites/player", 2, 10f);
            PlayerBullet = Add("bullet_player", "sprites/bullet_player", 1, 1f);
            PlayerPlasma = Add("bullet_plasma", "sprites/bullet_plasma", 1, 1f);
            EnemyBullet = Add("bullet_enemy", "sprites/bullet_enemy", 2, 10f);
            EnemyHeavyBullet = Add("bullet_heavy", "sprites/bullet_heavy", 2, 8f);
            HomingBullet = Add("bullet_homing", "sprites/bullet_homing", 2, 10f);
            Explosion = Add("explosion", "sprites/explosion", 6, 20f, loop: false);
            Spark = Add("spark", "sprites/spark", 3, 24f, loop: false);
            Shield = Add("shield", "sprites/shield", 4, 12f);
        }

        public Animation Player { get; }
        public Animation PlayerBullet { get; }
        public Animation PlayerPlasma { get; }
        public Animation EnemyBullet { get; }
        public Animation EnemyHeavyBullet { get; }
        public Animation HomingBullet { get; }
        public Animation Explosion { get; }
        public Animation Spark { get; }
        public Animation Shield { get; }

        /// <summary>Animation for an enemy species, created on first use.</summary>
        public Animation ForEnemy(EnemyDefinition definition)
        {
            if (_cache.TryGetValue(definition.TextureName, out var existing))
                return existing;

            var animation = new Animation(
                _textures.Get(definition.TextureName), definition.Frames, definition.Fps);
            _cache[definition.TextureName] = animation;
            return animation;
        }

        /// <summary>Bullet animation for an enemy weapon kind.</summary>
        public Animation ForEnemyWeapon(EnemyWeapon weapon) =>
            weapon == EnemyWeapon.Heavy ? EnemyHeavyBullet : EnemyBullet;

        private Animation Add(string key, string texture, int frames, float fps, bool loop = true)
        {
            var animation = new Animation(_textures.Get(texture), frames, fps, loop);
            _cache[key] = animation;
            return animation;
        }
    }
}
