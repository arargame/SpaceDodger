using Microsoft.Xna.Framework;
using SpaceImpact.Entities;

namespace SpaceImpact.Core
{
    /// <summary>Event payloads exchanged over the <see cref="EventBus"/>.</summary>

    public readonly struct EnemyDestroyedEvent
    {
        public readonly int Score;
        public readonly Vector2 Position;
        public readonly bool IsBoss;

        public EnemyDestroyedEvent(int score, Vector2 position, bool isBoss)
        {
            Score = score;
            Position = position;
            IsBoss = isBoss;
        }
    }

    public readonly struct PlayerDamagedEvent
    {
        public readonly int RemainingLives;
        public readonly Vector2 Position;

        public PlayerDamagedEvent(int remainingLives, Vector2 position)
        {
            RemainingLives = remainingLives;
            Position = position;
        }
    }

    public readonly struct ScoreChangedEvent
    {
        public readonly int NewScore;
        public ScoreChangedEvent(int newScore) => NewScore = newScore;
    }

    public readonly struct PowerUpCollectedEvent
    {
        public readonly PowerUpType Type;
        public PowerUpCollectedEvent(PowerUpType type) => Type = type;
    }
}
