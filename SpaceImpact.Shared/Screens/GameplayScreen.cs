using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Core;
using SpaceImpact.Entities;
using SpaceImpact.Graphics;
using SpaceImpact.Input;
using SpaceImpact.Levels;
using SpaceImpact.Systems;

namespace SpaceImpact.Screens
{
    /// <summary>
    /// The playable screen. Composes the gameplay systems and owns the
    /// level flow (playing -> cleared -> next level / game over).
    /// </summary>
    public sealed class GameplayScreen : Screen
    {
        private enum Phase { Intro, Playing, Cleared, GameOver }

        private const float IntroDuration = 1.6f;
        private const float ClearedDuration = 2.2f;
        private const float GameOverDelay = 1.8f;

        private readonly ILevelRepository _levels;
        private readonly int _startLevel;

        private AnimationLibrary _animations;
        private EntityFactory _factory;
        private WaveSpawner _spawner;
        private ScoreTracker _score;
        private EnemyWorld _world;
        private Starfield _stars;
        private Hud _hud;
        private Player _player;

        private Phase _phase = Phase.Intro;
        private float _phaseTimer;
        private int _levelNumber;
        private float _elapsed;
        private float _bombFlash;

        public GameplayScreen(GameContext context, ILevelRepository levels, int startLevel)
            : base(context)
        {
            _levels = levels;
            _startLevel = startLevel;
        }

        public override void Load()
        {
            var bounds = Context.Screen.Bounds;
            // Reserve the top strip for the HUD.
            var playfield = new Rectangle(0, 12, bounds.Width, bounds.Height - 12);

            _animations = new AnimationLibrary(Context.Textures);
            _world = new EnemyWorld { Bounds = playfield };
            _factory = new EntityFactory(_animations, Context.Textures, playfield);
            _score = new ScoreTracker(Context.Events);
            _spawner = new WaveSpawner(_factory, Context.Events, _world);
            _stars = new Starfield(Context.Textures.Pixel, bounds.Width, bounds.Height);
            _hud = new Hud(Context.Font, Context.Textures.Pixel, bounds);

            _player = new Player(_animations.Player, playfield);
            _player.Fired += OnPlayerFired;
            _player.Damaged += OnPlayerDamaged;
            _player.Died += OnPlayerDied;
            _player.Reset(
                new Vector2(playfield.Left + 40, playfield.Center.Y),
                GameConfig.PlayerLives);

            _score.Reset();
            StartLevel(_startLevel);
        }

        public override void Unload()
        {
            _score.Detach();
            _player.Fired -= OnPlayerFired;
            _player.Damaged -= OnPlayerDamaged;
            _player.Died -= OnPlayerDied;
        }

        private void StartLevel(int number)
        {
            _levelNumber = number;
            _factory.ReleaseAll();
            _spawner.Begin(_levels.Load(number));
            _phase = Phase.Intro;
            _phaseTimer = IntroDuration;
            _elapsed = 0f;

            // Difficulty flavour: later levels scroll faster.
            _stars.SpeedMultiplier = 1f + (number - 1) * 0.12f;
        }

        public override void Update(float dt, in InputState input)
        {
            _elapsed += dt;
            _stars.Update(dt);
            _factory.UpdateAll(dt);
            _score.Update(dt);

            if (_bombFlash > 0f)
                _bombFlash -= dt;

            if (input.PausePressed && _phase == Phase.Playing)
            {
                Context.Screens.Push(new PauseScreen(Context));
                return;
            }

            _world.PlayerPosition = _player.Position;

            switch (_phase)
            {
                case Phase.Intro:
                    _player.Update(dt, input);
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                        _phase = Phase.Playing;
                    break;

                case Phase.Playing:
                    UpdatePlaying(dt, input);
                    break;

                case Phase.Cleared:
                    _player.Update(dt, input);
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                        AdvanceLevel();
                    break;

                case Phase.GameOver:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                        ShowGameOver();
                    break;
            }
        }

        private void UpdatePlaying(float dt, in InputState input)
        {
            _player.Update(dt, input);
            _spawner.Update(dt);

            ResolveCollisions();

            if (_spawner.IsCleared)
            {
                _phase = Phase.Cleared;
                _phaseTimer = ClearedDuration;
                UnlockNextLevel();
            }
        }

        private void ResolveCollisions()
        {
            // Player bullets vs enemies.
            CollisionSystem.Resolve(_factory.PlayerBullets.Items, _factory.Enemies.Items);

            if (!_player.Active)
                return;

            // Enemy bullets and enemy bodies vs the player.
            CollisionSystem.Resolve(_player, _factory.EnemyBullets.Items);

            if (_player.Active)
                CollisionSystem.Resolve(_player, _factory.Enemies.Items);

            if (_player.Active)
                CollectPowerUps();
        }

