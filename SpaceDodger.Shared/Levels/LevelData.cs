using System.Collections.Generic;

namespace SpaceDodger.Levels
{
    /// <summary>How the enemies of a wave are laid out vertically at spawn time.</summary>
    public enum WaveFormation
    {
        /// <summary>Evenly spaced down the playfield.</summary>
        Line,
        /// <summary>Staggered diagonally (V-ish arrowhead).</summary>
        Diagonal,
        /// <summary>Random Y positions.</summary>
        Scatter,
        /// <summary>All at the same Y (tight column).</summary>
        Column,
    }

    /// <summary>One group of identical enemies released together.</summary>
    public sealed class WaveData
    {
        /// <summary>Seconds from level start until this wave begins spawning.</summary>
        public float StartTime;

        /// <summary>Enemy key from <see cref="Entities.EnemyCatalog"/>.</summary>
        public string Enemy = "drone";

        /// <summary>Movement key from <see cref="Movement.MovementRegistry"/>.</summary>
        public string Movement = "straight";

        public WaveFormation Formation = WaveFormation.Line;

        public int Count = 4;

        /// <summary>Seconds between consecutive spawns inside the wave.</summary>
        public float Interval = 0.6f;

        public float HealthMultiplier = 1f;
        public float SpeedMultiplier = 1f;
    }

    /// <summary>A complete level definition loaded from JSON.</summary>
    public sealed class LevelData
    {
        public int Number = 1;
        public string Name = "";
        public List<WaveData> Waves = new List<WaveData>();

        /// <summary>Total enemies across all waves — used for the clear condition.</summary>
        public int TotalEnemies
        {
            get
            {
                int total = 0;
                foreach (var wave in Waves)
                    total += wave.Count;
                return total;
            }
        }
    }
}
