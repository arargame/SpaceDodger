using System;
using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    public sealed class SineBossMovement : IMovementStrategy
    {
        private const float ParkOffset = 60f;

        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            float targetX = world.Bounds.Right - ParkOffset;

            if (enemy.Position.X > targetX + 40f && enemy.Age < 1f)
            {
                enemy.Position.X -= enemy.EffectiveSpeed * dt;
                return;
            }

            float centerY = world.Bounds.Center.Y;
            float sweep = world.Bounds.Height / 2f - 40f;
            
            float freq = 0.8f + 0.4f * (float)Math.Sin(enemy.Age * 0.5f);
            
            enemy.Position.Y = centerY + sweep * (float)Math.Sin(enemy.Age * freq);
            enemy.Position.X = targetX + 30f * (float)Math.Cos(enemy.Age * 1.5f);
        }
    }
}
