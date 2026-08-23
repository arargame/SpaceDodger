using System;
using System.Collections.Generic;

namespace SpaceImpact.Entities
{
    /// <summary>How an enemy shoots (selects the bullet sprite and behaviour).</summary>
    public enum EnemyWeapon
    {
        /// <summary>Never fires.</summary>
        None,
        /// <summary>Single orb, straight or aimed.</summary>
        Orb,
        /// <summary>Slower, larger, higher-damage shell.</summary>
        Heavy,
        /// <summary>Three orbs in a fan.</summary>
        Spread,
    }

    /// <summary>
    /// Immutable data describing one enemy species. Adding a species is a single
    /// entry in <see cref="EnemyCatalog"/> plus a sprite sheet — no new class.
    /// </summary>
    public sealed class EnemyDefinition
    {
        public string Key { get; }
        public string TextureName { get; }
        public int Frames { get; }
        public float Fps { get; }
        public int MaxHealth { get; }
        public int Score { get; }
        public float Speed { get; }
        public EnemyWeapon Weapon { get; }
        public float FireInterval { get; }   // 0 = never fires
        public bool AimAtPlayer { get; }
        public bool IsBoss { get; }
        public int ContactDamage { get; }

        public EnemyDefinition(
            string key, string textureName, int frames, float fps,
            int maxHealth, int score, float speed,
            EnemyWeapon weapon = EnemyWeapon.None, float fireInterval = 0f,
            bool aimAtPlayer = false, bool isBoss = false, int contactDamage = 1)
        {
            Key = key;
            TextureName = textureName;
            Frames = frames;
            Fps = fps;
            MaxHealth = maxHealth;
            Score = score;
            Speed = speed;
            Weapon = weapon;
            FireInterval = fireInterval;
            AimAtPlayer = aimAtPlayer;
            IsBoss = isBoss;
            ContactDamage = contactDamage;
        }

        public bool Shoots => Weapon != EnemyWeapon.None && FireInterval > 0f;
    }

    /// <summary>Registry of all enemy species, keyed by the name used in level JSON.</summary>
    public static class EnemyCatalog
    {
        private static EnemyDefinition Def(
            string key, int frames, float fps, int hp, int score, float speed,
            EnemyWeapon weapon = EnemyWeapon.None, float fire = 0f,
            bool aim = false, bool boss = false)
        {
            var texture = boss ? $"sprites/{key}" : $"sprites/enemy_{key}";
            return new EnemyDefinition(
                key, texture, frames, fps, hp, score, speed,
                weapon, fire, aim, boss);
        }

        private static readonly Dictionary<string, EnemyDefinition> Definitions =
            new Dictionary<string, EnemyDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                // --- fodder -------------------------------------------------
                ["drone"] = Def("drone", 2, 6f, hp: 1, score: 10, speed: 42f),
                ["scout"] = Def("scout", 2, 10f, hp: 1, score: 15, speed: 86f),
                ["mine"] = Def("mine", 2, 4f, hp: 2, score: 20, speed: 18f),

                // --- shooters -----------------------------------------------
                ["fighter"] = Def("fighter", 2, 8f, hp: 2, score: 25, speed: 58f,
                    weapon: EnemyWeapon.Orb, fire: 2.6f, aim: true),
                ["wasp"] = Def("wasp", 2, 12f, hp: 2, score: 30, speed: 74f,
                    weapon: EnemyWeapon.Orb, fire: 2.2f),
                ["seeker"] = Def("seeker", 2, 10f, hp: 2, score: 35, speed: 66f,
                    weapon: EnemyWeapon.Orb, fire: 3.0f, aim: true),
                ["raider"] = Def("raider", 2, 8f, hp: 4, score: 45, speed: 62f,
                    weapon: EnemyWeapon.Spread, fire: 3.4f, aim: true),
                ["lancer"] = Def("lancer", 2, 10f, hp: 3, score: 40, speed: 96f),

                // --- heavies ------------------------------------------------
                ["bomber"] = Def("bomber", 2, 4f, hp: 6, score: 55, speed: 26f,
                    weapon: EnemyWeapon.Heavy, fire: 3.2f),
                ["shielder"] = Def("shielder", 2, 6f, hp: 8, score: 65, speed: 34f,
                    weapon: EnemyWeapon.Orb, fire: 2.8f, aim: true),
                ["turret"] = Def("turret", 2, 5f, hp: 10, score: 80, speed: 20f,
                    weapon: EnemyWeapon.Spread, fire: 2.4f, aim: true),
                ["spinner"] = Def("spinner", 2, 8f, hp: 5, score: 60, speed: 40f,
                    weapon: EnemyWeapon.Spread, fire: 2.0f),
                ["hulk"] = Def("hulk", 2, 3f, hp: 18, score: 120, speed: 22f,
                    weapon: EnemyWeapon.Heavy, fire: 2.6f, aim: true),

                // --- bosses (one per 10 levels) -----------------------------
                ["boss_warden"] = Def("boss_warden", 2, 5f, hp: 60, score: 500, speed: 30f,
                    weapon: EnemyWeapon.Orb, fire: 1.3f, aim: true, boss: true),
                ["boss_hydra"] = Def("boss_hydra", 2, 5f, hp: 95, score: 800, speed: 32f,
                    weapon: EnemyWeapon.Spread, fire: 1.5f, aim: true, boss: true),
                ["boss_titan"] = Def("boss_titan", 2, 4f, hp: 140, score: 1200, speed: 26f,
                    weapon: EnemyWeapon.Heavy, fire: 1.2f, aim: true, boss: true),
                ["boss_core"] = Def("boss_core", 2, 6f, hp: 190, score: 1700, speed: 34f,
                    weapon: EnemyWeapon.Spread, fire: 1.0f, aim: true, boss: true),
                ["boss_nemesis"] = Def("boss_nemesis", 2, 6f, hp: 260, score: 2500, speed: 36f,
                    weapon: EnemyWeapon.Spread, fire: 0.85f, aim: true, boss: true),
            };

        /// <summary>Look up a species; unknown names fall back to the drone.</summary>
        public static EnemyDefinition Get(string name)
        {
            if (!string.IsNullOrEmpty(name) && Definitions.TryGetValue(name, out var definition))
                return definition;
            return Definitions["drone"];
        }

        public static IEnumerable<string> Keys => Definitions.Keys;
    }
}
