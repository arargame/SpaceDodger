using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDodger.Entities
{
    /// <summary>
    /// Chaining electric bolt that leaps between live visible enemies until
    /// no enemies remain or the bounce quota is exhausted.
    /// </summary>
    public sealed class ChainLightning : Entity, IPlayerProjectile
    {
        private const float JumpInterval = 0.065f;

        private readonly List<Vector2> _boltPoints = new List<Vector2>(16);
        private readonly List<Enemy> _recentTargets = new List<Enemy>(8);
        private readonly Random _random = new Random();

        private Texture2D _pixel;
        private Rectangle _world;
        private IReadOnlyList<Enemy> _allEnemies;
        private Player _player;
        private Action<Vector2> _onSpark;
        private Enemy _currentTarget;
        private Vector2 _sourcePos;
        private float _jumpTimer;
        private float _boltLife;

        public int Damage { get; private set; }

        public override Rectangle Bounds =>
            _currentTarget != null ? _currentTarget.Bounds : Rectangle.Empty;

        public void Configure(
            Texture2D pixel,
            Player player,
            Rectangle world,
            IReadOnlyList<Enemy> allEnemies,
            int damage,
            Action<Vector2> onSpark)
        {
            _pixel = pixel;
            _player = player;
            _world = world;
            _allEnemies = allEnemies;
            Damage = damage;
            _onSpark = onSpark;
            _recentTargets.Clear();
            _sourcePos = player.MuzzlePosition;
            _jumpTimer = 0f;
            _boltLife = 0f;

            // Immediately target the nearest visible enemy
            _currentTarget = FindBestNextTarget(_sourcePos, null);
            if (_currentTarget != null)
            {
                ApplyHitToCurrent();
            }
            else
            {
                Deactivate();
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            _boltLife += dt;
            _jumpTimer += dt;

            if (_currentTarget == null || !_player.Active || _player.SpecialCharges <= 0)
            {
                Deactivate();
                return;
            }

            if (_jumpTimer >= JumpInterval)
            {
                _jumpTimer = 0f;

                // Move source to current target before picking next
                _sourcePos = _currentTarget.Position;

                var nextTarget = FindBestNextTarget(_sourcePos, _currentTarget);
                if (nextTarget == null)
                {
                    // No visible live enemies left on screen
                    Deactivate();
                    return;
                }

                _currentTarget = nextTarget;
                ApplyHitToCurrent();
            }
        }

        private void ApplyHitToCurrent()
        {
            if (_currentTarget == null || !_currentTarget.Active)
                return;

            _currentTarget.TakeDamage(Damage);
            _onSpark?.Invoke(_currentTarget.Position);
            _player.DecrementSpecialCharge();

            _recentTargets.Add(_currentTarget);
            if (_recentTargets.Count > 4)
                _recentTargets.RemoveAt(0);

            GenerateBoltPath(_sourcePos, _currentTarget.Position);

            if (_player.SpecialCharges <= 0)
            {
                Deactivate();
            }
        }

        private Enemy FindBestNextTarget(Vector2 fromPosition, Enemy exclude)
        {
            Enemy best = null;
            float bestDistSq = float.MaxValue;

            for (int i = 0; i < _allEnemies.Count; i++)
            {
                var enemy = _allEnemies[i];
                if (!enemy.Active)
                    continue;

                // Must be on-screen
                if (enemy.Position.X < _world.Left || enemy.Position.X > _world.Right + 8)
                    continue;

                // Prioritize enemies not recently struck
                bool recentlyStruck = _recentTargets.Contains(enemy);
                if (enemy == exclude && _allEnemies.Count > 1)
                    continue;

                float distSq = Vector2.DistanceSquared(fromPosition, enemy.Position);
                if (recentlyStruck)
                    distSq += 15000f; // Soft penalty so fresh targets get picked first

                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = enemy;
                }
            }

            // Fallback: if all active enemies were in the penalty box, retarget any live enemy
            if (best == null && exclude != null && exclude.Active)
            {
                best = exclude;
            }

            return best;
        }

        private void GenerateBoltPath(Vector2 from, Vector2 to)
        {
            _boltPoints.Clear();
            _boltPoints.Add(from);

            var diff = to - from;
            float dist = diff.Length();
            if (dist < 4f)
            {
                _boltPoints.Add(to);
                return;
            }

            var normal = new Vector2(-diff.Y, diff.X) / dist;
            int segments = MathHelper.Clamp((int)(dist / 12f), 2, 7);

            for (int i = 1; i < segments; i++)
            {
                float t = i / (float)segments;
                float jitter = ((float)_random.NextDouble() * 2f - 1f) * 6f;
                _boltPoints.Add(from + diff * t + normal * jitter);
            }

            _boltPoints.Add(to);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_boltPoints.Count < 2)
                return;

            var cyan = new Color(130, 240, 255);
            var white = Color.White;

            for (int i = 0; i < _boltPoints.Count - 1; i++)
            {
                DrawSegment(spriteBatch, _boltPoints[i], _boltPoints[i + 1], cyan, 2);
                DrawSegment(spriteBatch, _boltPoints[i], _boltPoints[i + 1], white, 1);
            }
        }

        private void DrawSegment(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Color color, int thickness)
        {
            var diff = p2 - p1;
            float length = diff.Length();
            if (length < 0.5f) return;

            float angle = (float)Math.Atan2(diff.Y, diff.X);
            spriteBatch.Draw(
                _pixel,
                p1,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(length, thickness),
                SpriteEffects.None,
                0f);
        }

        public override void OnRelease()
        {
            base.OnRelease();
            _boltPoints.Clear();
            _recentTargets.Clear();
            _currentTarget = null;
            _allEnemies = null;
            _player = null;
            _onSpark = null;
        }
    }
}
