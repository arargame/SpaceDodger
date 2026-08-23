using SpaceImpact.Entities;

namespace SpaceImpact.Movement
{
    /// <summary>Constant leftward drift — the classic Space Impact enemy.</summary>
    public sealed class StraightMovement : IMovementStrategy
    {
        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            enemy.Position.X -= enemy.EffectiveSpeed * dt;
        }
    }
}
