using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    /// <summary>
    /// Moves slowly from right to left while bouncing continuously off the top and bottom screen margins.
    /// </summary>
    public sealed class ReboundMovement : IMovementStrategy
    {
        private const float VerticalSpeed = 46f;
        private const float Margin = 16f;

        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            var bounds = world.Bounds;
            float top = bounds.Top + Margin;
            float bottom = bounds.Bottom - Margin;
            float span = bottom - top;

            if (span > 10f)
            {
                // Derive starting phase offset from the spawn position so multiple enemies don't bounce in lockstep
                float initialOffset = (enemy.SpawnY - top) + (enemy.SpawnX % 50f);
                float period = span * 2f;
                float progress = (enemy.Age * VerticalSpeed + initialOffset) % period;
                if (progress < 0f) progress += period;

                enemy.Position.Y = progress <= span
                    ? top + progress
                    : top + (period - progress);
            }

            enemy.Position.X -= enemy.EffectiveSpeed * dt;
        }
    }
}
