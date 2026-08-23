using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Core;
using SpaceImpact.Input;
using SpaceImpact.Levels;

namespace SpaceImpact.Screens
{
    /// <summary>
    /// Paged grid of level buttons (10 x 5 per page). Locked levels are dimmed
    /// and unselectable; boss levels are marked.
    /// </summary>
    public sealed class LevelSelectScreen : Screen
    {
        private const int Columns = 10;
        private const int Rows = 5;
        private const int PerPage = Columns * Rows;
        private const int CellWidth = 30;
        private const int CellHeight = 24;

        private static readonly Color Unlocked = new Color(150, 160, 190);
        private static readonly Color Locked = new Color(58, 64, 80);
        private static readonly Color Highlight = new Color(255, 220, 60);
        private static readonly Color BossTint = new Color(230, 90, 90);

        private readonly ILevelRepository _levels;

        private int _cursor;      // index within the whole campaign (0-based)
        private int _maxUnlocked;
        private Point _origin;
        private int _pageCount;

        public LevelSelectScreen(GameContext context, ILevelRepository levels) : base(context)
        {
            _levels = levels;
        }

        private int Page => _cursor / PerPage;

        public override void Load()
        {
            _maxUnlocked = Context.Save.Data.MaxUnlockedLevel;
            _pageCount = (_levels.Count + PerPage - 1) / PerPage;

            // Start on the page holding the furthest unlocked level.
            _cursor = MathHelper.Clamp(_maxUnlocked - 1, 0, _levels.Count - 1);

            _origin = new Point(
                (Context.Screen.Width - Columns * CellWidth) / 2,
                26);
        }

        public override void Update(float dt, in InputState input)
        {
            if (input.BackPressed)
            {
                Context.Screens.Pop();
                return;
            }

            if (input.LeftPressed) Move(-1);
            if (input.RightPressed) Move(1);
            if (input.UpPressed) Move(-Columns);
            if (input.DownPressed) Move(Columns);

            if (input.Tap.HasValue)
            {
                int hit = HitTest(input.Tap.Value);
                if (hit >= 0)
                {
                    _cursor = hit;
                    Launch();
                }
                return;
            }

            if (input.ConfirmPressed)
                Launch();
        }

        private void Move(int delta)
        {
            int next = _cursor + delta;
            if (next >= 0 && next < _levels.Count)
                _cursor = next;
        }

        private void Launch()
        {
            int level = _cursor + 1;
            if (level > _maxUnlocked)
                return;

            Context.Screens.Reset(new GameplayScreen(Context, _levels, level));
        }

        private int HitTest(Vector2 point)
        {
            int first = Page * PerPage;
            for (int i = first; i < System.Math.Min(first + PerPage, _levels.Count); i++)
            {
                if (CellRect(i).Contains((int)point.X, (int)point.Y))
                    return i;
            }
            return -1;
        }

        private Rectangle CellRect(int index)
        {
            int local = index % PerPage;
            return new Rectangle(
                _origin.X + (local % Columns) * CellWidth,
                _origin.Y + (local / Columns) * CellHeight,
                CellWidth - 4, CellHeight - 4);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float cx = Context.Screen.Width / 2f;
            Context.Font.DrawCentered(spriteBatch, "SELECT LEVEL", cx, 8, Color.White, 2f);

            var pixel = Context.Textures.Pixel;
            int first = Page * PerPage;
            int last = System.Math.Min(first + PerPage, _levels.Count);

            for (int i = first; i < last; i++)
            {
                int level = i + 1;
                bool unlocked = level <= _maxUnlocked;
                bool selected = i == _cursor;
                bool isBoss = level % GameConfig.BossEvery == 0;

                var rect = CellRect(i);

                if (selected)
                {
                    var frame = rect;
                    frame.Inflate(2, 2);
                    spriteBatch.Draw(pixel, frame, Highlight * 0.3f);
                }

                spriteBatch.Draw(pixel, rect, new Color(20, 24, 38));

                var color = !unlocked
                    ? Locked
                    : selected ? Highlight : (isBoss ? BossTint : Unlocked);

                string label = unlocked ? level.ToString("00") : "--";
                Context.Font.DrawCentered(
                    spriteBatch, label,
                    rect.Center.X, rect.Center.Y - Context.Font.LineHeight / 2f, color);
            }

            if (_pageCount > 1)
            {
                Context.Font.DrawCentered(
                    spriteBatch, $"PAGE {Page + 1}/{_pageCount}",
                    cx, Context.Screen.Height - 26, new Color(110, 118, 140));
            }

            Context.Font.DrawCentered(
                spriteBatch,
                Context.Platform.IsMobile ? "TAP A LEVEL" : "ARROWS + ENTER, ESC TO GO BACK",
                cx, Context.Screen.Height - 14, new Color(90, 96, 116));
        }
    }
}
