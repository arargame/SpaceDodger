using System;
using Microsoft.Xna.Framework;
using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    /// <summary>
    /// Dual binary orbit pattern: two entities orbit a shared center of mass
    /// as it drifts horizontally from right to left across the playfield.
    /// Phase offset determines the entity's angular position along the binary orbit.
    /// When paired with an opposing phase (e.g. 0 and Pi), the entities appear
    /// locked in a mutual binary rotation around each other.
    /// </summary>
    public sealed class BinaryOrbitMovement : IMovementStrategy
    {
        private readonly float _phaseOffset;
        private const float OrbitRadius = 18f;
        private const float AngularSpeed = 2.8f; // ~0.45 rev/sec

        public BinaryOrbitMovement(float phaseOffset = 0f)
        {
            _phaseOffset = phaseOffset;
        }

        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            // Center of mass translates horizontally right to left at the enemy's effective speed
            float centerX = enemy.SpawnX - enemy.EffectiveSpeed * enemy.Age;
            float centerY = enemy.SpawnY;

            // Dual orbital position around the center of mass
            float angle = enemy.Age * AngularSpeed + _phaseOffset;
            float offsetX = OrbitRadius * (float)Math.Cos(angle);
            float offsetY = OrbitRadius * (float)Math.Sin(angle);

            // Keep vertical bounds safely inside the active playfield
            const float halfHeight = 8f;
            float minY = world.Bounds.Top + halfHeight + 2f;
            float maxY = world.Bounds.Bottom - halfHeight - 2f;

            enemy.Position.X = centerX + offsetX;
            enemy.Position.Y = MathHelper.Clamp(centerY + offsetY, minY, maxY);
        }
    }
}
