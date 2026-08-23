using System;
using System.IO;
using System.Text.Json;

namespace SpaceImpact.Persistence
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
