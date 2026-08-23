using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Core;
using SpaceImpact.Input;
using SpaceImpact.Levels;

namespace SpaceImpact.Screens
{
    /// <summary>
    /// End-of-run screen. Records the score into the save file (with a small
    /// initials entry when it makes the table) and offers retry / menu.
    /// </summary>
    public sealed class GameOverScreen : Screen
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
        private const int NameLength = 3;

        private readonly ILevelRepository _levels;
        private readonly int _score;
        private readonly int _level;
        private readonly bool _victory;

        private MenuList _menu;
        private bool _enteringName;
        private int _rank = -1;
        private int _charIndex;
        private readonly int[] _letters = new int[NameLength];

        public GameOverScreen(
            GameContext context, ILevelRepository levels,
            int score, int level, bool victory)
            : base(context)
        {
            _levels = levels;
            _score = score;
            _level = level;
            _victory = victory;
        }

        public override void Load()
        {
            Context.Save.Data.RemoveLiveScore();
            // Does this run make the table? Insert a placeholder, then let the
            // player edit the initials in place.
            _rank = Context.Save.Data.AddScore(
                "AAA", _score, _level, GameConfig.HighScoreCapacity);

            if (_rank >= 0 && !Context.Platform.IsMobile)
            {
                _enteringName = true;
            }
            else
            {
                if (_rank >= 0)
                    Context.Save.Data.HighScores[_rank].Name = "ACE";
                Context.Save.Save();
                BuildMenu();
            }
        }

        private void BuildMenu()
        {
            _menu = new MenuList(Context.Font, Context.Screen.Width / 2f, 110f)
                .Add("RETRY", Retry)
                .Add("MAIN MENU", ToMenu);
        }

        private void Retry() =>
            Context.Screens.Reset(new GameplayScreen(Context, _levels, _victory ? 1 : _level));

        private void ToMenu() => Context.Screens.Reset(new MenuScreen(Context));

        public override void Update(float dt, in InputState input)
        {
            if (_enteringName)
            {
                UpdateNameEntry(input);
                return;
            }

            _menu.Update(input);
        }

        private void UpdateNameEntry(in InputState input)
        {
            if (input.UpPressed)
                _letters[_charIndex] = (_letters[_charIndex] + 1) % Alphabet.Length;
            if (input.DownPressed)
                _letters[_charIndex] = (_letters[_charIndex] - 1 + Alphabet.Length) % Alphabet.Length;
            if (input.RightPressed)
                _charIndex = System.Math.Min(_charIndex + 1, NameLength - 1);
            if (input.LeftPressed)
                _charIndex = System.Math.Max(_charIndex - 1, 0);

            // On touch, tapping the left/right half of a letter cycles it.
            if (input.Tap.HasValue)
            {
                var tap = input.Tap.Value;
                int slot = SlotAt(tap);
                if (slot >= 0)
                {
                    _charIndex = slot;
                    _letters[slot] = (_letters[slot] + 1) % Alphabet.Length;
                    return;
                }
                ConfirmName();
                return;
            }

            if (input.ConfirmPressed)
                ConfirmName();
        }

        private int SlotAt(Vector2 point)
        {
            for (int i = 0; i < NameLength; i++)
                if (SlotRect(i).Contains((int)point.X, (int)point.Y))
                    return i;
            return -1;
        }

        private Rectangle SlotRect(int index)
        {
            const int slotWidth = 20;
            int totalWidth = NameLength * slotWidth;
            int left = (Context.Screen.Width - totalWidth) / 2;
            return new Rectangle(left + index * slotWidth, 86, slotWidth - 4, 14);
        }

        private void ConfirmName()
        {
            var name = new char[NameLength];
            for (int i = 0; i < NameLength; i++)
                name[i] = Alphabet[_letters[i]];

            Context.Save.Data.HighScores[_rank].Name = new string(name).Trim();
            if (string.IsNullOrEmpty(Context.Save.Data.HighScores[_rank].Name))
                Context.Save.Data.HighScores[_rank].Name = "ACE";

            Context.Save.Save();

            _enteringName = false;
            BuildMenu();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float cx = Context.Screen.Width / 2f;

            var title = _victory ? "GALAXY SAVED" : "GAME OVER";
            var titleColor = _victory ? new Color(120, 230, 90) : new Color(230, 70, 70);
            Context.Font.DrawCentered(spriteBatch, title, cx, 28, titleColor, 2f);

            Context.Font.DrawCentered(
                spriteBatch, $"SCORE {_score}", cx, 50, Color.White);
            Context.Font.DrawCentered(
                spriteBatch, $"REACHED LEVEL {_level}", cx, 62, new Color(150, 160, 190));

            if (_victory && !_enteringName)
                Context.Font.DrawCentered(
                    spriteBatch, "NEMESIS DESTROYED", cx, 72, new Color(120, 230, 90));

            if (_enteringName)
            {
                Context.Font.DrawCentered(
                    spriteBatch, "NEW HIGH SCORE", cx, 74, new Color(255, 220, 60));
                DrawNameEntry(spriteBatch);
            }
            else
            {
                _menu.Draw(spriteBatch);
            }
        }

        private void DrawNameEntry(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < NameLength; i++)
            {
                var rect = SlotRect(i);
                bool active = i == _charIndex;

                spriteBatch.Draw(
                    Context.Textures.Pixel, rect,
                    active ? new Color(50, 56, 76) : new Color(24, 28, 42));

                Context.Font.DrawCentered(
                    spriteBatch, Alphabet[_letters[i]].ToString(),
                    rect.Center.X, rect.Y + 3,
                    active ? new Color(255, 220, 60) : Color.White);
            }

            Context.Font.DrawCentered(
                spriteBatch,
                Context.Platform.IsMobile ? "TAP LETTERS, THEN TAP BELOW" : "UP/DOWN TO PICK, ENTER TO SAVE",
                Context.Screen.Width / 2f, 108, new Color(90, 96, 116));
        }
    }
}
