using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceDodger.Core;
using SpaceDodger.Difficulty;
using SpaceDodger.Entities;
using SpaceDodger.Levels;
using SpaceDodger.Movement;

namespace SpaceDodger.Systems
{
    /// <summary>
    /// Drives a level's wave schedule: decides what spawns, when and where.
    /// Knows nothing about rendering, collision or scoring (SRP) — it only
    /// asks the <see cref="EntityFactory"/> for enemies and publishes events.
    /// </summary>
    public sealed class WaveSpawner
    {
        private static readonly string[] EarlyReinforcements = { "drone", "scout" };
        private static readonly string[] MidReinforcements = { "scout", "wasp", "fighter" };
        private static readonly string[] LateReinforcements = { "wasp", "seeker", "lancer" };
        private static readonly string[] EndgameReinforcements = { "seeker", "lancer", "raider", "spinner" };

        private sealed class WaveRuntime
        {
            public WaveData Data;
            public EnemyDefinition Definition;
            public IMovementStrategy Movement;
            public int TargetCount;
            public int Spawned;
            public float NextSpawnTime;
        }

        private readonly EntityFactory _factory;
        private readonly EventBus _events;
        private readonly EnemyWorld _world;
        private readonly IDifficultyDirector _director;
        private readonly Random _random = new Random();
        private readonly List<WaveRuntime> _waves = new List<WaveRuntime>();

        private float _time;
        private float _reinforcementTimer;
        private float _supplyDriftTimer;
        private float _fatDrifterTimer;
        private float _splitterDrifterTimer;
        private float _bossAddTimer;
        private float _bossSupplyTimer;

        public LevelData Level { get; private set; }

        /// <summary>True once every wave has finished spawning.</summary>
        public bool AllWavesSpawned { get; private set; }

        /// <summary>Enemies spawned so far this level (for progress display).</summary>
        public int SpawnedCount { get; private set; }

        public WaveSpawner(EntityFactory factory, EventBus events, EnemyWorld world, IDifficultyDirector director)
        {
            _factory = factory;
            _events = events;
            _world = world;
            _director = director;
        }

        public void Begin(LevelData level)
        {
            Level = level;
            _time = 0f;
            SpawnedCount = 0;
            AllWavesSpawned = false;
            _reinforcementTimer = 4.5f;
            _fatDrifterTimer = 12f + (float)_random.NextDouble() * 8f;
            _splitterDrifterTimer = 18f + (float)_random.NextDouble() * 12f;
            _bossAddTimer = 10f;
            _bossSupplyTimer = GameConfig.BossSupplyCheckInterval;
            _supplyDriftTimer = GameConfig.SupplyDriftMinimumInterval +
                (float)_random.NextDouble() * (GameConfig.SupplyDriftMaximumInterval - GameConfig.SupplyDriftMinimumInterval);
            _waves.Clear();

            foreach (var wave in level.Waves)
            {
                var def = EnemyCatalog.Get(wave.Enemy);
                int targetCount = wave.Count;
                // Shorten tight shooting formations by 1 enemy from the end of the flock so the player has dodging lanes
                if (def.Shoots && targetCount >= 3 && (wave.Formation == WaveFormation.Line || wave.Formation == WaveFormation.Diagonal || wave.Formation == WaveFormation.Column))
                {
                    targetCount = Math.Max(2, targetCount - 1);
                }

                _waves.Add(new WaveRuntime
                {
                    Data = wave,
                    Definition = def,
                    Movement = MovementRegistry.Get(wave.Movement),
                    TargetCount = targetCount,
                    Spawned = 0,
                    NextSpawnTime = wave.StartTime,
                });
            }
        }

        public void Update(float dt)
        {
            _time += dt;

            bool bossActive = IsBossActive();
            bool anyPending = false;

            if (bossActive)
            {
                // Boss encounter is active: block parallel swarm dumps.
                // Minion swarms arrive strictly sequentially every 10 seconds until the boss is destroyed.
                anyPending = true;
                _bossAddTimer -= dt;
                if (_bossAddTimer <= 0f)
                {
                    _bossAddTimer = 10.0f;
                    SpawnNextBossAddWave();
                }
            }
            else
            {
                foreach (var wave in _waves)
                {
                    if (wave.Spawned >= wave.TargetCount)
                        continue;

                    anyPending = true;

                    while (wave.Spawned < wave.TargetCount && _time >= wave.NextSpawnTime)
                    {
                        SpawnAuthoredEnemy(wave);
                        wave.Spawned++;
                        wave.NextSpawnTime += wave.Data.Interval * _director.Current.WaveInterval;

                        // When a boss enters the playfield, immediately engage 10-second sequential add cadence
                        if (wave.Definition.IsBoss)
                        {
                            _bossAddTimer = 10.0f;
                            break;
                        }
                    }

                    if (IsBossActive())
                        break;
                }
            }

            AllWavesSpawned = !anyPending;
            UpdateAdaptiveReinforcements(dt);
            UpdateFatDrifter(dt);
            UpdateSplitterDrifter(dt);
            UpdateSupplyDrift(dt);
            UpdateBossSupplyDrift(dt);
        }

