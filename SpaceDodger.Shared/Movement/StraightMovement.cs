using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    /// <summary>Constant leftward drift — the classic Space Dodger enemy.</summary>
    public sealed class StraightMovement : IMovementStrategy
    {
        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            enemy.Position.X -= enemy.EffectiveSpeed * dt;
        }
    }
}
