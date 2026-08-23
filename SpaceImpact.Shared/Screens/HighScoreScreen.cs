using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Core;
using SpaceImpact.Input;

namespace SpaceImpact.Screens
{
    /// <summary>Read-only list of saved high scores.</summary>
    public sealed class HighScoreScreen : Screen
    {
        private static readonly Color Header = new Color(255, 220, 60);
        private static readonly Color Row = new Color(150, 160, 190);
        private MenuList _menu;
        private Rectangle _worldButton;
        private Rectangle _backButton;
        private Rectangle TopBackButton => new Rectangle(Context.Screen.Width - 62, 4, 56, 18);

        public HighScoreScreen(GameContext context) : base(context) { }

        public override void Load()
        {
            if (Context.Platform.IsMobile)
            {
                _worldButton = new Rectangle(20, 142, 132, 20);
                _backButton = new Rectangle(168, 142, 132, 20);
            }
            else
                _menu = new MenuList(Context.Font, Context.Screen.Width / 2f, 142f)
                    .Add("BACK", () => Context.Screens.Pop());
        }

        public override void Update(float dt, in InputState input)
        {
            if (input.BackPressed)
                Context.Screens.Pop();
            else if (Context.Platform.IsMobile && input.Tap.HasValue)
            {
                var tap = input.Tap.Value;
                if (TopBackButton.Contains((int)tap.X, (int)tap.Y)) Context.Screens.Pop();
                else if (_worldButton.Contains((int)tap.X, (int)tap.Y)) Context.Games.ShowLeaderboards();
                else if (_backButton.Contains((int)tap.X, (int)tap.Y)) Context.Screens.Pop();
            }
            else
                _menu.Update(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float cx = Context.Screen.Width / 2f;
            Context.Font.DrawCentered(spriteBatch, "HIGH SCORES", cx, 16, Color.White, 2f);
            if (Context.Platform.IsMobile)
            {
                spriteBatch.Draw(Context.Textures.Pixel, TopBackButton, new Color(42, 48, 68));
                Context.Font.DrawCentered(spriteBatch, "BACK", TopBackButton.Center.X, TopBackButton.Y + 6, Header);
            }

            var scores = Context.Save.Data.HighScores;

            Context.Font.DrawCentered(
                spriteBatch,
                $"BEST RUN {Context.Save.Data.BestRunScore}  LV {Context.Save.Data.BestRunLevel}",
                cx, 34, Header);

            if (scores.Count == 0)
            {
                Context.Font.DrawCentered(spriteBatch, "FINISH A RUN TO ENTER THE TABLE", cx, 80, Row);
            }
            else
            {
                float y = 48f;
                Context.Font.Draw(spriteBatch, "#  NAME   SCORE   LV", new Vector2(70, y), Header);
                y += 12f;

                for (int i = 0; i < scores.Count; i++)
                {
                    var e = scores[i];
                    string line = $"{i + 1}  {e.Name,-5}  {e.Score,6}  {e.Level,2}";
                    Context.Font.Draw(spriteBatch, line, new Vector2(70, y), Row);
                    y += 11f;
                }
            }

            Context.Font.DrawCentered(
                spriteBatch, "SCORE + HIGHEST LEVEL", cx,
                Context.Screen.Height - 35, new Color(90, 96, 116));
            if (Context.Platform.IsMobile)
            {
                DrawButton(spriteBatch, _worldButton, "WORLD RANKING");
                DrawButton(spriteBatch, _backButton, "BACK");
            }
            else _menu.Draw(spriteBatch);
        }

        private void DrawButton(SpriteBatch batch, Rectangle rect, string label)
        {
            batch.Draw(Context.Textures.Pixel, rect, new Color(42, 48, 68));
            Context.Font.DrawCentered(batch, label, rect.Center.X, rect.Y + 6, Header);
        }
    }
}
