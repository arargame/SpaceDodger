namespace SpaceImpact.Core
{
    /// <summary>Central tuning constants (single source of truth, DRY).</summary>
    public static class GameConfig
    {
        // Virtual (native pixel-art) resolution, 16:9. Everything is drawn here
        // and then scaled up with point sampling.
        public const int VirtualWidth = 320;
        public const int VirtualHeight = 180;

        // Desktop window = virtual * scale.
        public const int WindowScale = 4;

        public const int LevelCount = 50;

        /// <summary>A boss level occurs every N levels (10, 20, 30, 40, 50).</summary>
        public const int BossEvery = 10;

        public const int PlayerLives = 3;
        public const float PlayerSpeed = 95f;          // px/s in virtual space
        public const float PlayerFireCooldown = 0.22f; // seconds
        public const float PlayerInvulnTime = 2.0f;    // seconds after a hit
        public const int MaxWeaponLevel = 5;

        public const float PlayerBulletSpeed = 200f;
        public const float EnemyBulletSpeed = 90f;
        public const float EnemyHeavyBulletSpeed = 68f;

        public const float PowerUpDropChance = 0.14f;
        public const float PowerUpSpeed = 30f;

        /// <summary>Seconds a collected shield lasts.</summary>
        public const float ShieldDuration = 8f;

        /// <summary>Seconds of boosted fire rate from a rapid-fire pickup.</summary>
        public const float RapidFireDuration = 10f;

        /// <summary>Fire cooldown multiplier while rapid fire is active.</summary>
        public const float RapidFireMultiplier = 0.45f;

        /// <summary>Points awarded by a score pickup.</summary>
        public const int ScorePickupValue = 250;

        public const int HighScoreCapacity = 8;

        public const string SaveFileName = "savegame.json";
    }
}
