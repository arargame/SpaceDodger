using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Graphics;
using SpaceImpact.Input;
using SpaceImpact.Persistence;
using SpaceImpact.Screens;

namespace SpaceImpact.Core
{
    /// <summary>
    /// Aggregates the long-lived services every screen needs.
    /// Built once at the composition root (constructor injection, no service
    /// locator lookups scattered through gameplay code).
    /// </summary>
    public sealed class GameContext
    {
        public Game Game { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public IPlatformServices Platform { get; }
        public VirtualScreen Screen { get; }
        public TextureStore Textures { get; }
        public PixelFont Font { get; }
        public IInputProvider Input { get; }
        public ISaveGameService Save { get; }
        public EventBus Events { get; }
        public ScreenManager Screens { get; }

        public GameContext(
            Game game,
            GraphicsDevice graphicsDevice,
            IPlatformServices platform,
            VirtualScreen screen,
            TextureStore textures,
            PixelFont font,
            IInputProvider input,
            ISaveGameService save,
            EventBus events,
            ScreenManager screens)
        {
            Game = game;
            GraphicsDevice = graphicsDevice;
            Platform = platform;
            Screen = screen;
            Textures = textures;
            Font = font;
            Input = input;
            Save = save;
            Events = events;
            Screens = screens;
        }
    }
}
