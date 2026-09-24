namespace SpaceDodger.Difficulty
{
    /// <summary>
    /// The director's output. Values are intentionally expressed as multipliers
    /// so authored level data remains the source of the base encounter design.
    /// </summary>
    public readonly struct DifficultyModifiers
    {
        public static readonly DifficultyModifiers Neutral = new DifficultyModifiers(
            waveInterval: 1f, enemySpeed: 1f, enemyHealth: 1f,
            enemyFireInterval: 1f, powerUpDrop: 1f, proceduralRate: 1f,
            proceduralCap: 8);

        public readonly float WaveInterval;
        public readonly float EnemySpeed;
        public readonly float EnemyHealth;
        public readonly float EnemyFireInterval;
        public readonly float PowerUpDrop;
        public readonly float ProceduralRate;
        public readonly int ProceduralCap;

        public DifficultyModifiers(float waveInterval, float enemySpeed, float enemyHealth,
            float enemyFireInterval, float powerUpDrop, float proceduralRate, int proceduralCap)
        {
            WaveInterval = waveInterval;
            EnemySpeed = enemySpeed;
            EnemyHealth = enemyHealth;
            EnemyFireInterval = enemyFireInterval;
            PowerUpDrop = powerUpDrop;
            ProceduralRate = proceduralRate;
            ProceduralCap = proceduralCap;
        }

        public static DifficultyModifiers Lerp(in DifficultyModifiers from, in DifficultyModifiers to, float amount)
        {
            return new DifficultyModifiers(
                Microsoft.Xna.Framework.MathHelper.Lerp(from.WaveInterval, to.WaveInterval, amount),
                Microsoft.Xna.Framework.MathHelper.Lerp(from.EnemySpeed, to.EnemySpeed, amount),
                Microsoft.Xna.Framework.MathHelper.Lerp(from.EnemyHealth, to.EnemyHealth, amount),
                Microsoft.Xna.Framework.MathHelper.Lerp(from.EnemyFireInterval, to.EnemyFireInterval, amount),
                Microsoft.Xna.Framework.MathHelper.Lerp(from.PowerUpDrop, to.PowerUpDrop, amount),
                Microsoft.Xna.Framework.MathHelper.Lerp(from.ProceduralRate, to.ProceduralRate, amount),
                (int)System.Math.Round(Microsoft.Xna.Framework.MathHelper.Lerp(from.ProceduralCap, to.ProceduralCap, amount)));
        }
    }
}
