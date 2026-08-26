using System;
using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    public sealed class DriftMovement : IMovementStrategy
    {
        private const float ParkOffset = 180f;

        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            float centerX = world.Bounds.Right - ParkOffset;
            float centerY = world.Bounds.Center.Y;

            if (enemy.Position.X > centerX + 100f && enemy.Age < 1.5f)
            {
                enemy.Position.X -= enemy.EffectiveSpeed * dt;
                return;
            }

            float driftX = 80f * (float)Math.Sin(enemy.Age * 0.4f) + 40f * (float)Math.Cos(enemy.Age * 0.7f);
            float driftY = 120f * (float)Math.Sin(enemy.Age * 0.5f) + 60f * (float)Math.Sin(enemy.Age * 0.3f);

            enemy.Position.X = centerX + driftX;
            enemy.Position.Y = centerY + driftY;
        }
    }
}
