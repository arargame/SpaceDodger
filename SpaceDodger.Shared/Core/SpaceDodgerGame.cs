using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Graphics;
using SpaceDodger.Input;
using SpaceDodger.Persistence;
using SpaceDodger.Screens;
using SpaceDodger.Audio;

namespace SpaceDodger.Core
{
    /// <summary>
    /// Shared game class used by both Desktop and Android heads.
    /// Acts as the composition root: builds all services, wires them into a
    /// <see cref="GameContext"/> and hands control to the ScreenManager.
    /// </summary>
    public class SpaceDodgerGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly IPlatformServices _platform;

        private SpriteBatch _spriteBatch;
        private GameContext _context;
        private VirtualScreen _screen;
        private IInputProvider _input;
        private ScreenManager _screens;

        public GameContext Context => _context;

        public SpaceDodgerGame(IPlatformServices platform)
        {
            _platform = platform;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Exiting += (sender, args) => _platform?.ExitGame();

            if (platform.IsMobile)
            {
                _graphics.IsFullScreen = true;
                _graphics.SupportedOrientations =
                    DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = GameConfig.VirtualWidth * GameConfig.WindowScale;
                _graphics.PreferredBackBufferHeight = GameConfig.VirtualHeight * GameConfig.WindowScale;
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _screen = new VirtualScreen(
                GraphicsDevice, GameConfig.VirtualWidth, GameConfig.VirtualHeight,
                fillDisplay: _platform.IsMobile);
            var textures = new TextureStore(GraphicsDevice);
            var font = new PixelFont(textures.Get("sprites/font"));
            _input = _platform.CreateInputProvider(_screen);

            var storage = new FileStorageProvider(_platform.SaveDirectory, GameConfig.SaveFileName);
            var save = new JsonSaveGameService(storage);
            save.Load();

            var events = new EventBus();
            _screens = new ScreenManager();

            _context = new GameContext(
                this, GraphicsDevice, _platform, _screen, textures, font,
                _input, save, events, _screens, _platform.CreateGameServices(), new AudioService(save.Data));

            _screens.Push(new MenuScreen(_context));
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _input.Update();
            _screens.Update(dt, _input.State);

            if (_screens.IsEmpty)
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // 1) Render the whole game into the small virtual target.
            _screen.BeginCapture();
            GraphicsDevice.Clear(new Color(10, 12, 24));

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _screens.Draw(_spriteBatch);
            _spriteBatch.End();

            // 2) Scale it up to the real backbuffer with crisp pixels.
            _screen.EndCapture(_spriteBatch);

            // 3) Draw high-res UI overlays directly on the backbuffer.
            _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
            _screens.DrawHighRes(_spriteBatch, _screen);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _context?.Textures.Dispose();
            _context?.Audio.Dispose();
            base.UnloadContent();
        }
    }
}
