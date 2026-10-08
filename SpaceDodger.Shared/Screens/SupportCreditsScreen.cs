using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Input;
using ArarGames.Core.Applications;

namespace SpaceDodger.Screens
{
    public sealed class SupportCreditsScreen : Screen
    {
        private MenuList _menu;
        private Texture2D _paintTrek;
        private Texture2D _blocked;
        private Texture2D _iconAndroid;
        private Texture2D _iconMsStore;
        
        private readonly Rectangle _blockedRect = new Rectangle(36, 42, 48, 48);
        private readonly Rectangle _paintRect = new Rectangle(186, 42, 48, 48);

        // Clickable market areas
        private readonly Rectangle _blockedAndroidBtn = new Rectangle(90, 44, 44, 20);
        private readonly Rectangle _blockedMsStoreBtn = new Rectangle(90, 68, 44, 20);
        
        private readonly Rectangle _paintAndroidBtn = new Rectangle(240, 44, 44, 20);
        private readonly Rectangle _paintMsStoreBtn = new Rectangle(240, 68, 44, 20);

        // Rewarded ad bonus lives state
        private bool _isAdLoading;
        private bool _rewardEarned;
        private bool _adClosed;
        private string _statusMessage = string.Empty;
        private float _statusTimer;

        // Floating gold text animation
        private string _floatingText = string.Empty;
        private float _floatingTextTimer;
        private float _floatingTextY;

        public SupportCreditsScreen(Core.GameContext context) : base(context) { }

        public override void Load()
        {
            _paintTrek = Context.Textures.Get("ui/paint_trek");
            _blocked = Context.Textures.Get("ui/blocked");
            _iconAndroid = Context.Textures.Get("ui/market_icons_android");
            _iconMsStore = Context.Textures.Get("ui/market_icons_microsoftstore");

            if (Context.Platform.IsMobile)
            {
                Context.Platform.LoadRewardedAd();
            }

            BuildMenu();
        }

        private void BuildMenu()
        {
            int prevIndex = _menu?.SelectedIndex ?? 0;
            if (Context.Platform.IsMobile)
            {
                string removeAdsLabel = Context.Save.Data.AdsRemoved ? "ADS REMOVED (ACTIVE)" : "REMOVE ADS";
                
                string adBonusLabel;
                if (_isAdLoading)
                {
                    adBonusLabel = "LOADING AD...";
                }
                else if (Context.Save.Data.BonusStartingLives > 0)
                {
                    adBonusLabel = $"WATCH AD (+2 STARTING LIFE) [+{Context.Save.Data.BonusStartingLives}]";
                }
                else
                {
                    adBonusLabel = "WATCH AD (+2 STARTING LIFE)";
                }

                _menu = new MenuList(Context.Font, Context.Screen.Width / 2f, 108f, spacing: 12)
                    .Add(adBonusLabel, WatchAdForBonusLives, enabled: !_isAdLoading)
                    .Add("BUY ME A COFFEE", BuyCoffee)
                    .Add(removeAdsLabel, BuyRemoveAds)
                    .Add("RESTORE PURCHASES", RestorePurchases)
                    .Add("BACK", () => Context.Screens.Pop());
            }
            else
            {
                // Desktop: no mobile In-App Purchases or Ad removal needed.
                _menu = new MenuList(Context.Font, Context.Screen.Width / 2f, 136f)
                    .Add("BACK", () => Context.Screens.Pop());
            }
            _menu.SetSelection(prevIndex);
        }

