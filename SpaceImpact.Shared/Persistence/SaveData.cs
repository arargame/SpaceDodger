using System;
using System.Collections.Generic;

namespace SpaceImpact.Persistence
{
    public sealed class ScoreEntry
    {
        public string Name = "ACE";
        public int Score;
        public int Level;
        public string Date = "";
    }

    /// <summary>All persisted progress: high scores + unlocked levels.</summary>
    public sealed class SaveData
    {
        public List<ScoreEntry> HighScores = new List<ScoreEntry>();
        public int MaxUnlockedLevel = 1; // 1-based; level 1 always available
        public int BestRunScore;
        public int BestRunLevel = 1;
        public bool MusicEnabled = true;
        public bool SoundEnabled = true;

        public bool RecordRun(int score, int level)
        {
            if (score <= BestRunScore)
                return false;

            BestRunScore = score;
            BestRunLevel = Math.Max(1, level);
            return true;
        }

        /// <summary>Insert a score keeping the list sorted/trimmed. Returns rank (0-based) or -1.</summary>
        public int AddScore(string name, int score, int level, int capacity)
        {
            var entry = new ScoreEntry
            {
                Name = name,
                Score = score,
                Level = level,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
            };

            int index = HighScores.FindIndex(e => score > e.Score);
            if (index < 0)
                index = HighScores.Count;

            if (index >= capacity)
                return -1;

            HighScores.Insert(index, entry);
            if (HighScores.Count > capacity)
                HighScores.RemoveAt(HighScores.Count - 1);
            return index;
        }
    }
}
