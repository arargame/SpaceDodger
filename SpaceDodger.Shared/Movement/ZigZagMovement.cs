using System;
using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    /// <summary>Leftward drift with a sharp triangle-wave vertical zigzag.</summary>
    public sealed class ZigZagMovement : IMovementStrategy
    {
        private const float Amplitude = 26f;
        private const float Period = 1.6f; // seconds for a full up-down cycle

        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            enemy.Position.X -= enemy.EffectiveSpeed * dt;

            // Triangle wave in [-1, 1].
            float phase = enemy.Age / Period % 1f;
            float tri = phase < 0.5f ? 4f * phase - 1f : 3f - 4f * phase;
            enemy.Position.Y = enemy.SpawnY + Amplitude * tri;
        }
    }
}
