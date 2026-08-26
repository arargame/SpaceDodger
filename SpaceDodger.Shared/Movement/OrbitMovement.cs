using System;
using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    public sealed class OrbitMovement : IMovementStrategy
    {
        private const float ParkOffset = 150f;

        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            float centerX = world.Bounds.Right - ParkOffset;
            float centerY = world.Bounds.Center.Y;

            if (enemy.Position.X > centerX && enemy.Age < 2f)
            {
                enemy.Position.X -= enemy.EffectiveSpeed * dt;
                return;
            }

            float orbitRadiusX = 100f;
            float orbitRadiusY = 150f;
            float speed = 1.2f;
            
            float t = (enemy.Age - 2f) * speed;
            if (t < 0) t = 0;

            enemy.Position.X = centerX + orbitRadiusX * (float)Math.Cos(t);
            enemy.Position.Y = centerY + orbitRadiusY * (float)Math.Sin(t);
        }
    }
}
