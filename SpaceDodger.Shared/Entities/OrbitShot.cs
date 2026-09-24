using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Graphics;

namespace SpaceDodger.Entities
{
    /// <summary>A short-lived orbital guard. It follows the ship instead of flooding the screen.</summary>
    public sealed class OrbitShot : Entity, ICollidable, IPlayerProjectile
    {
        public int Damage => 2;

        private Animation _animation;
        private AnimationPlayer _player;
        private Player _anchor;
        private float _phase;
        private float _angularVelocity;
        private float _radius;
        private float _remainingTime;

        public override Rectangle Bounds => CenteredRect(_animation?.FrameWidth ?? 6, _animation?.FrameHeight ?? 4);

        public void Configure(Animation animation, Player anchor, float phase, float radius, float duration)
        {
            _animation = animation;
            _player.Play(animation);
            _anchor = anchor;
            _phase = phase;
            _radius = radius;
            _angularVelocity = 4.4f + radius * .05f;
            _remainingTime = duration;
            Position = anchor.Position;
        }

        public override void Update(float dt)
        {
            Age += dt;
            _player.Update(dt);
            _remainingTime -= dt;
            if (_anchor == null || !_anchor.Active || _remainingTime <= 0f)
            {
                Deactivate();
                return;
            }

            _phase += _angularVelocity * dt;
            Position = _anchor.Position + new Vector2(
                (float)System.Math.Cos(_phase) * _radius,
                (float)System.Math.Sin(_phase) * _radius * .68f);
        }

        public void OnCollision(ICollidable other)
        {
            // Hop to the opposite arc after an impact so one target cannot be melted every frame.
            _phase += MathHelper.PiOver2;
        }

        public override void OnRelease()
        {
            base.OnRelease();
            _anchor = null;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var origin = new Vector2(_animation.FrameWidth / 2f, _animation.FrameHeight / 2f);
            spriteBatch.Draw(_animation.Texture, Position, _animation.FrameRect(_player.FrameIndex),
                new Color(210, 130, 255), _phase, origin, 1f, SpriteEffects.None, 0f);
        }
    }
}
