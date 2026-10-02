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
        public const int DamageBonusEveryLevels = 10;

        public const int PlayerLives = 3;
        public const float PlayerSpeed = 95f;          // px/s in virtual space
        public const float PlayerFireCooldown = 0.22f; // seconds
        public const float PlayerInvulnTime = 2.0f;    // seconds after a hit
        public const int MaxWeaponLevel = 5;
        public const float WeaponTier2MinimumDuration = 15f; // was 12f (+25%)
        public const float WeaponTier2MaximumDuration = 24f; // was 20f (+20%)
        public const float WeaponTier3MinimumDuration = 12f; // was 10f (+20%)
        public const float WeaponTier3MaximumDuration = 22f; // was 18f (+22%)
        public const float WeaponTier4MinimumDuration = 10f; // was 8f  (+25%)
        public const float WeaponTier4MaximumDuration = 18f; // was 15f (+20%)
        public const float WeaponTier5MinimumDuration = 9f;  // was 7f  (+28%)
        public const float WeaponTier5MaximumDuration = 24f; // was 20f (+20%)

        public const float PlayerBulletSpeed = 200f;
        public const float EnemyBulletSpeed = 90f;
        public const float EnemyHeavyBulletSpeed = 68f;
        public const float ScatterBulletSpeed = 160f;
        public const float HomingMissileSpeed = 140f;
        public const float SpiralBulletSpeed = 190f;
        public const float RicochetBulletSpeed = 172f;

        /// <summary>Base per-enemy supply chance before the DDA reward multiplier (5%).</summary>
        public const float PowerUpDropChance = 0.05f;
        public const float PowerUpSpeed = 30f;
        public const int MaximumActivePowerUps = 8;
        public const float PowerUpDropCooldown = 3.5f;
        public const float SupplyDriftMinimumInterval = 11f;
        public const float SupplyDriftMaximumInterval = 17f;

        /// <summary>Supply magnet capture tuning (derived from Blocked project, adapted to 320x180 playfield).</summary>
        public const float SupplyMagnetRadius = 60f;     // px capture radius around the ship
        public const float SupplyMagnetAccel = 240f;      // px/s^2 attraction pull force
        public const float SupplyMagnetMaxSpeed = 120f;   // px/s maximum pickup approach speed
        public const float SupplyMagnetDamping = 3.5f;    // rate to relax back to natural drift

        /// <summary>Seconds a collected shield lasts.</summary>
        public const float ShieldDuration = 10f; // was 8f (+25%)

        /// <summary>Seconds of boosted fire rate from a rapid-fire pickup.</summary>
        public const float RapidFireDuration = 12f; // was 10f (+20%)
        public const int SpecialMinimumCharges = 12; // was 10 (+20%)
        public const int SpecialMaximumCharges = 24; // was 20 (+20%)
        public const int HomingMinimumCharges = 10;  // was 8  (+25%)
        public const int HomingMaximumCharges = 24;  // was 20 (+20%)
        public const int WaveMinimumCharges = 10;    // was 8  (+25%)
        public const int WaveMaximumCharges = 24;    // was 20 (+20%)
        public const int SweepLaserMinimumCharges = 4; // was 3 (+33%)
        public const int SweepLaserMaximumCharges = 12; // was 10 (+20%)
        public const int ChainLightningMinimumCharges = 15; // was 10 (+50%)
        public const int ChainLightningMaximumCharges = 25; // was 20 (+25%)
        public const int OrbitMinimumShots = 3;      // was 2  (+50%)
        public const int OrbitMaximumShots = 4;      // was 3  (+33%)
        public const float TimedEffectMinimumDuration = 6f;  // was 5f  (+20%)
        public const float TimedEffectMaximumDuration = 18f; // was 15f (+20%)
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
