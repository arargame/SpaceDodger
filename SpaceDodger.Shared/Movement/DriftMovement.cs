using System;
using Microsoft.Xna.Framework;
using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    /// <summary>
    /// Complex multi-harmonic drift pattern for advanced bosses (Level 60 Sentinel).
    /// Sweeps smoothly across the right half of the playfield without leaving bounds.
    /// </summary>
    public sealed class DriftMovement : IMovementStrategy
    {
        private const float ParkOffset = 70f;

        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            float targetX = world.Bounds.Right - ParkOffset;

            // Smooth entry from right edge
            if (enemy.Position.X > targetX && enemy.Age < 2.0f)
            {
                enemy.Position.X -= enemy.EffectiveSpeed * dt;
                return;
            }

            float centerX = targetX;
            float centerY = world.Bounds.Center.Y;

            // Controlled harmonics scaled for native 320x180 resolution (playfield height: 168px)
            float driftX = 26f * (float)Math.Sin(enemy.Age * 0.6f) + 14f * (float)Math.Cos(enemy.Age * 0.9f);
            float driftY = 38f * (float)Math.Sin(enemy.Age * 0.7f) + 16f * (float)Math.Sin(enemy.Age * 0.35f);

            float halfHeight = 16f;
            float halfWidth = 20f;

            float minX = world.Bounds.Left + world.Bounds.Width * 0.5f;
            float maxX = world.Bounds.Right - halfWidth;
            float minY = world.Bounds.Top + halfHeight + 4f;
            float maxY = world.Bounds.Bottom - halfHeight - 4f;

            enemy.Position.X = MathHelper.Clamp(centerX + driftX, minX, maxX);
            enemy.Position.Y = MathHelper.Clamp(centerY + driftY, minY, maxY);
        }
    }
}
