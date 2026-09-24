using System;
using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    /// <summary>Enters from above, weaving unpredictably as it dives through the playfield.</summary>
    public sealed class TopDiveMovement : IMovementStrategy
    {
        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            enemy.Position.Y += enemy.EffectiveSpeed * 1.12f * dt;
            enemy.Position.X = enemy.SpawnX + 18f * (float)Math.Sin(enemy.Age * 3.1f + enemy.SpawnX * .08f);
        }
    }
}
