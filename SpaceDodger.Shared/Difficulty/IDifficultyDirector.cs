namespace SpaceDodger.Difficulty
{
    /// <summary>Strategy seam between player telemetry and encounter pacing.</summary>
    public interface IDifficultyDirector
    {
        DifficultyModifiers Current { get; }
        string StateLabel { get; }
        string CycleLabel { get; }
        float Intensity { get; }
        float HealthSupplyBias { get; }

        void BeginLevel(int levelNumber, int startingLives, bool isBossLevel, bool preserveMomentum);
        void Update(float deltaTime, int lives, int combo, int activeEnemies, int activeEnemyBullets);
        void NotifyEnemyDestroyed();
        void NotifyPlayerDamaged();
    }
}
