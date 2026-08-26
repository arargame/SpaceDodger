namespace SpaceDodger.Core
{
    /// <summary>Google Play Games seam. Android can replace the no-op service
    /// once Play Console leaderboard IDs and credentials are available.</summary>
    public interface IGameServices
    {
        void SubmitHighScore(long score);
        void SubmitHighestLevel(int level);
        void ShowLeaderboards();
    }

    public sealed class NullGameServices : IGameServices
    {
        public static readonly NullGameServices Instance = new NullGameServices();
        private NullGameServices() { }
        public void SubmitHighScore(long score) { }
        public void SubmitHighestLevel(int level) { }
        public void ShowLeaderboards() { }
    }
}
