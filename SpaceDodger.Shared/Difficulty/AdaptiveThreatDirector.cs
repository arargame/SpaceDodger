using Microsoft.Xna.Framework;

namespace SpaceDodger.Difficulty
{
    /// <summary>
    /// Fast-reacting arcade DDA. A short reward window follows strong play,
    /// then a hunter surge asks the player to earn the next breathing space.
    /// It only changes procedural reinforcement; authored waves stay intact.
    /// </summary>
    public sealed class AdaptiveThreatDirector : IDifficultyDirector
    {
        private enum FlowState { Recovering, Struggling, InFlow, Dominating, Godlike }

        private const float EvaluationInterval = 0.75f;
        private const float ModifierLerpSpeed = 1.7f;
        private const float RewardDuration = 3.5f;
        private const float SurgeDuration = 8.5f;

        private static readonly DifficultyModifiers Recovering = new DifficultyModifiers(1.55f, .72f, .70f, 1.35f, 2.00f, .35f, 4);
        private static readonly DifficultyModifiers Struggling = new DifficultyModifiers(1.28f, .85f, .84f, 1.18f, 1.45f, .65f, 6);
        private static readonly DifficultyModifiers Flow = DifficultyModifiers.Neutral;
        private static readonly DifficultyModifiers Dominating = new DifficultyModifiers(.76f, 1.13f, 1.18f, .82f, .82f, 1.35f, 10);
        private static readonly DifficultyModifiers Godlike = new DifficultyModifiers(.56f, 1.34f, 1.48f, .64f, .52f, 1.90f, 14);
        private static readonly DifficultyModifiers Reward = new DifficultyModifiers(.92f, 1.04f, 1.05f, .94f, 1.70f, .92f, 8);
        private static readonly DifficultyModifiers HunterSurge = new DifficultyModifiers(.48f, 1.42f, 1.55f, .58f, .42f, 2.15f, 15);

        private FlowState _state = FlowState.InFlow;
        private DifficultyModifiers _current = DifficultyModifiers.Neutral;
        private float _evaluationTimer;
        private float _timeSinceHit;
        private float _killHeat;
        private float _cycleTimer;
        private bool _rewardCycle;
        private bool _wasHighPerformance;
        private bool _isBossLevel;
        private int _startingLives;

        public DifficultyModifiers Current => _current;
        public float Intensity => _current.ProceduralRate;
        public float HealthSupplyBias => _state switch
        {
            FlowState.Recovering => .48f,
            FlowState.Struggling => .24f,
            FlowState.InFlow => .02f,
            _ => 0f
        };

        public string StateLabel => _isBossLevel ? "BOSS LOCK" : _state switch
        {
            FlowState.Recovering => "RECOVERING",
            FlowState.Struggling => "STRUGGLING",
            FlowState.Dominating => "DOMINATING",
            FlowState.Godlike => "GODLIKE",
            _ => "IN FLOW"
        };

        public string CycleLabel
        {
            get
            {
                if (_isBossLevel) return "AUTHORED BOSS";
                if (_state != FlowState.Dominating && _state != FlowState.Godlike) return "STEADY";
                return _rewardCycle ? "REWARD WINDOW" : "HUNTER SURGE";
            }
        }

        public void BeginLevel(int levelNumber, int startingLives, bool isBossLevel, bool preserveMomentum)
        {
            if (!preserveMomentum)
            {
                _state = FlowState.InFlow;
                _current = DifficultyModifiers.Neutral;
                _timeSinceHit = 0f;
                _killHeat = 0f;
                _cycleTimer = 0f;
                _rewardCycle = false;
                _wasHighPerformance = false;
                _startingLives = startingLives;
            }

            // A level boundary is not a skill reset. Keep clean-run momentum,
            // combo-derived kill heat and the current pressure target intact.
            _evaluationTimer = 0f;
            _isBossLevel = isBossLevel;
        }

        public void Update(float deltaTime, int lives, int combo, int activeEnemies, int activeEnemyBullets)
        {
            _timeSinceHit += deltaTime;
            _killHeat = MathHelper.Max(0f, _killHeat - deltaTime * .24f);

            if (_isBossLevel)
            {
                _current = DifficultyModifiers.Lerp(_current, DifficultyModifiers.Neutral,
                    MathHelper.Clamp(ModifierLerpSpeed * deltaTime, 0f, 1f));
                return;
            }

            _evaluationTimer += deltaTime;
            if (_evaluationTimer >= EvaluationInterval)
            {
                _evaluationTimer = 0f;
                _state = Evaluate(lives, combo, activeEnemies, activeEnemyBullets);
            }

            AdvanceRewardPunishCycle(deltaTime);
            DifficultyModifiers target = SelectTarget();
            _current = DifficultyModifiers.Lerp(_current, target,
                MathHelper.Clamp(ModifierLerpSpeed * deltaTime, 0f, 1f));
        }

        public void NotifyEnemyDestroyed() => _killHeat = MathHelper.Min(1f, _killHeat + .085f);

        public void NotifyPlayerDamaged()
        {
            _timeSinceHit = 0f;
            _rewardCycle = false;
            _cycleTimer = 0f;
        }

        private FlowState Evaluate(int lives, int combo, int activeEnemies, int activeEnemyBullets)
        {
            // A recent loss or an overloaded screen always wins over a high score.
            if (lives <= System.Math.Max(1, _startingLives - 2) || _timeSinceHit < 3.5f || activeEnemyBullets >= 18)
                return FlowState.Recovering;
            if (_timeSinceHit < 8f || (activeEnemies >= 9 && activeEnemyBullets >= 10))
                return FlowState.Struggling;

            float performance = .42f
                + MathHelper.Clamp((combo - 1) / 7f, 0f, 1f) * .24f
                + MathHelper.Clamp(_timeSinceHit / 28f, 0f, 1f) * .22f
                + _killHeat * .18f;

            if (performance >= .86f) return FlowState.Godlike;
            if (performance >= .67f) return FlowState.Dominating;
            return FlowState.InFlow;
        }

        private void AdvanceRewardPunishCycle(float dt)
        {
            bool isHighPerformance = _state == FlowState.Dominating || _state == FlowState.Godlike;
            if (!isHighPerformance)
            {
                _rewardCycle = false;
                _cycleTimer = 0f;
                _wasHighPerformance = false;
                return;
            }

            // Strong play first receives a visible breath of generosity. The
            // ensuing surge feels like an earned escalation, not a cheap ambush.
            if (!_wasHighPerformance)
            {
                _rewardCycle = true;
                _cycleTimer = 0f;
            }
            _wasHighPerformance = true;

            _cycleTimer += dt;
            float duration = _rewardCycle ? RewardDuration : SurgeDuration;
            if (_cycleTimer < duration) return;

            _cycleTimer = 0f;
            _rewardCycle = !_rewardCycle;
        }

        private DifficultyModifiers SelectTarget()
        {
            if (_state == FlowState.Dominating || _state == FlowState.Godlike)
            {
                if (_rewardCycle) return Reward;
                if (_state == FlowState.Godlike) return HunterSurge;
            }

            return _state switch
            {
                FlowState.Recovering => Recovering,
                FlowState.Struggling => Struggling,
                FlowState.Dominating => Dominating,
                FlowState.Godlike => Godlike,
                _ => Flow
            };
        }
    }
}
