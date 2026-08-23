using SpaceImpact.Entities;

namespace SpaceImpact.Movement
{
    /// <summary>
    /// Strategy pattern: how an enemy moves each frame. Implementations are
    /// stateless singletons (flyweight) — per-enemy state lives on the enemy
    /// itself (Age, SpawnY, SpeedMultiplier).
    /// </summary>
    public interface IMovementStrategy
    {
        void Move(Enemy enemy, EnemyWorld world, float dt);
    }
}
