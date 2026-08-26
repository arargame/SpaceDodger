namespace SpaceDodger.Levels
{
    /// <summary>Source of level definitions (JSON files today, anything tomorrow).</summary>
    public interface ILevelRepository
    {
        int Count { get; }

        /// <summary>Load a level by its 1-based number.</summary>
        LevelData Load(int levelNumber);
    }
}
