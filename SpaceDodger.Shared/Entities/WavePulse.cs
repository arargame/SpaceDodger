using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDodger.Entities
{
    /// <summary>A growing travelling ring that can strike each enemy once.</summary>
    public sealed class WavePulse : Entity
    {
        private readonly HashSet<Enemy> _hitEnemies = new HashSet<Enemy>();
        private Texture2D _pixel;
        private Rectangle _world;
        private IReadOnlyList<Enemy> _targets;
        private float _radius;

        public override Rectangle Bounds => CenteredRect((int)(_radius * 2f), (int)(_radius * 2f));

        public void Configure(Texture2D pixel, Vector2 position, Rectangle world, IReadOnlyList<Enemy> targets)
        {
            _pixel = pixel;
            Position = position;
            _world = world;
            _targets = targets;
            _radius = 4f;
            _hitEnemies.Clear();
        }

        public override void Update(float dt)
        {
            Age += dt;
            Position.X += 124f * dt;
            _radius = 4f + Age * 38f;
            var bounds = Bounds;

            for (int i = 0; i < _targets.Count; i++)
            {
                var enemy = _targets[i];
                if (enemy.Active && !_hitEnemies.Contains(enemy) && bounds.Intersects(enemy.Bounds))
                {
                    _hitEnemies.Add(enemy);
                    enemy.TakeDamage(2);
                }
            }

            if (Age >= 1.6f || Position.X - _radius > _world.Right)
                Deactivate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            int radius = (int)_radius;
            int x = (int)Position.X - radius;
            int y = (int)Position.Y - radius;
            var color = new Color(110, 255, 196) * MathHelper.Clamp(1f - Age / 1.6f, 0f, 1f);

            spriteBatch.Draw(_pixel, new Rectangle(x, y, radius * 2, 1), color);
            spriteBatch.Draw(_pixel, new Rectangle(x, y + radius * 2 - 1, radius * 2, 1), color);
            spriteBatch.Draw(_pixel, new Rectangle(x, y, 1, radius * 2), color);
            spriteBatch.Draw(_pixel, new Rectangle(x + radius * 2 - 1, y, 1, radius * 2), color);
        }

        public override void OnRelease()
        {
            base.OnRelease();
            _hitEnemies.Clear();
            _targets = null;
        }
    }
}
