namespace SpaceImpact.Input
{
    /// <summary>
    /// Strategy interface for producing an <see cref="InputState"/> each frame.
    /// Desktop uses keyboard+mouse, Android uses touch — the game is oblivious.
    /// </summary>
    public interface IInputProvider
    {
        void Update();
        InputState State { get; }
    }
}
