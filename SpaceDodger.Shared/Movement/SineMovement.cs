using System;
using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    /// <summary>Leftward drift with a smooth vertical sine wave.</summary>
    public sealed class SineMovement : IMovementStrategy
    {
        private const float Amplitude = 22f;
        private const float Frequency = 2.1f;

        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            enemy.Position.X -= enemy.EffectiveSpeed * dt;
            enemy.Position.Y = enemy.SpawnY + Amplitude * (float)Math.Sin(enemy.Age * Frequency);
        }
    }
}
