using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Entities;
using SpaceDodger.Graphics;
using SpaceDodger.Systems;

namespace SpaceDodger.Screens
{
    /// <summary>Draws the in-game overlay: score, lives, weapon, buffs, boss health.</summary>
    public sealed class Hud
    {
        private static readonly Color Dim = new Color(150, 160, 190);
        private static readonly Color Accent = new Color(255, 220, 60);
        private static readonly Color ShieldColor = new Color(96, 204, 246);
        private static readonly Color RapidColor = new Color(238, 148, 58);
        private static readonly Color ScatterColor = new Color(190, 90, 230);
        private static readonly Color HomingColor = new Color(96, 246, 160);

        private readonly PixelFont _font;
        private readonly Texture2D _pixel;
        private readonly Rectangle _bounds;
        private readonly bool _showPauseButton;

        public Hud(PixelFont font, Texture2D pixel, Rectangle bounds, bool showPauseButton)
        {
            _font = font;
            _pixel = pixel;
            _bounds = bounds;
            _showPauseButton = showPauseButton;
        }

        /// <summary>Large touch target for the visible pause glyph.</summary>
        public Rectangle PauseButtonBounds =>
            new Rectangle((_bounds.Width / 2) + 40, 0, 48, 20); // Moved to between LV (center) and W (right edge)

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

            _font.DrawCentered(spriteBatch, $"LV{levelNumber}", _bounds.Width / 2f, 1, Dim);

            _font.Draw(spriteBatch, $"W{player.WeaponLevel}", new Vector2(_bounds.Width - 54, 1), Dim); // Moved weapon slightly right

            if (_showPauseButton)
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
            // Draw a subtle background for the pause button to show the touch area clearly
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X + 6, bounds.Y, bounds.Width - 12, bounds.Height), new Color(44, 50, 70));
            // Draw the two vertical pause lines centered in the area
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X + 20, bounds.Y + 4, 2, 7), Color.White);
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X + 26, bounds.Y + 4, 2, 7), Color.White);
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

            if (player.IsScatterActive)
            {
                DrawBar(spriteBatch, 57, y, player.ScatterTimer / Core.GameConfig.ScatterDuration, ScatterColor);
                _font.Draw(spriteBatch, "C", new Vector2(57, y - 9), ScatterColor);
            }

            if (player.HomingCount > 0)
            {
                _font.Draw(spriteBatch, $"M{player.HomingCount}", new Vector2(84, y - 4), HomingColor);
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
            _font.Draw(spriteBatch, "DRAG TO MOVE", new Vector2(20, _bounds.Height - 22), color);
            _font.Draw(spriteBatch, "AUTO FIRE", new Vector2(_bounds.Width - 66, _bounds.Height - 22), color);
        }
    }
}
