using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    /// <summary>Crosses the screen from an upper corner to the opposite lower corner.</summary>
    public sealed class DiagonalFlockMovement : IMovementStrategy
    {
        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            float horizontalDirection = enemy.SpawnX > world.Bounds.Center.X ? -1f : 1f;
            enemy.Position.X += horizontalDirection * enemy.EffectiveSpeed * .92f * dt;
            enemy.Position.Y += enemy.EffectiveSpeed * .62f * dt;
        }
    }
}
