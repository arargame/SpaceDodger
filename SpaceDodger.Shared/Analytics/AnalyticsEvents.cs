#nullable enable

namespace SpaceDodger.Analytics
{
    /// <summary>
    /// GA4-compliant telemetry event names (max 40 characters, letters, numbers, underscores).
    /// </summary>
    public static class AnalyticsEvents
    {
        /// <summary>Screen view navigation event (Menu, Gameplay, Pause, GameOver, SecondChance, etc.).</summary>
        public const string ScreenView = "screen_view";

        /// <summary>Level started.</summary>
        public const string LevelStarted = "level_started";

        /// <summary>Level cleared successfully.</summary>
        public const string LevelCompleted = "level_completed";

        /// <summary>Level failed (player died).</summary>
        public const string LevelFailed = "level_failed";

        /// <summary>Key Google Ads conversion milestone: player cleared Level 5 / reached Level 6.</summary>
        public const string Level5Reached = "level_5_reached";

        /// <summary>Boss fight encounter outcome (victory or defeat).</summary>
        public const string BossFightResult = "boss_fight_result";

        /// <summary>Supply crate / power-up collected.</summary>
        public const string SupplyCollected = "supply_collected";

        /// <summary>Specific weapon upgrade or special fire cartridge equipped.</summary>
        public const string WeaponCollected = "weapon_collected";

        /// <summary>Rewarded ad revive offer shown (Second Chance).</summary>
        public const string SecondChanceOffered = "second_chance_offered";

        /// <summary>Player accepted or declined rewarded ad revive.</summary>
        public const string SecondChanceUsed = "second_chance_used";
    }

    /// <summary>
    /// GA4-compliant parameter keys (max 40 characters).
    /// </summary>
    public static class AnalyticsParams
    {
        // Navigation & Screens
        public const string ScreenName = "screen_name";
        public const string ScreenClass = "screen_class";

        // Level & Run Context
        public const string LevelNumber = "level_number";
        public const string Score = "score";
        public const string DurationSeconds = "duration_seconds";
        public const string LivesRemaining = "lives_remaining";
        public const string ComboMax = "combo_max";
        public const string WeaponLevel = "weapon_level";
        public const string KilledBy = "killed_by";
        public const string IsBossLevel = "is_boss_level";

        // Boss Encounters
        public const string BossLevel = "boss_level";
        public const string BossName = "boss_name";
        public const string IsVictory = "is_victory";
        public const string DamageTaken = "damage_taken";

        // Supplies & Weapons
        public const string SupplyType = "supply_type";
        public const string WeaponType = "weapon_type";
        public const string IsBoostedSkin = "is_boosted_skin";

        // Rewarded Revive
        public const string ReviveAccepted = "revive_accepted";
    }

    /// <summary>
    /// GA4 User Properties for player segmentation and cohort analysis (name max 24 chars, value max 36 chars).
    /// </summary>
    public static class AnalyticsUserProps
    {
        public const string MaxLevelReached = "max_level_reached";
        public const string HasPassedLevel5 = "has_passed_level_5";
        public const string TotalPlaytimeMinutes = "playtime_minutes";
        public const string TotalBossesDefeated = "bosses_defeated";
        public const string TotalRunsStarted = "total_runs";
        public const string TotalDeaths = "total_deaths";
    }

    /// <summary>
    /// Standardized parameter values to prevent casing / typo fragmentation in BigQuery / GA4.
    /// </summary>
    public static class AnalyticsValues
    {
        public const string KilledByEnemyBullet = "enemy_bullet";
        public const string KilledByBossBullet = "boss_bullet";
        public const string KilledByEnemyCollision = "enemy_collision";
        public const string KilledByBossCollision = "boss_collision";
        public const string KilledByUnknown = "unknown";
    }
}
