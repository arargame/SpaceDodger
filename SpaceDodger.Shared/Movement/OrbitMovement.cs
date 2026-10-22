using System;
using Microsoft.Xna.Framework;
using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    /// <summary>
    /// Elliptical orbital movement pattern for late-game bosses (Level 90 Phantom).
    /// Orbits predictably within the right portion of the playfield without leaving bounds.
    /// </summary>
    public sealed class OrbitMovement : IMovementStrategy
    {
        private const float ParkOffset = 72f;

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

            // Scaled elliptical orbit tailored for 320x180 resolution (168px playfield height)
            float orbitRadiusX = 36f;
            float orbitRadiusY = 46f;
            float speed = 1.2f;

            float t = (enemy.Age - 2.0f) * speed;
            if (t < 0f) t = 0f;

            float halfHeight = 16f;
            float halfWidth = 20f;

            float minX = world.Bounds.Left + world.Bounds.Width * 0.5f;
            float maxX = world.Bounds.Right - halfWidth;
            float minY = world.Bounds.Top + halfHeight + 4f;
            float maxY = world.Bounds.Bottom - halfHeight - 4f;

            float newX = centerX + orbitRadiusX * (float)Math.Cos(t);
            float newY = centerY + orbitRadiusY * (float)Math.Sin(t);

            enemy.Position.X = MathHelper.Clamp(newX, minX, maxX);
            enemy.Position.Y = MathHelper.Clamp(newY, minY, maxY);
        }
    }
}
