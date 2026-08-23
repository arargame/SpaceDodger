using SpaceImpact.Graphics;
using SpaceImpact.Input;

namespace SpaceImpact.Core
{
    /// <summary>
    /// Abstraction over platform differences (Dependency Inversion Principle).
    /// The shared game never references a concrete platform; Desktop and Android
    /// each supply their own implementation at the composition root.
    /// </summary>
    public interface IPlatformServices
    {
        bool IsMobile { get; }

        /// <summary>Writable directory for save files on this platform.</summary>
        string SaveDirectory { get; }

        IInputProvider CreateInputProvider(VirtualScreen screen);

        IGameServices CreateGameServices() => NullGameServices.Instance;
    }
}
