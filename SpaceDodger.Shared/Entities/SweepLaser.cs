using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDodger.Entities
{
    /// <summary>A single vertical energy sweep that travels left-to-right and damages each target once.</summary>
    public sealed class SweepLaser : Entity
    {
        private readonly HashSet<Enemy> _hitEnemies = new HashSet<Enemy>();
        private Texture2D _pixel;
        private Rectangle _world;
        private IReadOnlyList<Enemy> _targets;

        public override Rectangle Bounds => new Rectangle((int)Position.X - 3, _world.Top, 6, _world.Height);

        public void Configure(Texture2D pixel, Rectangle world, IReadOnlyList<Enemy> targets)
        {
            _pixel = pixel;
            _world = world;
            _targets = targets;
            Position = new Vector2(world.Left - 6f, world.Center.Y);
            _hitEnemies.Clear();
        }

        public override void Update(float dt)
        {
            Age += dt;
            Position.X += 265f * dt;
            var bounds = Bounds;

            for (int i = 0; i < _targets.Count; i++)
            {
                var enemy = _targets[i];
                if (enemy.Active && !_hitEnemies.Contains(enemy) && bounds.Intersects(enemy.Bounds))
                {
                    _hitEnemies.Add(enemy);
                    enemy.TakeDamage(3);
                }
            }

            if (Position.X > _world.Right + 8f)
                Deactivate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var outer = new Rectangle((int)Position.X - 3, _world.Top, 6, _world.Height);
            var core = new Rectangle((int)Position.X - 1, _world.Top, 2, _world.Height);
            spriteBatch.Draw(_pixel, outer, new Color(88, 180, 255) * .55f);
            spriteBatch.Draw(_pixel, core, new Color(236, 250, 255));
        }

        public override void OnRelease()
        {
            base.OnRelease();
            _hitEnemies.Clear();
            _targets = null;
        }
    }
}
