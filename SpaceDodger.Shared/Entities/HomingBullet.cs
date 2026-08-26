using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Graphics;

namespace SpaceDodger.Entities
{
    public sealed class HomingBullet : Entity, ICollidable
    {
        public BulletOwner Owner => BulletOwner.Player;
        public int Damage => 3;

        private Animation _animation;
        private AnimationPlayer _player;
        private Rectangle _world;
        private IReadOnlyList<Enemy> _targets;
        private float _heading;
        private float _speed;

        public override Rectangle Bounds =>
            CenteredRect(_animation?.FrameWidth ?? 4, _animation?.FrameHeight ?? 2);

        public void Configure(
            Animation animation, 
            Vector2 position, 
            float initialHeading, 
            float speed, 
            Rectangle world, 
            IReadOnlyList<Enemy> targets)
        {
            _animation = animation;
            _player.Play(animation);
            Position = position;
            _heading = initialHeading;
            _speed = speed;
            Velocity = new Vector2((float)Math.Cos(_heading), (float)Math.Sin(_heading)) * _speed;
            _world = world;
            _targets = targets;
        }

        public override void Update(float dt)
        {
            Age += dt;
            _player.Update(dt);

            // Find nearest active target
            Enemy nearest = null;
            float minSqDist = float.MaxValue;
            foreach (var enemy in _targets)
            {
                if (enemy.Active)
                {
                    float sqDist = Vector2.DistanceSquared(Position, enemy.Position);
                    if (sqDist < minSqDist)
                    {
                        minSqDist = sqDist;
                        nearest = enemy;
                    }
                }
            }

            if (nearest != null)
            {
                // Steer towards target
                float targetAngle = (float)Math.Atan2(nearest.Position.Y - Position.Y, nearest.Position.X - Position.X);
                
                // Calculate shortest angular distance
                float diff = targetAngle - _heading;
                while (diff > MathHelper.Pi) diff -= MathHelper.TwoPi;
                while (diff < -MathHelper.Pi) diff += MathHelper.TwoPi;

                // Smooth rotation using turn rate
                float turnRate = 3.5f; // radians per second
                float turn = turnRate * dt;
                
                if (Math.Abs(diff) <= turn)
                {
                    _heading = targetAngle;
                }
                else
                {
                    _heading += Math.Sign(diff) * turn;
                }
            }
            else
            {
                // No target, steer towards 0 (straight right)
                float diff = 0 - _heading;
                while (diff > MathHelper.Pi) diff -= MathHelper.TwoPi;
                while (diff < -MathHelper.Pi) diff += MathHelper.TwoPi;
                
                float turnRate = 3.5f;
                float turn = turnRate * dt;
                
                if (Math.Abs(diff) <= turn)
                    _heading = 0;
                else
                    _heading += Math.Sign(diff) * turn;
            }

            Velocity = new Vector2((float)Math.Cos(_heading), (float)Math.Sin(_heading)) * _speed;

            // Base update applies velocity
            Position += Velocity * dt;

            var margin = _world;
            margin.Inflate(12, 12);
            if (!margin.Contains((int)Position.X, (int)Position.Y))
                Deactivate();
        }

        public void OnCollision(ICollidable other) => Deactivate();

        public override void Draw(SpriteBatch spriteBatch)
        {
            var origin = new Vector2(_animation.FrameWidth / 2f, _animation.FrameHeight / 2f);
            spriteBatch.Draw(
                _animation.Texture, 
                Position, 
                _animation.FrameRect(_player.FrameIndex),
                Color.White, 
                _heading, 
                origin, 
                1f, 
                SpriteEffects.None, 
                0f);
        }
    }
}
