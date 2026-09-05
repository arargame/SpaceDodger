using System;
using System.Collections.Generic;
using SpaceDodger.Core;

namespace SpaceDodger.Persistence
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
        public bool AutoAttackEnabled = true;
        public bool AdsRemoved;
        public int ResumeLevel = 1;
        public int ResumeLives = 3;
        public int ResumeWeaponLevel = 1;
        public float ResumeShieldTime;
        public float ResumeRapidTime;
        public float ResumeScatterTime;
        public float ResumeSpiralTime;
        public int ResumeHomingCount;

        public bool RecordRun(int score, int level)
        {
            if (score <= BestRunScore)
                return false;

            BestRunScore = score;
            BestRunLevel = Math.Max(1, level);
            UpdateLiveScore(score, level, GameConfig.HighScoreCapacity);
            return true;
        }

        /// <summary>Keeps the current run visible in the high-score table even
        /// if Android interrupts the app before the game-over screen appears.</summary>
        private void UpdateLiveScore(int score, int level, int capacity)
        {
            HighScores.RemoveAll(e => e.Name == "RUN");
            AddScore("RUN", score, level, capacity);
        }

        public void RemoveLiveScore() => HighScores.RemoveAll(e => e.Name == "RUN");

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
