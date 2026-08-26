using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDodger.Graphics
{
    /// <summary>
    /// Tiny bitmap font renderer. The atlas holds ASCII 32..126 as hand-drawn
    /// 5x7 glyphs in 6x8 cells (see tools/font_data.py). No SpriteFont/MGCB.
    /// The one-pixel margin inside each cell provides the letter spacing.
    /// </summary>
    public sealed class PixelFont
    {
        private const int CellWidth = 6;
        private const int CellHeight = 8;
        private const int Columns = 16;
        private const int FirstChar = 32;
        private const int LastChar = 126;

        private readonly Texture2D _atlas;

        public PixelFont(Texture2D atlas) => _atlas = atlas;

        public int LineHeight => CellHeight;

        public Vector2 Measure(string text, float scale = 1f) =>
            new Vector2(text.Length * CellWidth * scale, CellHeight * scale);

        public void Draw(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale = 1f)
        {
            var pos = position;
            foreach (char raw in text)
            {
                char c = raw < FirstChar || raw > LastChar ? '?' : raw;
                int index = c - FirstChar;
                var src = new Rectangle(
                    (index % Columns) * CellWidth,
                    (index / Columns) * CellHeight,
                    CellWidth, CellHeight);

                spriteBatch.Draw(_atlas, pos, src, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                pos.X += CellWidth * scale;
            }
        }

        /// <summary>Draw horizontally centered around centerX.</summary>
        public void DrawCentered(SpriteBatch spriteBatch, string text, float centerX, float y, Color color, float scale = 1f)
        {
            float width = Measure(text, scale).X;
            Draw(spriteBatch, text, new Vector2(centerX - width / 2f, y), color, scale);
        }
    }
}
