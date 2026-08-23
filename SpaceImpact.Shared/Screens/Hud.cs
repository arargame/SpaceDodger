using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Entities;
using SpaceImpact.Graphics;
using SpaceImpact.Systems;

namespace SpaceImpact.Screens
{
    /// <summary>Draws the in-game overlay: score, lives, weapon, buffs, boss health.</summary>
    public sealed class Hud
    {
        private static readonly Color Dim = new Color(150, 160, 190);
        private static readonly Color Accent = new Color(255, 220, 60);
        private static readonly Color ShieldColor = new Color(96, 204, 246);
        private static readonly Color RapidColor = new Color(238, 148, 58);

        private readonly PixelFont _font;
        private readonly Texture2D _pixel;
        private readonly Rectangle _bounds;

        public Hud(PixelFont font, Texture2D pixel, Rectangle bounds)
        {
            _font = font;
            _pixel = pixel;
            _bounds = bounds;
        }

        /// <summary>Large touch target for the visible pause glyph.</summary>
        public Rectangle PauseButtonBounds =>
            new Rectangle(_bounds.Width - 72, 0, 22, 12);

        public bool IsPauseButton(Vector2 point) =>
            PauseButtonBounds.Contains((int)point.X, (int)point.Y);

        public void Draw(
            SpriteBatch spriteBatch, ScoreTracker score, Player player,
            int levelNumber, Enemy boss)
        {
            // Top bar background strip.
            spriteBatch.Draw(_pixel, new Rectangle(0, 0, _bounds.Width, 10), new Color(0, 0, 0, 150));

            _font.Draw(spriteBatch, $"{score.Score:D7}", new Vector2(3, 1), Color.White);

            if (score.Combo > 1)
                _font.Draw(spriteBatch, $"x{score.Combo}", new Vector2(48, 1), Accent);

            _font.DrawCentered(spriteBatch, $"LV{levelNumber:00}", _bounds.Width / 2f, 1, Dim);

            _font.Draw(spriteBatch, $"W{player.WeaponLevel}", new Vector2(_bounds.Width - 88, 1), Dim);

            DrawPauseButton(spriteBatch);

            // Lives as small ship pips on the right.
            for (int i = 0; i < player.Lives && i < 7; i++)
                spriteBatch.Draw(_pixel, new Rectangle(_bounds.Width - 46 + i * 6, 3, 4, 4), Accent);

            DrawBuffs(spriteBatch, player);

            if (boss != null && boss.Active)
                DrawBossHealth(spriteBatch, boss);
        }

        private void DrawPauseButton(SpriteBatch spriteBatch)
        {
            var bounds = PauseButtonBounds;
            spriteBatch.Draw(_pixel, bounds, new Color(44, 50, 70));
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X + 7, bounds.Y + 3, 2, 6), Color.White);
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X + 13, bounds.Y + 3, 2, 6), Color.White);
        }

        /// <summary>Timed buff bars along the bottom-left, only while active.</summary>
        private void DrawBuffs(SpriteBatch spriteBatch, Player player)
        {
            int y = _bounds.Height - 7;

            if (player.IsShielded)
            {
                DrawBar(spriteBatch, 3, y, player.ShieldTimer / Core.GameConfig.ShieldDuration, ShieldColor);
                _font.Draw(spriteBatch, "S", new Vector2(3, y - 9), ShieldColor);
            }

            if (player.IsRapidFiring)
            {
                DrawBar(spriteBatch, 30, y, player.RapidTimer / Core.GameConfig.RapidFireDuration, RapidColor);
                _font.Draw(spriteBatch, "R", new Vector2(30, y - 9), RapidColor);
            }
        }

        private void DrawBar(SpriteBatch spriteBatch, int x, int y, float fraction, Color color)
        {
            const int width = 22;
            const int height = 3;
            fraction = MathHelper.Clamp(fraction, 0f, 1f);

            spriteBatch.Draw(_pixel, new Rectangle(x - 1, y - 1, width + 2, height + 2), new Color(0, 0, 0, 160));
            spriteBatch.Draw(_pixel, new Rectangle(x, y, width, height), color * 0.25f);
            spriteBatch.Draw(_pixel, new Rectangle(x, y, (int)(width * fraction), height), color);
        }

        private void DrawBossHealth(SpriteBatch spriteBatch, Enemy boss)
        {
            const int barWidth = 140;
            const int barHeight = 4;
            int x = (_bounds.Width - barWidth) / 2;
            int y = 12;

            spriteBatch.Draw(_pixel, new Rectangle(x - 1, y - 1, barWidth + 2, barHeight + 2), new Color(30, 14, 20));
            spriteBatch.Draw(_pixel, new Rectangle(x, y, barWidth, barHeight), new Color(70, 30, 40));

            int fill = (int)(barWidth * boss.HealthFraction);
            var color = boss.HealthFraction > 0.5f
                ? new Color(230, 70, 70)
                : boss.HealthFraction > 0.25f ? new Color(240, 150, 50) : new Color(255, 220, 70);

            spriteBatch.Draw(_pixel, new Rectangle(x, y, fill, barHeight), color);
        }

        /// <summary>Touch-control hint drawn for the first seconds on mobile.</summary>
        public void DrawTouchHint(SpriteBatch spriteBatch, float alpha)
        {
            var color = Color.White * alpha;
            _font.Draw(spriteBatch, "DRAG = MOVE + FIRE", new Vector2(12, _bounds.Height - 22), color);
            _font.Draw(spriteBatch, "OR HOLD RIGHT", new Vector2(_bounds.Width - 78, _bounds.Height - 13), color);
        }
    }
}
