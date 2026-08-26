using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDodger.Graphics
{
    /// <summary>Procedural parallax starfield background (no texture assets).</summary>
    public sealed class Starfield
    {
        private struct Star
        {
            public Vector2 Position;
            public float Speed;
            public int Size;
            public Color Color;
        }

        private readonly Star[] _stars;
        private readonly Texture2D _pixel;
        private readonly int _width;
        private readonly int _height;
        private readonly Random _random = new Random(1234);

        public float SpeedMultiplier { get; set; } = 1f;

        public Starfield(Texture2D pixel, int width, int height, int count = 70)
        {
            _pixel = pixel;
            _width = width;
            _height = height;
            _stars = new Star[count];
            for (int i = 0; i < count; i++)
                _stars[i] = CreateStar(randomX: true);
        }

        private Star CreateStar(bool randomX)
        {
            // Two parallax layers: far (slow, dim) and mid (medium, medium bright).
            // Removed the near/fast layer to avoid confusion with bullets.
            int layer = _random.Next(2);
            float speed = 8f + layer * 14f + (float)_random.NextDouble() * 6f;
            byte lum = (byte)(70 + layer * 50);

            return new Star
            {
                Position = new Vector2(
                    randomX ? _random.Next(_width) : _width + 2,
                    _random.Next(_height)),
                Speed = speed,
                Size = 1, // Her zaman 1 piksel
                Color = new Color(lum, lum, (byte)Math.Min(255, lum + 30)),
            };
        }

        public void Update(float dt)
        {
            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i].Position.X -= _stars[i].Speed * SpeedMultiplier * dt;
                if (_stars[i].Position.X < -2)
                    _stars[i] = CreateStar(randomX: false);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var star in _stars)
            {
                spriteBatch.Draw(
                    _pixel,
                    new Rectangle((int)star.Position.X, (int)star.Position.Y, star.Size, star.Size),
                    star.Color);
            }
        }
    }
}
