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
            _supplyDriftTimer = GameConfig.SupplyDriftMinimumInterval +
                (float)_random.NextDouble() * (GameConfig.SupplyDriftMaximumInterval - GameConfig.SupplyDriftMinimumInterval);
            _waves.Clear();

            foreach (var wave in level.Waves)
            {
                _waves.Add(new WaveRuntime
                {
                    Data = wave,
                    Definition = EnemyCatalog.Get(wave.Enemy),
                    Movement = MovementRegistry.Get(wave.Movement),
                    Spawned = 0,
                    NextSpawnTime = wave.StartTime,
                });
            }
        }

        public void Update(float dt)
        {
            _time += dt;

            bool anyPending = false;

            foreach (var wave in _waves)
            {
                if (wave.Spawned >= wave.Data.Count)
                    continue;

                anyPending = true;

                while (wave.Spawned < wave.Data.Count && _time >= wave.NextSpawnTime)
                {
                    SpawnAuthoredEnemy(wave);
                    wave.Spawned++;
                    wave.NextSpawnTime += wave.Data.Interval * _director.Current.WaveInterval;
                }
            }

            AllWavesSpawned = !anyPending;
            UpdateAdaptiveReinforcements(dt);
            UpdateSupplyDrift(dt);
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
            bool fromRight = _random.Next(2) == 0;
            float x = fromRight ? _world.Bounds.Right - 12f : _world.Bounds.Left + 12f;
            for (int i = 0; i < 3 && _factory.Enemies.CountActive < _director.Current.ProceduralCap; i++)
            {
                SpawnEnemy(ChooseReinforcementDefinition(), MovementRegistry.Get("diagonal_flock"),
                    new Vector2(x + (fromRight ? i * 5f : -i * 5f), _world.Bounds.Top - 12f - i * 8f),
                    .78f, 1.16f);
            }
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
            enemy.Hit += OnEnemyHit;

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
            _factory.MaybeDropPowerUp(enemy.Position, guaranteed: enemy.IsBoss,
                chanceMultiplier: _director.Current.PowerUpDrop,
                healthSupplyBias: _director.HealthSupplyBias);
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
