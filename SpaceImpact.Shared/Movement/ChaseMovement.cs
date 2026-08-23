using SpaceImpact.Entities;

namespace SpaceImpact.Movement
{
    /// <summary>Drifts left while homing vertically toward the player.</summary>
    public sealed class ChaseMovement : IMovementStrategy
    {
        private const float VerticalSpeed = 34f;

        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            enemy.Position.X -= enemy.EffectiveSpeed * dt;

            float dy = world.PlayerPosition.Y - enemy.Position.Y;
            float step = VerticalSpeed * dt;
            if (System.Math.Abs(dy) <= step)
                enemy.Position.Y = world.PlayerPosition.Y;
            else
                enemy.Position.Y += System.Math.Sign(dy) * step;
        }
    }
}
