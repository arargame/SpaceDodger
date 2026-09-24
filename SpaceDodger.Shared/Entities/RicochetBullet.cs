using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Graphics;

namespace SpaceDodger.Entities
{
    /// <summary>A finite player bolt that rebounds from playfield edges before expiring.</summary>
    public sealed class RicochetBullet : Entity, ICollidable, IPlayerProjectile
    {
        public int Damage => 2;

        private Animation _animation;
        private AnimationPlayer _player;
        private Rectangle _world;
        private int _bouncesRemaining;

        public override Rectangle Bounds => CenteredRect(_animation?.FrameWidth ?? 8, _animation?.FrameHeight ?? 6);

        public void Configure(Animation animation, Vector2 position, Vector2 velocity, Rectangle world, int bounces)
        {
            _animation = animation;
            _player.Play(animation);
            Position = position;
            Velocity = velocity;
            _world = world;
            _bouncesRemaining = bounces;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            _player.Update(dt);

            if (Position.Y <= _world.Top || Position.Y >= _world.Bottom)
                BounceVertical();

            if (Position.X <= _world.Left || Position.X >= _world.Right)
            {
                if (--_bouncesRemaining < 0)
                {
                    Deactivate();
                    return;
                }
                Velocity.X = -Velocity.X;
                Position.X = MathHelper.Clamp(Position.X, _world.Left, _world.Right);
            }
        }

        public void OnCollision(ICollidable other)
        {
            if (--_bouncesRemaining < 0)
            {
                Deactivate();
                return;
            }

            Velocity.X = -Velocity.X;
            Position += Velocity * .025f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var origin = new Vector2(_animation.FrameWidth / 2f, _animation.FrameHeight / 2f);
            float rotation = (float)System.Math.Atan2(Velocity.Y, Velocity.X);
            spriteBatch.Draw(_animation.Texture, Position, _animation.FrameRect(_player.FrameIndex),
                new Color(154, 255, 212), rotation, origin, 1f, SpriteEffects.None, 0f);
        }

        private void BounceVertical()
        {
            if (--_bouncesRemaining < 0)
            {
                Deactivate();
                return;
            }

            Velocity.Y = -Velocity.Y;
            Position.Y = MathHelper.Clamp(Position.Y, _world.Top, _world.Bottom);
        }
    }
}
