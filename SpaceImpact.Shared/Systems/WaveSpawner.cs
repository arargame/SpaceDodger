using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceImpact.Core;
using SpaceImpact.Entities;
using SpaceImpact.Levels;
using SpaceImpact.Movement;

namespace SpaceImpact.Systems
{
    /// <summary>
    /// Drives a level's wave schedule: decides what spawns, when and where.
    /// Knows nothing about rendering, collision or scoring (SRP) — it only
    /// asks the <see cref="EntityFactory"/> for enemies and publishes events.
    /// </summary>
    public sealed class WaveSpawner
    {
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
        private readonly Random _random = new Random();
        private readonly List<WaveRuntime> _waves = new List<WaveRuntime>();

        private float _time;

        public LevelData Level { get; private set; }

        /// <summary>True once every wave has finished spawning.</summary>
        public bool AllWavesSpawned { get; private set; }

        /// <summary>Enemies spawned so far this level (for progress display).</summary>
        public int SpawnedCount { get; private set; }

        public WaveSpawner(EntityFactory factory, EventBus events, EnemyWorld world)
        {
            _factory = factory;
            _events = events;
            _world = world;
        }

        public void Begin(LevelData level)
        {
            Level = level;
            _time = 0f;
            SpawnedCount = 0;
            AllWavesSpawned = false;
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
                    SpawnOne(wave);
                    wave.Spawned++;
                    wave.NextSpawnTime += wave.Data.Interval;
                }
            }

            AllWavesSpawned = !anyPending;
        }

        private void SpawnOne(WaveRuntime wave)
        {
            var position = new Vector2(
                _world.Bounds.Right + 16f,
                ComputeSpawnY(wave));

            var enemy = _factory.SpawnEnemy(
                wave.Definition, wave.Movement, position, _world,
                wave.Data.HealthMultiplier, wave.Data.SpeedMultiplier);

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

        private void OnEnemyDestroyed(Enemy enemy)
        {
            _factory.SpawnExplosion(enemy.Position, enemy.IsBoss ? 2.5f : 1f);
            _factory.MaybeDropPowerUp(enemy.Position, guaranteed: enemy.IsBoss);

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
