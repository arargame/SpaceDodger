using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Core;
using SpaceImpact.Graphics;
using SpaceImpact.Input;
using SpaceImpact.Levels;

namespace SpaceImpact.Screens
{
    /// <summary>Title screen: the game's entry state.</summary>
    public sealed class MenuScreen : Screen
    {
        private readonly ILevelRepository _levels = new JsonLevelRepository();

        private MenuList _menu;
        private Starfield _stars;
        private float _time;

        public MenuScreen(GameContext context) : base(context) { }

        public override void Load()
        {
            var bounds = Context.Screen.Bounds;
            _stars = new Starfield(Context.Textures.Pixel, bounds.Width, bounds.Height);

            bool hasProgress = Context.Save.Data.MaxUnlockedLevel > 1;

            _menu = new MenuList(Context.Font, bounds.Width / 2f, 88f)
                .Add("NEW GAME", StartNewGame)
                .Add("SELECT LEVEL", OpenLevelSelect, hasProgress)
                .Add("HIGH SCORES", OpenHighScores)
                .Add("QUIT", Quit, !Context.Platform.IsMobile);
        }

        private void StartNewGame() =>
            Context.Screens.Replace(new GameplayScreen(Context, _levels, 1));

        private void OpenLevelSelect() =>
            Context.Screens.Push(new LevelSelectScreen(Context, _levels));

        private void OpenHighScores() =>
            Context.Screens.Push(new HighScoreScreen(Context));

        private void Quit() => Context.Screens.Pop();

        public override void Update(float dt, in InputState input)
        {
            _time += dt;
            _stars.Update(dt);
            _menu.Update(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _stars.Draw(spriteBatch);

            float cx = Context.Screen.Width / 2f;

            // Title with a subtle bob.
            float bob = (float)System.Math.Sin(_time * 1.6f) * 2f;
            Context.Font.DrawCentered(spriteBatch, "SPACE", cx, 28 + bob, new Color(255, 220, 60), 3f);
            Context.Font.DrawCentered(spriteBatch, "IMPACT", cx, 52 + bob, Color.White, 3f);

            _menu.Draw(spriteBatch);

            string hint = Context.Platform.IsMobile
                ? "TAP TO SELECT"
                : "ARROWS + ENTER";
            Context.Font.DrawCentered(
                spriteBatch, hint, cx, Context.Screen.Height - 14, new Color(90, 96, 116));
        }
    }
}
