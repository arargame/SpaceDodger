namespace SpaceImpact.Persistence
{
    /// <summary>High-level save/load API used by screens.</summary>
    public interface ISaveGameService
    {
        SaveData Data { get; }
        void Load();
        void Save();
    }
}
