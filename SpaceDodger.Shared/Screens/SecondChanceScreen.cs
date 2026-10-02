using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Core;
using SpaceDodger.Input;

namespace SpaceDodger.Screens
{
    /// <summary>
    /// Mobile-only second chance overlay.
    /// Freezes the gameplay screen behind it and allows the player to watch
    /// a rewarded video ad to revive with 2 lives and continue their current run.
    /// </summary>
    public sealed class SecondChanceScreen : Screen
    {
        private readonly Action _onRevive;
        private readonly Action _onGiveUp;
        private MenuList _menu;
        private bool _isAdShowing;
        private bool _rewardEarned;
        private bool _adClosed;
        private string _statusMessage = "";

        public override bool IsOverlay => true;

        public SecondChanceScreen(GameContext context, Action onRevive, Action onGiveUp)
            : base(context)
        {
            _onRevive = onRevive;
            _onGiveUp = onGiveUp;
        }

        public override void Load()
        {
            // Ensure rewarded ad is preloaded or loading
            Context.Platform.LoadRewardedAd();

            Analytics.AnalyticsManager.LogSecondChanceOffered(Context.Save.Data.ResumeLevel, 0);

            _menu = new MenuList(Context.Font, Context.Screen.Width / 2f, 96f, spacing: 18)
                .Add("WATCH AD (+2 LIVES)", WatchAd)
                .Add("GIVE UP", GiveUp);
        }

        private void WatchAd()
        {
            if (_isAdShowing) return;

            _isAdShowing = true;
            _statusMessage = "LOADING AD...";

            Context.Platform.ShowRewardedAd(
                onRewardEarned: () =>
                {
                    _rewardEarned = true;
                },
                onClosed: () =>
                {
                    _adClosed = true;
                });
        }

        private void GiveUp()
        {
            Analytics.AnalyticsManager.LogSecondChanceUsed(Context.Save.Data.ResumeLevel, false);
            _onGiveUp?.Invoke();
        }

        public override void Update(float dt, in InputState input)
        {
            if (_adClosed)
            {
                _adClosed = false;
                _isAdShowing = false;

                if (_rewardEarned)
                {
                    Analytics.AnalyticsManager.LogSecondChanceUsed(Context.Save.Data.ResumeLevel, true);
                    _onRevive?.Invoke();
                    return;
                }
                else
                {
                    _statusMessage = "AD NOT COMPLETED";
                }
            }

            if (_isAdShowing)
                return;

            _menu.Update(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Dim frozen gameplay screen behind the overlay
            spriteBatch.Draw(
                Context.Textures.Pixel, Context.Screen.Bounds, new Color(0, 0, 0, 195));

            float cx = Context.Screen.Width / 2f;

            Context.Font.DrawCentered(
                spriteBatch, "SECOND CHANCE", cx, 34, new Color(255, 220, 60), 2f);

            Context.Font.DrawCentered(
                spriteBatch, "CONTINUE MISSION WITH 2 LIVES?", cx, 62, Color.White);

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                var statusColor = _statusMessage == "LOADING AD..."
                    ? new Color(100, 200, 255)
                    : new Color(240, 80, 80);

                Context.Font.DrawCentered(spriteBatch, _statusMessage, cx, 78, statusColor);
            }

            _menu.Draw(spriteBatch);
        }
    }
}
