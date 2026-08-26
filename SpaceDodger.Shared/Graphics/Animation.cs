using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDodger.Graphics
{
    /// <summary>Immutable description of a horizontal-strip sprite animation.</summary>
    public sealed class Animation
    {
        public Texture2D Texture { get; }
        public int FrameWidth { get; }
        public int FrameHeight { get; }
        public int FrameCount { get; }
        public float FramesPerSecond { get; }
        public bool Loop { get; }

        public Animation(Texture2D texture, int frameCount, float fps, bool loop = true)
        {
            Texture = texture;
            FrameCount = frameCount;
            FrameWidth = texture.Width / frameCount;
            FrameHeight = texture.Height;
            FramesPerSecond = fps;
            Loop = loop;
        }

        public Rectangle FrameRect(int index) =>
            new Rectangle(index * FrameWidth, 0, FrameWidth, FrameHeight);
    }

    /// <summary>Mutable playback state for an <see cref="Animation"/> (flyweight:
    /// many entities share one Animation, each has its own player).</summary>
    public struct AnimationPlayer
    {
        private Animation _animation;
        private float _time;

        public Animation Animation => _animation;
        public bool Finished { get; private set; }

        public void Play(Animation animation)
        {
            _animation = animation;
            _time = 0f;
            Finished = false;
        }

        public void Update(float dt)
        {
            if (_animation == null || Finished)
                return;

            _time += dt;
            if (!_animation.Loop &&
                _time >= _animation.FrameCount / _animation.FramesPerSecond)
                Finished = true;
        }

        public int FrameIndex
        {
            get
            {
                if (_animation == null)
                    return 0;
                int frame = (int)(_time * _animation.FramesPerSecond);
                return _animation.Loop
                    ? frame % _animation.FrameCount
                    : System.Math.Min(frame, _animation.FrameCount - 1);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 topLeft, Color color)
        {
            if (_animation == null)
                return;
            spriteBatch.Draw(_animation.Texture, topLeft, _animation.FrameRect(FrameIndex), color);
        }
    }
}
