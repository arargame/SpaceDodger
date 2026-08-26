using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Graphics;

namespace SpaceDodger.Entities
{
    /// <summary>Pooled one-shot explosion effect; recycles itself when finished.</summary>
    public sealed class Explosion : Entity
    {
        private Animation _animation;
        private AnimationPlayer _player;
        private float _scale;

        public override Rectangle Bounds =>
            CenteredRect(_animation?.FrameWidth ?? 16, _animation?.FrameHeight ?? 16);

        public void Configure(Animation animation, Vector2 position, float scale = 1f)
        {
            _animation = animation;
            _player.Play(animation);
            Position = position;
            _scale = scale;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            _player.Update(dt);
            if (_player.Finished)
                Deactivate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var src = _animation.FrameRect(_player.FrameIndex);
            var origin = new Vector2(_animation.FrameWidth / 2f, _animation.FrameHeight / 2f);
            spriteBatch.Draw(
                _animation.Texture, Position, src, Color.White,
                0f, origin, _scale, SpriteEffects.None, 0f);
        }
    }
}
