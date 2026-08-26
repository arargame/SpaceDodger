using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Core;
using SpaceDodger.Input;

namespace SpaceDodger.Screens
{
    /// <summary>
    /// Overlay pause menu. Being an overlay, the frozen gameplay screen keeps
    /// drawing underneath it.
    /// </summary>
    public sealed class PauseScreen : Screen
    {
        private MenuList _menu;

        public PauseScreen(GameContext context) : base(context) { }

        public override bool IsOverlay => true;

        public override void Load()
        {
            _menu = new MenuList(Context.Font, Context.Screen.Width / 2f, 92f)
                .Add("RESUME", Resume)
                .Add("MAIN MENU", QuitToMenu);
        }

        private void Resume() => Context.Screens.Pop();

        private void QuitToMenu() => Context.Screens.Reset(new MenuScreen(Context));

        public override void Update(float dt, in InputState input)
        {
            // The pause key also un-pauses.
            if (input.BackPressed)
            {
                Resume();
                return;
            }

            _menu.Update(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Dim the gameplay behind the overlay.
            spriteBatch.Draw(
                Context.Textures.Pixel, Context.Screen.Bounds, new Color(0, 0, 0, 170));

            Context.Font.DrawCentered(
                spriteBatch, "PAUSED", Context.Screen.Width / 2f, 60, Color.White, 2f);

            _menu.Draw(spriteBatch);
        }
    }
}