        private bool IsBossActive()
        {
            var items = _factory.Enemies.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Active && items[i].IsBoss)
                    return true;
            }
            return false;
        }

        private void SpawnNextBossAddWave()
        {
            WaveRuntime nextWave = null;
            foreach (var wave in _waves)
            {
                if (!wave.Definition.IsBoss && wave.Spawned < wave.TargetCount)
                {
                    nextWave = wave;
                    break;
                }
            }

            if (nextWave != null)
            {
                while (nextWave.Spawned < nextWave.TargetCount)
                {
                    SpawnAuthoredEnemy(nextWave);
                    nextWave.Spawned++;
                }
            }
            else
            {
                // Authored waves exhausted while boss is still alive: spawn a light 3-ship escort swarm
                var escortDef = EnemyCatalog.Get(Level.Number < 30 ? "fighter" : "wasp");
                var movement = MovementRegistry.Get("sine");
                for (int i = 0; i < 3; i++)
                {
                    var pos = new Vector2(_world.Bounds.Right + 16f + i * 20f, RandomLane());
                    SpawnEnemy(escortDef, movement, pos, 1f, 1f);
                }
            }
        }

        private void SpawnAuthoredEnemy(WaveRuntime wave)
        {
            var position = new Vector2(
                _world.Bounds.Right + 16f,
                ComputeSpawnY(wave));

            SpawnEnemy(wave.Definition, wave.Movement, position,
                wave.Data.HealthMultiplier, wave.Data.SpeedMultiplier);
        }

        private void UpdateAdaptiveReinforcements(float dt)
        {
            // Procedural enemies heighten the middle of an encounter but never
            // keep a completed authored level alive forever.
            if (AllWavesSpawned || Level.Number % GameConfig.BossEvery == 0)
                return;

            _reinforcementTimer -= dt;
            if (_reinforcementTimer > 0f || _factory.Enemies.CountActive >= _director.Current.ProceduralCap)
                return;

            SpawnReinforcement();
            _reinforcementTimer = 2.75f / MathHelper.Max(.25f, _director.Current.ProceduralRate);
        }

        private void SpawnReinforcement()
        {
            int roll = _random.Next(100);
            if (roll < 42)
            {
                // Classic right-to-left interceptor: a familiar pressure beat.
                var movement = _random.Next(2) == 0 ? MovementRegistry.Get("sine") : MovementRegistry.Get("chase");
                SpawnEnemy(ChooseReinforcementDefinition(), movement,
                    new Vector2(_world.Bounds.Right + 16f, RandomLane()), 1f, 1f);
                return;
            }

            if (roll < 72)
            {
                // Dive bombers fall from above and weave horizontally.
                SpawnEnemy(ChooseReinforcementDefinition(), MovementRegistry.Get("top_dive"),
                    new Vector2(RandomDiveX(), _world.Bounds.Top - 16f), .88f, 1.08f);
                return;
            }

            // A compact flock crosses from one upper corner to the opposite lower corner.
            // Shortened by 1 enemy so the flock offers a dodging lane.
            bool fromRight = _random.Next(2) == 0;
            float x = fromRight ? _world.Bounds.Right - 12f : _world.Bounds.Left + 12f;
            for (int i = 0; i < 2 && _factory.Enemies.CountActive < _director.Current.ProceduralCap; i++)
            {
                SpawnEnemy(ChooseReinforcementDefinition(), MovementRegistry.Get("diagonal_flock"),
                    new Vector2(x + (fromRight ? i * 5f : -i * 5f), _world.Bounds.Top - 12f - i * 8f),
                    .78f, 1.16f);
            }
        }

        private void UpdateFatDrifter(float dt)
        {
            if (AllWavesSpawned || Level.Number % GameConfig.BossEvery == 0)
                return;

            _fatDrifterTimer -= dt;
            if (_fatDrifterTimer > 0f)
                return;

            _fatDrifterTimer = 20f + (float)_random.NextDouble() * 12f;
            SpawnProceduralFatDrifter();
        }

        private void SpawnProceduralFatDrifter()
        {
            var def = EnemyCatalog.Get("fat_drifter");
            int minHp = FindMinimumEnemyHealthInLevel(Level);
            int targetHp = minHp * 3;
            float hpMultiplier = (float)targetHp / def.MaxHealth;

            var movement = MovementRegistry.Get("rebound");
            var pos = new Vector2(_world.Bounds.Right + 16f, RandomLane());
            SpawnEnemy(def, movement, pos, hpMultiplier, 1f);
        }

        private void UpdateSplitterDrifter(float dt)
        {
            if (AllWavesSpawned || Level.Number % GameConfig.BossEvery == 0)
                return;

            _splitterDrifterTimer -= dt;
            if (_splitterDrifterTimer > 0f)
                return;

            _splitterDrifterTimer = 22f + (float)_random.NextDouble() * 12f;
            SpawnProceduralSplitter();
        }

        private void SpawnProceduralSplitter()
        {
            var def = EnemyCatalog.Get("splitter");
            var movement = MovementRegistry.Get("straight");
            var pos = new Vector2(_world.Bounds.Right + 16f, RandomLane());
            SpawnEnemy(def, movement, pos, 1f, 1f);
        }

        private int FindMinimumEnemyHealthInLevel(LevelData level)
        {
            int minHp = int.MaxValue;
            if (level?.Waves != null)
            {
                foreach (var w in level.Waves)
                {
                    var def = EnemyCatalog.Get(w.Enemy);
                    int hp = Math.Max(1, (int)Math.Round(def.MaxHealth * w.HealthMultiplier));
                    if (hp < minHp)
                        minHp = hp;
                }
            }
            return minHp == int.MaxValue ? 1 : minHp;
        }

        private void UpdateSupplyDrift(float dt)
        {
            // Supplies enter as isolated right-to-left crates. Bosses and level
            // end cleanup do not receive extra crates, and EntityFactory enforces
            // a global two-crate cap shared with enemy drops.
            if (AllWavesSpawned || Level.Number % GameConfig.BossEvery == 0)
                return;

            _supplyDriftTimer -= dt;
            if (_supplyDriftTimer > 0f)
                return;

            if (_factory.TrySpawnSupplyDrift(new Vector2(_world.Bounds.Right + 10f, RandomLane()),
                _director.HealthSupplyBias))
            {
                float baseInterval = GameConfig.SupplyDriftMinimumInterval +
                    (float)_random.NextDouble() * (GameConfig.SupplyDriftMaximumInterval - GameConfig.SupplyDriftMinimumInterval);
                _supplyDriftTimer = baseInterval / MathHelper.Max(.35f, _director.Current.PowerUpDrop);
            }
            else
            {
                // Recheck soon after a crate is collected instead of creating a backlog.
                _supplyDriftTimer = 1.5f;
            }
        }

        private void UpdateBossSupplyDrift(float dt)
        {
            if (!IsBossActive())
                return;

            _bossSupplyTimer -= dt;
            if (_bossSupplyTimer > 0f)
                return;

            _bossSupplyTimer = GameConfig.BossSupplyCheckInterval;

            if (_random.NextDouble() < GameConfig.BossSupplyChance)
            {
                _factory.TrySpawnBossSupplyDrift(
                    new Vector2(_world.Bounds.Right + 10f, RandomLane()),
                    _director.HealthSupplyBias);
            }
        }

        private EnemyDefinition ChooseReinforcementDefinition()
        {
            string[] choices;
            if (Level.Number < 6) choices = EarlyReinforcements;
            else if (Level.Number < 15) choices = MidReinforcements;
            else if (Level.Number < 30) choices = LateReinforcements;
            else choices = EndgameReinforcements;
            return EnemyCatalog.Get(choices[_random.Next(choices.Length)]);
        }

        private void SpawnEnemy(EnemyDefinition definition, IMovementStrategy movement, Vector2 position,
            float healthMultiplier, float speedMultiplier)
        {
            var modifiers = _director.Current;
            var enemy = _factory.SpawnEnemy(definition, movement, position, _world,
                healthMultiplier * modifiers.EnemyHealth,
                speedMultiplier * modifiers.EnemySpeed,
                modifiers.EnemyFireInterval);

            enemy.Destroyed += OnEnemyDestroyed;
            enemy.WantsToFire += OnEnemyWantsToFire;
            enemy.WantsToSplit += OnEnemyWantsToSplit;
            enemy.Hit += OnEnemyHit;

            SpawnedCount++;
        }

        private void OnEnemyWantsToSplit(Enemy parent)
        {
            if (!parent.Active || parent.SplitGeneration >= 3)
                return;

            int nextGen = parent.SplitGeneration + 1;
            // Strict lane separation ensures dividing enemies occupy separate vertical tracks without overlapping
            float separation = (parent.SplitGeneration == 0) ? 36f : (parent.SplitGeneration == 1 ? 24f : 16f);
            float halfSep = separation / 2f;
            float topLimit = _world.Bounds.Top + 14f;
            float bottomLimit = _world.Bounds.Bottom - 14f;
            float centerY = MathHelper.Clamp(parent.Position.Y, topLimit + halfSep, bottomLimit - halfSep);
            float upperY = centerY - halfSep;
            float lowerY = centerY + halfSep;

            _factory.SpawnSpark(parent.Position);

            var def = parent.Definition;
            var movement = MovementRegistry.Get("straight");
            int childHealth = Math.Max(1, parent.Health / 2);
            float childScale = (nextGen == 1) ? 0.78f : ((nextGen == 2) ? 0.60f : 0.48f);
            float x = parent.Position.X;
            float hpMult = childHealth / (float)def.MaxHealth;

            parent.Deactivate();

            // Spawn upper child in distinct upper lane
            var child1 = _factory.SpawnEnemy(def, movement, new Vector2(x, upperY), _world, hpMult, 1f, 1f);
            child1.Scale = childScale;
            child1.SplitGeneration = nextGen;
            child1.Destroyed += OnEnemyDestroyed;
            child1.WantsToFire += OnEnemyWantsToFire;
            child1.WantsToSplit += OnEnemyWantsToSplit;
            child1.Hit += OnEnemyHit;
            SpawnedCount++;

            // Spawn lower child in distinct lower lane
            var child2 = _factory.SpawnEnemy(def, movement, new Vector2(x, lowerY), _world, hpMult, 1f, 1f);
            child2.Scale = childScale;
            child2.SplitGeneration = nextGen;
            child2.Destroyed += OnEnemyDestroyed;
            child2.WantsToFire += OnEnemyWantsToFire;
            child2.WantsToSplit += OnEnemyWantsToSplit;
            child2.Hit += OnEnemyHit;
            SpawnedCount++;
        }

        private float ComputeSpawnY(WaveRuntime wave)
        {
            var bounds = _world.Bounds;
            const int margin = 16;
            int top = bounds.Top + margin;
            int usable = bounds.Height - margin * 2;
            int index = wave.Spawned;
            int count = Math.Max(1, wave.Data.Count);

            switch (wave.Data.Formation)
            {
                case WaveFormation.Line:
                    // Spread evenly across the playfield height.
                    return top + usable * (index + 0.5f) / count;

                case WaveFormation.Diagonal:
                    // Staircase down, wrapping after 5 steps.
                    return top + usable * ((index % 5) / 5f) + 8f;

                case WaveFormation.Scatter:
                    return top + (float)_random.NextDouble() * usable;

                case WaveFormation.Column:
                    return bounds.Center.Y;

                default:
                    return bounds.Center.Y;
            }
        }

        private float RandomLane()
        {
            const int margin = 16;
            return _world.Bounds.Top + margin + (float)_random.NextDouble() * (_world.Bounds.Height - margin * 2);
        }

        private float RandomDiveX()
        {
            const int margin = 22;
            return _world.Bounds.Left + margin + (float)_random.NextDouble() * (_world.Bounds.Width - margin * 2);
        }

        private void OnEnemyDestroyed(Enemy enemy)
        {
            _factory.SpawnExplosion(enemy.Position, enemy.IsBoss ? 2.5f : 1f);
            if (enemy.IsBoss)
            {
                _factory.SpawnGuaranteedHealthSupply(enemy.Position);
            }
            else if (enemy.Definition.Key == "fat_drifter")
            {
                if (_random.NextDouble() < 0.5)
                {
                    _factory.SpawnGuaranteedHealthSupply(enemy.Position);
                }
                else
                {
                    _factory.MaybeDropPowerUp(enemy.Position,
                        chanceMultiplier: 1f,
                        healthSupplyBias: _director.HealthSupplyBias);
                }
            }
            else
            {
                // Standard enemies have a flat 2% supply chance.
                // DDA changes the composition toward health when help is needed,
                // not the stated base chance itself.
                _factory.MaybeDropPowerUp(enemy.Position,
                    chanceMultiplier: 1f,
                    healthSupplyBias: _director.HealthSupplyBias);
            }
            _director.NotifyEnemyDestroyed();

            _events.Publish(new EnemyDestroyedEvent(
                enemy.Definition.Score, enemy.Position, enemy.IsBoss));
        }

        private void OnEnemyWantsToFire(Enemy enemy)
        {
            _factory.SpawnEnemyShot(enemy, _world.PlayerPosition);
        }

        private void OnEnemyHit(Enemy enemy)
        {
            // Impact spark on the enemy's leading edge so damage reads clearly.
            _factory.SpawnSpark(enemy.MuzzlePosition);
        }

        /// <summary>True when the level is finished: all spawned and none left alive.</summary>
        public bool IsCleared => AllWavesSpawned && _factory.Enemies.CountActive == 0;
    }
}
