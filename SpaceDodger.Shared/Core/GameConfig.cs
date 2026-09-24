namespace SpaceDodger.Core
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

        public const int LevelCount = 100;

        /// <summary>A boss level occurs every N levels (10, 20, 30, 40, 50).</summary>
        public const int BossEvery = 10;

        public const int PlayerLives = 3;
        public const float PlayerSpeed = 95f;          // px/s in virtual space
        public const float PlayerFireCooldown = 0.22f; // seconds
        public const float PlayerInvulnTime = 2.0f;    // seconds after a hit
        public const int MaxWeaponLevel = 5;
        public const float WeaponTier2MinimumDuration = 12f;
        public const float WeaponTier2MaximumDuration = 20f;
        public const float WeaponTier3MinimumDuration = 10f;
        public const float WeaponTier3MaximumDuration = 18f;
        public const float WeaponTier4MinimumDuration = 8f;
        public const float WeaponTier4MaximumDuration = 15f;
        public const float WeaponTier5MinimumDuration = 7f;
        public const float WeaponTier5MaximumDuration = 20f;

        public const float PlayerBulletSpeed = 200f;
        public const float EnemyBulletSpeed = 90f;
        public const float EnemyHeavyBulletSpeed = 68f;
        public const float ScatterBulletSpeed = 160f;
        public const float HomingMissileSpeed = 140f;
        public const float SpiralBulletSpeed = 190f;
        public const float RicochetBulletSpeed = 172f;

        /// <summary>Base per-enemy supply chance before the DDA reward multiplier.</summary>
        public const float PowerUpDropChance = 0.07f;
        public const float PowerUpSpeed = 30f;
        public const int MaximumActivePowerUps = 2;
        public const float PowerUpDropCooldown = 3.5f;
        public const float SupplyDriftMinimumInterval = 11f;
        public const float SupplyDriftMaximumInterval = 17f;

        /// <summary>Seconds a collected shield lasts.</summary>
        public const float ShieldDuration = 8f;

        /// <summary>Seconds of boosted fire rate from a rapid-fire pickup.</summary>
        public const float RapidFireDuration = 10f;
        public const int SpecialMinimumCharges = 10;
        public const int SpecialMaximumCharges = 20;
        public const int HomingMinimumCharges = 8;
        public const int HomingMaximumCharges = 20;
        public const int WaveMinimumCharges = 8;
        public const int WaveMaximumCharges = 20;
        public const int SweepLaserMinimumCharges = 3;
        public const int SweepLaserMaximumCharges = 10;
        public const int OrbitMinimumShots = 2;
        public const int OrbitMaximumShots = 3;
        public const float TimedEffectMinimumDuration = 5f;
        public const float TimedEffectMaximumDuration = 15f;
        public const float OrbitMinimumDuration = TimedEffectMinimumDuration;
        public const float OrbitMaximumDuration = TimedEffectMaximumDuration;

        /// <summary>Fire cooldown multiplier while rapid fire is active.</summary>
        public const float RapidFireMultiplier = 0.45f;

        /// <summary>Points awarded by a score pickup.</summary>
        public const int ScorePickupValue = 250;

        public const int HighScoreCapacity = 8;

        /// <summary>Temporary telemetry overlay for balancing the adaptive director.</summary>
#if DEBUG
        public const bool ShowDifficultyDebug = true;
#else
        public const bool ShowDifficultyDebug = false;
#endif

        public const string SaveFileName = "savegame.json";
    }
}