        private void CollectPowerUps()
        {
            var items = _factory.PowerUps.Items;
            var playerBounds = _player.Bounds;

            for (int i = 0; i < items.Count; i++)
            {
                var powerUp = items[i];
                if (!powerUp.Active || !playerBounds.Intersects(powerUp.Bounds))
                    continue;

                Apply(powerUp.Type);
                Context.Events.Publish(new PowerUpCollectedEvent(powerUp.Type));
                powerUp.Deactivate();
            }
        }

        private void Apply(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Health:
                    _player.AddLife();
                    break;

                case PowerUpType.Weapon:
                    _player.UpgradeWeapon();
                    break;

                case PowerUpType.Shield:
                    _player.GrantShield();
                    break;

                case PowerUpType.Rapid:
                    _player.GrantRapidFire();
                    break;

                case PowerUpType.Score:
                    _score.AddBonus(GameConfig.ScorePickupValue);
                    break;

                case PowerUpType.Bomb:
                    // Screen clear: every kill still scores and explodes.
                    _factory.DetonateBomb(enemy =>
                        _factory.SpawnExplosion(enemy.Position));
                    _bombFlash = 0.35f;
                    break;
            }
        }

        private void AdvanceLevel()
        {
            if (_levelNumber >= _levels.Count)
            {
                ShowVictory();
                return;
            }

            StartLevel(_levelNumber + 1);
        }

        private void UnlockNextLevel()
        {
            int next = Math.Min(_levelNumber + 1, _levels.Count);
            if (next > Context.Save.Data.MaxUnlockedLevel)
            {
                Context.Save.Data.MaxUnlockedLevel = next;
                Context.Save.Save();
            }
        }

        private void ShowGameOver() =>
            Context.Screens.Replace(new GameOverScreen(
                Context, _levels, _score.Score, _levelNumber, victory: false));

        private void ShowVictory() =>
            Context.Screens.Replace(new GameOverScreen(
                Context, _levels, _score.Score, _levelNumber, victory: true));

        // --- entity event handlers ---------------------------------------

        private void OnPlayerFired(Player player) =>
            _factory.SpawnPlayerShot(player.MuzzlePosition, player.WeaponLevel);

        private void OnPlayerDamaged(Player player)
        {
            _factory.SpawnExplosion(player.Position);
            _score.BreakCombo();
            Context.Events.Publish(new PlayerDamagedEvent(player.Lives, player.Position));
        }

        private void OnPlayerDied(Player player)
        {
            _factory.SpawnExplosion(player.Position, 2f);
            _phase = Phase.GameOver;
            _phaseTimer = GameOverDelay;
        }

        // --- drawing ------------------------------------------------------

        public override void Draw(SpriteBatch spriteBatch)
        {
            _stars.Draw(spriteBatch);
            _factory.DrawAll(spriteBatch);

            if (_player.Active)
            {
                _player.Draw(spriteBatch);
                if (_player.IsShielded)
                    DrawShield(spriteBatch);
            }

            if (_bombFlash > 0f)
            {
                spriteBatch.Draw(
                    Context.Textures.Pixel, Context.Screen.Bounds,
                    Color.White * (_bombFlash / 0.35f * 0.6f));
            }

            _hud.Draw(spriteBatch, _score, _player, _levelNumber, FindBoss());

            switch (_phase)
            {
                case Phase.Intro:
                    DrawBanner(spriteBatch, _spawner.Level.Name, $"LEVEL {_levelNumber}");
                    break;

                case Phase.Cleared:
                    DrawBanner(spriteBatch, "LEVEL CLEAR", $"SCORE {_score.Score}");
                    break;
            }

            if (Context.Platform.IsMobile && _elapsed < 4f && _phase != Phase.GameOver)
                _hud.DrawTouchHint(spriteBatch, MathHelper.Clamp(4f - _elapsed, 0f, 1f));
        }

        private void DrawShield(SpriteBatch spriteBatch)
        {
            var shield = _animations.Shield;
            int frame = (int)(_elapsed * shield.FramesPerSecond) % shield.FrameCount;
            var origin = new Vector2(shield.FrameWidth / 2f, shield.FrameHeight / 2f);

            // Fade out over the final second so its expiry is readable.
            float alpha = MathHelper.Clamp(_player.ShieldTimer, 0f, 1f);

            spriteBatch.Draw(
                shield.Texture, _player.Position, shield.FrameRect(frame),
                Color.White * (0.5f + 0.5f * alpha), 0f, origin, 1f, SpriteEffects.None, 0f);
        }

        private Enemy FindBoss()
        {
            var items = _factory.Enemies.Items;
            for (int i = 0; i < items.Count; i++)
                if (items[i].Active && items[i].IsBoss)
                    return items[i];
            return null;
        }

        private void DrawBanner(SpriteBatch spriteBatch, string title, string subtitle)
        {
            float cx = Context.Screen.Width / 2f;
            float cy = Context.Screen.Height / 2f;

            Context.Font.DrawCentered(spriteBatch, title, cx, cy - 14, Color.White, 2f);
            Context.Font.DrawCentered(spriteBatch, subtitle, cx, cy + 10, new Color(150, 160, 190));
        }
    }
}
