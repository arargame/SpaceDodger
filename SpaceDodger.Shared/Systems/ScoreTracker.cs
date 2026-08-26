using SpaceDodger.Core;

namespace SpaceDodger.Systems
{
    /// <summary>
    /// Owns the run's score and combo multiplier. Listens on the event bus so
    /// nothing has to call it directly (Single Responsibility).
    /// </summary>
    public sealed class ScoreTracker
    {
        private const float ComboWindow = 2.0f;
        private const int MaxCombo = 8;

        private readonly EventBus _events;

        private float _comboTimer;

        public int Score { get; private set; }
        public int Combo { get; private set; } = 1;

        public ScoreTracker(EventBus events)
        {
            _events = events;
            _events.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
        }

        public void Detach() => _events.Unsubscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);

        public void Reset()
        {
            Score = 0;
            Combo = 1;
            _comboTimer = 0f;
        }

        public void Update(float dt)
        {
            if (_comboTimer <= 0f)
                return;

            _comboTimer -= dt;
            if (_comboTimer <= 0f)
                Combo = 1;
        }

        /// <summary>Award flat points (pickups, end-of-level bonuses).</summary>
        public void AddBonus(int points)
        {
            if (points <= 0)
                return;

            Score += points;
            _events.Publish(new ScoreChangedEvent(Score));
        }

        /// <summary>Called when the player is hit — the combo chain breaks.</summary>
        public void BreakCombo()
        {
            Combo = 1;
            _comboTimer = 0f;
        }

        private void OnEnemyDestroyed(EnemyDestroyedEvent e)
        {
            Score += e.Score * Combo;
            Combo = System.Math.Min(Combo + 1, MaxCombo);
            _comboTimer = ComboWindow;

            _events.Publish(new ScoreChangedEvent(Score));
        }
    }
}
