using System;
using System.IO;
using System.Text.Json;

namespace SpaceDodger.Persistence
{
    /// <summary>
    /// JSON persistence via manual DOM reading + Utf8JsonWriter.
    /// No reflection-based serialization, so it is trimming/AOT safe on Android.
    /// Corrupt or missing files silently fall back to fresh data.
    /// </summary>
    public sealed class JsonSaveGameService : ISaveGameService
    {
        private readonly IStorageProvider _storage;

        public SaveData Data { get; private set; } = new SaveData();

        public JsonSaveGameService(IStorageProvider storage) => _storage = storage;

        public void Load()
        {
            if (!_storage.Exists)
            {
                Data = new SaveData();
                return;
            }

            try
            {
                using var stream = _storage.OpenRead();
                using var doc = JsonDocument.Parse(stream);
                var root = doc.RootElement;

                var data = new SaveData();

                if (root.TryGetProperty("maxUnlockedLevel", out var mul))
                    data.MaxUnlockedLevel = Math.Max(1, mul.GetInt32());
                if (root.TryGetProperty("bestRunScore", out var bestScore))
                    data.BestRunScore = Math.Max(0, bestScore.GetInt32());
                if (root.TryGetProperty("bestRunLevel", out var bestLevel))
                    data.BestRunLevel = Math.Max(1, bestLevel.GetInt32());
                if (root.TryGetProperty("musicEnabled", out var musicEnabled))
                    data.MusicEnabled = musicEnabled.GetBoolean();
                if (root.TryGetProperty("soundEnabled", out var soundEnabled))
                    data.SoundEnabled = soundEnabled.GetBoolean();
                if (root.TryGetProperty("autoAttackEnabled", out var autoAttackEnabled))
                    data.AutoAttackEnabled = autoAttackEnabled.GetBoolean();
                if (root.TryGetProperty("resumeLevel", out var resumeLevel)) data.ResumeLevel = Math.Max(1, resumeLevel.GetInt32());
                if (root.TryGetProperty("resumeLives", out var resumeLives)) data.ResumeLives = Math.Max(1, resumeLives.GetInt32());
                if (root.TryGetProperty("resumeWeaponLevel", out var resumeWeapon)) data.ResumeWeaponLevel = Math.Max(1, resumeWeapon.GetInt32());
                if (root.TryGetProperty("resumeShieldTime", out var resumeShield)) data.ResumeShieldTime = resumeShield.GetSingle();
                if (root.TryGetProperty("resumeRapidTime", out var resumeRapid)) data.ResumeRapidTime = resumeRapid.GetSingle();
                if (root.TryGetProperty("resumeScatterTime", out var resumeScatter)) data.ResumeScatterTime = resumeScatter.GetSingle();
                if (root.TryGetProperty("resumeHomingCount", out var resumeHoming)) data.ResumeHomingCount = resumeHoming.GetInt32();

                if (root.TryGetProperty("highScores", out var scores))
                {
                    foreach (var e in scores.EnumerateArray())
                    {
                        data.HighScores.Add(new ScoreEntry
                        {
                            Name = e.TryGetProperty("name", out var n) ? n.GetString() : "ACE",
                            Score = e.TryGetProperty("score", out var s) ? s.GetInt32() : 0,
                            Level = e.TryGetProperty("level", out var l) ? l.GetInt32() : 1,
                            Date = e.TryGetProperty("date", out var d) ? d.GetString() : "",
                        });
                    }
                }

                Data = data;
            }
            catch (Exception)
            {
                Data = new SaveData();
            }
        }

        public void Save()
        {
            try
            {
                using var stream = _storage.OpenWrite();
                using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

                writer.WriteStartObject();
                writer.WriteNumber("maxUnlockedLevel", Data.MaxUnlockedLevel);
                writer.WriteNumber("bestRunScore", Data.BestRunScore);
                writer.WriteNumber("bestRunLevel", Data.BestRunLevel);
                writer.WriteBoolean("musicEnabled", Data.MusicEnabled);
                writer.WriteBoolean("soundEnabled", Data.SoundEnabled);
                writer.WriteBoolean("autoAttackEnabled", Data.AutoAttackEnabled);
                writer.WriteNumber("resumeLevel", Data.ResumeLevel);
                writer.WriteNumber("resumeLives", Data.ResumeLives);
                writer.WriteNumber("resumeWeaponLevel", Data.ResumeWeaponLevel);
                writer.WriteNumber("resumeShieldTime", Data.ResumeShieldTime);
                writer.WriteNumber("resumeRapidTime", Data.ResumeRapidTime);
                writer.WriteNumber("resumeScatterTime", Data.ResumeScatterTime);
                writer.WriteNumber("resumeHomingCount", Data.ResumeHomingCount);
                writer.WriteStartArray("highScores");
                foreach (var e in Data.HighScores)
                {
                    writer.WriteStartObject();
                    writer.WriteString("name", e.Name);
                    writer.WriteNumber("score", e.Score);
                    writer.WriteNumber("level", e.Level);
                    writer.WriteString("date", e.Date);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            catch (Exception)
            {
                // Saving must never crash the game.
            }
        }
    }
}
