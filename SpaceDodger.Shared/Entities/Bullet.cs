using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Graphics;

namespace SpaceDodger.Entities
{
    public enum BulletOwner { Player, Enemy }

    /// <summary>Pooled projectile used by both the player and enemies.</summary>
    public sealed class Bullet : Entity, ICollidable
    {
        public BulletOwner Owner { get; private set; }
        public int Damage { get; private set; }

        private Animation _animation;
        private AnimationPlayer _player;
        private Rectangle _world;

        public override Rectangle Bounds =>
            CenteredRect(_animation?.FrameWidth ?? 4, _animation?.FrameHeight ?? 2);

        public void Configure(
            Animation animation, BulletOwner owner, int damage,
            Vector2 position, Vector2 velocity, Rectangle world)
        {
            _animation = animation;
            _player.Play(animation);
            Owner = owner;
            Damage = damage;
            Position = position;
            Velocity = velocity;
            _world = world;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            _player.Update(dt);

            // Recycle once safely off-screen.
            var margin = _world;
            margin.Inflate(12, 12);
            if (!margin.Contains((int)Position.X, (int)Position.Y))
                Deactivate();
        }

        public void OnCollision(ICollidable other) => Deactivate();

        public override void Draw(SpriteBatch spriteBatch)
        {
            _player.Draw(
                spriteBatch,
                Position - new Vector2(_animation.FrameWidth / 2f, _animation.FrameHeight / 2f),
                Color.White);
        }
    }
}
