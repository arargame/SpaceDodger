#nullable enable
using System;
using System.Collections.Generic;
using SpaceDodger.Entities;

namespace SpaceDodger.Analytics
{
    /// <summary>
    /// High-level facade for game telemetry (Facade Pattern).
    /// Provides typed, validated methods for game events while ensuring zero performance impact on 60 FPS gameplay.
    /// </summary>
    public static class AnalyticsManager
    {
        private static IAnalyticsService _service = NullAnalyticsService.Instance;
        private static readonly object _syncLock = new object();

        // Parameter dictionary pool to avoid GC allocations during frequent events (e.g. supply pickups).
        private static readonly Dictionary<string, object> _paramPool = new Dictionary<string, object>(10);

        public static IAnalyticsService Service
        {
            get => _service;
            set => _service = value ?? NullAnalyticsService.Instance;
        }

        public static bool IsEnabled => _service.IsEnabled;

        public static void SetUserId(string userId)
        {
            try { _service.SetUserId(userId); }
            catch { /* telemetry must never crash the game */ }
        }

        public static void SetUserProperty(string name, string value)
        {
            try { _service.SetUserProperty(name, value); }
            catch { }
        }

        public static void LogScreenView(string screenName, string? screenClass = null)
        {
            try
            {
                lock (_syncLock)
                {
                    _paramPool.Clear();
                    _paramPool[AnalyticsParams.ScreenName] = screenName;
                    _paramPool[AnalyticsParams.ScreenClass] = screenClass ?? screenName;
                    _service.LogEvent(AnalyticsEvents.ScreenView, _paramPool);
                }
            }
            catch { }
        }

        public static void LogLevelStarted(int levelNumber, int lives, int weaponLevel)
        {
            try
            {
                lock (_syncLock)
                {
                    _paramPool.Clear();
                    _paramPool[AnalyticsParams.LevelNumber] = levelNumber;
                    _paramPool[AnalyticsParams.LivesRemaining] = lives;
                    _paramPool[AnalyticsParams.WeaponLevel] = weaponLevel;
                    _paramPool[AnalyticsParams.IsBossLevel] = (levelNumber % 10 == 0);
                    _service.LogEvent(AnalyticsEvents.LevelStarted, _paramPool);
                }
            }
            catch { }
        }

        public static void LogLevelCompleted(int levelNumber, int score, float durationSeconds, int remainingLives, int comboMax)
        {
            try
            {
                lock (_syncLock)
                {
                    _paramPool.Clear();
                    _paramPool[AnalyticsParams.LevelNumber] = levelNumber;
                    _paramPool[AnalyticsParams.Score] = score;
                    _paramPool[AnalyticsParams.DurationSeconds] = (float)Math.Round(durationSeconds, 1);
                    _paramPool[AnalyticsParams.LivesRemaining] = remainingLives;
                    _paramPool[AnalyticsParams.ComboMax] = comboMax;
                    _service.LogEvent(AnalyticsEvents.LevelCompleted, _paramPool);

                    // Milestone Conversion Event for Ad campaigns (Level 5 cleared / Level 6 reached)
                    if (levelNumber == 5)
                    {
                        _paramPool.Clear();
                        _paramPool[AnalyticsParams.Score] = score;
                        _paramPool[AnalyticsParams.DurationSeconds] = (float)Math.Round(durationSeconds, 1);
                        _service.LogEvent(AnalyticsEvents.Level5Reached, _paramPool);
                        _service.SetUserProperty(AnalyticsUserProps.HasPassedLevel5, "true");
                    }
                }
            }
            catch { }
        }

        public static void LogLevelFailed(int levelNumber, int score, float durationSeconds, string killedBy, int weaponLevel)
        {
            try
            {
                lock (_syncLock)
                {
                    _paramPool.Clear();
                    _paramPool[AnalyticsParams.LevelNumber] = levelNumber;
                    _paramPool[AnalyticsParams.Score] = score;
                    _paramPool[AnalyticsParams.DurationSeconds] = (float)Math.Round(durationSeconds, 1);
                    _paramPool[AnalyticsParams.KilledBy] = string.IsNullOrEmpty(killedBy) ? AnalyticsValues.KilledByUnknown : killedBy;
                    _paramPool[AnalyticsParams.WeaponLevel] = weaponLevel;
                    _service.LogEvent(AnalyticsEvents.LevelFailed, _paramPool);
                }
            }
            catch { }
        }

        public static void LogSupplyCollected(PowerUpType type, int levelNumber, int lives, int score, bool isBoostedSkin)
        {
            try
            {
                lock (_syncLock)
                {
                    _paramPool.Clear();
                    _paramPool[AnalyticsParams.SupplyType] = type.ToString();
                    _paramPool[AnalyticsParams.LevelNumber] = levelNumber;
                    _paramPool[AnalyticsParams.LivesRemaining] = lives;
                    _paramPool[AnalyticsParams.Score] = score;
                    _paramPool[AnalyticsParams.IsBoostedSkin] = isBoostedSkin;
                    _service.LogEvent(AnalyticsEvents.SupplyCollected, _paramPool);
                }
            }
            catch { }
        }

        public static void LogWeaponCollected(string weaponType, int levelNumber)
        {
            try
            {
                lock (_syncLock)
                {
                    _paramPool.Clear();
                    _paramPool[AnalyticsParams.WeaponType] = weaponType;
                    _paramPool[AnalyticsParams.LevelNumber] = levelNumber;
                    _service.LogEvent(AnalyticsEvents.WeaponCollected, _paramPool);
                }
            }
            catch { }
        }

        public static void LogBossFightResult(int bossLevel, string bossName, bool isVictory, float durationSeconds, int damageTaken)
        {
            try
            {
                lock (_syncLock)
                {
                    _paramPool.Clear();
                    _paramPool[AnalyticsParams.BossLevel] = bossLevel;
                    _paramPool[AnalyticsParams.BossName] = bossName;
                    _paramPool[AnalyticsParams.IsVictory] = isVictory;
                    _paramPool[AnalyticsParams.DurationSeconds] = (float)Math.Round(durationSeconds, 1);
                    _paramPool[AnalyticsParams.DamageTaken] = damageTaken;
                    _service.LogEvent(AnalyticsEvents.BossFightResult, _paramPool);
                }
            }
            catch { }
        }

        public static void LogSecondChanceOffered(int levelNumber, int score)
        {
            try
            {
                lock (_syncLock)
                {
                    _paramPool.Clear();
                    _paramPool[AnalyticsParams.LevelNumber] = levelNumber;
                    _paramPool[AnalyticsParams.Score] = score;
                    _service.LogEvent(AnalyticsEvents.SecondChanceOffered, _paramPool);
                }
            }
            catch { }
        }

        public static void LogSecondChanceUsed(int levelNumber, bool accepted)
        {
            try
            {
                lock (_syncLock)
                {
                    _paramPool.Clear();
                    _paramPool[AnalyticsParams.LevelNumber] = levelNumber;
                    _paramPool[AnalyticsParams.ReviveAccepted] = accepted;
                    _service.LogEvent(AnalyticsEvents.SecondChanceUsed, _paramPool);
                }
            }
            catch { }
        }
    }
}
