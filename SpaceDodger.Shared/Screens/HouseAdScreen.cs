using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Core;
using SpaceDodger.Input;
using ArarGames.Core.Applications;

namespace SpaceDodger.Screens
{
    /// <summary>
    /// Interactive cross-promotion intermission screen displayed every 5 levels.
    /// Features first-party titles (Blocked / Paint Trek) with a 5-second countdown.
    /// </summary>
    public sealed class HouseAdScreen : Screen
    {
        private const float AdDuration = 5.0f;
        private const int TotalSquares = 5;

        private readonly Action _onFinished;
        private readonly bool _isBlocked;

        private Texture2D _poster;
        private Texture2D _iconAndroid;
        private float _timer;
        private float _pulse;

        private readonly Rectangle _cardRect = new Rectangle(24, 16, 272, 142);
        private readonly Rectangle _posterRect = new Rectangle(36, 32, 56, 56);
        private readonly Rectangle _storeBtnRect = new Rectangle(104, 76, 54, 22);
        private readonly Rectangle _continueBtnRect = new Rectangle(226, 126, 60, 24);

        public HouseAdScreen(GameContext context, Action onFinished)
            : base(context)
        {
            _onFinished = onFinished;
            // 50% Blocked, 50% Paint Trek
            _isBlocked = new Random().Next(2) == 0;
        }

        public override void Load()
        {
            _poster = Context.Textures.Get(_isBlocked ? "ui/blocked" : "ui/paint_trek");
            _iconAndroid = Context.Textures.Get("ui/market_icons_android");
        }

        public override void Update(float dt, in InputState input)
        {
            _timer += dt;
            _pulse += dt * 3f;

            // Grace period: ignore first 0.2s clicks to prevent accidental touch carry-over
            if (_timer < 0.2f)
                return;

            bool mayContinue = _timer >= AdDuration;

            if (input.Tap.HasValue)
            {
                var tap = input.Tap.Value;
                int tx = (int)tap.X;
                int ty = (int)tap.Y;

                // Continue / Skip button
                if (mayContinue && _continueBtnRect.Contains(tx, ty))
                {
                    Continue();
                    return;
                }

                // Promo Card or Store Button clicked
                if (_cardRect.Contains(tx, ty))
                {
                    OpenStoreUrl();
                    return;
                }
            }

            if (mayContinue && (input.ConfirmPressed || input.Fire))
            {
                Continue();
            }
        }

        private void Continue()
        {
            _onFinished?.Invoke();
        }

        private void OpenStoreUrl()
        {
            string url = _isBlocked
                ? ArarGamesApplications.BlockedGooglePlay
                : ArarGamesApplications.PaintTrekGooglePlay;
            Context.Platform.OpenUrl(url);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Background dim
            spriteBatch.Draw(Context.Textures.Pixel, Context.Screen.Bounds, new Color(8, 12, 24));

            float shimmer = (float)(Math.Sin(_pulse) * 0.5 + 0.5);

            // Promo Card Body
            spriteBatch.Draw(Context.Textures.Pixel, _cardRect, new Color(16, 22, 40));
            DrawBorder(spriteBatch, _cardRect, 2, Color.Lerp(new Color(60, 120, 220), Color.White, shimmer * 0.35f));

            // Header Title
            string adBadge = "RECOMMENDED GAME";
            Context.Font.DrawCentered(spriteBatch, adBadge, Context.Screen.Width / 2f, 22, new Color(255, 220, 60));

            // Game Title
            string gameTitle = _isBlocked ? "BLOCKED: PIXEL PANZER" : "PAINT TREK";
            Context.Font.Draw(spriteBatch, gameTitle, new Vector2(104, 38), Color.White);

            // Game Subtitle / Description
            string desc = _isBlocked
                ? "Retro Tank Combat & Action!"
                : "Classic Space Arcade Adventure!";
            Context.Font.Draw(spriteBatch, desc, new Vector2(104, 52), new Color(160, 180, 210));

            // Tap hint at bottom of card
            string tapHint = "TAP CARD TO GET ON GOOGLE PLAY";
            Context.Font.DrawCentered(
                spriteBatch, tapHint, _cardRect.Center.X, _cardRect.Bottom - 16,
                Color.Lerp(new Color(255, 210, 80), Color.White, shimmer * 0.5f));

            // Countdown squares or Continue button
            if (_timer < AdDuration)
            {
                DrawCountdown(spriteBatch);
            }
            else
            {
                // Continue Button
                spriteBatch.Draw(Context.Textures.Pixel, _continueBtnRect, new Color(40, 160, 60));
                DrawBorder(spriteBatch, _continueBtnRect, 1, Color.White);
                Context.Font.DrawCentered(
                    spriteBatch, "PLAY >", _continueBtnRect.Center.X, _continueBtnRect.Center.Y - 4, Color.White);
            }
        }

        private void DrawCountdown(SpriteBatch spriteBatch)
        {
            const int size = 6;
            const int gap = 4;
            int totalW = TotalSquares * size + (TotalSquares - 1) * gap;
            int startX = _continueBtnRect.Right - totalW;
            int y = _continueBtnRect.Center.Y - size / 2;

            float remaining = Math.Max(0f, AdDuration - _timer);
            int lit = (int)Math.Ceiling(remaining);

            for (int i = 0; i < TotalSquares; i++)
            {
                var r = new Rectangle(startX + i * (size + gap), y, size, size);
                spriteBatch.Draw(Context.Textures.Pixel, r, Color.White * 0.2f);
                if (i < lit)
                    spriteBatch.Draw(Context.Textures.Pixel, r, Color.White * 0.9f);
            }
        }

        /// <summary>Draw high-res posters and market icons at physical device resolution.</summary>
        public override void DrawHighRes(SpriteBatch spriteBatch, Graphics.VirtualScreen screen)
        {
            if (_poster != null)
                spriteBatch.Draw(_poster, screen.ToPhysical(_posterRect), Color.White);

            if (_iconAndroid != null)
                spriteBatch.Draw(_iconAndroid, screen.ToPhysical(_storeBtnRect), Color.White);
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle r, int thickness, Color color)
        {
            spriteBatch.Draw(Context.Textures.Pixel, new Rectangle(r.X, r.Y, r.Width, thickness), color);
            spriteBatch.Draw(Context.Textures.Pixel, new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness), color);
            spriteBatch.Draw(Context.Textures.Pixel, new Rectangle(r.X, r.Y, thickness, r.Height), color);
            spriteBatch.Draw(Context.Textures.Pixel, new Rectangle(r.Right - thickness, r.Y, thickness, r.Height), color);
        }
    }
}