        private void WatchAdForBonusLives()
        {
            if (_isAdLoading) return;

            _isAdLoading = true;
            _rewardEarned = false;
            _adClosed = false;
            _statusMessage = "LOADING AD...";
            _statusTimer = 5.0f;
            BuildMenu();

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

        private void BuyCoffee() => Context.Platform.PurchaseConsumable(ArarGamesApplications.CoffeeProductId);

        private void BuyRemoveAds()
        {
            if (Context.Save.Data.AdsRemoved) return;
            Context.Platform.PurchaseNonConsumable(ArarGamesApplications.RemoveAdsProductId);
        }

        private void RestorePurchases() => Context.Platform.RestorePurchases();

        public override void Update(float dt, in InputState input)
        {
            if (_adClosed)
            {
                _adClosed = false;
                _isAdLoading = false;

                if (_rewardEarned)
                {
                    _rewardEarned = false;
                    Context.Save.Data.BonusStartingLives += 2;
                    Context.Save.Data.ResumeLives += 2;
                    Context.Save.Save();

                    _floatingText = "+2 LIFE GRANTED";
                    _floatingTextTimer = 2.8f;
                    _floatingTextY = 104f;
                    _statusMessage = string.Empty;

                    try { Context.Audio.Play("pickup", 0.4f); } catch { }
                }
                else
                {
                    _statusMessage = "AD NOT COMPLETED";
                    _statusTimer = 2.5f;
                }

                BuildMenu();
            }

            if (_floatingTextTimer > 0f)
            {
                _floatingTextTimer -= dt;
                _floatingTextY -= dt * 18f; // floats upwards smoothly
            }

            if (_statusTimer > 0f)
            {
                _statusTimer -= dt;
                if (_statusTimer <= 0f)
                    _statusMessage = string.Empty;
            }

            if (input.BackPressed) { Context.Screens.Pop(); return; }
            if (input.Tap.HasValue)
            {
                var p = input.Tap.Value;
                // Oyun logolarına tıklandığında da ana mağazaya (Android) gitsin
                if (_blockedRect.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.BlockedGooglePlay); return; }
                if (_paintRect.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.PaintTrekGooglePlay); return; }
                
                // Market icon butonları
                if (_blockedAndroidBtn.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.BlockedGooglePlay); return; }
                if (_blockedMsStoreBtn.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.BlockedMicrosoftStore); return; }
                if (_paintAndroidBtn.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.PaintTrekGooglePlay); return; }
                if (_paintMsStoreBtn.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.PaintTrekMicrosoftStore); return; }
            }
            _menu.Update(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float cx = Context.Screen.Width / 2f;
            Context.Font.DrawCentered(spriteBatch, "ARAR GAMES", cx, 12, new Color(255, 220, 60), 2f);
            Context.Font.DrawCentered(spriteBatch, "APPLICATIONS", cx, 32, Color.White);
            
            Context.Font.DrawCentered(spriteBatch, "BLOCKED", 60, 95, new Color(150,160,190));
            Context.Font.DrawCentered(spriteBatch, "PAINT TREK", 210, 95, new Color(150,160,190));
            
            _menu.Draw(spriteBatch);

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                var statusColor = _isAdLoading
                    ? new Color(100, 200, 255)
                    : new Color(240, 80, 80);

                Context.Font.DrawCentered(spriteBatch, _statusMessage, cx, 168f, statusColor);
            }
        }

        /// <summary>Draw high-res game posters, market icons, and top-layer notifications at real screen resolution.</summary>
        public override void DrawHighRes(SpriteBatch spriteBatch, Graphics.VirtualScreen screen)
        {
            // Map virtual rectangles to physical backbuffer coordinates
            spriteBatch.Draw(_blocked, screen.ToPhysical(_blockedRect), Color.White);
            spriteBatch.Draw(_paintTrek, screen.ToPhysical(_paintRect), Color.White);
            spriteBatch.Draw(_iconAndroid, screen.ToPhysical(_blockedAndroidBtn), Color.White);
            spriteBatch.Draw(_iconMsStore, screen.ToPhysical(_blockedMsStoreBtn), Color.White);
            spriteBatch.Draw(_iconAndroid, screen.ToPhysical(_paintAndroidBtn), Color.White);
            spriteBatch.Draw(_iconMsStore, screen.ToPhysical(_paintMsStoreBtn), Color.White);

            // Floating gold grant text drawn on the absolute topmost layer over all icons and posters
            if (_floatingTextTimer > 0f)
            {
                float alpha = MathHelper.Clamp(_floatingTextTimer / 0.8f, 0f, 1f);
                Color goldColor = new Color(255, 215, 0) * alpha;
                Color shadowColor = new Color(0, 0, 0, 220) * alpha;

                float cx = screen.Width / 2f;
                Vector2 physicalPos = screen.ToPhysical(new Vector2(cx, _floatingTextY));
                float fontScale = 1.5f * screen.ScaleY;
                float shadowOffset = Math.Max(1f, 1.5f * screen.ScaleY);

                // Sleek backdrop badge to guarantee readability over high-contrast images
                Vector2 textSize = Context.Font.Measure(_floatingText, fontScale);
                float padX = 8f * screen.ScaleX;
                float padY = 4f * screen.ScaleY;
                var badgeRect = new Rectangle(
                    (int)(physicalPos.X - textSize.X / 2f - padX),
                    (int)(physicalPos.Y - padY),
                    (int)(textSize.X + padX * 2f),
                    (int)(textSize.Y + padY * 2f));

                Color badgeBg = new Color(12, 14, 24, (int)(220 * alpha));
                spriteBatch.Draw(Context.Textures.Pixel, badgeRect, badgeBg);

                Color borderCol = new Color(255, 215, 0, (int)(160 * alpha));
                int borderThickness = Math.Max(1, (int)(1f * screen.ScaleY));
                spriteBatch.Draw(Context.Textures.Pixel, new Rectangle(badgeRect.X, badgeRect.Y, badgeRect.Width, borderThickness), borderCol);
                spriteBatch.Draw(Context.Textures.Pixel, new Rectangle(badgeRect.X, badgeRect.Bottom - borderThickness, badgeRect.Width, borderThickness), borderCol);
                spriteBatch.Draw(Context.Textures.Pixel, new Rectangle(badgeRect.X, badgeRect.Y, borderThickness, badgeRect.Height), borderCol);
                spriteBatch.Draw(Context.Textures.Pixel, new Rectangle(badgeRect.Right - borderThickness, badgeRect.Y, borderThickness, badgeRect.Height), borderCol);

                // Subtle shadow for legibility over any graphics
                Context.Font.DrawCentered(spriteBatch, _floatingText, physicalPos.X + shadowOffset, physicalPos.Y + shadowOffset, shadowColor, fontScale);
                Context.Font.DrawCentered(spriteBatch, _floatingText, physicalPos.X, physicalPos.Y, goldColor, fontScale);
            }
        }
    }
}
