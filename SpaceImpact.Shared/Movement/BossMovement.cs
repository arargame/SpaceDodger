using System;
using SpaceImpact.Entities;

namespace SpaceImpact.Movement
{
    /// <summary>
    /// Boss behavior: enter from the right, park near the right edge,
    /// then sweep up and down.
    /// </summary>
    public sealed class BossMovement : IMovementStrategy
    {
        private const float ParkOffset = 46f;

        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            float targetX = world.Bounds.Right - ParkOffset;

            if (enemy.Position.X > targetX)
            {
                enemy.Position.X -= enemy.EffectiveSpeed * dt;
                return;
            }

            float centerY = world.Bounds.Center.Y;
            float sweep = world.Bounds.Height / 2f - 28f;
            enemy.Position.Y = centerY + sweep * (float)Math.Sin(enemy.Age * 0.9f);
        }
    }
}
