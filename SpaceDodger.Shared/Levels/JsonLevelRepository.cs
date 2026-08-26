using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Xna.Framework;
using SpaceDodger.Core;

namespace SpaceDodger.Levels
{
    /// <summary>
    /// Loads Content/levels/levelNN.json through TitleContainer (works from the
    /// desktop output folder and from Android APK assets alike).
    /// Parsing is manual (JsonDocument) so it stays trimming/AOT safe.
    /// Levels are cached after first load.
    /// </summary>
    public sealed class JsonLevelRepository : ILevelRepository
    {
        private readonly Dictionary<int, LevelData> _cache = new Dictionary<int, LevelData>();

        public int Count => GameConfig.LevelCount;

        public LevelData Load(int levelNumber)
        {
            if (_cache.TryGetValue(levelNumber, out var cached))
                return cached;

            var level = Parse(levelNumber);
            _cache[levelNumber] = level;
            return level;
        }

        private static LevelData Parse(int levelNumber)
        {
            var path = $"Content/levels/level{levelNumber:00}.json";

            try
            {
                using var stream = TitleContainer.OpenStream(path);
                using var doc = JsonDocument.Parse(stream);
                var root = doc.RootElement;

                var level = new LevelData
                {
                    Number = root.TryGetProperty("number", out var n) ? n.GetInt32() : levelNumber,
                    Name = root.TryGetProperty("name", out var nm) ? nm.GetString() : $"LEVEL {levelNumber}",
                };

                if (root.TryGetProperty("waves", out var waves))
                {
                    foreach (var w in waves.EnumerateArray())
                        level.Waves.Add(ParseWave(w));
                }

                return level;
            }
            catch (Exception)
            {
                // A missing or malformed level must not crash the game;
                // fall back to a simple generated wave set.
                return Fallback(levelNumber);
            }
        }

        private static WaveData ParseWave(JsonElement w)
        {
            var wave = new WaveData();

            if (w.TryGetProperty("startTime", out var st)) wave.StartTime = st.GetSingle();
            if (w.TryGetProperty("enemy", out var e)) wave.Enemy = e.GetString();
            if (w.TryGetProperty("movement", out var m)) wave.Movement = m.GetString();
            if (w.TryGetProperty("count", out var c)) wave.Count = c.GetInt32();
            if (w.TryGetProperty("interval", out var i)) wave.Interval = i.GetSingle();
            if (w.TryGetProperty("healthMultiplier", out var hm)) wave.HealthMultiplier = hm.GetSingle();
            if (w.TryGetProperty("speedMultiplier", out var sm)) wave.SpeedMultiplier = sm.GetSingle();

            if (w.TryGetProperty("formation", out var f) &&
                Enum.TryParse<WaveFormation>(f.GetString(), true, out var formation))
                wave.Formation = formation;

            return wave;
        }

        private static LevelData Fallback(int levelNumber)
        {
            var level = new LevelData { Number = levelNumber, Name = $"LEVEL {levelNumber}" };
            level.Waves.Add(new WaveData
            {
                StartTime = 1f,
                Enemy = "drone",
                Movement = "straight",
                Formation = WaveFormation.Line,
                Count = 5 + levelNumber,
                Interval = 0.6f,
            });
            return level;
        }
    }
}
