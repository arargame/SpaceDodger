using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Graphics;
using SpaceDodger.Input;

namespace SpaceDodger.Screens
{
    /// <summary>
    /// Reusable vertical menu: keyboard navigation on desktop, tap targets on
    /// mobile. Shared by every menu screen so no screen re-implements this (DRY).
    /// </summary>
    public sealed class MenuList
    {
        private sealed class Item
        {
            public string Label;
            public Action Action;
            public bool Enabled = true;
        }

        private static readonly Color Normal = new Color(150, 160, 190);
        private static readonly Color Selected = new Color(255, 220, 60);
        private static readonly Color Disabled = new Color(70, 76, 92);

        private readonly List<Item> _items = new List<Item>();
        private readonly PixelFont _font;
        private readonly float _centerX;
        private readonly float _topY;
        private readonly int _spacing;

        public int SelectedIndex { get; private set; }

        public MenuList(PixelFont font, float centerX, float topY, int spacing = 14)
        {
            _font = font;
            _centerX = centerX;
            _topY = topY;
            _spacing = spacing;
        }

        public MenuList Add(string label, Action action, bool enabled = true)
        {
            _items.Add(new Item { Label = label, Action = action, Enabled = enabled });
            if (!_items[SelectedIndex].Enabled)
                MoveSelection(1);
            return this;
        }

        public void Update(in InputState input)
        {
            if (_items.Count == 0)
                return;

            if (input.UpPressed)
                MoveSelection(-1);
            if (input.DownPressed)
                MoveSelection(1);

            if (input.Tap.HasValue)
            {
                int hit = HitTest(input.Tap.Value);
                if (hit >= 0)
                {
                    SelectedIndex = hit;
                    Activate();
                    return;
                }
                // A tap that missed every item should not also confirm.
                return;
            }

            if (input.ConfirmPressed)
                Activate();
        }

        private void Activate()
        {
            var item = _items[SelectedIndex];
            if (item.Enabled)
                item.Action?.Invoke();
        }

        private void MoveSelection(int direction)
        {
            if (_items.Count == 0)
                return;

            // Skip disabled entries, give up after a full loop.
            for (int step = 0; step < _items.Count; step++)
            {
                SelectedIndex = (SelectedIndex + direction + _items.Count) % _items.Count;
                if (_items[SelectedIndex].Enabled)
                    return;
            }
        }

        private int HitTest(Vector2 point)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (RectFor(i).Contains((int)point.X, (int)point.Y))
                    return _items[i].Enabled ? i : -1;
            }
            return -1;
        }

        private Rectangle RectFor(int index)
        {
            var size = _font.Measure(_items[index].Label);
            float y = _topY + index * _spacing;
            // Padded to a comfortable touch target.
            return new Rectangle(
                (int)(_centerX - size.X / 2f) - 8,
                (int)y - 3,
                (int)size.X + 16,
                (int)size.Y + 6);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var color = !item.Enabled
                    ? Disabled
                    : i == SelectedIndex ? Selected : Normal;

                float y = _topY + i * _spacing;
                _font.DrawCentered(spriteBatch, item.Label, _centerX, y, color);

                if (i == SelectedIndex && item.Enabled)
                {
                    var size = _font.Measure(item.Label);
                    _font.Draw(spriteBatch, ">", new Vector2(_centerX - size.X / 2f - 10, y), Selected);
                }
            }
        }
    }
}
