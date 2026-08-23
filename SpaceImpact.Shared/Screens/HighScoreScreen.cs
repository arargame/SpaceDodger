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

        public HighScoreScreen(GameContext context) : base(context) { }

        public override void Update(float dt, in InputState input)
        {
            if (input.BackPressed || input.ConfirmPressed || input.Tap.HasValue)
                Context.Screens.Pop();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float cx = Context.Screen.Width / 2f;
            Context.Font.DrawCentered(spriteBatch, "HIGH SCORES", cx, 16, Color.White, 2f);

            var scores = Context.Save.Data.HighScores;

            if (scores.Count == 0)
            {
                Context.Font.DrawCentered(spriteBatch, "NO SCORES YET", cx, 80, Row);
            }
            else
            {
                float y = 44f;
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
                spriteBatch, "PRESS ANY KEY", cx,
                Context.Screen.Height - 14, new Color(90, 96, 116));
        }
    }
}
